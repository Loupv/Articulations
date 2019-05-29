using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scenarioEvents : MonoBehaviour
{
    public GameEngine gameEngine;
    public GameObject [] mirrors, calibrationTransforms;
    public GameObject bubbles, terrainCenter;
    public bool mirrorAct;
    public bool bubblesAct;
    public GameObject[] particleSystems;
    //public List<GameObject> particleList;
    public UserManager userManager;
    GameObject shortTrails;
    GameObject longTrails;
    public Material[] skyboxes;
    int i;
    int j;
    // Start is called before the first frame update


    void Start()
    {
        i = 1;
        shortTrails = userManager.TrailRendererPrefab;
        longTrails = userManager.SparkParticlesPrefab;
        //particleList = new List<GameObject>();


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.M))
            ToggleMirror();


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
            SetNextSkybox();
        }

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


    public void ToggleMirror()
    {
        //foreach (GameObject mirror in mirrors) mirror.SetActive(!mirror.activeSelf);

        mirrorAct = !mirrorAct;
        if (mirrorAct)
        {
            mirrors[0].SetActive(true);
            mirrors[1].SetActive(true);
        }
        else
        {
            mirrors[0].SetActive(false);
            mirrors[1].SetActive(false);
        }
    }

    // server side
    public void CalibratePlayersPositions(){

        int i = 0;
        // stores calib gaps into UserData to be sent later to clients and applied server side
        while(userManager.usersPlaying.Count > i){
            Vector3 tmp2D = new Vector3(userManager.usersPlaying[i].head.transform.position.x,0,userManager.usersPlaying[i].head.transform.position.z);
            userManager.usersPlaying[i].calibrationPositionGap = -tmp2D + calibrationTransforms[i].transform.position;
            userManager.usersPlaying[i].gameObject.transform.position += userManager.usersPlaying[i].calibrationPositionGap;
            i++;
        }

        // client side
        gameEngine.networkManager.SendClientPositionGap();
    }
}
