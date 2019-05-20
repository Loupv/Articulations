using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scenarioEvents : MonoBehaviour
{
    public GameObject [] mirror;
    public GameObject bubbles;
    public bool mirrorAct;
    public bool bubblesAct;

    public Material[] skyboxes;
    int i;
    // Start is called before the first frame update
    void Start()
    {
        i = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.M))
            mirrorAct = !mirrorAct;
        if (mirrorAct) { 
            mirror[0].SetActive(true);
            mirror[1].SetActive(true);
        }
        else { 
            mirror[0].SetActive(false);
            mirror[1].SetActive(false);
        }

        if (Input.GetKeyUp(KeyCode.B))
            bubblesAct = !bubblesAct;

        if (bubblesAct)
        {
            bubbles.SetActive(true);
        }
        else 
        {
            bubbles.SetActive(false);
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            RenderSettings.skybox = skyboxes[i];
            i++;
        }

        if (i > skyboxes.Length-1)
            i = 0;

    }
}
