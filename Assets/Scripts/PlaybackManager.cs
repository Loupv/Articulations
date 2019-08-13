using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaybackManager : MonoBehaviour
{
    public PerformanceFile performanceFile;
    public GameEngine gameEngine;
    public bool play, addGestureAnalyser;
    public GameObject gestureAnalyserPrefab;
    public int currentRecordLine;
    PerformanceLine currentLine;
    public int playerNumber, mode;
    public float performanceMaxTime;
    public double floatTimeTracker, playbackSpeed, lastSpeed;
    string currentViz = "null";
    bool paused;

    

    void Start(){

    }

    public void StartPlayback(){
        play = true;
        currentRecordLine = 0;
        floatTimeTracker = 0;
        if(addGestureAnalyser) GameObject.Instantiate(gestureAnalyserPrefab);
        Debug.Log("Launching playback");
        InvokeRepeating("UpdatePlayback",0f,1/(float)gameEngine.targetFrameRate);
    }

    void UpdatePlayback()
    {
        if(play){
            currentLine = performanceFile.lines[currentRecordLine];
            
            if(mode == 0){ //online
                if(playerNumber == 0){
                    gameEngine.userManager.me.SetPlayerPosition(currentLine.p1HeadPosition,currentLine.p1LeftHandPosition,currentLine.p1RightHandPosition);
                    gameEngine.userManager.me.SetPlayerRotation(currentLine.p1HeadRotation,currentLine.p1LeftHandRotation,currentLine.p1RightHandRotation);
                }
                else{ 
                    gameEngine.userManager.me.SetPlayerPosition(currentLine.p2HeadPosition,currentLine.p2LeftHandPosition,currentLine.p2RightHandPosition);
                    gameEngine.userManager.me.SetPlayerRotation(currentLine.p2HeadRotation,currentLine.p2LeftHandRotation,currentLine.p2RightHandRotation);
                }
            }
            else{
                if(currentViz != currentLine.Condition){
                    Debug.Log("Changing viz to : "+currentLine.Condition);
                    gameEngine.userManager.ChangeVisualisationMode(currentLine.Condition, gameEngine, false);
                    currentViz = currentLine.Condition;
                }
                gameEngine.userManager.usersPlaying[1].SetPlayerPosition(currentLine.p1HeadPosition,currentLine.p1LeftHandPosition,currentLine.p1RightHandPosition);
                gameEngine.userManager.usersPlaying[1].SetPlayerRotation(currentLine.p1HeadRotation,currentLine.p1LeftHandRotation,currentLine.p1RightHandRotation);
                gameEngine.userManager.usersPlaying[2].SetPlayerPosition(currentLine.p2HeadPosition,currentLine.p2LeftHandPosition,currentLine.p2RightHandPosition);
                gameEngine.userManager.usersPlaying[2].SetPlayerRotation(currentLine.p2HeadRotation,currentLine.p2LeftHandRotation,currentLine.p2RightHandRotation);
            } 

            gameEngine.uiHandler.playbackTime.text = "Playback Time : "+(currentLine.Time/1000).ToString()+" / "+performanceMaxTime;
            gameEngine.uiHandler.currentViz.text = "Current viz : "+currentLine.Condition;

            // the loop is set to run at 60fps, the record file has 60fps, so we need to read 1 lines per frame * desired speed
            floatTimeTracker += (double)gameEngine.targetFrameRate/(double)gameEngine.gameData.saveFileFrequency * playbackSpeed;
            currentRecordLine = (int)floatTimeTracker ; // we round the frame number
            if(currentRecordLine >= performanceFile.lines.Count){
                currentRecordLine = 0;
            } 
        }
    }

    
    public void ChangePlayerTracked(UnityEngine.UI.Dropdown dropdown){
        playerNumber = dropdown.value;
    }

    public void ChangePlaybackMode(){
        mode = gameEngine.uiHandler.onlineOfflinePlaybackMode.value;
        if(mode == 0) {
            gameEngine.uiHandler.switchPlaybackPlayer.gameObject.SetActive(true);
            gameEngine.uiHandler.OSCServerAddressInput.gameObject.SetActive(true);
        }
        else if(mode == 1){ 
            gameEngine.uiHandler.switchPlaybackPlayer.gameObject.SetActive(false);
            gameEngine.uiHandler.OSCServerAddressInput.gameObject.SetActive(false);
        }
    }

    public void PausePlayback(){
        if(!paused){ // pause
            lastSpeed = playbackSpeed;
            playbackSpeed = 0;
            paused = true;
        }
        else {
            playbackSpeed = lastSpeed; // resume
            paused = false;
        }
    }

    public void StopPlayback(){
        Debug.Log("Playback Stopped");
        if(paused) PausePlayback(); // unpause
        if(addGestureAnalyser) Destroy(GameObject.FindGameObjectWithTag("GestureAnalyser"));
        CancelInvoke("UpdatePlayback");
    }
    
}
