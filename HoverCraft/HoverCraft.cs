using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverCraft : MonoBehaviour {

    [Range(1,20)]
    public float speed = 1f;
    [Range(1,20)]
    public float turnSpeed = 1f;
    public float hoverForce = 65f;
    private float powerInput;
    private float turnInput = 0f;
    private float turnDegree = 0f;
    private Vector3 normal = Vector3.zero;
    private Rigidbody carRigidbody;
    private Quaternion surfaceRotation;


    void Awake()
    {
        carRigidbody = GetComponent<Rigidbody>();
        surfaceRotation = Quaternion.Euler(Vector3.zero);
    }

    void Update()
    {
        powerInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal") * (turnSpeed+5)/20;
        turnDegree = (turnDegree + turnInput) % 360;
    }

    void FixedUpdate()
    {
        Ray ray = new Ray(transform.position + (transform.forward * .5f), -Vector3.up);
        RaycastHit hit;
        Debug.DrawRay(transform.position + (transform.forward * .5f), -Vector3.up);

        if (Physics.Raycast(ray, out hit, 1, 1 << 9)) 
        {
            normal = hit.normal;
            transform.position = new Vector3(transform.position.x, hit.point.y + 1, transform.position.z);
        } else
        {
            transform.position += Vector3.down * Time.deltaTime;
            normal = Vector3.Slerp(normal, Quaternion.Euler(-30, 0, 0) * Vector3.forward, .005f);
        }
        surfaceRotation = Quaternion.Slerp(surfaceRotation, Quaternion.Euler(0, turnDegree, -turnInput * (50 * (turnSpeed + 45) / 50)) * Quaternion.FromToRotation(Vector3.up, normal), .2f);
        transform.rotation = surfaceRotation;
        carRigidbody.velocity = transform.forward * (20 + (speed));
    }
}
