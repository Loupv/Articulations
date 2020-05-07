﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Scenario{
    public int scenarioId;
    public int toFade;
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
    public GameObject [] calibrationTransforms;
    public List<GameObject> mirrors, mirrorsNoVR;
    public GameObject bubbles, terrainCenter, naotoCloth;
    public PerformanceRecorder performanceRecorder;
    public bool mirrorAct;
    public bool bubblesAct;
    public bool naotoAct;
    //public GameObject[] particleSystems;
    public Scenario[] scenarios;
    public int currentScenario;
    public int currentCondition;
    //public List<GameObject> particleList;
    public UserManager userManager;
    GameObject shortTrails;
    GameObject longTrails;
    public Material[] skyboxes;
    public GameObject[] scenography;
    public bool timerPaused, scenarioIsRunning;
    public int skyboxID;
    int timeRemaining, hourOfDay;
    public int oldMirrorMask;
    public float lerpSunSpeed = 0.1f;
    // Start is called before the first frame update


    void Start()
    {
        skyboxID = 0;
        hourOfDay = -1;
        shortTrails = userManager.TrailRendererPrefab;
        longTrails = userManager.SparkParticlesPrefab;
        //particleList = new List<GameObject>();
        
        if(gameEngine.useVRHeadset)
            oldMirrorMask = mirrors[0].GetComponent<MirrorScript>().ReflectLayers;    
        else
            oldMirrorMask = mirrorsNoVR[0].GetComponent<MirrorScript>().ReflectLayers;
        
    }


    public void StartScenario(){

        currentCondition = 0;
        currentScenario = gameEngine.uiHandler.scenarioDropDown.value;
        //timeRemaining = scenarios[currentScenario].durations[currentCondition];

        scenarioIsRunning = true;
        timerPaused = false;
        gameEngine.uiHandler.ToggleScenarioButton(1);
        gameEngine.uiHandler.conditionTimeRemaining.gameObject.SetActive(true);
        gameEngine.uiHandler.stopScenario.gameObject.SetActive(true);
        gameEngine.osc.sender.StartOnlinePlaybacks(gameEngine.userManager.usersPlaying);
        InvokeRepeating("RunCondition", 0f, 1f);
    }

    public void PauseScenario(){
        if(gameEngine.uiHandler.autoRecordPerformance.isOn) performanceRecorder.PauseRecording();
        timerPaused = !timerPaused;
        gameEngine.osc.sender.PauseOnlinePlaybacks(gameEngine.userManager.usersPlaying);
        gameEngine.uiHandler.ToggleScenarioButton(2);
    }


    // run every second, actualize timer and checks if condition needs to be changes
    public void RunCondition(){

        if(!timerPaused){
            // if we reached the end of time for this condition
            if(timeRemaining <= 0){
                if(currentCondition < scenarios[currentScenario].conditions.Length){
                    // remove previous condition parameters
                    if(mirrorAct) ToggleMirror(false, 0, 0);
                    if(naotoAct) ToggleNaoto(false);
                    // change visualisation
                    if(gameEngine.uiHandler.autoRecordPerformance.isOn && !performanceRecorder.isRecording) performanceRecorder.StartRecording();
                    userManager.ChangeVisualisationMode(scenarios[currentScenario].conditions[currentCondition], gameEngine, scenarios[currentScenario].toFade == 1);
                    timeRemaining = scenarios[currentScenario].durations[currentCondition];
                    gameEngine.uiHandler.currentConditionText.text = "CurrentCondition : " + scenarios[currentScenario].conditions[currentCondition];
                    currentCondition += 1;
                }
                else{
                    StopScenario(0);
                }
            }

            timeRemaining--;
            if(timeRemaining >= 0) gameEngine.uiHandler.conditionTimeRemaining.text = "Time Remaining : "+timeRemaining;
        }
    }

    public void StopScenario(int interrupted){

        if(gameEngine.uiHandler.autoRecordPerformance.isOn && !gameEngine.uiHandler.recordAudioAfterScenario.isOn) 
            performanceRecorder.StopRecording(); // if we record sound after performance we let the recording active, otherwise we cut it after sound record

        userManager.ChangeVisualisationMode("0", gameEngine, scenarios[currentScenario].toFade == 1);
        gameEngine.uiHandler.ToggleScenarioButton(0);
        
        if(interrupted == 0){ // if scenario had ended well 
            Debug.Log("Scenario "+(currentScenario+1)+" done !");
            if(gameEngine.soundHandler.recordPostScenarioAudio){
                
                gameEngine.osc.sender.StartAudioRecording(gameEngine.soundHandler.postScenarioRecordingLenght, gameEngine.userManager.usersPlaying);
            }
        }
        else Debug.Log("Scenario "+(currentScenario+1)+" Interrupted !");
        
        timeRemaining = 0;
        gameEngine.uiHandler.conditionTimeRemaining.text = "Time Remaining : "+timeRemaining;
        gameEngine.uiHandler.conditionTimeRemaining.gameObject.SetActive(false);
        gameEngine.uiHandler.stopScenario.gameObject.SetActive(false);
        
        gameEngine.osc.sender.StopOnlinePlaybacks(gameEngine.userManager.usersPlaying);

        CancelInvoke("RunCondition");
        //userManager.ChangeVisualisationMode(scenarioId, gameEngine);

    }



    public void SetNextSkybox()
    {
        //RenderSettings.skybox = skyboxes[skyboxID];
        skyboxID++;
        //if (skyboxID == 4)
        if (skyboxID == skyboxes.Length ) skyboxID = 0;
        Debug.Log("skybox ID " + skyboxID);

        if (skyboxID == 0)
        {
            RenderSettings.skybox = skyboxes[skyboxID];
            scenography[0].SetActive(true);
            scenography[1].SetActive(true);
            scenography[2].SetActive(false);
            scenography[3].SetActive(false);
            scenography[4].SetActive(false);
            scenography[5].SetActive(false);
            RenderSettings.sun = GameObject.FindGameObjectWithTag("Sun").GetComponent<Light>();
        }
        else if (skyboxID == 1)
        {
            RenderSettings.skybox = skyboxes[skyboxID];
            scenography[0].SetActive(false);
            scenography[1].SetActive(true);
            scenography[2].SetActive(false);
            scenography[3].SetActive(false);
            scenography[4].SetActive(false);
            scenography[5].SetActive(true);
        }
        else if(skyboxID == 2)
        {
            RenderSettings.skybox = skyboxes[skyboxID];
            scenography[0].SetActive(false);
            scenography[1].SetActive(false);
            scenography[2].SetActive(true);
            scenography[3].SetActive(false);
            scenography[4].SetActive(false);
            scenography[5].SetActive(false);
            RenderSettings.fog = true;
            RenderSettings.sun = scenography[3].GetComponentInChildren<Light>();
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.015f;
            RenderSettings.fogColor = new Color(0.3867925f, 0.2827964f, 0.3604879f);
        }
        else if (skyboxID == 3)
        {
            RenderSettings.skybox = skyboxes[skyboxID];
            scenography[0].SetActive(false);
            scenography[1].SetActive(false);
            scenography[2].SetActive(false);
            scenography[3].SetActive(true);
            scenography[4].SetActive(false);
            scenography[5].SetActive(false);
            RenderSettings.fog = false;
            RenderSettings.sun = null;
        }
        else if (skyboxID == 4)
        {
            RenderSettings.skybox = skyboxes[skyboxID];
            scenography[0].SetActive(false);
            scenography[1].SetActive(false);
            scenography[2].SetActive(false);
            scenography[3].SetActive(false);
            scenography[4].SetActive(true);
            scenography[5].SetActive(false);
        }
    }

    public void SetSkybox(int id){
        skyboxID = id-1;
        SetNextSkybox();
        //RenderSettings.skybox = skyboxes[skyboxID];
    }

    // 0h is -90°, 6h is 0°, 12h is 90°, 18h is 180°
    public void SetTimeOfDay(int h){
        if(hourOfDay != h){
            hourOfDay = h;
            InvokeRepeating("LerpHourOfDay",0f,0.1f);
            Debug.Log("Lerping Hour of Day");
        }
    }

    void LerpHourOfDay(){
        GameObject sun = UnityEngine.GameObject.FindGameObjectWithTag("Sun");
        if (sun != null)
        {
            sun.transform.rotation = Quaternion.RotateTowards(sun.transform.rotation, Quaternion.Euler(new Vector3(hourOfDay * 360 / 24 - 90, sun.transform.rotation.y, sun.transform.rotation.z)), lerpSunSpeed * Time.deltaTime);
            if (Mathf.Abs(sun.transform.rotation.eulerAngles.x - (hourOfDay * 360 / 24 - 90)) < 0.1)
            {
                CancelInvoke("LerpHourOfDay");
                Debug.Log("Sun lerping ended");
            }
        }
    }


    public void ToggleMirror(bool b, int maskMode, int rank) // maskmode 0 -> tous les reflets sont visibles, maskmode 1 -> only my reflect, maskmode 2 -> chaque danseur voit l'autre dans le miroir mais ne se voit pas
    {
        //foreach (GameObject mirror in mirrors) mirror.SetActive(!mirror.activeSelf);
        //if (gameEngine.gameData.useVr == 1) // quick fix
        //{
            if (!mirrorAct && b)
            {
                if(gameEngine.useVRHeadset){ 
                    foreach(GameObject mirror in mirrors) mirror.SetActive(true);
                }
                else{
                    foreach(GameObject mirror in mirrorsNoVR) mirror.SetActive(true);
                }
                mirrorAct = !mirrorAct;
            }
            else if(mirrorAct && !b)
            {
                if(gameEngine.useVRHeadset){ 
                    foreach(GameObject mirror in mirrors) mirror.SetActive(false);
                }
                else{
                    foreach(GameObject mirror in mirrorsNoVR) mirror.SetActive(false);
                }
                mirrorAct = !mirrorAct;
            }


            // check mirror layers
            if(gameEngine.useVRHeadset){ 
                
                foreach(GameObject go in GameObject.FindObjectsOfType(typeof(Camera)))
                {
                    if(go.name == "Stereo Camera Eye [Mirror Plane]") {
                        go.GetComponent<Camera>().cullingMask = oldMirrorMask; // revert back each layer
                        if(maskMode == 1) go.GetComponent<Camera>().cullingMask &=  ~(1 << LayerMask.NameToLayer("Player1")); // we're player1, off player1 in mirror
                        else if(maskMode == 2) go.GetComponent<Camera>().cullingMask &=  ~(1 << LayerMask.NameToLayer("Player2")); // we're player2, off player2 in mirror
                    }
                }
            }
            else{
                if(maskMode == 0){
                    foreach(GameObject mirror in mirrorsNoVR) {
                        mirror.GetComponent<MirrorScript>().ReflectLayers = oldMirrorMask;
                    }
                }
                else{
                    foreach(GameObject mirror in mirrorsNoVR){ 
                        mirror.GetComponent<MirrorScript>().ReflectLayers = oldMirrorMask;
                        if(maskMode == 1){
                            if(rank == 1) mirror.GetComponent<MirrorScript>().ReflectLayers &=  ~(1 << LayerMask.NameToLayer("Player2"));
                            else if(rank == 2) mirror.GetComponent<MirrorScript>().ReflectLayers &=  ~(1 << LayerMask.NameToLayer("Player1"));
                        } 
                        else if(maskMode == 2) {
                            if(rank == 1) mirror.GetComponent<MirrorScript>().ReflectLayers &=  ~(1 << LayerMask.NameToLayer("Player1"));
                            else if(rank == 2) mirror.GetComponent<MirrorScript>().ReflectLayers &=  ~(1 << LayerMask.NameToLayer("Player2"));
                        }
                    }
                    
                }
            }
            
            Camera.main.cullingMask = oldMirrorMask;
            //if(maskMode == 1){
            //    if(rank == 1) Camera.main.cullingMask &=  ~(1 << LayerMask.NameToLayer("Player1"));
            //    else if(rank == 2) Camera.main.cullingMask &=  ~(1 << LayerMask.NameToLayer("Player2")); 
            //} 
            //else if(maskMode == 2){
            if(maskMode == 1 || maskMode ==  2){
                if(rank == 1) Camera.main.cullingMask &=  ~(1 << LayerMask.NameToLayer("Player2"));
                else if(rank == 2)Camera.main.cullingMask &=  ~(1 << LayerMask.NameToLayer("Player1")); // if no reflect mode, we only see us in real
            }
            //}
            
        Debug.Log("Mirror Toggled");
    }

    public void ToggleNaoto(bool b)
    {
        //foreach (GameObject mirror in mirrors) mirror.SetActive(!mirror.activeSelf);
        //if (gameEngine.gameData.useVr == 1) // quick fix
        //{
        if (!naotoAct && b)
        {
            naotoCloth.SetActive(true);
            naotoAct = !naotoAct;
            Debug.Log("Naoto Toggled");
        }
        else if(naotoAct && !b )
        {
            naotoCloth.SetActive(false);
            naotoAct = !naotoAct;
            Debug.Log("Naoto Toggled");
        }

    }

    // server side
    public void CalibratePlayersPositions(List<UserData> usersPlaying){

        int i = 0, j= 0;
        // stores calib gaps into UserData to be sent later to clients and applied server side
        while(userManager.usersPlaying.Count > i){
            if(userManager.usersPlaying[i]._userRole == UserRole.Player){
                Vector3 tmp2D = new Vector3(userManager.usersPlaying[i].head.transform.position.x - userManager.usersPlaying[i].calibrationPositionGap.x, 0,
                    userManager.usersPlaying[i].head.transform.position.z - userManager.usersPlaying[i].calibrationPositionGap.z);

                userManager.usersPlaying[i].calibrationPositionGap = -tmp2D + calibrationTransforms[j].transform.position;
                userManager.usersPlaying[i].gameObject.transform.position += userManager.usersPlaying[i].calibrationPositionGap;
                j+=1;
            }
            i++;
        }

        // client side
        gameEngine.networkManager.SendClientPositionGap(usersPlaying);
    }


    public string GetScenarioConditionsPattern(){

        string returnStr = "";
        int i = 1;

        foreach(string str in  scenarios[currentScenario].conditions){
            returnStr += str;
            if(i < scenarios[currentScenario].conditions.Length) returnStr += "-";
            i++;
        }
        return returnStr;
    }



}
