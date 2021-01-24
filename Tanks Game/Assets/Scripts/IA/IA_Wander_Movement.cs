using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IA_Wander_Movement : MonoBehaviour
{
    public NavMeshAgent agent;
    public Vector3 cTarget = Vector3.zero;
    public float stopDistance = 0.1f;

    private void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        cTarget = transform.position;
    }

    public void SetWander()
    {
        cTarget = Wander();
        agent.SetDestination(cTarget);
    }

    public void SetDestination(Vector3 destination)
    {
        cTarget = destination;
        agent.SetDestination(cTarget);
    }

    public Vector3 Wander()
    {
        float radius = 2f;
        float offset = 3f;

        Vector3 localTarget = new Vector3(
            Random.Range(-1.0f, 1.0f), 0,
            Random.Range(-1.0f, 1.0f));

        localTarget.Normalize();
        localTarget *= radius;
        localTarget += new Vector3(0, 0, offset);

        Vector3 worldTarget =
            transform.TransformPoint(localTarget);

        worldTarget.y += 1;
        if(Physics.CheckSphere(worldTarget, 0.2f, LayerMask.NameToLayer("Default"))) 
        {
            worldTarget = (gameObject.transform.forward * -1) * (worldTarget.sqrMagnitude);
        }
        worldTarget.y -= 1;


        return worldTarget;
    }
}
