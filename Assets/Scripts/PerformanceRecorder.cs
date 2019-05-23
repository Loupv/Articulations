using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceRecorder : MonoBehaviour
{
    public GameEngine gameEngine;
    //public string filePrefix = "";
    public string filePath;
    public GameObject startButton, pauseButton;
    public UIHandler uiHandler;
    StreamWriter sr;
    public double startTime;
    public bool isRecording, isPaused;

    void Start(){
       uiHandler.ActualizeGizmos(isRecording, isPaused);
       filePath = Application.dataPath +"/StreamingAssets/Recordings/";
    }

    public void StartRecording()
    {
        startTime = Time.time * 1000;
        string fileName = gameEngine._user._ID+"_"+System.DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss") + ".csv";
        //Debug.Log(filePath+fileName);

        if (File.Exists(filePath+fileName))
        {
            Debug.Log(fileName+" already exists.");
            return;
        }
        sr = File.CreateText(filePath+fileName);
        sr.WriteLine ("ID;Time;Viz;x;y;z;rotx;roty;rotz;lhx;lhy;lhz;lhrotx;lhroty;lhrotz;rhx;rhy;rhz;rhrotx;rhroty;rhrotz");
        isRecording = true;
        uiHandler.ActualizeGizmos(isRecording, isPaused);
        startButton.SetActive(false);
    }

    public void SaveData(List<UserData> usersPlaying){

        foreach(UserData user in usersPlaying){
            if(user._userRole == UserRole.Player) AddLine(user._ID, user.head.transform, user.leftHand.transform, user.rightHand.transform, gameEngine.currentVisualisationMode);
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
        startButton.SetActive(true);
        uiHandler.ActualizeGizmos(isRecording, isPaused);
        SaveTofile();
    }

    public void AddLine(int ID, Transform headTransform, Transform leftHandTransform, Transform rightHandTransform, int vizMode){
        sr.WriteLine (ID+";"+(Time.time*1000 - startTime).ToString("N3")+";"+vizMode.ToString()+";"+headTransform.position.x+";"+headTransform.position.y+";"+headTransform.position.z+
        ";"+headTransform.rotation.x+";"+headTransform.rotation.y+";"+headTransform.rotation.z+
        ";"+leftHandTransform.position.x+";"+leftHandTransform.position.y+";"+leftHandTransform.position.z+
        ";"+leftHandTransform.rotation.x+";"+leftHandTransform.rotation.y+";"+leftHandTransform.rotation.z+
        ";"+rightHandTransform.position.x+";"+rightHandTransform.position.y+";"+rightHandTransform.position.z+
        ";"+rightHandTransform.rotation.x+";"+rightHandTransform.rotation.y+";"+rightHandTransform.rotation.z);
    }



    public void SaveTofile(){
        sr.Close();
        Debug.Log("PerformanceRecordingFile saved !");
    }


    string LineFormator(){
        return "";
    }

}
