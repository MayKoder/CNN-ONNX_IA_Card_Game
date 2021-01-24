using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Patrol_Movement : MonoBehaviour
{

    public Vector3[] points;

    public NavMeshAgent agent;

    public float stopDistance = 0;

    public BehaviorExecutor exec;

    public Vector3 cTarget = Vector3.zero;
    int targetID = 0;
    private void OnDrawGizmos()
    {

        for (int i = 0; i < points.Length; i++)
        {
            Gizmos.DrawWireSphere(points[i], 1.0f);

            if(i + 1 < points.Length)
            {
                Gizmos.DrawLine(points[i], points[i + 1]);
            }
            else
            {
                Gizmos.DrawLine(points[i], points[0]);
            }

        }

    }

    // Start is called before the first frame update
    void Start()
    {
        cTarget = points[0];
        agent.SetDestination(cTarget);
    }

    public void SetDestination(Vector3 destination)
    {
        cTarget = destination;
        agent.SetDestination(cTarget);
    }

    public void ChangeTarget()
    {

        if(targetID + 1 >= points.Length)
        {
            targetID = 0;
        }
        else
        {
            targetID++;
        }
        cTarget = points[targetID];

        agent.SetDestination(cTarget);
        exec.blackboard.vector3Params[0] = cTarget;
    }
}
