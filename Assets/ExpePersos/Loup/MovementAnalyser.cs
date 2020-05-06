using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAnalyser : MonoBehaviour
{

    UserManager userManager;

    UserData p1, p2;
    Rigidbody rbh1, rblh1, rbrh1, rbh2, rblh2, rbrh2;

    float p1hVelocity, p1lhVelocity, p1rhVelocity;

    public float leftHandHeightRatio, rightHandHeightRatio;

    public float p1handsMaxHeight, p1handsMinHeight;


    public bool init;


    public void Init(UserManager um)
    {
        userManager = um;
        p1 = um.usersPlaying[1];
        p2 = um.usersPlaying[2];

        rbh1 = p1.head.GetComponent<Rigidbody>();
        rblh1 = p1.leftHand.GetComponent<Rigidbody>();
        rbrh1 = p1.rightHand.GetComponent<Rigidbody>();
        rbh2 = p2.head.GetComponent<Rigidbody>();
        rblh2 = p2.leftHand.GetComponent<Rigidbody>();
        rbrh2 = p2.rightHand.GetComponent<Rigidbody>();

        p1handsMaxHeight = 0;
        p1handsMinHeight = 10;

        init = true;
    }


    // Update is called once per frame
    void Update()
    {
        if (init)
        {


            if (p1.leftHand.transform.position.y > p1handsMaxHeight || p1.rightHand.transform.position.y > p1handsMaxHeight)
                {
                    p1handsMaxHeight = Mathf.Max(p1.leftHand.transform.position.y, p1.rightHand.transform.position.y);
                }

            if (p1.leftHand.transform.position.y < p1handsMinHeight || p1.rightHand.transform.position.y < p1handsMinHeight)
                {
                    if(Mathf.Min(p1.leftHand.transform.position.y, p1.rightHand.transform.position.y) != 0)
                        p1handsMinHeight = Mathf.Min(p1.leftHand.transform.position.y, p1.rightHand.transform.position.y);
                }


            leftHandHeightRatio = (p1.leftHand.transform.position.y - p1handsMinHeight) / (p1handsMaxHeight-p1handsMinHeight);
            rightHandHeightRatio = (p1.rightHand.transform.position.y - p1handsMinHeight) / (p1handsMaxHeight - p1handsMinHeight);
            Debug.Log(leftHandHeightRatio);

        }

        if (Input.GetKey(KeyCode.Escape))
        {
            init = false;
        }
    }

}
