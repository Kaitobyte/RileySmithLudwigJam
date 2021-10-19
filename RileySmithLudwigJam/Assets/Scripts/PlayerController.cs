using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float accel;
    [SerializeField] private float friction;
    [SerializeField] private float gravity;

    private float ropeLength, xSpeed, ySpeed, xDirection, yDirection;
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


        //to-do change movement so that it goes based on player velocity
        //test to see if player movement is restricted by the length of the rope
        //add gravity to see if player hangs from rope, add/restrict movement in the air


        // -1 for left / down
        // +1 for right / up
        xDirection = Input.GetAxis("Horizontal");
        yDirection = Input.GetAxis("Vertical");

        //to-do account for movement in both directions. This only applies friction when moving right/up
        //also the player just falls/launches left right now so fix that
        xSpeed += (xDirection * accel) - friction;
        ySpeed += (yDirection * accel) - friction;
        
        //to-do add sneak, walk, run, and mid-grapple speeds
        if (xSpeed > walkSpeed)
        {
            xSpeed = walkSpeed;
        }

        playerVelocity = new Vector3(xSpeed, ySpeed, 0.0f);



        transform.position += playerVelocity * moveSpeed;
    }
}
