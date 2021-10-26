using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float walkSpeed;
    [SerializeField] private float grappleSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float friction;
    [SerializeField] private Rigidbody2D playerRigidBody;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Transform feet;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float airSpeed;
    [SerializeField] private SpringJoint2D playerSpringJoint;
    [SerializeField] private float scrollScale;
    [SerializeField] private Animator playerAniamtor;
    [SerializeField] private SpriteRenderer playerSprite;

    private float userInput;
    private bool isGrounded;
    private bool isGrappled;
    private bool toggleRopeEnd = false;

    private float ropeLength = float.MaxValue;
    private Vector3 grappleLocation, playerVelocity;
    private LineRenderer grapple;
    private float timer;
    private bool airborne;

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


        timer += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        float xMovement = 0f;

        if (IsGrounded())
        {
            playerAniamtor.SetBool("isJumping", false);
            playerAniamtor.SetBool("isGrapple", false);

            airborne = false;
            if (playerRigidBody.velocity.x > friction)
            {
                xMovement = playerRigidBody.velocity.x + (userInput * acceleration) - friction;
                playerAniamtor.SetBool("isWalking", true);
                playerSprite.flipX = false;
            }
            else if (playerRigidBody.velocity.x < -friction)
            {
                xMovement = playerRigidBody.velocity.x + (userInput * acceleration) + friction;
                playerAniamtor.SetBool("isWalking", true);
                playerSprite.flipX = true;
            }
            else
            {
                xMovement = userInput * acceleration;
                playerAniamtor.SetBool("isWalking", false);
            }

        }
        else if (isGrappled)
        {
            airborne = false;

            if (playerRigidBody.velocity.x > airSpeed)
            {
                xMovement = playerRigidBody.velocity.x + (userInput * acceleration / 2) - airSpeed;
                playerSprite.flipX = false;
            }
            else if (playerRigidBody.velocity.x < -airSpeed)
            {
                xMovement = playerRigidBody.velocity.x + (userInput * acceleration / 2) + airSpeed;
                playerSprite.flipX = true;
            }
            else
            {
                xMovement = userInput * acceleration / 10;
            }


        }
        else
        {
            if (airborne)
            {
                if (timer >= 1.5f)
                {
                    playerAniamtor.SetBool("isFalling", true);
                }
            }
            else
            {
                timer = 0;
                airborne = true;
            }
            playerAniamtor.SetBool("isJumping", true);
            if (playerRigidBody.velocity.x > airSpeed * 2)
            {
                xMovement = playerRigidBody.velocity.x - airSpeed;
            }
            else if (playerRigidBody.velocity.x < airSpeed * 2)
            {
                xMovement = playerRigidBody.velocity.x + airSpeed;
            }
        }

        if (isGrappled)
        {
            if (xMovement > grappleSpeed)
            {
                xMovement = grappleSpeed;
            }
            else if (xMovement < -grappleSpeed)
            {
                xMovement = -grappleSpeed;
            }
        }
        else
        {
            if (xMovement > walkSpeed)
            {
                xMovement = walkSpeed;
            }
            else if (xMovement < -walkSpeed)
            {
                xMovement = -walkSpeed;
            }
        }


        if (grapple.GetComponent<Rope>().GrappleEnded())
        {
            if (!toggleRopeEnd)
            {
                ropeLength = grapple.GetComponent<Rope>().RopeLength();
                grappleLocation = grapple.GetComponent<Rope>().GrappleLocation();
                toggleRopeEnd = true;
            }

            if (IsGrounded())
            {
                grapple.GetComponent<Rope>().stopGrapple();
            }

            isGrappled = true;
            playerSpringJoint.enabled = true;
            playerSpringJoint.enableCollision = true;
            playerSpringJoint.connectedAnchor = grappleLocation;
            playerSpringJoint.distance = ropeLength;
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
        Collider2D groundCheck = Physics2D.OverlapCircle(feet.position, 0.15f, groundLayers);

        if (groundCheck != null)
        {
            return true;
        }

        return false;
    }

}
