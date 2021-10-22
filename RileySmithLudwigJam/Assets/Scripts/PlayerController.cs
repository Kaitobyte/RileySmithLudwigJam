using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float walkSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float friction;
    [SerializeField] private Rigidbody2D playerRigidBody;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Transform feet;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float airSpeed;
    [SerializeField] private SpringJoint2D playerSpringJoint;
    [SerializeField] private float scrollScale;

    private float userInput;
    private bool isGrounded;
    private bool isGrappled;
    private bool toggleRopeEnd = false;

    private float ropeLength = float.MaxValue;
    private Vector3 grappleLocation, playerVelocity;
    private LineRenderer grapple;

    // Start is called before the first frame update
    void Start()
    {
        playerSpringJoint.enabled = false;
        grapple = GetComponentInChildren<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (grapple.GetComponent<Rope>().StoppedGrapple())
        {
            toggleRopeEnd = false;
            isGrappled = false;
            playerSpringJoint.enabled = false;
        }

        userInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && IsGrounded() && !isGrappled)
        {
            Jump();
        }

    }

    private void FixedUpdate()
    {
        float xMovement = 0f;

        if (IsGrounded())
        {
            if (playerRigidBody.velocity.x > friction)
            {
                xMovement = playerRigidBody.velocity.x + (userInput * acceleration) - friction;
            }
            else if (playerRigidBody.velocity.x < -friction)
            {
                xMovement = playerRigidBody.velocity.x + (userInput * acceleration) + friction;
            }
            else
            {
                xMovement = userInput * acceleration;
            }

        }
        else if (isGrappled)
        {
            if (playerRigidBody.velocity.x > 0)
            {
                xMovement = playerRigidBody.velocity.x + (userInput * acceleration / 2);
            }
            else if (playerRigidBody.velocity.x < 0)
            {
                xMovement = playerRigidBody.velocity.x + (userInput * acceleration / 2);
            }
            else
            {
                xMovement = userInput * acceleration;
            }
        }
        else
        {
            if (playerRigidBody.velocity.x > 0)
            {
                xMovement = playerRigidBody.velocity.x - airSpeed;
            }
            else if (playerRigidBody.velocity.x < 0)
            {
                xMovement = playerRigidBody.velocity.x + airSpeed;
            }
        }

        if (xMovement > walkSpeed)
        {
            xMovement = walkSpeed;
        }
        else if (xMovement < -walkSpeed)
        {
            xMovement = -walkSpeed;
        }

        if (grapple.GetComponent<Rope>().GrappleEnded())
        {
            if (!toggleRopeEnd || IsGrounded())
            {
                ropeLength = grapple.GetComponent<Rope>().RopeLength();
                grappleLocation = grapple.GetComponent<Rope>().GrappleLocation();
                toggleRopeEnd = true;
            }

            if (Input.mouseScrollDelta.y != 0 && !IsGrounded())
            {
                ropeLength = ropeLength + (scrollScale * Input.mouseScrollDelta.y);
            }

            isGrappled = true;
            playerSpringJoint.enabled = true;
            playerSpringJoint.enableCollision = true;
            playerSpringJoint.connectedAnchor = grappleLocation;
            playerSpringJoint.distance = ropeLength;
        }


        Vector2 movement = new Vector2(xMovement, playerRigidBody.velocity.y);

        playerRigidBody.velocity = movement;

        //float playerRopeDif = Vector2.Distance(a: transform.position, b: grappleLocation);

        //if (ropeLength < playerRopeDif)
        //{

        //    playerRigidBody.MovePosition(new Vector3(Mathf.MoveTowards(transform.position.x, grappleLocation.x, acceleration * Time.deltaTime), 
        //        Mathf.MoveTowards(transform.position.y, grappleLocation.y, acceleration * Time.deltaTime), 0));
        //
        // }

    }

    private void Jump()
    {
        Vector2 movement = new Vector2(playerRigidBody.velocity.x, jumpForce);

        playerRigidBody.velocity = movement;
    }

    public bool IsGrounded()
    {
        Collider2D groundCheck = Physics2D.OverlapCircle(feet.position, 0.35f, groundLayers);

        if (groundCheck != null)
        {
            return true;
        }

        return false;
    }

}
