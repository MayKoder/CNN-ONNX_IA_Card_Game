using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekTank : MonoBehaviour
{

    public Transform targetPos;

    public float maxTurnSpeed = 0;
    public float maxSpeed = 0;

    public float acceleration = 0;
    public float turnAcceleration = 0;
    public float movSpeed = 0;
    public float turnSpeed = 0;

    public float stopDistance = 0;

    //unknown variables xD
    private Vector3 movement = Vector3.zero;
    private Quaternion rotation;

    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Vector3.Distance(targetPos.position, transform.position) < stopDistance)
            return;
        
        Seek();

        turnSpeed += turnAcceleration * Time.fixedDeltaTime;
        turnSpeed = Mathf.Min(turnSpeed, maxTurnSpeed);
        movSpeed += acceleration * Time.fixedDeltaTime;
        movSpeed = Mathf.Min(movSpeed, maxSpeed);

        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.fixedDeltaTime * turnSpeed);
        transform.position += transform.forward.normalized * movSpeed * Time.fixedDeltaTime;
    }

    private void Seek()
    {
        Vector3 direction = targetPos.position - transform.position;
        direction.y = 0.0f;
        movement = direction.normalized * acceleration;
        float angle = Mathf.Rad2Deg * Mathf.Atan2(movement.x, movement.z);
        rotation = Quaternion.AngleAxis(angle, Vector3.up);
    }
}
