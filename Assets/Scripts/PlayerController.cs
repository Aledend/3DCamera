using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{

    private Vector3 movementInput;
    private Rigidbody rb;
    private bool jump;

    [SerializeField]
    private LayerMask groundLayer;
    bool onGround = false;
    [SerializeField]
    Transform feetTransform;
    [SerializeField]
    float collisionRadius;

    Quaternion targetRotation = Quaternion.identity;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        

        

    }

    // Update is called once per frame
    void Update()
    {
        CheckGround();
        movementInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (movementInput.sqrMagnitude > 1)
        {
            movementInput = movementInput.normalized;
        }

        if (movementInput.sqrMagnitude != 0)
        {
            transform.Translate(movementInput * Time.deltaTime * 5f);
        }

        if (onGround && Input.GetKeyDown(KeyCode.Space))
        {
            rb.velocity += new Vector3(0, 5f, 0);
        }

        

        if (Input.GetAxis("Mouse X") != 0)
        {
            //transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0));
            targetRotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse X"), Vector3.up);
        }
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 30f);
    }

    private void CheckGround()
    {
        //Debug.Log(feetTransform.position);
        //onGround = Physics.CheckSphere(feetTransform.position, collisionRadius, groundLayer);
        
        RaycastHit[] hits;

        hits = Physics.SphereCastAll(feetTransform.position, collisionRadius, Vector3.down, 0);

        foreach(RaycastHit hit in hits)
        {
            if (hit.transform.gameObject != gameObject)
            {
                onGround = true;
                return;
            }
        }
        onGround = false;
    }
}
