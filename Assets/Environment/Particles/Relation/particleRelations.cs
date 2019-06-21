using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particleRelations : MonoBehaviour
{

    public float acceleration = 0f;
    private float tresholdMove = .01f;
    public float moveRatio = 0f;
    private Vector3 lastPosition;
    ParticleSystem myparticles;
    //public GameObject _manager;
    //string condition;
    //float dist;
    //bool moduleEnabled;

    void Start()
    {
        lastPosition = transform.position;
        myparticles = GetComponentInChildren<ParticleSystem>();
        //_manager = GameObject.FindGameObjectWithTag("UserManager");
        //moduleEnabled = false;

    }

    void Update()
    {


        acceleration = (transform.position - lastPosition).magnitude;
        lastPosition = transform.position;
        moveRatio = Mathf.InverseLerp(0f, tresholdMove, acceleration);
        if (moveRatio > 0.1f) myparticles.startLifetime = 3f;
        else myparticles.startLifetime = 0f;


        //Debug.Log(Mathf.Clamp(acceleration, 0f, 3f));

        // all this has been transfered to user manager
        /*
        if (_manager != null)
        {
            condition = _manager.GetComponent<UserManager>().trailsCondition;
            if (condition == "relation")
            {
                dist = _manager.GetComponent<UserManager>().dist;
                if (dist > 1f)
                {
                    //moduleEnabled = myparticles.inheritVelocity.enabled;
                    var veloModule = myparticles.inheritVelocity;
                    veloModule.enabled = true;
                }
                else
                {
                    var veloModule = myparticles.inheritVelocity;
                    veloModule.enabled = false;
                    float lfTime = dist.Remap(1, 0, 3, 20);
                    myparticles.startLifetime = lfTime;

                }


            }
        }
        */
        //all this has been transfered to user manager



    }
}
