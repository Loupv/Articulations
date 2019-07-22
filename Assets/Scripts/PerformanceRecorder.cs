using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PerformanceRecorder : MonoBehaviour
{
    public GameEngine gameEngine;
    //public string filePrefix = "";
    [HideInInspector]
    public string filePath;
    
    //[HideInInspector]
    public float saveRate = 1;
    public GameObject startButton, pauseButton;
    public UIHandler uiHandler;
    StreamWriter sr;
    [HideInInspector]
    //public double startTime;
    public bool isRecording, isPaused;
    public string fileName;
    public int sessionID = 0;
    string line, conditionPattern;
    private Clock clock;
    
    //double newTs, oldTs, newTime, oldTime;

    void Start(){
       uiHandler.ActualizeGizmos(isRecording, isPaused);
       filePath = Application.dataPath +"/StreamingAssets/Recordings/";
       if(!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
       sessionID = Directory.GetFiles(filePath).Length +1;
       uiHandler.sessionIDInputBox.text = sessionID.ToString();
       clock = FindObjectOfType<Clock>();
    }

    public void StartRecording()
    {
        //startTime = Time.time * 1000;
        saveRate = 1/(float)gameEngine.gameData.saveFileFrequency;
        fileName = "S"+uiHandler.sessionIDInputBox.text+"_"+System.DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss") + ".csv";
        conditionPattern = gameEngine.scenarioEvents.GetScenarioConditionsPattern();

        if (File.Exists(filePath+fileName))
        {
            Debug.Log(fileName+" already exists.");
            return;
        }
        else{
            sr = File.CreateText(filePath+fileName);
            sr.WriteLine ("SessionID;TS_Unix;Time;Scenario;Condition;" +
            	"x1;y1;z1;rotx1;roty1;rotz1;rotw1;lhx1;lhy1;lhz1;lhrotx1;lhroty1;lhrotz1;lhrotw1;rhx1;rhy1;rhz1;rhrotx1;rhroty1;rhrotz1;rhrotw1;" +
                "x2;y2;z2;rotx2;roty2;rotz2;rotw2;lhx2;lhy2;lhz2;lhrotx2;lhroty2;lhrotz2;lhrotw2;rhx2;rhy2;rhz2;rhrotx2;rhroty2;rhrotz2;rhrotw2");
            isRecording = true;
            uiHandler.ActualizeGizmos(isRecording, isPaused);
            startButton.SetActive(false);
            
            #if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
            #endif

            FindObjectOfType<Clock>().SetRecordingStartTs();

            saveRate = (float)Math.Round(saveRate,3);
            InvokeRepeating("SaveData",0f, saveRate);
        }
    }

    public void SaveData(){
 
        if(isRecording && !isPaused){
            AddLine2(gameEngine.userManager.usersPlaying, gameEngine.currentVisualisationMode);
        } 
    }

    // mettre dans un autre doc ? préciser que c'est un timemark dans le fichier
    public void TimeMark(int id){
        sr.WriteLine(id+";"+Time.frameCount);
    }

    public void PauseRecording(){
        if(isRecording)
            if(isPaused) {
                isPaused = false;
                }
            else{
                isPaused = true;
            } 
        uiHandler.ActualizeGizmos(isRecording, isPaused);
    }

    // triggered by UI
    public void StopRecording(){
        isRecording = false;
        isPaused = false;
        startButton.SetActive(true);
        uiHandler.ActualizeGizmos(isRecording, isPaused);
        CancelInvoke("SaveData");
        SaveTofile();
    }

    // both user on same line
    public void AddLine2(List<UserData> usersPlaying, string vizMode)
    {
        
        line = sessionID+ ";" + clock.GetUnixTs() + ";" + Math.Round(clock.GetTimeSinceRecordingStart(),0) + ";" + conditionPattern + ";" + vizMode;

        foreach (UserData user in gameEngine.userManager.usersPlaying)
        {
            if (user._userRole == UserRole.Player) 
                line += ";" + Math.Round(user.head.transform.position.x,3) + ";" + Math.Round(user.head.transform.position.y,3) + ";" + Math.Round(user.head.transform.position.z,3) +
                ";" + Math.Round(user.head.transform.rotation.x,3) + ";" + Math.Round(user.head.transform.rotation.y,3) + ";" + Math.Round(user.head.transform.rotation.z,3) + ";" + Math.Round(user.head.transform.rotation.w,3) +
                ";" + Math.Round(user.leftHand.transform.position.x,3) + ";" + Math.Round(user.leftHand.transform.position.y,3) + ";" + Math.Round(user.leftHand.transform.position.z,3) +
                ";" + Math.Round(user.leftHand.transform.rotation.x,3) + ";" + Math.Round(user.leftHand.transform.rotation.y,3) + ";" + Math.Round(user.leftHand.transform.rotation.z,3) + ";" + Math.Round(user.leftHand.transform.rotation.w,3) +
                ";" + Math.Round(user.rightHand.transform.position.x,3) + ";" + Math.Round(user.rightHand.transform.position.y,3) + ";" + Math.Round(user.rightHand.transform.position.z,3) +
                ";" + Math.Round(user.rightHand.transform.rotation.x,3) + ";" + Math.Round(user.rightHand.transform.rotation.y,3) + ";" + Math.Round(user.rightHand.transform.rotation.z,3) + ";" + Math.Round(user.rightHand.transform.rotation.w,3); 
        }
        line = line.Replace(",", ".");
        sr.WriteLine(line);
    }


    public void SaveTofile(){
        sr.Close();
        Debug.Log("PerformanceRecordingFile saved !");
    }


    string LineFormator(){
        return "";
    }

}
