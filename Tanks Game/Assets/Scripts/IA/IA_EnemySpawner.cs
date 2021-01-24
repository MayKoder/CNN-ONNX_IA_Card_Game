using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IA_EnemySpawner : MonoBehaviour
{
    public static IA_EnemySpawner instance;

    public Transform[] spawnPoints;
    public GameObject enemy;
    public Complete.CameraControl cameraControl;

    private void Awake()
    {
        if (instance != null)
            Debug.LogError("More than one spawn manager instance");

        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //for (int i = 0; i < 10; i++)
        //{
            SpawnEnemy();
        //}
    }

    public void SpawnEnemy()
    {
        //Add timer
        Transform cTransform = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject nTank = Instantiate(enemy, cTransform.position, cTransform.localRotation);
        nTank.GetComponent<IA_AutoShootSystem>().SetTarget(IA_GameManager.instance.playerTank);

        IA_GameManager.instance.sceneEnemyTanks.Add(nTank);
        cameraControl.m_Targets.Add(nTank.transform);

    }

    public void DeleteEnemy(GameObject toDelete)
    {
        if (cameraControl.m_Targets.Contains(toDelete.transform))
            cameraControl.m_Targets.Remove(toDelete.transform);
    }

}
