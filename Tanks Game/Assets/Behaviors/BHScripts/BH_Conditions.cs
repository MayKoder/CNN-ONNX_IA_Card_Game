using UnityEngine;
using Pada1.BBCore;
using Pada1.BBCore.Framework;
using UnityEngine.AI;

[Condition("MyConditions/Is ammo over?")]
[Help("Checks if the ammo is over")]
public class CheckAmmo : ConditionBase
{
    [InParam("Target Shoot")]
    [Help("Game object to add the component, if no assigned the component is added to the game object of this behavior")]
    public IA_AutoShootSystem targetAIShoot;

    public override bool Check()
    {
        return (targetAIShoot.currentAmmo == 0 && targetAIShoot.currentState != TankState.PICKING_AMMO) ? true:  false;
    }
}

[Condition("MyConditions/AngleWithEnemy")]
[Help("Checks if there is a shoot angle with the enemy")]
public class AngleWithEnemy : ConditionBase
{
    [InParam("Target Shoot")]
    [Help("Game object to add the component, if no assigned the component is added to the game object of this behavior")]
    public IA_AutoShootSystem targetAIShoot;

    [OutParam("Shoot Angle")]
    [Help("Game object to add the component, if no assigned the component is added to the game object of this behavior")]
    public float TANyAngle;

    public override bool Check()
    {
        if (targetAIShoot.target)
        {
            float g = Physics.gravity.y;

            Vector3 dist = (targetAIShoot.target.transform.position - targetAIShoot.shootPoint.position);
            float x = Mathf.Sqrt(Mathf.Pow(dist.x, 2) + Mathf.Pow(dist.z, 2));
            x -= Vector3.Distance(targetAIShoot.turret.position, targetAIShoot.shootPoint.position) * 2;

            float y = targetAIShoot.target.transform.position.y/*+ 2*/;

            float sq1 = Mathf.Pow(targetAIShoot.v, 4) - g * (g * Mathf.Pow(x, 2) + 2 * y * Mathf.Pow(targetAIShoot.v, 2));
            float sqrt = Mathf.Pow(targetAIShoot.v, 2) + Mathf.Sqrt(sq1);

            if (!targetAIShoot.canShot)
            {
                targetAIShoot.cTime += Time.deltaTime;
                if (targetAIShoot.cTime >= targetAIShoot.shotDelay)
                {
                    targetAIShoot.canShot = true;
                    targetAIShoot.cTime = 0.0f;
                }

            }


            targetAIShoot.lookVector.Set(targetAIShoot.target.transform.position.x, targetAIShoot.shootPoint.position.y, targetAIShoot.target.transform.position.z);
            targetAIShoot.turret.LookAt(targetAIShoot.lookVector);

            if (targetAIShoot.currentAmmo > 0)
            {
                TANyAngle = sqrt / (g * x);

                if (!float.IsNaN(Mathf.Atan(TANyAngle)))
                {
                    return true;
                }
                else
                {
                    targetAIShoot.gameObject.GetComponent<NavMeshAgent>().isStopped = false;
                }
            }
        }

        return false;
    }
}

[Condition("MyConditions/Is Enemy")]
[Help("Checks if the tanks is an enemy")]
public class CheckEnemy : ConditionBase
{
    [InParam("Is Enemy")]
    //[Help("Game object to add the component, if no assigned the component is added to the game object of this behavior")]
    public bool isEnemy;

    public override bool Check()
    {
        return isEnemy;
    }
}

[Condition("MyConditions/Is point reached")]
[Help("Checks if the tanks is an enemy")]
public class PointReached : ConditionBase
{
    [OutParam("Target point")]
    public Vector3 cTarget;

    [InParam("Game Object")]
    public GameObject targetGameobject;

    [InParam("Stop distance")]
    public float stopDistance;

    public override bool Check()
    {

        if (targetGameobject.GetComponent<Patrol_Movement>())
        {
            cTarget = targetGameobject.GetComponent<Patrol_Movement>().cTarget;
        }
        else if (targetGameobject.GetComponent<IA_Wander_Movement>())
        {
            cTarget = targetGameobject.GetComponent<IA_Wander_Movement>().cTarget;
        }

        if (Vector3.Distance(cTarget, targetGameobject.transform.position) < stopDistance && targetGameobject.GetComponent<IA_AutoShootSystem>().currentState != TankState.PICKING_AMMO)
        {
            targetGameobject.GetComponent<IA_AutoShootSystem>().currentState = TankState.NOT_MOVING;
            return true;
        }

        return false;
    }
}