using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_AmmoSpawner : MonoBehaviour
{
    public static AI_AmmoSpawner instance;

    public GameObject ammoPrefab;

    public Vector3[] ammoSpawnPoints;
    public List<GameObject> spawnedAmmoCrates = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        RespawnCrates();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < ammoSpawnPoints.Length; i++)
        {
            Gizmos.DrawWireSphere(ammoSpawnPoints[i], 1.0f);
        }
        Gizmos.color = Color.white;
    }

    public void AmmoPickUp(GameObject picked)
    {
        spawnedAmmoCrates.Remove(picked);
        Destroy(picked);

        if (spawnedAmmoCrates.Count == 0)
            RespawnCrates();

    }

    public Vector3[] GetSpawnedPositions()
    {
        Vector3[] positions = new Vector3[spawnedAmmoCrates.Count];

        for (int i = 0; i < spawnedAmmoCrates.Count; i++)
        {
            positions[i] = spawnedAmmoCrates[i].transform.position;
        }

        return positions;
    }

    void RespawnCrates()
    {
        for (int i = 0; i < ammoSpawnPoints.Length; i++)
        {
           spawnedAmmoCrates.Add(Instantiate(ammoPrefab, ammoSpawnPoints[i], Quaternion.identity));
        }
    }
}
