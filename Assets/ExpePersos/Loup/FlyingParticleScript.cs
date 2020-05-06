using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingParticleScript : MonoBehaviour
{

    public bool init;
    public float ySpeed, zSpeed;

    public float expressivity, openness;

    public GameObject target;

    public float minDistance, distance, maxDistance;

    public float theta;
    public float minTheta, maxTheta ;
    public float betha;
    public float minBetha, maxBetha ;

    public float newTheta, newBetha, newDistance;
    float oldTheta;
    float oldBetha;
    float oldDistance;
    float oldTime;
    float timeUntilNextTarget;


    // Start is called before the first frame update
    void Start()
    {
        Invoke("AdjustParameters", 0f);
        init = true;

        minTheta = 0;
        maxTheta = Mathf.PI * 2;
        minBetha = Mathf.PI * 2;
        maxBetha = Mathf.PI * 2;
    }

    // Update is called once per frame
    void Update()
    {

        //update parameters manually

        
        if (init)
        {

            // faire un lerp qui dure jusque timeUntilNextTarget

            theta = Mathf.LerpAngle(oldTheta, newTheta, (Time.time - oldTime) / timeUntilNextTarget);
            betha = Mathf.LerpAngle(oldBetha, newBetha, (Time.time - oldTime) / timeUntilNextTarget);

            distance = Mathf.Lerp(oldDistance, newDistance, (Time.time - oldTime) / timeUntilNextTarget);


            //transform.RotateAround(target.transform.position, Vector3.up, Time.deltaTime * ySpeed);
            //transform.RotateAround(target.transform.position, Vector3.forward, Time.deltaTime * zSpeed);

            float x, y, z;

            x = distance * Mathf.Sin(theta) * Mathf.Cos(betha);
            y = distance * Mathf.Sin(theta) * Mathf.Sin(betha);
            z = distance * Mathf.Cos(theta);


            transform.position = new Vector3(x, y, z);
            //transform.position = target.transform.position + new Vector3(x, y, z);

            //if (target == null) init = false;
        }
        else
        {
            target = GameObject.FindGameObjectWithTag("Player").GetComponent<UserData>().head;
            transform.position = target.transform.position + new Vector3(1, 0, 0);
            init = true;
        }
    }


    void AdjustParameters()
    {
        expressivity = Mathf.Abs(Mathf.Cos(Time.deltaTime * Mathf.PI));
        openness = Mathf.Abs(Mathf.Cos(Time.deltaTime * Mathf.PI));

        oldTheta = theta;
        oldBetha = betha;
        oldDistance = distance;

        newTheta = Random.Range(minTheta, maxTheta);

        newBetha = Random.Range(minBetha, maxBetha);
        newDistance = Random.Range(minDistance, maxDistance);

        oldTime = Time.time;
        timeUntilNextTarget = Random.Range(3f, 6f);
        Invoke("AdjustParameters", timeUntilNextTarget);
    }
}
