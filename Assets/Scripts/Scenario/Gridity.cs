using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// stupid fact:
// always set Head, LeftHand, RightHand scale to 0.1, 0.1, 0.1

public class Gridity : MonoBehaviour
{
    public GameObject line;

    public float GridRes = 2;

    List<GameObject> parts = new List<GameObject>();
    List<GameObject> lineMasters = new List<GameObject>();
    List<Color> palette = new List<Color>();

    bool inited = false;
    // Start is called before the first frame update
    void Start()
    {
        Color c;
        ColorUtility.TryParseHtmlString("#91A6FF", out c);
        palette.Add(c);
        ColorUtility.TryParseHtmlString("#FF88DC", out c);
        palette.Add(c);
        ColorUtility.TryParseHtmlString("#FAFF7F", out c);
        palette.Add(c);
        ColorUtility.TryParseHtmlString("#FFFFFF", out c);
        palette.Add(c);
        ColorUtility.TryParseHtmlString("#FF5154", out c);
        palette.Add(c);

        for (int n = 0; n < 6; n++)
        {
            GameObject g = new GameObject("Grid");
            lineMasters.Add(g);
            for (int i = 0; i < 8; i++)
            {
                GameObject gc = Instantiate(line);
                gc.transform.SetParent(g.transform);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        int count = 0;
        if (inited == false)
        {
            if (GameObject.Find("UserManager").GetComponent<UserManager>().usersPlaying.Count < 2) return;
            UserData Player1 = GameObject.Find("UserManager").GetComponent<UserManager>().usersPlaying[0];
            parts.Add(Player1.head);
            parts.Add(Player1.leftHand);
            parts.Add(Player1.rightHand);
            UserData Player2 = GameObject.Find("UserManager").GetComponent<UserManager>().usersPlaying[GameObject.Find("UserManager").GetComponent<UserManager>().usersPlaying.Count-1];
            parts.Add(Player2.head);
            parts.Add(Player2.leftHand);
            parts.Add(Player2.rightHand);

            count = 0;
            foreach (var part in parts)
            {
                GameObject g = lineMasters[count];
                g.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                count++;
            }
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

        count = 0;
        foreach (var part in parts)
        {
            if(count == 0)
            {
                count++;
                continue;
            }
            for (int i = 0; i < 8; i++)
            {
                GameObject gc = lineMasters[count].transform.GetChild(i).gameObject;

                Vector3 pos = part.transform.position;
                float ix = i % 2;
                float iy = (i / 2) % 2;
                float iz = (i / 4) % 2;
                Vector3 quant = new Vector3(Mathf.Floor(pos.x * GridRes + ix) / GridRes, Mathf.Floor(pos.y * GridRes + iy) / GridRes, Mathf.Floor(pos.z * GridRes + iz) / GridRes);

                gc.transform.position = Vector3.Lerp(pos, quant, 0.5f);
                gc.transform.rotation = Quaternion.FromToRotation(Vector3.up, pos - quant);
                float s = Vector3.Distance(pos, quant) * 5;
                float sm = Mathf.Max(s, 0.01f);
                gc.transform.localScale = new Vector3(0.1f, s, 0.1f);
                Color c;
                GameObject head;
                if (count < 3)
                {
                    head = parts[0];
                }
                else
                {
                    head = parts[3];
                }

                if (head == part)
                {
                    c = palette[0];
                }
                else
                {
                    if (part.transform.position.x < head.transform.position.x)
                    {
                        c = palette[1];
                    }
                    else
                    {
                        c = palette[2];
                    }
                }
                gc.GetComponent<Renderer>().material.SetColor("_Color", c);
            }
            count++;
        }
    }
}
