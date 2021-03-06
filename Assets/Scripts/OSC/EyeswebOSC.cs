﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


// this class is only used by server
// client just send/get infos from the server

/* public class UserPerformanceData{
    public PlayerPartInfos head, leftHand, rightHand;
}*/

public class UserPerformanceData{

    public float handsVelocityMean;
    public float handsAccelerationMean;
    public float kineticEnergy;
    public float handsHeightRatio;
    public float headHeightRatio;
    public int handsActivity;
    public int leftHandActivity;
    public int rightHandActivity;

}

public class SharedPerformanceData{

    public float distance;
    public Vector3 playersBarycenter;

}

public class EyeswebOSC : MonoBehaviour
{

    public OSC osc;
    OSCEndPoint oscEndPoint;
    OscMessage message;
    public GameEngine gameEngine;
	public UserManager userManager;
    private UserRole _userRole;
    UserPerformanceData player1PerformanceData, player2PerformanceData;
    SharedPerformanceData sharedPerformanceData;
    public string customEyeswebAddress = "192.168.1.80";
    public int customEyeswebPort;
    public bool drawBarycenter;
    GameObject barycenter;
    //private DrawShape drawer1, drawer2;
    public bool initialized;
    double ts, oldTs;


	// Use this for initialization
	public void Init (UserRole userRole) {
        
        if(!osc.initialized) osc.Init();

        oscEndPoint.remotePort = customEyeswebPort;
        oscEndPoint.ip = customEyeswebAddress;

        osc.SetAddressHandler("/P1SummaryMatrix", GetFloatFromOsc);
        osc.SetAddressHandler("/P2SummaryMatrix", GetFloatFromOsc);
        osc.SetAddressHandler("/SharedDataMatrix", GetFloatFromOsc);
        gameEngine = FindObjectOfType<GameEngine>();
        
        player1PerformanceData = new UserPerformanceData();
        player2PerformanceData = new UserPerformanceData();
        sharedPerformanceData = new SharedPerformanceData();
        
        _userRole = userRole;
        
        oldTs = Math.Round(System.DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000,2);
        
        initialized = true;
        Debug.Log("Eyesweb osc initialized");
        if(drawBarycenter){
            barycenter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            barycenter.transform.localScale *= 0.15f;
            //osc = new OSC();
        }
    }
	


    public void SendPositionsToEyesWeb(){

        int i =0;

        foreach(UserData user in userManager.usersPlaying){
            if(user._ID != userManager.me._ID && 
                (user._userRole == UserRole.Player || (user._userRole == UserRole.Playback && !user._isMe))){

                osc.sender.SendOSCPosition(user, 0, oscEndPoint);
                osc.sender.SendOSCPosition(user, 1, oscEndPoint);
                osc.sender.SendOSCPosition(user, 2, oscEndPoint);

                i+=1;
            }
        }
        osc.sender.SendTimeStampData(oscEndPoint);
    }

    void GetFloatFromOsc(OscMessage message){
     
        if(message.address.Contains("P1") || message.address.Contains("P2")){
        float f1 = message.GetFloat(2);
        float f2 = message.GetFloat(3);
        float f3 = message.GetFloat(4);
        float f4 = message.GetFloat(5);
        float f5 = message.GetFloat(6);
        int activity = message.GetInt(7);
        int lhactivity = message.GetInt(7);
        int rhactivity = message.GetInt(7);

        if(message.address.Contains("P1")) {
            player1PerformanceData.handsVelocityMean = f1;
            player1PerformanceData.handsAccelerationMean = f2;
            player1PerformanceData.kineticEnergy = f3;
            player1PerformanceData.handsHeightRatio = f4;
            player1PerformanceData.headHeightRatio = f5;
            player1PerformanceData.handsActivity = activity;
            player1PerformanceData.leftHandActivity = lhactivity;
            player1PerformanceData.rightHandActivity = rhactivity;
        }

        if(message.address.Contains("P2")){
            player2PerformanceData.handsVelocityMean = f1;
            player2PerformanceData.handsAccelerationMean = f2;
            player2PerformanceData.kineticEnergy = f3;
            player2PerformanceData.handsHeightRatio = f4;
            player2PerformanceData.headHeightRatio = f5;
            player2PerformanceData.handsActivity = activity;
            player2PerformanceData.leftHandActivity = lhactivity;
            player2PerformanceData.rightHandActivity = rhactivity;
        } 
}
        if(message.address.Contains("Shared")){
            sharedPerformanceData.distance = message.GetFloat(0);
            sharedPerformanceData.playersBarycenter.x = message.GetFloat(1);
            sharedPerformanceData.playersBarycenter.y = message.GetFloat(2);
            sharedPerformanceData.playersBarycenter.z = message.GetFloat(3);
            if(drawBarycenter) barycenter.transform.position = sharedPerformanceData.playersBarycenter;
        } 

        //if(_userRole == UserRole.Server) SendEyesWebDataToPlayers(); // if we have real players to send data coming back from eyesweb
        
    }


    public UserPerformanceData GetUserPerformanceData(int id)
    {
        if (id == 1) return player1PerformanceData;
        else if (id == 2) return player2PerformanceData;
        else return null;
    }
    public SharedPerformanceData GetSharedPerformanceData()
    {
        return sharedPerformanceData;
    }


    void SendEyesWebDataToPlayers(){
        // TODO differenciate each player and filter data to send
    }
}
