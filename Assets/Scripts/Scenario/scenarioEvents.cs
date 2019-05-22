using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scenarioEvents : MonoBehaviour
{
    public GameObject [] mirror;
    public GameObject bubbles;
    public bool mirrorAct;
    public bool bubblesAct;
    public GameObject[] particleSystems;
    //public List<GameObject> particleList;
    public GameObject userManager;
    GameObject shortTrails;
    GameObject longTrails;
    public Material[] skyboxes;
    int i;
    int j;
    // Start is called before the first frame update
    void Start()
    {
        i = 1;
        shortTrails = userManager.GetComponent<UserManager>().TrailRendererPrefab;
        longTrails = userManager.GetComponent<UserManager>().SparkParticlesPrefab;
        //particleList = new List<GameObject>();


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


            if (Input.GetKeyUp(KeyCode.P))
            {

                shortTrails = particleSystems[j];
                longTrails = particleSystems[j+1];
            j += 2;
            Debug.Log(j);
            }

        if (j > particleSystems.Length - 1)
            j = 0;


    }

    public void SetNextSkybox()
    {
        RenderSettings.skybox = skyboxes[i];
        i++;
        if (i > skyboxes.Length - 1) i = 0;
    }
}
