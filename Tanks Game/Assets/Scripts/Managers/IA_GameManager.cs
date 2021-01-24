using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.AI;

public class IA_GameManager : MonoBehaviour
{

    public static IA_GameManager instance = null;

    public GameObject playerTank = null;
    public List<GameObject> sceneEnemyTanks = new List<GameObject>();

    [Header("Script Controllers")]
    public Complete.CameraControl camControl = null;

    [Header("Score")]
    public float score = 0;
    public GameObject scoreUI;
    public Text scoreText;

    private void Awake()
    {
        if (instance != null)
            Debug.LogError("More than one game manager in the scene");

        instance = this;
        Time.timeScale = 1f;
    }

    public void FinishGame()
    {
        Debug.Log("Game is over");
        playerTank.GetComponent<IA_AutoShootSystem>().SetTarget(null);


        camControl.enabled = false;
        scoreText.text = score.ToString() + " points";
        scoreUI.SetActive(true);

        //camControl.cameras
        foreach (GameObject item in camControl.cameras)
        {
            item.SetActive(false);
        }
        camControl.cameras[0].SetActive(true);

        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        SceneManager.LoadSceneAsync("IA_Tanks-Miquel-Nadine");
    }
    public void Exit()
    {
        Application.Quit();
    }

    public static Vector3 GetClosestElement(Vector3 point, Vector3[] array, IA_AutoShootSystem target)
    {
        Vector3 closest = array[0];
        NavMeshAgent targetAgent = target.GetComponent<NavMeshAgent>();

        float lastDist = 0;
        for (int i = 0; i < array.Length; i++)
        {
            float cDist = Vector3.Distance(point, array[i]);
            if (cDist < lastDist && targetAgent.pathEndPosition != array[i]) // Not the fastest way tbh
            { 
                closest = array[i];
                lastDist = cDist;
            }
        }
        return closest;
    }
}
