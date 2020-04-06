using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsInsightCheck : MonoBehaviour
{

    public bool inited, active;
    public int hFov = 110;

    public bool p1SeesP2Head, p1SeesP2LeftHand, p1SeesP2RightHand,
        p2SeesP1Head, p2SeesP1LeftHand, p2SeesP1RightHand;

    Transform p1Head,p1LeftHand,p1RightHand;
    Transform p2Head,p2LeftHand,p2RightHand;

    GameObject headed;

    // Start is called before the first frame update
    void Init()
    {
        List<GameObject> parts = new List<GameObject>();
        int count = 0;

        if (GameObject.Find("UserManager").GetComponent<UserManager>().usersPlaying.Count < 2) return;

        foreach (UserData player in GameObject.Find("UserManager").GetComponent<UserManager>().usersPlaying)
        {
            if (player.gameObject.tag == "Player")
            {
                parts.Add(player.head);
                parts.Add(player.leftHand);
                parts.Add(player.rightHand);
            }
        }
        count = parts.Count;

        p1Head = parts[0].gameObject.transform;
        p1LeftHand = parts[1].gameObject.transform;
        p1RightHand = parts[2].gameObject.transform;

        p2Head = parts[3].gameObject.transform;
        p2LeftHand = parts[4].gameObject.transform;
        p2RightHand = parts[5].gameObject.transform;

        if (count == 6) inited = true;

        //headed = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //headed.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {

        if (!inited) Init();
        else if(active)
            CheckIfInsight();
    }



    public void CheckIfInsight()
    {

        float dot;
        Vector3 horizontalHeading; // avoid up/down fov

        // P1
        horizontalHeading = new Vector3(p2Head.transform.position.x - p1Head.transform.position.x, 0, p2Head.transform.position.z - p1Head.transform.position.z);
        dot = Vector3.Dot((horizontalHeading).normalized, p1Head.gameObject.transform.forward.normalized);
        Debug.Log(dot);
        if (dot > Mathf.Cos(hFov / 2))
            p1SeesP2Head = true;
        else p1SeesP2Head = false;

        horizontalHeading = new Vector3(p2LeftHand.transform.position.x - p1Head.transform.position.x, 0, p2LeftHand.transform.position.z - p1Head.transform.position.z);
        dot = Vector3.Dot((horizontalHeading).normalized, p1Head.gameObject.transform.forward.normalized);
        if (dot > Mathf.Cos(hFov / 2))
            p1SeesP2LeftHand = true;
        else p1SeesP2LeftHand = false;

        horizontalHeading = new Vector3(p2RightHand.transform.position.x - p1Head.transform.position.x, 0, p2RightHand.transform.position.z - p1Head.transform.position.z);
        dot = Vector3.Dot((horizontalHeading).normalized, p1Head.gameObject.transform.forward.normalized);
        if (dot > Mathf.Cos(hFov / 2))
            p1SeesP2RightHand = true;
        else p1SeesP2RightHand = false;

        //headed.transform.position = p1Head.transform.position + new Vector3(p1Head.transform.forward.x, 0, p1Head.transform.forward.z);

        // P2
        horizontalHeading = new Vector3(p1Head.transform.position.x - p2Head.transform.position.x, 0, p1Head.transform.position.z - p2Head.transform.position.z);
        dot = Vector3.Dot((horizontalHeading).normalized, p2Head.gameObject.transform.forward.normalized);
        Debug.Log(dot);
        if (dot > Mathf.Cos(hFov / 2))
            p2SeesP1Head = true;
        else p2SeesP1Head = false;

        horizontalHeading = new Vector3(p1LeftHand.transform.position.x - p2Head.transform.position.x, 0, p1LeftHand.transform.position.z - p2Head.transform.position.z);
        dot = Vector3.Dot((horizontalHeading).normalized, p2Head.gameObject.transform.forward.normalized);
        if (dot > Mathf.Cos(hFov / 2))
            p2SeesP1LeftHand = true;
        else p2SeesP1LeftHand = false;

        horizontalHeading = new Vector3(p1RightHand.transform.position.x - p2Head.transform.position.x, 0, p1RightHand.transform.position.z - p2Head.transform.position.z);
        dot = Vector3.Dot((horizontalHeading).normalized, p2Head.gameObject.transform.forward.normalized);
        if (dot > Mathf.Cos(hFov / 2))
            p2SeesP1RightHand = true;
        else p2SeesP1RightHand = false;

        /*        angle = Vector3.Angle(p1Head.gameObject.transform.forward, p2Head.gameObject.transform.position);
                if (Mathf.Abs(angle) > hFov / 2)
                    p2seesP1Head = true;
                else p2seesP1Head = false;
        */

        if (p2SeesP1Head) p1Head.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.red;
        else p1Head.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.white;
        if (p2SeesP1LeftHand) p1LeftHand.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.red;
        else p1LeftHand.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.white;
        if (p2SeesP1RightHand) p1RightHand.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.red;
        else p1RightHand.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.white;

        if (p1SeesP2Head) p2Head.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.red;
        else p2Head.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.white;
        if (p1SeesP2LeftHand) p2LeftHand.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.red;
        else p2LeftHand.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.white;
        if (p1SeesP2RightHand) p2RightHand.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.red;
        else p2RightHand.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.white;


    }

    public void Switch()
    {
        active = !active;

        if (!active)
        {            
            p1Head.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.white;
            p1LeftHand.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.white;
            p1RightHand.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.white;
            p2Head.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.white;
            p2LeftHand.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.white;
            p2RightHand.gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.white;
        }
    }
}
