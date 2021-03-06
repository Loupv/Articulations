﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PlaybackMode{
    Online, Offline
}

public class PlaybackManager : MonoBehaviour
{
    public PerformanceFile performanceFile;
    public GameEngine gameEngine;
    public bool play, addGestureAnalyser;
    public GameObject gestureAnalyserPrefab;
    public IsInsightCheck isInsightCheck;
    public int currentRecordLine;
    public PerformanceLine currentLine;
    public GestureAnalyser movementAnalyser;
    public int playerNumber;
    public PlaybackMode mode;
    public float performanceMaxTime;
    public int conditionCount = 4;
    public double floatTimeTracker, playbackSpeed, lastSpeed;
    string currentViz = "null";
    bool paused;



    void Start(){
        ChangePlaybackMode(); // in start to actualize if quick mode change is made in editor
    }

    public void StartPlayback(){
        play = true;
        floatTimeTracker = 0;
        if(addGestureAnalyser) GameObject.Instantiate(gestureAnalyserPrefab);
        Debug.Log("Launching playback");
        InvokeRepeating("UpdatePlayback",0f,1/(float)gameEngine.targetFrameRate);
        gameEngine.uiHandler.playPauseButtonImage.sprite = gameEngine.uiHandler.pauseSprite;
    }

    void UpdatePlayback()
    {
        if(play){
            currentLine = performanceFile.lines[currentRecordLine];
            
            if(mode == PlaybackMode.Online){ //online
                if(playerNumber == 0){
                    gameEngine.userManager.me.SetPlayerPosition(currentLine.p1HeadPosition,currentLine.p1LeftHandPosition,currentLine.p1RightHandPosition);
                    gameEngine.userManager.me.SetPlayerRotation(currentLine.p1HeadRotation,currentLine.p1LeftHandRotation,currentLine.p1RightHandRotation);
                }
                else{ 
                    gameEngine.userManager.me.SetPlayerPosition(currentLine.p2HeadPosition,currentLine.p2LeftHandPosition,currentLine.p2RightHandPosition);
                    gameEngine.userManager.me.SetPlayerRotation(currentLine.p2HeadRotation,currentLine.p2LeftHandRotation,currentLine.p2RightHandRotation);
                }
            }
            else{ //offline
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
            
            if(floatTimeTracker >= performanceFile.lines.Count){
                floatTimeTracker = 0;
                currentRecordLine = 0;
            } 
            else if(floatTimeTracker < 0){
                floatTimeTracker = performanceFile.lines.Count-1;
                currentRecordLine = performanceFile.lines.Count-1;
            } 
        }
    }

    
    public void ChangePlayerTracked(UnityEngine.UI.Dropdown dropdown){
        playerNumber = dropdown.value;
    }

    public void ChangePlaybackMode(){
        int intMode = gameEngine.uiHandler.onlineOfflinePlaybackMode.value;
        if(intMode == 0) {
            gameEngine.uiHandler.switchPlaybackPlayer.gameObject.SetActive(true);
            gameEngine.uiHandler.OSCServerAddressInput.gameObject.SetActive(true);
            mode = PlaybackMode.Online;
        }
        else if(intMode == 1){ 
            gameEngine.uiHandler.switchPlaybackPlayer.gameObject.SetActive(false);
            gameEngine.uiHandler.OSCServerAddressInput.gameObject.SetActive(false);
            mode = PlaybackMode.Offline;
        }
    }

    public void JumpToCondition(int i){
        floatTimeTracker = performanceFile.lines.Count/conditionCount * (i-1);
    }

    public void JumpInTime(int timeGap){
        floatTimeTracker += timeGap * (double)gameEngine.gameData.saveFileFrequency;
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
        if (isInsightCheck.active) isInsightCheck.StopInsightCheck();
        if(addGestureAnalyser) Destroy(GameObject.FindGameObjectWithTag("GestureAnalyser"));
        CancelInvoke("UpdatePlayback");
    }
    
}
