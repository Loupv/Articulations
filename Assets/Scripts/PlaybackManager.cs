﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaybackManager : MonoBehaviour
{
    public PerformanceFile performanceFile;
    public GameEngine gameEngine;
    public bool play, addGestureAnalyser;
    public GameObject gestureAnalyserPrefab;
    public int currentRecordLine = 0;
    double lastTime;
    PerformanceLine currentLine;
    public int playerNumber, mode;
    public float performanceMaxTime;
    Clock clock;
    private double startTs;
    string currentViz;
    [HideInInspector]
    

    void Start(){
        //ChangePlaybackMode();
        clock = FindObjectOfType<Clock>();
    }

    public void StartPlayback(GameEngine ge){
        gameEngine = ge;
        play = true;
        if(addGestureAnalyser) GameObject.Instantiate(gestureAnalyserPrefab);
        Invoke("UpdatePlayback",0f);
        startTs = clock.GetUnixTs();
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
                gameEngine.userManager.usersPlaying[0].SetPlayerPosition(currentLine.p1HeadPosition,currentLine.p1LeftHandPosition,currentLine.p1RightHandPosition);
                gameEngine.userManager.usersPlaying[0].SetPlayerRotation(currentLine.p1HeadRotation,currentLine.p1LeftHandRotation,currentLine.p1RightHandRotation);
                gameEngine.userManager.usersPlaying[1].SetPlayerPosition(currentLine.p2HeadPosition,currentLine.p2LeftHandPosition,currentLine.p2RightHandPosition);
                gameEngine.userManager.usersPlaying[1].SetPlayerRotation(currentLine.p2HeadRotation,currentLine.p2LeftHandRotation,currentLine.p2RightHandRotation);
            } 

            gameEngine.uiHandler.playbackTime.text = "Playback Time : "+(currentLine.Time/1000).ToString()+" / "+performanceMaxTime;

            //float timeToWait = (float)(currentLine.Time - lastTime)/1000;
            double realTimeElapsed = (clock.GetUnixTs() - startTs)/1000;
            double csvTimeElapsed =  (currentLine.TimeStamp - performanceFile.lines[0].TimeStamp)/1000 ;

            lastTime = currentLine.Time;
            
            float timeToWait = 0;

            if(Mathf.Abs((float)realTimeElapsed - (float)csvTimeElapsed) > 1/gameEngine.targetFrameRate){ // if csv playback is ahead or too late
                timeToWait = (float)(csvTimeElapsed - realTimeElapsed);
                if(timeToWait <0 ) timeToWait = 0;
            }

            Debug.Log(realTimeElapsed+", "+csvTimeElapsed);
            currentRecordLine += 1 ;
            if(currentRecordLine >= performanceFile.lines.Count) currentRecordLine = 0;           
            
            Invoke("UpdatePlayback", timeToWait);
    
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
    
}
