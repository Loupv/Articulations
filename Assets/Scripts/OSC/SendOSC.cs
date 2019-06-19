using System.Collections;
using System.Net;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SendOSC : MonoBehaviour {

    public GameEngine gameEngine;
    public UserManager userManager;
    private OscMessage message;
    public OSC osc;

    public void tmpStringOSC(UserData userData){
        if (osc.initialized)
        {
            message = new OscMessage();
            message.address = "/Say";
            message.values.Add("Hello");
            osc.OscPacketIO.RemoteHostName = userData.oscEndPoint.ip;
            osc.OscPacketIO.RemotePort = userData.oscEndPoint.remotePort;
            osc.Send(message);
            if(gameEngine.debugMode) Debug.Log("Sending : " + message);
        }
    }



/*
    -------------------------------------
    -----------SERVER FUNCTIONS----------
    -------------------------------------
 */

     // server sends confirmation to client that he's registered
    public void SendRegistrationConfirmation(UserData user)
    {
        if (osc.initialized)
        {
            message = new OscMessage();
            message.address = "/RegistrationConfirmed";
            message.values.Add(gameEngine.performanceRecorder.sessionID);
            
            message.values.Add(user._ID);
            message.values.Add(user.oscEndPoint.remotePort);
            message.values.Add(gameEngine.currentVisualisationMode);
            message.values.Add(user._registeredRank);
            
            if(gameEngine.audioRecordManager.recordPostScenarioAudio) message.values.Add(1);
            else message.values.Add(0);
            
            message.values.Add(gameEngine.gameData.audioRecordLength);

            osc.OscPacketIO.RemoteHostName = user.oscEndPoint.ip;
            osc.OscPacketIO.RemotePort = user.oscEndPoint.remotePort;
            osc.Send(message);
            if(gameEngine.debugMode) Debug.Log("Sending : " + message);

            AddEveryPlayerToClientDict(user, userManager.usersPlaying);
        }
    }

    public void RefuseRegistration(string playerIP, int requestedPort)
    {
       
        if (osc.initialized)
        {
            message = new OscMessage();
            message.address = "/RegistrationRefused";
            message.values.Add(requestedPort);
            osc.OscPacketIO.RemoteHostName = playerIP;
            osc.OscPacketIO.RemotePort = requestedPort;
            osc.Send(message);
            if(gameEngine.debugMode) Debug.Log("Sending : " + message);
        }
    }


    public void SendOSCPosition(UserData userData, int playerPart, OSCEndPoint targetEndPoint)
    {
        Vector3 pos = new Vector3();
        Quaternion rot = new Quaternion();

        if (osc.initialized)
        {   
            message = new OscMessage();
            message.address =  "/PlayerData";
            message.values.Add(userData._ID);

            if(playerPart == 0){ 
                pos = userData.head.transform.position- userData.calibrationPositionGap;
                rot = userData.head.transform.rotation;
            }
            if(playerPart == 1){ 
                pos = userData.leftHand.transform.position- userData.calibrationPositionGap;
                rot = userData.leftHand.transform.rotation;
            }
            if(playerPart == 2){ 
                pos = userData.rightHand.transform.position- userData.calibrationPositionGap;
                rot = userData.rightHand.transform.rotation;
            }

            if(gameEngine._userRole == UserRole.Server){
                //pos -=  userData.calibrationPositionGap;
            }

            message.values.Add(playerPart);
            message.values.Add(pos.x);
            message.values.Add(pos.y);
            message.values.Add(pos.z);
            message.values.Add(rot.x);
            message.values.Add(rot.y);
            message.values.Add(rot.z);
            message.values.Add(rot.w);
            

            osc.OscPacketIO.RemoteHostName = targetEndPoint.ip;
            osc.OscPacketIO.RemotePort = targetEndPoint.remotePort;
            osc.Send(message);
            if(gameEngine.debugMode) Debug.Log("Sending : " + message);
            
        }
    }


    public void SendUserDataToAudioSystem(UserData userData, OSCEndPoint audioHandlerEndPoint){
        
        Vector3 pos = new Vector3();
        Quaternion rot = new Quaternion();

        if (osc.initialized)
        {   
            
            osc.OscPacketIO.RemoteHostName = audioHandlerEndPoint.ip;
            osc.OscPacketIO.RemotePort = audioHandlerEndPoint.remotePort;

            for(int i = 0; i < 3; i++){

                message = new OscMessage();
                message.address =  "/AudioData";

                if(i == 0){ 
                    pos = userData.head.transform.position;
                    rot = userData.head.transform.rotation;
                }
                if(i == 1){ 
                    pos = userData.leftHand.transform.position;
                    rot = userData.leftHand.transform.rotation;
                }
                if(i == 2){ 
                    pos = userData.rightHand.transform.position;
                    rot = userData.rightHand.transform.rotation;
                }

                //message.values.Add(userData._ID);
                message.values.Add(userData._registeredRank);
                message.values.Add(i);
                message.values.Add(pos.x);
                message.values.Add(pos.y);
                message.values.Add(pos.z);
                message.values.Add(rot.x);
                message.values.Add(rot.y);
                message.values.Add(rot.z);
                message.values.Add(rot.w);

                osc.Send(message);
                if(gameEngine.debugMode) Debug.Log("Sending : " + message);
            }
        }
    }


    // after registration confirmation server sends every existing player to client
    public void AddEveryPlayerToClientDict(UserData userTarget, List<UserData> usersPlaying)
    {
        foreach (UserData user in usersPlaying)
        {
            if(user._userRole == UserRole.Player){
                message = new OscMessage();
                message.address = "/AddPlayerToGame";
                message.values.Add(user._ID);
                if (user._userRole == UserRole.Player) message.values.Add(1);
                else if (user._userRole == UserRole.Tracker) message.values.Add(2);
                else if (user._userRole == UserRole.Viewer) message.values.Add(0);

                message.values.Add(user._playerName);
                message.values.Add(user._registeredRank);
                osc.OscPacketIO.RemoteHostName = userTarget.oscEndPoint.ip;
                osc.OscPacketIO.RemotePort = userTarget.oscEndPoint.remotePort;
                osc.Send(message);
                if(gameEngine.debugMode) Debug.Log("Sending : " + message);
            }
        }
          
    }

    // sent to all existing client when a new one is registering
    public void AddNewPlayerToClientsGames(int playerID, string name, List<UserData> usersPlaying, int isPlayer, int rank){
        
        foreach(UserData user in usersPlaying)
        {
            if(user._ID != gameEngine._user._ID){ // don't send to itself
                message = new OscMessage();
                message.address = "/AddPlayerToGame";
                message.values.Add(playerID);
                message.values.Add(isPlayer);
                message.values.Add(name);
                message.values.Add(rank);

                osc.OscPacketIO.RemoteHostName = user.oscEndPoint.ip;
                osc.OscPacketIO.RemotePort = user.oscEndPoint.remotePort;
                osc.Send(message);
                if(gameEngine.debugMode) Debug.Log("Sending : " + message);
            }
        }
    }


    // triggered by server when it gets the information that one user has left
    public void RemovePlayerInClientsGame(int playerID, List<UserData> usersPlaying)
    {
        foreach (UserData user in usersPlaying)
        {
            if(user._ID != playerID){
                message = new OscMessage();
                message.address = "/RemovePlayerFromGame";
                message.values.Add(playerID);
                osc.OscPacketIO.RemoteHostName = user.oscEndPoint.ip;
                osc.OscPacketIO.RemotePort = user.oscEndPoint.remotePort;
                osc.Send(message);
                if(gameEngine.debugMode) Debug.Log("Sending : " + message);
            }  
        }
    }

    public void SendVisualisationChange(string mode, List<UserData> usersPlaying){

        foreach (UserData user in usersPlaying)
        {
            if(user._ID != gameEngine._user._ID){
                osc.OscPacketIO.RemoteHostName = user.oscEndPoint.ip;
                osc.OscPacketIO.RemotePort = user.oscEndPoint.remotePort;
                message = new OscMessage();
                message.address = "/VisualisationModeChange";
                message.values.Add(mode);
                message.values.Add(gameEngine.scenarioEvents.scenarios[gameEngine.scenarioEvents.currentScenario].toFade);
                osc.Send(message);
                if(gameEngine.debugMode) Debug.Log("Sending : " + message +" to "+user._ID);
            }  
        }

    }

    public void SendTrailValueChange(int valueId, float value, List<UserData> usersPlaying){
        
        foreach (UserData user in usersPlaying)
        {
            if(user._ID != gameEngine._user._ID){
                osc.OscPacketIO.RemoteHostName = user.oscEndPoint.ip;
                osc.OscPacketIO.RemotePort = user.oscEndPoint.remotePort;
                message = new OscMessage();
                message.address = "/TrailsParameterChange";
                message.values.Add(valueId);
                message.values.Add(value);
                osc.Send(message);
                Debug.Log("Sending : " + message);
            }  
        }
    }

    public void SendCalibrationInfo(UserData targetUser, List<UserData> usersPlaying){

        foreach (UserData userToSend in usersPlaying)
        {
            osc.OscPacketIO.RemoteHostName = targetUser.oscEndPoint.ip;
            osc.OscPacketIO.RemotePort = targetUser.oscEndPoint.remotePort;
            message = new OscMessage();
            message.address = "/CalibrationChange";
            message.values.Add(userToSend._ID);
            message.values.Add(userToSend.calibrationPositionGap.x);
            //message.values.Add(userToSend.calibrationPositionGap.y);
            message.values.Add(userToSend.calibrationPositionGap.z);
            osc.Send(message);
            Debug.Log("Sending : " + message);  
        }

    }


    public void StartAudioRecording(int postScenarioRecordingLenght, List<UserData> usersPlaying){

        foreach (UserData targetUser in usersPlaying)
        {
            
            osc.OscPacketIO.RemoteHostName = targetUser.oscEndPoint.ip;
            osc.OscPacketIO.RemotePort = targetUser.oscEndPoint.remotePort;
            message = new OscMessage();
            message.address = "/StartAudioRecording";
            message.values.Add(postScenarioRecordingLenght);
            osc.Send(message);
            Debug.Log("Sending : " + message);  
        }

    }

/*
    -------------------------------------
    -----------CLIENT FUNCTIONS----------
    -------------------------------------
 */

    public void RequestUserRegistation(UserData userData, UserRole userRole)
    {
        int isPlayer = 0; // 0 is always viewer
        if(userRole == UserRole.Player) isPlayer = 1;
        if (userRole == UserRole.Tracker) isPlayer = 2;

        if (osc.initialized)
        {
            message = new OscMessage();
            message.address = "/PlayerRegistrationRequest";
            message.values.Add(userData._ID);
            message.values.Add(gameEngine.osc.inPort);
            message.values.Add(gameEngine.gameData.OSC_LocalIP);
            message.values.Add(isPlayer);
            message.values.Add(userData._playerName);

            osc.OscPacketIO.RemoteHostName = userData.oscEndPoint.ip;
            osc.OscPacketIO.RemotePort = userData.oscEndPoint.remotePort;
            osc.Send(message);
            if(gameEngine.debugMode) Debug.Log("Sending : " + message);
        }
    }


    public void SendClientOSCPosition(UserData userData, int playerPart)
    {
        Vector3 pos = new Vector3();
        Quaternion rot = new Quaternion();
        
        if (osc.initialized)
        {   
            message = new OscMessage();
            message.address =  "/ClientPlayerData";
            message.values.Add(userData._ID);

            if(playerPart == 0){ 
                pos = userData.head.transform.position - userData.calibrationPositionGap; // send position without taking care of gap
                rot = userData.head.transform.rotation;
            }
            if(playerPart == 1){ 
                pos = userData.leftHand.transform.position - userData.calibrationPositionGap;
                rot = userData.leftHand.transform.rotation;
            }
            if(playerPart == 2){ 
                pos = userData.rightHand.transform.position - userData.calibrationPositionGap;
                rot = userData.rightHand.transform.rotation;
            }

            message.values.Add(playerPart);
            message.values.Add(pos.x);
            message.values.Add(pos.y);
            message.values.Add(pos.z);
            message.values.Add(rot.x);
            message.values.Add(rot.y);
            message.values.Add(rot.z);
            message.values.Add(rot.w);

            osc.OscPacketIO.RemoteHostName = gameEngine.osc.outIP;
            osc.OscPacketIO.RemotePort = gameEngine.osc.outPort;
            osc.Send(message);
            if(gameEngine.debugMode) Debug.Log("Sending : "+message+", "+gameEngine.osc.outIP);
            
        }
    }

    public void ToggleMirrorFromServer(UserData user)
    {
        if (osc.initialized)
        {
            message = new OscMessage();
            message.address = "/MirrorToggle";
            osc.OscPacketIO.RemoteHostName = user.oscEndPoint.ip;
            osc.OscPacketIO.RemotePort = user.oscEndPoint.remotePort;
            osc.Send(message);
        }

    }

    public void ChangeClientsEnvironment(UserData user, string newEnv)
    {
        if (osc.initialized)
        {
            message = new OscMessage();
            message.address = "/EnvironmentChange";
            message.values.Add(newEnv);
            osc.OscPacketIO.RemoteHostName = user.oscEndPoint.ip;
            osc.OscPacketIO.RemotePort = user.oscEndPoint.remotePort;
            osc.Send(message);
            if (gameEngine.debugMode) Debug.Log("Sending : " + message + ", " + gameEngine.osc.outIP);
        }
    }


    public void RecordAudioConfirmation(){
        if (osc.initialized)
        {
            message = new OscMessage();
            message.address = "/AudioRecordingConfirmation";
            osc.OscPacketIO.RemoteHostName = gameEngine.osc.outIP;
            osc.OscPacketIO.RemotePort = gameEngine.osc.outPort;
            osc.Send(message);
            if (gameEngine.debugMode) Debug.Log("Sending : " + message + ", " + gameEngine.osc.outIP);
        }
    }

    public void AudioRecordHasStopped(){
        if (osc.initialized)
        {
            message = new OscMessage();
            message.address = "/AudioRecordingStopped";
            osc.OscPacketIO.RemoteHostName = gameEngine.osc.outIP;
            osc.OscPacketIO.RemotePort = gameEngine.osc.outPort;
            osc.Send(message);
            if (gameEngine.debugMode) Debug.Log("Sending : " + message + ", " + gameEngine.osc.outIP);
        }
    }


    /*
        -------------------------------------
        -------------BOTH FUNCTIONS----------
        -------------------------------------
     */

    public void SendQuitMessage(UserRole userRole)
    {
        if (userRole == UserRole.Player || userRole == UserRole.Viewer || userRole == UserRole.Tracker)
        {
            osc.OscPacketIO.RemoteHostName = gameEngine.osc.outIP;
            osc.OscPacketIO.RemotePort = gameEngine.osc.outPort;
            message = new OscMessage();
            message.address = "/ClientHasLeft";
            message.values.Add(gameEngine._user._ID);
            osc.Send(message);
            if(gameEngine.debugMode) Debug.Log("Sending : " + message);
        }
        else if (userRole == UserRole.Server)
        {
            foreach(UserData user in userManager.usersPlaying){
                osc.OscPacketIO.RemoteHostName = user.oscEndPoint.ip;
                osc.OscPacketIO.RemotePort = user.oscEndPoint.remotePort;
                message = new OscMessage();
                message.address = "/ServerShutDown";
                osc.Send(message);
                if(gameEngine.debugMode) Debug.Log("Sending : " + message);
            }
        }

    }




}
