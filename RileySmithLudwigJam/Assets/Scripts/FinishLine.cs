using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    [SerializeField] private Animator finishLineAnimator;
    [SerializeField] private GameObject player;

    private PolygonCollider2D myCollider;

    // Start is called before the first frame update
    void Start()
    {
        myCollider = this.GetComponent<PolygonCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (myCollider.IsTouching(player.GetComponent<BoxCollider2D>()))
        {
            finishLineAnimator.SetBool("isFinished", true);
        }
    }

    public bool isFinished()
    {
        return finishLineAnimator.GetBool("isFinished");
    }

}
