using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingParticleScript2 : MonoBehaviour
{

    Rigidbody rg;
    public GameObject target, particleSystemParent, center;
    public GestureAnalyser gestureAnalyser;
    public SpringJoint springJoint;
    ParticleSystem particleSystem;
    public float distanceFromTarget = 1;
    public float systemSpeed = 1;

    public float minDistance = 1, maxDistance = 4;

    public float particleIntensity;
    float oldSpringJoint, springJointTarget;
    float oldParticleIntensity, particleIntensityTarget;
    float lastActualisationtime;

    public Color initialParticleColor ;

    public bool inited;
    bool playerFound;

    //Vector3 target;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("Init",0f);
    }

    void Init()
    {

        FindPlayer();

        if (!playerFound)
        {
            center = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            center.transform.localScale = Vector3.one * 0.1f;
        }

        gestureAnalyser = FindObjectOfType<GestureAnalyser>();

        //initialParticleIntensity =
        particleSystem = particleSystemParent.transform.GetChild(0).GetComponent<ParticleSystem>();
        initialParticleColor = particleSystem.GetComponent<Renderer>().material.color;

        //DrawGizmos();
        Invoke("PickNewTarget", 0.1f);
        InvokeRepeating("ActualizeParticleBehaviourTarget", 0f, 1f);

        inited = true;
    }


    void ActualizeParticleBehaviourTarget()
    {
        oldParticleIntensity = particleIntensity;
        oldSpringJoint = springJoint.damper;

        particleIntensityTarget = gestureAnalyser.leftHandHeightRatio;

        float distFromHand = Vector3.Distance(gestureAnalyser.p1.head.transform.position, transform.position);

        springJointTarget = 0.1f * gestureAnalyser.p1LHSpeed / distFromHand * 100 ;
        lastActualisationtime = Time.time;
    }


    void ActualizeParticleSystem()
    {
        float timeRatio = (Time.time - lastActualisationtime);
        particleIntensity = Mathf.Lerp(oldParticleIntensity, particleIntensityTarget, timeRatio);
        springJoint.damper = Mathf.Lerp(oldSpringJoint, springJointTarget, timeRatio);

        particleSystem.GetComponent<Renderer>().material.SetColor("_Color", particleIntensity * initialParticleColor);

    }


    // Update is called once per frame
    void Update()
    {
        //transform.position = Vector3.MoveTowards(transform.position, target, 1);
        if (!playerFound) FindPlayer();

        if (Vector3.Distance(target.transform.position, particleSystemParent.transform.position) < distanceFromTarget) PickNewTarget();

        ActualizeParticleSystem();
    }


    void PickNewTarget()
    {

        float theta = Random.Range(0, Mathf.PI / 2 ); 
        float betha = Random.Range(0, Mathf.PI * 2);

        //float distance = Random.Range(minDistance,maxDistance);
        float distance = maxDistance;
        //springJoint.spring = 0.1f * systemSpeed;

        //transform.RotateAround(target.transform.position, Vector3.up, Time.deltaTime * ySpeed);
        //transform.RotateAround(target.transform.position, Vector3.forward, Time.deltaTime * zSpeed);

        float x, y, z;

        x = distance * Mathf.Sin(theta) * Mathf.Cos(betha);
        y = distance * Mathf.Cos(theta);
        z = distance * Mathf.Sin(theta) * Mathf.Sin(betha); 

        target.transform.position = center.transform.position + new Vector3(x,y,z);
    }



   

    


    // create a dome that correspond to the target spawn zone
    void DrawGizmos()
    {
        float ray = maxDistance;
        GameObject parent = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        parent.transform.position = Vector3.zero;
        parent.GetComponent<MeshRenderer>().enabled = false;
        for(int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                float theta = Mathf.Lerp(0, Mathf.PI / 2, (float)i / 20f);
                float betha = Mathf.Lerp(0, Mathf.PI * 2, (float)j / 20f); 

                float x, y, z;

                x = ray* Mathf.Sin(theta) * Mathf.Cos(betha) ;
                y = ray * Mathf.Cos(theta); 
                z = ray * Mathf.Sin(theta) * Mathf.Sin(betha);

                GameObject gizmo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                gizmo.transform.localScale *= 0.05f;
                gizmo.transform.position = center.transform.position + new Vector3(x, y, z);
                gizmo.transform.parent = parent.transform;
            }
        }

        //transform.RotateAround(target.transform.position, Vector3.up, Time.deltaTime * ySpeed);
        //transform.RotateAround(target.transform.position, Vector3.forward, Time.deltaTime * zSpeed);

        
    }


    private void OnApplicationQuit()
    {
        particleSystem.GetComponent<Renderer>().material.SetColor("_Color", initialParticleColor);
    }



    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (center != null) Destroy(center.gameObject);
            center = player.GetComponent<UserData>().head;
            playerFound = true;
        }
        else
        {
            playerFound = false;
        }
    }


}
