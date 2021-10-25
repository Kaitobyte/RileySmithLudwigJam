using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Riley Smith 2021
//I refrenced these tutorials when writing this code
//https://www.youtube.com/watch?v=tPtKNvifpj0
//https://www.youtube.com/watch?v=dnNCVcVS6uw&t=510s
//If you are looking for a base grappling hook you should check out the tutorials
//as this is a heavily modified version suited to this project specifically.
//The function of this code is to create a grappling hook that the player can fire to
//navigate the environment. I still have some adjustments to make so that it is more adaptible.

public class Rope : MonoBehaviour
{

    //this is where we are aiming
    [SerializeField] private Transform target;

    [SerializeField] private int resolution, waveCount, wobbleCount;
    [SerializeField] private float waveSize, animSpeed, angle, maxLength;
    [SerializeField] private LayerMask groundLayers;

    private LineRenderer line;
    private Coroutine grapple;
    private PlayerController player;
    private Animator playerAnimator;
    private float length;
    private bool endOfRope;
    private bool stoppedGrapple = true;
    private Vector3 ropeEnd;
    private bool emergencyStop;
    private bool grappleStart;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        endOfRope = false;
        emergencyStop = false;
        line = GetComponentInChildren<LineRenderer>();
        player = GetComponentInParent<PlayerController>();
        playerAnimator = player.GetComponentInParent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        //this if statement saved me and was found here
        //https://answers.unity.com/questions/692892/linerenderer-appear-behind-sprites.html
        if (gameObject.GetComponent<Renderer>().sortingLayerName != default && default != "")
        {
            //Debug.Log("Forcing sorting layer: "+sortingLayer);
            gameObject.GetComponent<Renderer>().sortingLayerName = default;
        }

            if ((grappleStart && timer >= .2) || (playerAnimator.GetBool("isGrapple") && grappleStart ))
        {
            setTarget();
            stoppedGrapple = false;
            grapple = StartCoroutine(routine: AnimateRope(target.position));
            grappleStart = false;
        }

        if (Input.GetMouseButtonDown(0) && stoppedGrapple && !player.IsGrounded() && !playerAnimator.GetBool("isFalling"))
        {
            Debug.Log("Starting Grapple");
            playerAnimator.SetBool("isGrapple", true);
            grappleStart = true;
            timer = 0;
        }

        if ((Input.GetMouseButtonDown(1) || length > maxLength || emergencyStop || player.transform.position.y >= ropeEnd.y) && !stoppedGrapple)
        {
            Debug.Log("Ending Grapple");

            line.enabled = false;
            endOfRope = false;
            stoppedGrapple = true;
            emergencyStop = false;
            StopCoroutine(grapple);
        }

