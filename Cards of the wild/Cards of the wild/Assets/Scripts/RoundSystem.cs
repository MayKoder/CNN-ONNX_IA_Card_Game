using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundSystem
{
    public int iaRounds = 0;
    public int enemyRounds = 0;
    public int currentRoundToPlay = 0;
    public bool isPlaying = false;

    public void StartGame(int rounds)
    {
        currentRoundToPlay = rounds;
        isPlaying = true;
    }

    public void ProcessRound(float iaScore)
    {
        if (iaScore > 0)
        {
            iaRounds++;
        }
        else if (iaScore != -0.1f)
        {
            enemyRounds++;
        }

        if (currentRoundToPlay <= 0)
        {
            isPlaying = false;
            GetWinner();
            //UI_Manager.instance.FinishFullGame();
            UI_Manager.instance.StartFinishAnimation();
            iaRounds = 0;
            enemyRounds = 0;
            currentRoundToPlay = 0;
        }
    }

    public void GetWinner()
    {
        if(iaRounds > enemyRounds)
        {
            UI_Manager.instance.fadeInText.color = UI_Manager.instance.winColor;
            UI_Manager.instance.fadeInText.text = "IA won" + System.Environment.NewLine + "With " + iaRounds.ToString() + " wins out of 15 rounds";
        }
        else if(iaRounds == enemyRounds)
        {
            UI_Manager.instance.fadeInText.color = UI_Manager.instance.tieColor;
            UI_Manager.instance.fadeInText.text = "It was a tie" + System.Environment.NewLine + "With " + (enemyRounds+iaRounds).ToString() + " draws out of 15 rounds";
        }
        else if(iaRounds < enemyRounds)
        {
            UI_Manager.instance.fadeInText.color = UI_Manager.instance.lossColor;
            UI_Manager.instance.fadeInText.text = "Enemy won" + System.Environment.NewLine + "With " + enemyRounds.ToString() + " wins out of 15 rounds";
        }
    }

}
