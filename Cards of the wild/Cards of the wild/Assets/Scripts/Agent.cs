
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;

public class Agent : MonoBehaviour
{
	// Neural network: asset and worker
	public NNModel CNN_Model_Asset;
	private Model model;
	private IWorker worker;

	// Render textures (enemy's cards)
	public RenderTexture [] renderTextures;

	private int imgWidth, imgHeight;

	// Game parameters
	private int numEnemyCards;
	public int [] myCards;
	private int NUM_CLASSES = 3;
	private int DECK_SIZE   = 4;

	// States and Q-table
	private int enemyCombinations;
	private int deckCombinations;
	private int numActions;
	private int state, oldState;
	private int action;
	public float[, ] qTable;

    [Header("HUD / UI")]
    public SpriteRenderer[] cardDisplay;
    public Sprite[] assetsCardDisplays;
    public Text randomIndicator;


    // RL management

    // Update rate of learned values
    private float learningRate = 0.5f;	
	// Discount factor when computing the Q values
	// Note: Usually it is 0.9-0.99, but here the next state is random and is not relevant, so it is not used
	//private float gamma = 0.0f;    
	// Starting epsilon (choose random actions)
	//private float epsilon = 1.0f;
	// Minimum epsilon (after coolingSteps)
	//private float minEpsilon = 0.1f;
	// Factor to reduce epsilon (number of actions that it
	// takes it to go down to minEpsilon)
	//private int coolingSteps = 10000;



	// Not using Start() so we are positive that this is run before the agent is asked to play
    public void Initialize()
    {

    	imgWidth  = renderTextures[0].width;
    	imgHeight = renderTextures[0].height;

    	// Create the neural network
    	model  = ModelLoader.Load(CNN_Model_Asset, false);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharp, model, false);
        Debug.Log("started"); 

        // Gather other game information
        numEnemyCards = renderTextures.Length;

        myCards = new int[NUM_CLASSES];  // counts how many cards of each class the agent has

        enemyCombinations = Mathf.FloorToInt(Mathf.Pow(NUM_CLASSES, numEnemyCards));      
        deckCombinations  = Mathf.FloorToInt(Mathf.Pow(DECK_SIZE+1, myCards.Length-1));      

        // The max number of possible actions is the same as the number of enemy combinations
        // (NUM_CLASSES ^ numCards)
        numActions = enemyCombinations;

        // Generate the Q-table 
        qTable = new float[enemyCombinations*deckCombinations, numActions];

        //Debug.LogError(qTable.GetLength(0) + " and " + qTable.GetLength(1));
        LoadLearning();
        
