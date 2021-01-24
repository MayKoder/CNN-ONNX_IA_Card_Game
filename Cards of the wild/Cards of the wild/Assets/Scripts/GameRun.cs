using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

enum CardType
{
    Fox,
    Frog,
    Opossum,
}

public class GameRun : MonoBehaviour
{
    public static GameRun instance;

	// Management of sprites
	private Object[] backgrounds;
	private Object[] props;
	private Object[] chars;

	// Game management
	private GameObject enemyCards;
	private int [] enemyChars;	
	private Agent agent;

	private int NUM_ENEMY_CARDS = 3;
	private int NUM_CLASSES     = 3;
	private int DECK_SIZE       = 4;

	// Rewards
	public float RWD_ACTION_INVALID = -2.0f;
	private float RWD_HAND_LOST      = -1.0f;
	private float RWD_TIE            = -0.1f;
	private float RWD_HAND_WON       =  1.0f;

    public RoundSystem roundGame = new RoundSystem();

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        ///////////////////////////////////////
        // Sprite management
        ///////////////////////////////////////

        // Load all prefabs
        backgrounds = Resources.LoadAll("Backgrounds/");
        props       = Resources.LoadAll("Props/");
        chars       = Resources.LoadAll("Chars/");


        ///////////////////////////////////////
        // UI management
        ///////////////////////////////////////
        UI_Manager.instance.playerDeck = GameObject.Find("PlayerDeck").GetComponent<UnityEngine.UI.Text>();


        ///////////////////////////////////////
        // Game management
        ///////////////////////////////////////
        enemyCards = GameObject.Find("EnemyCards");
        enemyChars = new int[NUM_ENEMY_CARDS];

        agent      = GameObject.Find("AgentManager").GetComponent<Agent>();

        agent.Initialize();

        UI_Manager.instance.loopGame.isOn = false;

        ///////////////////////////////////////
        // Start the game
        ///////////////////////////////////////
        //StartCoroutine("GenerateTurn");


        ///////////////////////////////////////
        // Image generation
        ///////////////////////////////////////
    	//renderTexture = gameObject.GetComponent<Camera>().targetTexture;

