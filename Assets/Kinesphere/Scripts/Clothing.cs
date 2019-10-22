using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clothing : MonoBehaviour
{
    public GameObject line;
    public float extrapolate = 1.0f;

    List<GameObject> parts = new List<GameObject>();
    List<GameObject> lines = new List<GameObject>();

    bool inited = false;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 50; i++)
        {
            GameObject gc = Instantiate(line);
            gc.transform.SetParent(transform);
            lines.Add(gc);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(inited == false)
        {
            if (GameObject.Find("UserManager").GetComponent<UserManager>().usersPlaying.Count < 2) return;
            UserData Player1 = GameObject.Find("UserManager").GetComponent<UserManager>().usersPlaying[0];
            parts.Add(Player1.leftHand);
            parts.Add(Player1.rightHand);
            UserData Player2 = GameObject.Find("UserManager").GetComponent<UserManager>().usersPlaying[GameObject.Find("UserManager").GetComponent<UserManager>().usersPlaying.Count-1];
            parts.Add(Player2.leftHand);
            parts.Add(Player2.rightHand);
            inited = true;
        }

        foreach (var part in parts)
        {
            if (part == null)
            {
                parts.Clear();
                inited = false;
                return;
            }
        }

        Vector3 centroid1 = Vector3.Lerp(parts[0].transform.position, parts[1].transform.position, 0.5f);
        Vector3 centroid2 = Vector3.Lerp(parts[2].transform.position, parts[3].transform.position, 0.5f);
        float length1 = Vector3.Distance(parts[0].transform.position, parts[1].transform.position);
        float length2 = Vector3.Distance(parts[2].transform.position, parts[3].transform.position);
        for (int i = 0; i < 50; i++)
        {
            GameObject gc = lines[i];
            float lerp = ((float)i / 50) * (1 + extrapolate) - extrapolate * 0.5f;
            gc.transform.position = centroid1 * (1 - lerp) + centroid2 * lerp;
            Vector3 leftLerp = parts[0].transform.position * (1-lerp) + parts[3].transform.position * lerp;
            Vector3 rightLerp = parts[1].transform.position * (1 - lerp) + parts[2].transform.position * lerp;
            gc.transform.rotation = Quaternion.FromToRotation(Vector3.up, leftLerp - rightLerp);
            float s = (length1 * (1 - lerp) + length2 * lerp) * 0.5f;
            gc.transform.localScale = new Vector3(0.01f, s, 0.01f);
        }
    }
}
