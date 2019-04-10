using System.Collections;
using System.Net;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SendOSC : MonoBehaviour {

    public GameEngine gameEngine;
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
            message.values.Add(user._ID);
            message.values.Add(user.oscEndPoint.remotePort);
            message.values.Add(gameEngine.currentVisualisationMode);
            osc.OscPacketIO.RemoteHostName = user.oscEndPoint.ip;
            osc.OscPacketIO.RemotePort = user.oscEndPoint.remotePort;
            osc.Send(message);
            if(gameEngine.debugMode) Debug.Log("Sending : " + message);

            AddEveryPlayerToClientDict(user, gameEngine.usersPlaying);
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
            message.address =  "/PlayerPosition";
            message.values.Add(userData._ID);

            if(playerPart == 0){ 
                pos = userData.head.transform.position;
                rot = userData.head.transform.rotation;
            }
            if(playerPart == 1){ 
                pos = userData.leftHand.transform.position;
                rot = userData.leftHand.transform.rotation;
            }
            if(playerPart == 2){ 
                pos = userData.rightHand.transform.position;
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
            

            osc.OscPacketIO.RemoteHostName = targetEndPoint.ip;
            osc.OscPacketIO.RemotePort = targetEndPoint.remotePort;
            osc.Send(message);
            if(gameEngine.debugMode) Debug.Log("Sending : " + message);
            
        }
    }


    // after registration confirmation server sends every existing player to client
    public void AddEveryPlayerToClientDict(UserData userTarget, List<UserData> usersPlaying)
    {
        foreach (UserData user in usersPlaying)
        {
            message = new OscMessage();
            message.address = "/AddPlayerToGame";
            message.values.Add(user._ID);
            message.values.Add(user._isPlayer);
            message.values.Add(user._playerName);
            osc.OscPacketIO.RemoteHostName = userTarget.oscEndPoint.ip;
            osc.OscPacketIO.RemotePort = userTarget.oscEndPoint.remotePort;
            osc.Send(message);
            if(gameEngine.debugMode) Debug.Log("Sending : " + message);
        }
          
    }

    // sent to all existing client when a new one is registering
    public void AddNewPlayerToClientsGames(int playerID, string name, List<UserData> usersPlaying, int isPlayer){
        
        foreach(UserData user in usersPlaying)
        {
            if(user._ID != gameEngine._user._ID){ // don't send to itself
                message = new OscMessage();
                message.address = "/AddPlayerToGame";
                message.values.Add(playerID);
                message.values.Add(isPlayer);
                message.values.Add(user._playerName);
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

    public void SendVisualisationChange(int mode, List<UserData> usersPlaying){

        foreach (UserData user in usersPlaying)
        {
            if(user._ID != gameEngine._user._ID){
                osc.OscPacketIO.RemoteHostName = user.oscEndPoint.ip;
                osc.OscPacketIO.RemotePort = user.oscEndPoint.remotePort;
                message = new OscMessage();
                message.address = "/VisualisationModeChange";
                message.values.Add(mode);
                osc.Send(message);
                Debug.Log("Sending : " + message);
            }  
        }

    }


/*
    -------------------------------------
    -----------CLIENT FUNCTIONS----------
    -------------------------------------
 */

    public void RequestUserRegistation(UserData userData, UserRole userRole)
    {
        int isPlayer = 0;
        if(userRole == UserRole.Player) isPlayer = 1; 

        if (osc.initialized)
        {
            message = new OscMessage();
            message.address = "/PlayerRegistrationRequest";
            message.values.Add(userData._ID);
            message.values.Add(gameEngine.osc.inPort);
            message.values.Add(Utils.GetLastIntFromIp(gameEngine.gameData.OSC_LocalIP));
            message.values.Add(isPlayer);
            message.values.Add(userData._playerName);

            osc.OscPacketIO.RemoteHostName = userData.oscEndPoint.ip;
            osc.OscPacketIO.RemotePort = userData.oscEndPoint.remotePort;
            osc.Send(message);
            if(gameEngine.debugMode) Debug.Log("Sending : " + message+ ", "+userData.oscEndPoint.remotePort+", "+userData.oscEndPoint.ip);
        }
    }


    public void SendClientOSCPosition(UserData userData, int playerPart)
    {
        Vector3 pos = new Vector3();
        Quaternion rot = new Quaternion();
        
        if (osc.initialized)
        {   
            message = new OscMessage();
            message.address =  "/ClientPlayerPosition";
            message.values.Add(userData._ID);

            if(playerPart == 0){ 
                pos = userData.head.transform.position;
                rot = userData.head.transform.rotation;
            }
            if(playerPart == 1){ 
                pos = userData.leftHand.transform.position;
                rot = userData.leftHand.transform.rotation;
            }
            if(playerPart == 2){ 
                pos = userData.rightHand.transform.position;
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

/*
    -------------------------------------
    -------------BOTH FUNCTIONS----------
    -------------------------------------
 */

    public void SendQuitMessage(UserNetworkType userNetworkType)
    {
        if (userNetworkType == UserNetworkType.Client)
        {
            osc.OscPacketIO.RemoteHostName = gameEngine.osc.outIP;
            osc.OscPacketIO.RemotePort = gameEngine.osc.outPort;
            message = new OscMessage();
            message.address = "/ClientHasLeft";
            message.values.Add(gameEngine._user._ID);
            osc.Send(message);
            if(gameEngine.debugMode) Debug.Log("Sending : " + message);
        }
        else if (userNetworkType == UserNetworkType.Server)
        {
            foreach(UserData user in gameEngine.usersPlaying){
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
