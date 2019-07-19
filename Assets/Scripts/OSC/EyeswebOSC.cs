using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EyeswebOSC : MonoBehaviour
{
    public OSC osc;
	public GestureVisualiser gestureVisualiser;
    public bool initialized;
    double oldTs;

	// Use this for initialization
	public void Init (GameObject[] p1ObjectsToTrack, GameObject[] p2ObjectsToTrack) {
        osc.Init();
		
        /*osc.SetAddressHandler( "/P1_LeftSpeed" , GetFloatFromOsc);
		osc.SetAddressHandler("/P1_RightSpeed", GetFloatFromOsc);
		osc.SetAddressHandler( "/P1_LeftAcc" , GetFloatFromOsc);
		osc.SetAddressHandler("/P1_RightAcc", GetFloatFromOsc);
		osc.SetAddressHandler( "/P1_LeftJerk" , GetFloatFromOsc);
		osc.SetAddressHandler("/P1_RightJerk", GetFloatFromOsc);
		osc.SetAddressHandler( "/P1_LeftCurvature" , GetFloatFromOsc);
		osc.SetAddressHandler("/P1_RightCurvature", GetFloatFromOsc);
		osc.SetAddressHandler( "/P1_LeftSmoothness" , GetFloatFromOsc);
		osc.SetAddressHandler("/P1_RightSmoothness", GetFloatFromOsc);
        
        osc.SetAddressHandler( "/P2_LeftSpeed" , GetFloatFromOsc);
		osc.SetAddressHandler("/P2_RightSpeed", GetFloatFromOsc);
		osc.SetAddressHandler( "/P2_LeftAcc" , GetFloatFromOsc);
		osc.SetAddressHandler("/P2_RightAcc", GetFloatFromOsc);
		osc.SetAddressHandler( "/P2_LeftJerk" , GetFloatFromOsc);
		osc.SetAddressHandler("/P2_RightJerk", GetFloatFromOsc);
		osc.SetAddressHandler( "/P2_LeftCurvature" , GetFloatFromOsc);
		osc.SetAddressHandler("/P2_RightCurvature", GetFloatFromOsc);
		osc.SetAddressHandler( "/P2_LeftSmoothness" , GetFloatFromOsc);
		osc.SetAddressHandler("/P2_RightSmoothness", GetFloatFromOsc);*/

        osc.SetAddressHandler("/MovementData", GetFloatFromOsc);
        initialized = true;
        oldTs = Math.Floor(Math.Round(System.DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000,2));
        
    }
	

	// Update is called once per frame
	void Update () {

        //osc.outIP = "127.0.0.1";
        //osc.outPort = 6161;
        if(initialized){

            double ts = Math.Round(System.DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000,2);
            ts = Math.Floor(ts) - oldTs;
            
            OscMessage message = new OscMessage();
            
            foreach(GameObject objectToTrack in gestureVisualiser.p1ObjectsToTrack){
                message = new OscMessage();
                message.address = "/P1_"+objectToTrack.name+"_PositionXYZ";
                message.values.Add(objectToTrack.transform.position.x);
                message.values.Add(objectToTrack.transform.position.y);
                message.values.Add(objectToTrack.transform.position.z);
                osc.Send(message);
                Debug.Log (message);
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
            message.address = "/TimeStamp";
            message.values.Add(ts.ToString());
            osc.Send(message);
        }
    }


    void GetFloatFromOsc(OscMessage message){

        int playerID = message.GetInt(0);
        int handedness = message.GetInt(1);
        int type = message.GetInt(3);
        float f = message.GetFloat(2);

        if(handedness == 1){
            if(type == 1) gestureVisualiser.usersPerformanceData[playerID-1].leftHand.speed = f;
            else if(type == 2) gestureVisualiser.usersPerformanceData[playerID-1].leftHand.acceleration = f;
            else if(type == 3) gestureVisualiser.usersPerformanceData[playerID-1].leftHand.jerk = f;
            else if(type == 4) gestureVisualiser.usersPerformanceData[playerID-1].leftHand.curvature = f;
            else if(type == 5) gestureVisualiser.usersPerformanceData[playerID-1].leftHand.smoothness = f;
        } 
        else if(handedness == 2){
            if(type == 1) gestureVisualiser.usersPerformanceData[playerID-1].rightHand.speed = f;
            else if(type == 2) gestureVisualiser.usersPerformanceData[playerID-1].rightHand.acceleration = f;
            else if(type == 3) gestureVisualiser.usersPerformanceData[playerID-1].rightHand.jerk = f;
            else if(type == 4) gestureVisualiser.usersPerformanceData[playerID-1].rightHand.curvature = f;
            else if(type == 5) gestureVisualiser.usersPerformanceData[playerID-1].rightHand.smoothness = f;
        } 

    }


}
