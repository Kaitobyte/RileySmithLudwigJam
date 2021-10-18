using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Riley Smith 2021
//I refrenced this tutorial when writing this code
//https://www.youtube.com/watch?v=tPtKNvifpj0
//I highly recomend you watch it rather than just copying the code
//It will help you understand what you are writing!
//The function of this code is to create a grappling hook that the player can fire to
//navigate the environment. I still have some adjustments to make so that it is more adaptible.

public class Rope : MonoBehaviour
{

    //this is where we are aiming
    [SerializeField] private Transform target;

    [SerializeField] private int resolution, waveCount, wobbleCount;
    [SerializeField] private float waveSize, animSpeed, angle;

    private LineRenderer line;

    // Start is called before the first frame update
    void Start()
    {

        line = GetComponentInChildren<LineRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(routine: AnimateRope(target.position));
        }
    }

    //IEnumerator runs in parallel with the update function
    private IEnumerator AnimateRope(Vector3 targetPos)
    {
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
            //add more code so that the rope stays attatched even when it finishes firing
            //also move the player and restrict player movement once rope is attacthed
            //also add some grapple spam prevention
        }

        //this line was in the tutorial, not sure if we need it
        //SetPoints(targetPos, 1, angle);
    }

    private void SetPoints(Vector3 targetPos,  float percent, float angle)
    {
        //figures out where the end of our rope is by checking the percent of the way we have moved bewteen to the two points
        Vector3 ropeEnd = Vector3.Lerp(a: transform.position, b: targetPos, percent);

        //gets the current length of the rope
        float length = Vector2.Distance(a: transform.position, b: ropeEnd);

        //looping through all the points in the line renderer
        for (int currentPoint = 0; currentPoint < resolution; currentPoint++)
        {
            //gives us the position we are along the rope from 0 - 1
            float xPos = (float) currentPoint / resolution * length;
            float reversePercent = 1 - percent;

            //makes the rope move in a decreasing sin wave based on our wobbles and adjust the size of the waves
            float amplitude = Mathf.Sin(f: reversePercent * wobbleCount * Mathf.PI) * ((1f - (float)currentPoint / resolution) * waveSize); ;

            //figure out the yPos of the rope by following along the sin wave
            float yPos = Mathf.Sin(f: (float)waveCount * currentPoint / resolution * 2 * Mathf.PI * reversePercent) * amplitude;

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

}
