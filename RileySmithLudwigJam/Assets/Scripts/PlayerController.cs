using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private SpriteRenderer playerSprite;

    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip grappleSound;
    [SerializeField] private AudioClip hitTheGroundSound;
    [SerializeField] private AudioClip fallingSound;

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private float volume;

    private float userInput;
    private bool isGrounded;
    private bool isGrappled;
    private bool toggleRopeEnd = false;

    private float ropeLength = float.MaxValue;
    private Vector3 grappleLocation, playerVelocity;
    private LineRenderer grapple;
    private float fallingTimer;
    private float gameTime;
    private bool airborne;
    private bool emergencyFix = false;
    private Vector2 originalPos = new Vector2(-6.058f, -1.195f);
    private Vector2 savedPos;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("hasSaved"))
        {
            savedPos = new Vector2(PlayerPrefs.GetFloat("savedX"), PlayerPrefs.GetFloat("savedY"));
            gameTime = PlayerPrefs.GetFloat("gameTime");
            this.transform.position = savedPos;
        } else
        {
            this.transform.position = originalPos;
        }
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
            playerAnimator.SetBool("isStanding", true);
            audioSource.PlayOneShot(jumpSound, volume);
            Jump();
        }

        PlayerPrefs.SetFloat("savedX", this.transform.position.x);
        PlayerPrefs.SetFloat("savedY", this.transform.position.y);
        PlayerPrefs.SetInt("hasSaved", 1);
        PlayerPrefs.SetFloat("gameTime", gameTime);

        fallingTimer += Time.deltaTime;
        gameTime += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        float xMovement = 0f;

        if (IsGrounded())
        {
            playerAnimator.SetBool("isJumping", false);
            playerAnimator.SetBool("isGrapple", false);
            playerAnimator.SetBool("isFalling", false);

            if (airborne && fallingTimer > 1.5)
            {
                audioSource.PlayOneShot(hitTheGroundSound, volume);
            }

            airborne = false;
            if (playerRigidBody.velocity.x > friction)
            {
                if (!playerAnimator.GetBool("isStanding"))
                {
                    playerAnimator.SetBool("isStanding", true);
                    playerRigidBody.transform.position.Set(playerRigidBody.transform.position.x, playerRigidBody.transform.position.y + 5,
                        playerRigidBody.transform.position.z);
                }
                xMovement = playerRigidBody.velocity.x + (userInput * acceleration) - friction;
                playerAnimator.SetBool("isWalking", true);
                playerSprite.flipX = false;
                if (!audioSource.isPlaying && Time.time % .3 < .1)
                {
                    audioSource.PlayOneShot(footstepSound, volume);
                }
            }
            else if (playerRigidBody.velocity.x < -friction)
            {
                if (!playerAnimator.GetBool("isStanding"))
                {
                    playerAnimator.SetBool("isStanding", true);
                    playerRigidBody.transform.position.Set(playerRigidBody.transform.position.x, playerRigidBody.transform.position.y + 5,
    playerRigidBody.transform.position.z);
                }
                xMovement = playerRigidBody.velocity.x + (userInput * acceleration) + friction;
                playerAnimator.SetBool("isWalking", true);
                playerSprite.flipX = true;
                if (!audioSource.isPlaying && Time.time % .3 < .1)
                {
                    audioSource.PlayOneShot(footstepSound, volume);
                }
            }
            else
            {
                xMovement = userInput * acceleration;
                playerAnimator.SetBool("isWalking", false);
            }
            emergencyFix = false;

        }
        else if (isGrappled)
        {
            playerAnimator.SetBool("isFalling", false);
            playerAnimator.SetBool("isJumping", false);
            playerAnimator.SetBool("isStanding", true);
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
                xMovement = userInput * acceleration;
            }
            emergencyFix = false;

        }
        else
        {
            if (airborne)
            {
                if (fallingTimer >= 1.5f)
                {
                    audioSource.PlayOneShot(fallingSound, volume);
                    playerAnimator.SetBool("isFalling", true);
                    playerAnimator.SetBool("isStanding", false);
                }
            }
            else
            {
                fallingTimer = 0;
                airborne = true;
            }
            playerAnimator.SetBool("isJumping", true);
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
                audioSource.PlayOneShot(grappleSound, volume);
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

        if(playerAnimator.GetBool("isFalling") && (playerRigidBody.velocity.y < .1 && playerRigidBody.velocity.y > -.1) && !emergencyFix)
        {
            xMovement = 4;
            playerRigidBody.velocity = new Vector2(xMovement, jumpForce / 2);
            emergencyFix = true;
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

    public float getGameTime()
    {
        return gameTime;
    }

}
