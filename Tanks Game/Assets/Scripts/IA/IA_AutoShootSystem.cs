using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO: Move tank logic as states, MOVING, SHOOTING, RELOADING AMMO etcetc, will make all this cleaner
public enum TankState
{
    MOVING, SHOOTING, PICKING_AMMO, DYING, NOT_MOVING
}

public class IA_AutoShootSystem : MonoBehaviour
{
    public GameObject target;
    public Vector3 lookVector = Vector3.zero;

    public float cTime = 0.0f;
    public float shotDelay = 2.0f;
    public bool canShot = true;

    public float v = 20.0f;
    public Transform turret;
    public Transform shootPoint;

    public GameObject bullet;
    public bool isPlayer = false;

    [Header("Ammo logic")]

    [Range(0, 50)]
    public int maxAmmo = 10;
    public int currentAmmo;
    public TankState currentState;

    public Text ammoText;

    // Start is called before the first frame update
    void Awake()
    {
        ChangeAmmo(maxAmmo);
        currentState = TankState.MOVING;
    }

    // Update is called once per frame
    void Update()
    {

        if (isPlayer && target == null)
        {
            GameObject closestOne = null;
            float closestDistance = float.PositiveInfinity;
            foreach (GameObject item in IA_GameManager.instance.sceneEnemyTanks)
            {
                //ERROR: Only a
                float currentDistance = Vector3.Distance(transform.position, item.transform.position);
                if (currentDistance < closestDistance)
                {
                    closestOne = item;
                    closestDistance = currentDistance;
                    break;
                }
            }
            SetTarget(closestOne);
        }

    }

    public void SetTarget(GameObject _target)
    {
        target = _target;

        //ERROR: Only stop if there is a possible angle
        if(target == null)
        {
            //gameObject.GetComponent<NavMeshAgent>().isStopped = false;
            turret.LookAt(turret.position + (transform.forward * 2));
        }
    }
    public GameObject GetTarget()
    {
        return target;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (!AI_AmmoSpawner.instance.spawnedAmmoCrates.Contains(other.gameObject))
            return; 

        Debug.Log("Ayo picking ammo");
        AI_AmmoSpawner.instance.AmmoPickUp(other.gameObject);
        ChangeAmmo(maxAmmo);

        currentState = TankState.MOVING;
        if (gameObject.GetComponent<Patrol_Movement>() != null)
        {
            gameObject.GetComponent<Patrol_Movement>().ChangeTarget(); //TODO: Maybe get closest patrol movement?

            IA_Wander_Movement enemyWander = IA_GameManager.instance.sceneEnemyTanks[0].GetComponent<IA_Wander_Movement>();
            if(enemyWander.cTarget.x == other.transform.position.x && enemyWander.cTarget.y == other.transform.position.y)
            {
                Debug.Log("Same target ammo lamo");
                enemyWander.SetDestination(AI_AmmoSpawner.instance.GetSpawnedPositions()[0]);
            }

        }
        else{
            gameObject.GetComponent<IA_Wander_Movement>().SetWander();
            SetTarget(IA_GameManager.instance.playerTank);

            Patrol_Movement playerPatrol = IA_GameManager.instance.playerTank.GetComponent<Patrol_Movement>();
            if (playerPatrol.cTarget.x == other.transform.position.x && playerPatrol.cTarget.y == other.transform.position.y)
            {
                Debug.Log("Same target ammo lamo");
                playerPatrol.SetDestination(AI_AmmoSpawner.instance.GetSpawnedPositions()[0]);
            }
        }
    }

    public void ChangeAmmo(int newAmmoValue)
    {
        currentAmmo = newAmmoValue;
        ammoText.text = currentAmmo.ToString();
    }
}
