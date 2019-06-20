using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particleRelations : MonoBehaviour
{

    public float acceleration = 0f;
    private float tresholdMove = .01f;
    public float moveRatio = 0f;
    public float cancelRatio = 0f;
    private Vector3 lastPosition;
    ParticleSystem myparticles;
    public GameObject _manager;
    float dist;


    void Start()
    {
        lastPosition = transform.position;
        myparticles = GetComponentInChildren<ParticleSystem>();
        _manager = GameObject.FindGameObjectWithTag("UserManager");


    }

    void Update()
    {
        acceleration = (transform.position - lastPosition).magnitude;
        lastPosition = transform.position;
        moveRatio = Mathf.InverseLerp(0f, tresholdMove, acceleration);
        if (moveRatio > 0.1f) myparticles.startLifetime = 3f;
        else myparticles.startLifetime = 0f;

       // Debug.Log(acceleration);


       // dist = _manager.GetComponent<UserManager>().dist;
    }
}
