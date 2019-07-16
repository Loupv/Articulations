using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaybackManager : MonoBehaviour
{
    public PerformanceFile performanceFile;
    public GameEngine gameEngine;
    public bool play;
    public int currentRecordLine = 0;
    double lastTime;
    PerformanceLine currentLine;
    public int playerNumber;
    
    public void StartPlayback(GameEngine ge){
        gameEngine = ge;
        play = true;
        Invoke("UpdatePlayback",0f);
    }

    void UpdatePlayback()
    {
        if(play){
            currentLine = performanceFile.lines[currentRecordLine];
            if(playerNumber == 1)
                gameEngine.userManager.me.SetPlayerPosition(currentLine.p1HeadPosition,currentLine.p1LeftHandPosition,currentLine.p1RightHandPosition);
            else 
                gameEngine.userManager.me.SetPlayerPosition(currentLine.p2HeadPosition,currentLine.p2LeftHandPosition,currentLine.p2RightHandPosition);
                
            float timeToWait = (float)(currentLine.Time - lastTime)/1000;
            lastTime = currentLine.Time;
            currentRecordLine += 1 ;
            Invoke("UpdatePlayback", timeToWait);   
        }
    }

    
    public void ChangePlayerTracked(UnityEngine.UI.Dropdown dropdown){
        if(dropdown.value == 0) playerNumber = 1;
        else if(dropdown.value == 1) playerNumber = 2;
    }
}
