using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Scenario{
    public int scenarioId;
    public string[] conditions;
    public int[] durations;
}
[System.Serializable]
public class ScenarioList{
    public Scenario[] scenarios;
}




public class ScenarioEvents : MonoBehaviour
{
    public GameEngine gameEngine;
    public GameObject [] mirrors, calibrationTransforms;
    public GameObject bubbles, terrainCenter;
    public PerformanceRecorder performanceRecorder;
    public bool mirrorAct;
    public bool bubblesAct;
    public GameObject[] particleSystems;
    public Scenario[] scenarios;
    public int currentScenario;
    public int currentCondition;
    //public List<GameObject> particleList;
    public UserManager userManager;
    GameObject shortTrails;
    GameObject longTrails;
    public Material[] skyboxes;
    public bool timerPaused, scenarioIsRunning;
    int skyboxID, timeRemaining;
    int j;
    // Start is called before the first frame update


    void Start()
    {
        skyboxID = 1;
        shortTrails = userManager.TrailRendererPrefab;
        longTrails = userManager.SparkParticlesPrefab;
        //particleList = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        /*/if (Input.GetKeyUp(KeyCode.M))
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

        */

    }


    public void StartScenario(){

        currentCondition = 0;
        currentScenario = gameEngine.uiHandler.scenarioDropDown.value;
        //timeRemaining = scenarios[currentScenario].durations[currentCondition];

        scenarioIsRunning = true;
        timerPaused = false;
        gameEngine.uiHandler.ToggleScenarioButton(1);
        InvokeRepeating("RunCondition", 0f, 1f);
    }

    public void PauseScenario(){
        if(gameEngine.uiHandler.autoRecordPerformance.isOn) performanceRecorder.PauseRecording();
        timerPaused = !timerPaused;
        gameEngine.uiHandler.ToggleScenarioButton(2);
    }

    public void StopScenario(){

        Debug.Log("Scenario "+currentScenario+" done !");
        if(gameEngine.uiHandler.autoRecordPerformance.isOn) performanceRecorder.StopRecording();
        userManager.ChangeVisualisationMode("0", gameEngine,gameEngine.weFade);
        gameEngine.uiHandler.ToggleScenarioButton(0);
        CancelInvoke("RunCondition");
        //userManager.ChangeVisualisationMode(scenarioId, gameEngine);

    }

    public void RunCondition(){

        if(!timerPaused){
            // if we reached the end of time for this condition
            if(timeRemaining <= 0){
                if(currentCondition < scenarios[currentScenario].conditions.Length){
                    // remove previous condition parameters
                    if(mirrorAct) ToggleMirror();
                    // change visualisation
                    userManager.ChangeVisualisationMode(scenarios[currentScenario].conditions[currentCondition], gameEngine, gameEngine.weFade);
                    timeRemaining = scenarios[currentScenario].durations[currentCondition];
                    if(gameEngine.uiHandler.autoRecordPerformance.isOn && !performanceRecorder.isRecording) performanceRecorder.StartRecording();
                    currentCondition += 1;
                }
                else{
                    StopScenario();
                }
            }

            timeRemaining--;
        }
    }


    public void SetNextSkybox()
    {
        RenderSettings.skybox = skyboxes[skyboxID];
        skyboxID++;
        if (skyboxID > skyboxes.Length - 1) skyboxID = 0;
    }

    public void SetSkybox(int id){
        skyboxID = id;
        RenderSettings.skybox = skyboxes[skyboxID];
    }


    public void ToggleMirror()
    {
        //foreach (GameObject mirror in mirrors) mirror.SetActive(!mirror.activeSelf);
        if (gameEngine.gameData.useVr == 1) // quick fix
        {
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
        Debug.Log("Mirror Toggled");
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
