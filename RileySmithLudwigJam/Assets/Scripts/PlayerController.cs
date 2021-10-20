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

    private float userInput;
    private bool isGrounded;
    private bool isGrappled;

    private float ropeLength;
    private Vector3 grappleLocation, playerVelocity;
    private LineRenderer grapple;

    // Start is called before the first frame update
    void Start()
    {
        grapple = GetComponentInChildren<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

        //this doesn't work because rope length is being continually updated - fix
        if (grapple.GetComponent<Rope>().grappleEnded())
        {
            ropeLength = grapple.GetComponent<Rope>().ropeLength();
            grappleLocation = grapple.GetComponent<Rope>().grappleLocation();

            if (ropeLength < Mathf.Abs(transform.position.x - grappleLocation.x))
            {
                playerVelocity.x = -playerVelocity.x;
            }

            if (ropeLength < Mathf.Abs(transform.position.y - grappleLocation.y))
            {
                playerVelocity.y = -playerVelocity.y;
            }

        }

        userInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            Jump();
        }

    }

    private void FixedUpdate()
    {
        float xMovement = 0f;

        if (IsGrounded())
        {
            if (playerRigidBody.velocity.x > 0)
            {
                xMovement = playerRigidBody.velocity.x + (userInput * acceleration) - friction;
            }
            else if (playerRigidBody.velocity.x < 0)
            {
                xMovement = playerRigidBody.velocity.x + (userInput * acceleration) + friction;
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

        Vector2 movement = new Vector2(xMovement, playerRigidBody.velocity.y);

        playerRigidBody.velocity = movement;
    }

    private void Jump()
    {
        Vector2 movement = new Vector2(playerRigidBody.velocity.x, jumpForce);

        playerRigidBody.velocity = movement;
    }

    public bool IsGrounded()
    {
        Collider2D groundCheck = Physics2D.OverlapCircle(feet.position, 0.4f, groundLayers);

        if (groundCheck != null)
        {
            return true;
        }

        return false;
    }

}
