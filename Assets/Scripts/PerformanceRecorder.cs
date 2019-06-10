using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public double startTime;
    public float lastFrameTime;//, timeAtFrameStart;
    public bool isRecording, isPaused;
    public string fileName;
    string line;

    void Start(){
       uiHandler.ActualizeGizmos(isRecording, isPaused);
       filePath = Application.dataPath +"/StreamingAssets/Recordings/";
       if(!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
    }

    public void StartRecording()
    {
        startTime = Time.time * 1000;
        saveRate = 1/(float)gameEngine.gameData.saveFileFrequency;
        fileName = System.DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss") + ".csv";

        if (File.Exists(filePath+fileName))
        {
            Debug.Log(fileName+" already exists.");
            return;
        }
        else{
            sr = File.CreateText(filePath+fileName);
            sr.WriteLine ("TS_Unix;Time;Viz;" +
            	"ID1;x1;y1;z1;rotx1;roty1;rotz1;lhx1;lhy1;lhz1;lhrotx1;lhroty1;lhrotz1;rhx1;rhy1;rhz1;rhrotx1;rhroty1;rhrotz1;" +
                "ID2;x2;y2;z2;rotx2;roty2;rotz2;lhx2;lhy2;lhz2;lhrotx2;lhroty2;lhrotz2;rhx2;rhy2;rhz2;rhrotx2;rhroty2;rhrotz2");
            isRecording = true;
            uiHandler.ActualizeGizmos(isRecording, isPaused);
            startButton.SetActive(false);
            
            #if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
            #endif

            InvokeRepeating("SaveData",0f, saveRate);
        }
    }

 /*   public void Update() {
        float delta = Time.time - lastFrameTime;
        if (lastFrameTime == 0 || delta >= 0.016f) {
            lastFrameTime = Time.time;
            Debug.Log("Update===== " + delta);
        }
    }*/

    public void SaveData(){
 
        if(isRecording && !isPaused){
            /*foreach(UserData user in gameEngine.userManager.usersPlaying){
                if(user._userRole == UserRole.Player) AddLine(user._ID, user.head.transform, user.leftHand.transform, user.rightHand.transform, gameEngine.currentVisualisationMode);
            }*/
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

    // every line a user 
    public void AddLine(int ID, Transform headTransform, Transform leftHandTransform, Transform rightHandTransform, int vizMode){
        double ts = (System.DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        ts *= 1000;
        ts = Math.Floor(ts);
        line = ID+";"+ts+";"+((int)(Time.time*1000 - startTime))+";"+vizMode.ToString()+";"+headTransform.position.x+";"+headTransform.position.y+";"+headTransform.position.z+
        ";"+headTransform.rotation.x+";"+headTransform.rotation.y+";"+headTransform.rotation.z+
        ";"+leftHandTransform.position.x+";"+leftHandTransform.position.y+";"+leftHandTransform.position.z+
        ";"+leftHandTransform.rotation.x+";"+leftHandTransform.rotation.y+";"+leftHandTransform.rotation.z+
        ";"+rightHandTransform.position.x+";"+rightHandTransform.position.y+";"+rightHandTransform.position.z+
        ";"+rightHandTransform.rotation.x+";"+rightHandTransform.rotation.y+";"+rightHandTransform.rotation.z;
        line = line.Replace(",",".");
        sr.WriteLine (line);
    }

    // both user on same line
    public void AddLine2(List<UserData> usersPlaying, string vizMode)
    {
        double ts = (System.DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        ts *= 1000;
        ts = Math.Floor(ts);
        line = ts + ";" + ((int)(Time.time * 1000 - startTime)) + ";" + vizMode;

        foreach (UserData user in gameEngine.userManager.usersPlaying)
        {
            if (user._userRole == UserRole.Player) 
                line += ";" + user._ID + 
                ";" + user.head.transform.position.x + ";" + user.head.transform.position.y + ";" + user.head.transform.position.z +
                ";" + user.head.transform.rotation.x + ";" + user.head.transform.rotation.y + ";" + user.head.transform.rotation.z +
                ";" + user.leftHand.transform.position.x + ";" + user.leftHand.transform.position.y + ";" + user.leftHand.transform.position.z +
                ";" + user.leftHand.transform.rotation.x + ";" + user.leftHand.transform.rotation.y + ";" + user.leftHand.transform.rotation.z +
                ";" + user.rightHand.transform.position.x + ";" + user.rightHand.transform.position.y + ";" + user.rightHand.transform.position.z +
                ";" + user.rightHand.transform.rotation.x + ";" + user.rightHand.transform.rotation.y + ";" + user.rightHand.transform.rotation.z; 
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