    	//imgWidth  = renderTexture.width;
    	//imgHeight = renderTexture.height;

        
    }

    private void Update()
    {
        if (UI_Manager.instance.loopGame.isOn)
            StartCoroutine("GenerateTurn");

        if (roundGame.currentRoundToPlay > 0)
        {
            roundGame.currentRoundToPlay--;
            StartCoroutine("GenerateTurn");
        }
    }

    public void PlayOnCommand()
    {
        StartCoroutine("GenerateTurn");
    }

    IEnumerator GenerateTurn()
    {
        //turn++;
        //for (int turn = 0; turn < 100000; turn++)
        //{        
        ///////////////////////////////////////
        // Generate enemy cards
        ///////////////////////////////////////

        // Destroy previous sprites (if any) and generate new cards
        int c = 0;
        foreach (Transform card in enemyCards.transform)
        {
            foreach (Transform sprite in card)
            {
                Destroy(sprite.gameObject);
            }

            enemyChars[c++] = GenerateCard(card);

        }
        UI_Manager.instance.enemyDeck.text = "Deck: ";
        foreach (int card in enemyChars)
            UI_Manager.instance.enemyDeck.text += IntToCardName(card) + "/";


        ///////////////////////////////////////
        // Generate player deck
        ///////////////////////////////////////
        int[] deck = GeneratePlayerDeck();
        UI_Manager.instance.playerDeck.text = "Deck: ";
        foreach (int card in deck)
            UI_Manager.instance.playerDeck.text += IntToCardName(card) + "/";


        ///////////////////////////////////////
        // Tell the player to play
        ///////////////////////////////////////
        yield return new WaitForEndOfFrame();

        int[] action = agent.Play(deck, enemyChars);

        if(action != null)
        {

            //playerDeck.text += System.Environment.NewLine + "Action:";
            //foreach (int a in action)
            //    playerDeck.text += IntToCardName(a) + "/";

            ///////////////////////////////////////
            // Compute reward
            ///////////////////////////////////////
            //float reward = ComputeReward(agent.myCards, action);
            float reward = ComputeReward(agent.myCards, action);

            if (reward > 0)
            {
                UI_Manager.instance.winStatus.color = Color.green;
            }
            else if (reward == -0.1f)
            {
                UI_Manager.instance.winStatus.color = Color.yellow;
            }
            else
            {
                UI_Manager.instance.winStatus.color = Color.red;
            }

            //Debug.Log("Turn/reward: " + turn.ToString() + "->" + reward.ToString()); //UNCOMMENT THIS

            agent.GetReward(reward);

            if (roundGame.isPlaying)
            {
                Debug.Log("Round played");
                roundGame.ProcessRound(reward);
            }


            ///////////////////////////////////////
            // Manage turns/games
            ///////////////////////////////////////
            agent.PrintSelectedCards(action);

            //yield return new WaitForSeconds(0.001f);
        }
        else
        {
            PlayOnCommand();
        }
        //}
    }

    // Generate a card on a given transform
    // Return the label (0-2) of the card
    private int GenerateCard(Transform parent)
    {

    	int idx = Random.Range(0, backgrounds.Length);
    	Instantiate(backgrounds[idx], parent.position, Quaternion.identity, parent);


    	idx               = Random.Range(0, props.Length);
    	Vector3 position = new Vector3(Random.Range(-3.0f, 3.0f), Random.Range(-3.0f, 3.0f), -1.0f);
   	  	Instantiate(props[idx], parent.position+position, Quaternion.identity, parent);

    	idx         = Random.Range(0, chars.Length);
    	position    = new Vector3(Random.Range(-3.0f, 3.0f), Random.Range(-3.0f, 3.0f), -2.0f);    	
   	  	Instantiate(chars[idx], parent.position+position, Quaternion.identity, parent);

        // Determine label of the character, return it
        int label = -1;

        if (chars[idx].name.StartsWith("fox"))
            label = 0;
        else if (chars[idx].name.StartsWith("frog")) 
            label = 1;
   	  	else if(chars[idx].name.StartsWith("opossum")) 
            label = 2;

        if (label == -1)
            Debug.LogError("Error on enemy card generation" + "// " + chars[idx].name);

        return label;
    } 



    // Auxiliary methods
    private int [] GeneratePlayerDeck()
    {
    	int [] deck = new int [DECK_SIZE];

    	for(int i=0; i<DECK_SIZE; i++)
    	{
    		deck[i] = Random.Range(0, NUM_CLASSES);  // high limit is exclusive so [0, NUM_CLASSES-1]
    	}

    	return deck;
    }

    // Compute the result of the turn and return the associated reward 
    // given the cards selected by the agent (action)
   	// deck -> array with the number of cards of each class the player has
   	// action -> array with the class of each card played
    private float ComputeReward(int [] cardCounter, int [] action)
    {
    	// First check if the action is valid given the player's deck
    	foreach(int card in action)
    	{
    		cardCounter[card]--;
    		if(cardCounter[card] < 0)
            {
                Debug.LogError("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    			return RWD_ACTION_INVALID;
            }
    	}


    	// Second see who wins
    	int score = 0;
    	for(int i=0; i<NUM_ENEMY_CARDS; i++)
    	{
    		if(action[i] != enemyChars[i])
    		{
                if ((enemyChars[i] == (int)CardType.Fox && action[i] == (int)CardType.Opossum) 
                    || (enemyChars[i] == (int)CardType.Opossum && action[i] == (int)CardType.Frog)
                    || (enemyChars[i] == (int)CardType.Frog && action[i] == (int)CardType.Fox))
                    score--;

                else if ((enemyChars[i] == (int)CardType.Fox && action[i] == (int)CardType.Frog)
                    || (enemyChars[i] == (int)CardType.Opossum && action[i] == (int)CardType.Fox)
                    || (enemyChars[i] == (int)CardType.Frog && action[i] == (int)CardType.Opossum))
                    score++;
            }		
    	}

    	if(score == 0) return RWD_TIE;
    	else if(score > 0) return RWD_HAND_WON;
    	else if(score < 0) return RWD_HAND_LOST;

        Debug.LogError("This can't be happening");
        return RWD_ACTION_INVALID;
    }

    public static string IntToCardName(int id)
    {
        string ret = "";
        switch (id)
        {
            case 0:
                ret = "Fox";
                break;
            case 1:
                ret = "Frog";
                break;
            case 2:
                ret = "Opossum";
                break;
            default:
                Debug.LogError("UUUUUUGH");
                break;
        }
        return ret;
    }

    static public bool IsActionValid(int[] cardCounter, int actionInt, Agent agent)
    {
        int[] actionCards = agent.ActionToCards(actionInt);
        int[] counterArray = (int[])cardCounter.Clone();

        foreach (int card in actionCards)
        {
            counterArray[card]--;
            if (counterArray[card] < 0)
            {
                return false;
            }
        }

        return true;
    }
}