        //LoadLearning();
        //foreach (var item in qTable)
        //{
        //    Debug.Log(item.ToString());
        //}
    }

    private void OnDestroy()
    {
        SaveLearning();
    }


    // Play one turn
    // deck: the cards the player has to play with
    // enemyChars: the labels of the enemy's characters (for debug/evaluation purposes only)
    public int [] Play(int [] deck, int [] realEnemyCards) 
    {
    	// First: read the enemy cards
    	int [] predictedEnemyCards = new int[numEnemyCards];

    	for(int i=0; i<renderTextures.Length; i++)
    	{
    		predictedEnemyCards[i] = ClassifyCard(renderTextures[i]);

            if (predictedEnemyCards[i] != realEnemyCards[i])
            {
                Debug.LogError("IA failed here");
                return null;
            }

           // Debug.Log(i + "IA thinks - Real enemy card: " + GameRun.IntToCardName(predictedEnemyCards[i]) + " -> " + GameRun.IntToCardName(realEnemyCards[i]));
    	}

    	//foreach(int x in enemyCards)
    	//	Debug.Log(x);

    	// Second: compute the state of the game
    	CountMyCards(deck);

    	oldState = state;
    	state    = ComputeState(predictedEnemyCards);

    	// Third: choose an action
    	action = ChooseAction();

    	//Debug.Log("ACTION");
    	//Debug.Log(action);


    	// Translate the action to cards and return
    	return ActionToCards(action);
    }

    public void PrintSelectedCards(int[] cardsToPlay)
    {
        for (int i = 0; i < cardsToPlay.Length; i++)
        {
            //Debug.Log(cardsToPlay[i]);
            cardDisplay[i].sprite = assetsCardDisplays[cardsToPlay[i]];
        }

       //Debug.Log("---------------------------------------------");
    }

    // Get the reward obtained from the current hand; 
    // update q-table accordingly
    // Note: the next state is not relevant here, as it is completely random
    public void GetReward(float reward)
    {
		qTable [state, action] += learningRate * (reward - qTable [state, action]);
    }

    // PRIVATE 

    // Return character in the given card (0-2)
    private int ClassifyCard(RenderTexture rtex)
    {
        // Barracuda expects NHWC (batch, height, width, channels) as image format
        // but it also accepts a RenderTexture directly, much easier here
        // Important: the pixel values must are in [0, 1] (the NN must be trained
        // with that range too)
        Tensor input = new Tensor(rtex, 3); 
        worker.Execute(input);
        var res = worker.PeekOutput();

        // This gives the class of the card
		float[] output = res.AsFloats();

        string log = rtex.name + ": ";
        foreach (var item in output)
        {
            log += item + " // ";
        }
        //Debug.Log(log);

        input.Dispose();
        return argmax(output);
    }

    private int argmax(float [] values)
    {
    	float maxVal = values[0];
    	int   maxIdx = 0;

    	for(int i=1; i<values.Length; i++)
    		if(values[i] > maxVal)
    		{
    			maxVal = values[i];
    			maxIdx = i;
    		}

    	return maxIdx;
    }

    
    // Given an array of cards [0, 1, 2, 1] that encodes the cards drawn by the player, 
    // set array myCards with the amount of cards of each class
    private void CountMyCards(int [] deck) 
    {
    	for(int i=0; i<myCards.Length; i++)
    		myCards[i] = 0;

    	for(int i=0; i<deck.Length; i++)
    		myCards[deck[i]]++;

    	//Debug.Log("ExtractMyCards");
    	//foreach(int x in myCards)
    	//	Debug.Log(x);

    }


    // Compute the state of the game given the cards of the enemy and
    // the deck of cards of the player
    // Idea: 
    // enemy = all combinations of enemy cards (NUM_CLASSES^numCards)
    // player = number of cards of each class but the last one
    // (as it will be the remaining number of cards, so always the same
    // given the first two)
    // state = find all combinations of enemy and player
    private int ComputeState(int [] enemyCards) //I think this is wrong, if it is, all the systems won't work
    {
    	int enemy = 0;
    	foreach(int card in enemyCards)
    	{
    		enemy *= NUM_CLASSES;
    		enemy += card;
    	}
        //if(enemyCards[0] == 2 && enemyCards[1] == 2 && enemyCards[2] == 2)
        //    Debug.LogWarning("Max enemy state is: " + enemy.ToString());

        // player cards: each class can have 5 (DECK_SIZE+1) different values
        int player = 0;
    	for(int i=0; i<myCards.Length-1; i++)
    	{
    		player *= (DECK_SIZE + 1);
    		player += myCards[i];
    	}

        //if(myCards[0] == 4)
        //{
        //    GameRun.instance.loopGame.isOn = false;
        //    Debug.Log(myCards);
        //    Debug.LogWarning("Max player state is: " + player.ToString());
        //}

        // Combine both. We need the max possible of one of them to convert
        // 2-indices to 1-index
        int state = enemy + player*enemyCombinations;
    	//Debug.Log("STATE");
    	//Debug.Log(enemy);
    	//Debug.Log(player);
    	//Debug.Log(state);

    	return state;
    }


    private int ChooseAction()
    {
    	int action = -1;

        bool reCheck = true;
        while (reCheck == true)
        {
		    // Decide if it is going to be a random action or
		    // according to Q table
		    if (/*Random.Range (0.0f, 1.0f) < epsilon &&*/ UI_Manager.instance.useRandomGeneration) 	// Random action
		    {
                // Random.Range with ints does not include the maximum
                action = Random.Range(0, numActions);
                randomIndicator.text = "Action is: Random with value " + action.ToString();
		    } 
		    else 	// Best action according to Q-table (column with highest value)
		    {
			    int colMax = 0;
			    for (int col = 1; col < numActions; col++)
				    if (qTable [state, col] > qTable [state, colMax])
					    colMax = col;

			    action = colMax;

                randomIndicator.text = "Action is: Not random with value " + action.ToString();
            }

            if (action != -1)
            {
                if(GameRun.IsActionValid(myCards, action, this) == true)
                {
                    //Debug.LogWarning("This should be valid: " + action);
                    reCheck = false;
                }
                else
                {
                    qTable[state, action] += GameRun.instance.RWD_ACTION_INVALID; 
                }
            }
        }


		// Reduce epsilon (to gradually reduce the random exploration)
		//if (epsilon > minEpsilon && GameRun.instance.useRandomGeneration) 
  //      { 
		//	epsilon -= ((1.0f - minEpsilon) / (float)coolingSteps); 
		//}

		return action;
    }
   

    // Translate the int that encodes the action into an array with the
    // cards that the agent is playing
   	public int [] ActionToCards(int action)
   	{
   		//Debug.Log("ActionToCards");
   		//Debug.Log(action);

   		int [] cards = new int[numEnemyCards];  // Enemy and player show the same number of cards

   		for(int i=numEnemyCards-1; i>=0; i--)
   		{
   			cards[i] = action % NUM_CLASSES;
   			action  /= NUM_CLASSES;
   		}

        //foreach (int x in cards)
        //    Debug.Log(x);

        return cards;
   	}

    public void SaveLearning()
    {
        FileStream file;

        string destination = Application.streamingAssetsPath + "/qTableData.dat";
        //Debug.Log(destination);
        file = File.Create(destination);

        Debug.Log(qTable[10, 10]);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, qTable);

        file.Close();
    }

    public void LoadLearning()
    {
        FileStream file;

        string destination = Application.streamingAssetsPath + "/qTableData.dat";
        if (File.Exists(destination))
            file = File.OpenRead(destination);
        else
            return;

        BinaryFormatter bf = new BinaryFormatter();
        qTable = (float[,])bf.Deserialize(file);

        Debug.Log(qTable[10, 10]);

        file.Close();
    }

    public void Debug2DArray(ref string text, float[,] array)
    {
        text = string.Empty;

        for (int y = 0; y < array.GetLength(1); y++)
        {
            for (int x = 0; x < array.GetLength(0); x++)
            {
                text += array[x, y].ToString() + " | ";
            }
            text += System.Environment.NewLine;
        }
    }
}
