﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EyeswebOSC : MonoBehaviour
{
    public OSC osc;
    public GameEngine gameEngine;
	public GestureVisualiser gestureVisualiser;
    private DrawShape drawer1, drawer2;
    public bool initialized;
    double ts, oldTs;

	// Use this for initialization
	public void Init (GameObject[] p1ObjectsToTrack, GameObject[] p2ObjectsToTrack) {
        if(!osc.initialized) osc.Init();
		
        osc.SetAddressHandler("/MovementData", GetFloatFromOsc);
        initialized = true;

        gameEngine = FindObjectOfType<GameEngine>();
        
        drawer1 = gestureVisualiser.transform.Find("Mesh1").GetComponent<DrawShape>();
        drawer2 = gestureVisualiser.transform.Find("Mesh2").GetComponent<DrawShape>();
        oldTs = Math.Round(System.DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000,2);
    }
	

	// Update is called once per frame
	void Update () {

        //osc.outIP = "127.0.0.1";
        //osc.outPort = 6161;
        if(initialized){

            
            OscMessage message = new OscMessage();
            
            foreach(GameObject objectToTrack in gestureVisualiser.p1ObjectsToTrack){
                message = new OscMessage();
                message.address = "/P1_"+objectToTrack.name+"_PositionXYZ";
                //message.values.Add(Math.Round(objectToTrack.transform.position.x,2);
                
                message.values.Add(objectToTrack.transform.position.x);
                message.values.Add(objectToTrack.transform.position.y);
                message.values.Add(objectToTrack.transform.position.z);
                osc.Send(message);
                //Debug.Log (message);
            }
            foreach(GameObject objectToTrack in gestureVisualiser.p2ObjectsToTrack){
                message = new OscMessage();
                message.address = "/P2_"+objectToTrack.name+"_PositionXYZ";
                message.values.Add(objectToTrack.transform.position.x);
                message.values.Add(objectToTrack.transform.position.y);
                message.values.Add(objectToTrack.transform.position.z);
                osc.Send(message);
                //Debug.Log (message);
            }

            message = new OscMessage();
            message.address = "/P1_HighLevelFeatures";
            message.values.Add(drawer1.contractionIndex);
            osc.Send(message);
            
            message = new OscMessage();
            message.address = "/P2_HighLevelFeatures";
            message.values.Add(drawer2.contractionIndex);
            osc.Send(message);


            ts = gameEngine.clock.GetTimeSinceSceneStart();
            //Debug.Log(ts+", "+(Math.Round(System.DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000,2)-oldTs));

            message = new OscMessage();
            message.address = "/TimeStamp";
            message.values.Add(ts.ToString());
            osc.Send(message);
        }
    }


    void GetFloatFromOsc(OscMessage message){
        int playerID = message.GetInt(0);
        int handedness = message.GetInt(1);
        float f1 = message.GetFloat(2);
        float f2 = message.GetFloat(3);
        float f3 = message.GetFloat(4);
        float f4 = message.GetFloat(5);
        float f5 = message.GetFloat(6);

        if(handedness == 1){
            gestureVisualiser.usersPerformanceData[playerID-1].leftHand.speed = f1;
            gestureVisualiser.usersPerformanceData[playerID-1].leftHand.acceleration = f2;
            gestureVisualiser.usersPerformanceData[playerID-1].leftHand.jerk = f3;
            gestureVisualiser.usersPerformanceData[playerID-1].leftHand.curvature = f4;
            gestureVisualiser.usersPerformanceData[playerID-1].leftHand.smoothness = f5;
        } 
        else if(handedness == 2){
            gestureVisualiser.usersPerformanceData[playerID-1].rightHand.speed = f1;
            gestureVisualiser.usersPerformanceData[playerID-1].rightHand.acceleration = f2;
            gestureVisualiser.usersPerformanceData[playerID-1].rightHand.jerk = f3;
            gestureVisualiser.usersPerformanceData[playerID-1].rightHand.curvature = f4;
            gestureVisualiser.usersPerformanceData[playerID-1].rightHand.smoothness = f5;
        } 

    }


}
