using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Clock : MonoBehaviour
{

    private double ts, sceneStartTs, recordingStartTs;
    private float refreshRate;

    // Start is called before the first frame update

    void Start()
    {
        //ts = Math.Round(System.DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000,2);
        //oldTs = ts;
        refreshRate = FindObjectOfType<GameEngine>().targetFrameRate;
        InvokeRepeating("RefreshTimeStamp", 0f, 1/refreshRate);
    }

    // Update is called once per frame
    void RefreshTimeStamp()
    {
        ts = Math.Round(System.DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000,2);
    }

    public void SetSceneStartTs(){
        sceneStartTs = Math.Round(System.DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000,2);
    }

    public void SetRecordingStartTs(){
        recordingStartTs = Math.Round(System.DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000,2);
    }

    public double GetUnixTs(){
        return(ts);
    }

    public double GetTimeSinceSceneStart(){
        return(ts-sceneStartTs);
    }

    public double GetTimeSinceRecordingStart(){
        return(ts-recordingStartTs);
    }

}
