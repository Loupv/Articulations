using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingParticleScript2 : MonoBehaviour
{

    Rigidbody rg;
    public GameObject target, particleSystemParent, center;
    public MovementAnalyser movementAnalyser;
    public SpringJoint springJoint;
    ParticleSystem particleSystem;
    public float distanceFromTarget = 1;
    public float systemSpeed = 1;

    public float minDistance = 1, maxDistance = 4;

    public float particleIntensity, initialParticleIntensity;
    public Color initialParticleColor;

    //Vector3 target;

    // Start is called before the first frame update
    void Start()
    {
        if (center == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            movementAnalyser = FindObjectOfType<MovementAnalyser>();

            if (player != null) center = player.GetComponent<UserData>().head;
            else
            {
                center = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                center.transform.localScale = Vector3.one * 0.1f;
            }
        }

        //initialParticleIntensity =
        particleSystem = particleSystemParent.transform.GetChild(0).GetComponent<ParticleSystem>();
        initialParticleColor = particleSystem.GetComponent<Renderer>().material.color;

        DrawGizmos();
        Invoke("PickNewTarget",0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = Vector3.MoveTowards(transform.position, target, 1);
      
        if (Vector3.Distance(target.transform.position, particleSystemParent.transform.position) < distanceFromTarget) PickNewTarget();

        ActualizeParticleSystem(movementAnalyser.leftHandHeightRatio);
    }


    void PickNewTarget()
    {

        float theta = Random.Range(0, Mathf.PI / 2 * 8/10); 
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



    void ActualizeParticleSystem(float intensity)
    {
        particleSystem.GetComponent<Renderer>().material.SetColor("_Color", intensity*initialParticleColor);
    }




    void DrawGizmos()
    {
        float ray = maxDistance;
        GameObject parent = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        parent.GetComponent<MeshRenderer>().enabled = false;
        for(int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                float theta = Mathf.Lerp(0, Mathf.PI / 2 * 8 / 10, (float)i / 20f);
                float betha = Mathf.Lerp(0, Mathf.PI * 2, (float)j / 20f); 

                float x, y, z;

                x = ray* Mathf.Sin(theta) * Mathf.Cos(betha) ;
                y = ray * Mathf.Cos(theta); 
                z = ray * Mathf.Sin(theta) * Mathf.Sin(betha);

                GameObject gizmo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                gizmo.transform.localScale *= 0.05f;
                gizmo.transform.parent = parent.transform;
                gizmo.transform.position = center.transform.position + new Vector3(x, y, z);
            }
        }

        //transform.RotateAround(target.transform.position, Vector3.up, Time.deltaTime * ySpeed);
        //transform.RotateAround(target.transform.position, Vector3.forward, Time.deltaTime * zSpeed);

        
    }


    private void OnApplicationQuit()
    {
        particleSystem.GetComponent<Renderer>().material.SetColor("_Color", initialParticleColor);
    }

}
