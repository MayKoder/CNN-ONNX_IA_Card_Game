using UnityEngine;
using Pada1.BBCore;           // Code attributes
using Pada1.BBCore.Tasks;     // TaskStatus
using Pada1.BBCore.Framework; // BasePrimitiveAction
using UnityEngine.AI;

[Action("MyActions/MoveToAmmo")]
[Help("MoveToAmmo")]
public class MoveToAmmo : BasePrimitiveAction
{
    [InParam("Game Object")]
    [Help("Game object to add the component, if no assigned the component is added to the game object of this behavior")]
    public GameObject targetGameobject;

    public override TaskStatus OnUpdate()
    {
        IA_AutoShootSystem aiShoot = targetGameobject.GetComponent<IA_AutoShootSystem>();

        aiShoot.gameObject.GetComponent<NavMeshAgent>().isStopped = false;

        Vector3 ammoPoint;
        if (aiShoot.target != null)
            ammoPoint = IA_GameManager.GetClosestElement(aiShoot.gameObject.transform.position, AI_AmmoSpawner.instance.GetSpawnedPositions(), aiShoot.target.GetComponent<IA_AutoShootSystem>());
        else
            ammoPoint = AI_AmmoSpawner.instance.GetSpawnedPositions()[0];

        aiShoot.target = null;


        if (targetGameobject.GetComponent<Patrol_Movement>() != null)
        {
            targetGameobject.GetComponent<Patrol_Movement>().SetDestination(ammoPoint);
        }
        else
        {
            targetGameobject.GetComponent<IA_Wander_Movement>().SetDestination(ammoPoint);
        }

        aiShoot.currentState = TankState.PICKING_AMMO;

        Debug.Log("Moving to new ammo box");
        return TaskStatus.COMPLETED;
    }
}

[Action("MyActions/Shoot")]
[Help("Shooting to enemy")]
public class Shoot : BasePrimitiveAction
{
    [InParam("Game Object")]
    [Help("Game object to add the component, if no assigned the component is added to the game object of this behavior")]
    public GameObject targetGameobject;

    [InParam("Target Shoot")]
    [Help("Game object to add the component, if no assigned the component is added to the game object of this behavior")]
    public IA_AutoShootSystem targetAIShoot;

    [InParam("Shoot Angle")]
    [Help("Game object to add the component, if no assigned the component is added to the game object of this behavior")]
    public float TANyAngle;

    public override TaskStatus OnUpdate()
    {
        //Debug.Log("Ayo nigga amma pick some ammo to kill this hood niggas");
        targetAIShoot.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
        //Debug.Log(Mathf.Rad2Deg * Mathf.Atan(TANyAngle));

        float finalAngle = Mathf.Rad2Deg * Mathf.Atan(TANyAngle);

        //if (target.transform.position.x < 0)
        //    finalAngle *= -1;

        targetAIShoot.turret.Rotate(targetAIShoot.turret.right, finalAngle, Space.World);

        //Debug.Log(TANyAngle.ToString());
        if (targetAIShoot.canShot)
        {
            GameObject bl = GameObject.Instantiate(targetAIShoot.bullet, targetAIShoot.shootPoint.position, Quaternion.identity) as GameObject;
            bl.GetComponent<Rigidbody>().velocity = targetAIShoot.shootPoint.forward * targetAIShoot.v;

            //Remove one shell from current ammo
            targetAIShoot.ChangeAmmo(targetAIShoot.currentAmmo - 1);

            targetAIShoot.canShot = false;
            targetAIShoot.shotDelay = Random.Range(2.0f, 4.0f);
        }
        targetGameobject.GetComponent<IA_AutoShootSystem>().currentState = TankState.SHOOTING;

        return TaskStatus.COMPLETED;
    }
}

[Action("MyActions/Patrol Movement")]
[Help("Set patrol movement")]
public class PatrolMovement : BasePrimitiveAction
{
    [InParam("Game Object")]
    [Help("Game object to add the component, if no assigned the component is added to the game object of this behavior")]
    public GameObject targetGameobject;

    public override TaskStatus OnUpdate()
    {
        if (targetGameobject.GetComponent<IA_AutoShootSystem>().currentState == TankState.NOT_MOVING)
        {
            targetGameobject.GetComponent<Patrol_Movement>().ChangeTarget();
            Debug.Log("Moving to patrol point");
            targetGameobject.GetComponent<IA_AutoShootSystem>().currentState = TankState.MOVING;
        }
        return TaskStatus.COMPLETED;
    }
}

[Action("MyActions/Wander Movement")]
[Help("Set wander movement")]
public class WanderMovement : BasePrimitiveAction
{
    [InParam("Game Object")]
    [Help("Game object to add the component, if no assigned the component is added to the game object of this behavior")]
    public GameObject targetGameobject;

    public override TaskStatus OnUpdate()
    {
        if (targetGameobject.GetComponent<IA_AutoShootSystem>().currentState == TankState.NOT_MOVING)
        {
            IA_Wander_Movement wanderComp = targetGameobject.GetComponent<IA_Wander_Movement>();
            wanderComp.cTarget = wanderComp.Wander();
            wanderComp.agent.SetDestination(wanderComp.cTarget);

            targetGameobject.GetComponent<IA_AutoShootSystem>().currentState = TankState.MOVING;
        }

        return TaskStatus.COMPLETED;
    }
}