using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager instance;

    [Header("Game UI")]
    public Text playerDeck;
    public Text enemyDeck;
    public Image winStatus;
    public Toggle loopGame;
    public GameObject pauseIcon;
    public GameObject loopIcon;

    public Button[] disableOnLoop;

    // Other UI elements
    public bool useRandomGeneration = false;


    [Header("15 round settings")]
    public int maxRoundsToPlay = 15;
    public GameObject fadeIn;
    public Text fadeInText;

    [Header("Color")]
    public Color winColor;
    public Color tieColor;
    public Color lossColor;

    private void Awake()
    {
        instance = this;
    }

    public void ChangeRandomState(bool state)
    {
        useRandomGeneration = state;
        Debug.Log(state);
    }
    public void ChangeLoopState(bool state)
    {
        pauseIcon.SetActive(state);
        loopIcon.SetActive(!state);

        foreach (Button item in disableOnLoop)
        {
            item.interactable = !state;
        }
    }

    public void PlayOneGame()
    {
        if(loopGame.isOn == false)
            GameRun.instance.PlayOnCommand();
    }

    public void PlayFullGame()
    {
        foreach (Button item in disableOnLoop)
        {
            item.interactable = false;
        }
        loopGame.interactable = false;

        GameRun.instance.roundGame.StartGame(maxRoundsToPlay);
    }

    public void StartFinishAnimation()
    {
        fadeIn.SetActive(true);
        fadeIn.GetComponent<Animator>().SetBool("play", true);
    }
    public void EndFinishAnimation()
    {
        foreach (Button item in disableOnLoop)
        {
            item.interactable = true;
        }
        loopGame.interactable = true;

        fadeIn.GetComponent<Animator>().SetBool("play", false);
        fadeIn.SetActive(false);
    }
}