        timer += Time.deltaTime;

    }

    //IEnumerator runs in parallel with the update function
    private IEnumerator AnimateRope(Vector3 targetPos)
    {
        line.enabled = true;
        line.positionCount = resolution;

        //calculates the angle we are shooting
        //might need to move this inside the loop if you want character to be able to move while shooting
        angle = LookAtAngle(target: targetPos - transform.position);

        //percent goes from 0 to 1 over time as our grapple moves towards its target
        //sets our points over time so the rope actully moves towards its target
        float percent = 0;
        while (percent <= 1f)
        {
            if (percent != 1f)
            {
                //calculates the angle we are shooting in case we move durring the shot
                angle = LookAtAngle(target: targetPos - transform.position);
            }

            percent += Time.deltaTime * animSpeed;
            SetPoints(targetPos, percent, angle);
            yield return null;

            //to-do:
            //add more code so that the rope stays attatched even when it finishes firing - done
            //also move the player and restrict player movement once rope is attacthed - being done in PlayerController
            //also add some grapple spam prevention - not done
        }

        endOfRope = true;

        while (!Input.GetMouseButtonDown(1))
        {

            //calculates the angle we are shooting in case we move durring the shot
            angle = LookAtAngle(target: targetPos - transform.position);

            percent += Time.deltaTime * animSpeed;
            AdjustPoints(targetPos, percent, angle);
            yield return null;

            yield return null;

        }

        //this line was in the tutorial, not sure if we need it
        //SetPoints(targetPos, 1, angle);
    }

    private void SetPoints(Vector3 targetPos, float percent, float angle)
    {
        //figures out where the end of our rope is by checking the percent of the way we have moved bewteen to the two points
        ropeEnd = Vector3.Lerp(a: transform.position, b: targetPos, percent);

        //gets the current length of the rope
        length = Vector2.Distance(a: transform.position, b: ropeEnd);

        //looping through all the points in the line renderer
        for (int currentPoint = 0; currentPoint < resolution; currentPoint++)
        {
            //gives us the position we are along the rope from 0 - 1
            float xPos = (float)currentPoint / resolution * length;
            float reversePercent = 1 - percent;

            //makes the rope move in a decreasing sin wave based on our wobbles and adjust the size of the waves
            float amplitude = Mathf.Sin(f: reversePercent * wobbleCount * Mathf.PI) * ((1f - (float)currentPoint / resolution) * waveSize); ;

            //figure out the yPos of the rope by following along the sin wave
            float yPos = Mathf.Sin(f: (float)waveCount * currentPoint / resolution * 2 * Mathf.PI * reversePercent) * amplitude;

            Vector2 pos = RotatePoint(new Vector2(x: xPos + transform.position.x, y: yPos + transform.position.y), pivot: transform.position,angle);
            line.SetPosition(currentPoint, pos);
        }

        //multiplying by a direction rotates the vector
        Vector2 RotatePoint(Vector2 point, Vector2 pivot, float angle1)
        {
            Vector2 dir = point - pivot;
            dir = Quaternion.Euler(x: 0, y: 0, z: angle1) * dir;
            point = dir + pivot;
            return point;
        }

    }

    //this is largely redundant code, but the mechanism is very fragile and with limited time I don't
    //think that its worth to spend the time condensing it down to one method with set points
    private void AdjustPoints(Vector3 targetPos, float percent, float angle)
    {
        //figures out where the end of our rope is by checking the percent of the way we have moved bewteen to the two points
        Vector3 ropeEnd = Vector3.Lerp(a: transform.position, b: targetPos, percent);

        //gets the current length of the rope
        length = Vector2.Distance(a: transform.position, b: ropeEnd);

        for (int currentPoint = 0; currentPoint < resolution; currentPoint++)
        {
            //gives us the position we are along the rope from 0 - 1
            float xPos = (float)currentPoint / resolution * length;
            float yPos = Mathf.Sin(f: 0);
            float reversePercent = 1 - percent;

            Vector2 pos = RotatePoint(new Vector2(x: xPos + transform.position.x, y: yPos + transform.position.y), pivot: transform.position, angle);
            line.SetPosition(currentPoint, pos);
        }

        //multiplying by a direction rotates the vector
        Vector2 RotatePoint(Vector2 point, Vector2 pivot, float angle1)
        {
            Vector2 dir = point - pivot;
            dir = Quaternion.Euler(x: 0, y: 0, z: angle1) * dir;
            point = dir + pivot;
            return point;
        }


    }

    //calculates the angle we are shooting the rope at
    private float LookAtAngle(Vector2 target)
    {
        return Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
    }

    //checks if the the grapple has fully reached the target
    public bool GrappleEnded()
    {
        return endOfRope;
    }

    //returns the length of the current rope
    public float RopeLength()
    {
        return length;
    }

    //returns the position that the end of the rope reached
    public Vector3 GrappleLocation()
    {
        return target.position;
    }

    public bool StoppedGrapple()
    {
        return stoppedGrapple;
    }


    //to-do allow the use to choose any target by clicking, - done
    //and detect if the rope has hit anything from where it starts
    //until it gets to full length and stops

    //try using LookAt also the rope does the side glitch on edges because you told it to.
    private void setTarget()
    {
        Vector2 distanceVector = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        if (Physics2D.Raycast(transform.position, distanceVector.normalized))
        {
            RaycastHit2D _hit = Physics2D.Raycast(transform.position, distanceVector.normalized);
            Debug.Log(_hit.transform.gameObject.layer);
            if (_hit.transform.gameObject.layer == 8)
            {
                target.position = _hit.point;

            }
            else
            {
                line.enabled = false;
                endOfRope = false;
                stoppedGrapple = true;
                emergencyStop = false;
                StopCoroutine(grapple);
            }


        }


    }

    private void grappleHitCheck()
    {
        if (Physics2D.OverlapCircle(ropeEnd, 0.15f, groundLayers))
        {
            target.position = ropeEnd;
        }
    }

    public void stopGrapple()
    {
        emergencyStop = true;
    }

}
