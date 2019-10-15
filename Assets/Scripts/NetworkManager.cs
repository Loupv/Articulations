using System;
using System.Net;
using System.Collections.Generic;

using UnityEngine.UI;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{

    public OSC osc;
    public GameEngine gameEngine;
    public string serverAddress;  
    public Image oscToggle;
    public OSCEndPoint serverEndpoint;
    public EyeswebOSC eyeswebOSC;
    public bool sendToAudioDevice, sendToEyesweb;


    public void InitNetwork(UserRole userRole, GameData gameData, string uiServerIP){

        serverEndpoint.ip = gameData.OSC_ServerIP;
        serverEndpoint.remotePort = gameData.OSC_ClientPort;

        int inPort;
        int outPort;

        if (userRole == UserRole.Server || (userRole == UserRole.Playback && gameEngine.playbackManager.mode == PlaybackMode.Offline))
        {
            inPort = gameData.OSC_ServerPort;
            outPort = gameData.OSC_ClientPort;
        }
        else
        {
            inPort = gameData.OSC_ClientPort;
            outPort = gameData.OSC_ServerPort;   
        }

        osc.inPort = inPort;
        osc.outPort = outPort;
        osc.outIP = uiServerIP;
        osc.Init();

        eyeswebOSC = GetComponentInChildren<EyeswebOSC>();
        if(userRole == UserRole.Server || userRole == UserRole.Playback) {
            eyeswebOSC.gameObject.SetActive(true);
            eyeswebOSC.Init(userRole);
        }
        else if(eyeswebOSC != null) eyeswebOSC.gameObject.SetActive(false); 
        

        print("OSC Connexion initiation to " + osc.outIP + " : " + osc.outPort + "/" + osc.inPort);
        
    }


    public void NetworkGameLoop(UserData _user, UserManager userManager, PlaybackManager playbackManager, SoundHandler soundHandler){
        if(_user._userRole == UserRole.Player || (_user._userRole == UserRole.Playback && playbackManager.mode == PlaybackMode.Online)){
            SendOwnPosition(_user); // don't send if you're viewer
        }
        else if(_user._userRole == UserRole.Server){
            SendAllPositionsToClients(userManager.usersPlaying);
            if(sendToAudioDevice) SendAllPositionsToAudioSystem(userManager.usersPlaying, soundHandler);
            if(sendToEyesweb && eyeswebOSC.initialized) eyeswebOSC.SendPositionsToEyesWeb();
        }
    }

    // client only
    public void SendOwnPosition(UserData user)
    {        
        osc.sender.SendClientOSCPosition(user, 0);
        osc.sender.SendClientOSCPosition(user, 1);
        osc.sender.SendClientOSCPosition(user, 2);
    }

    // server only
    public void SendAllPositionsToClients(List<UserData> usersPlaying)
    {
        foreach (UserData targetUser in usersPlaying)
        {
            if (targetUser._ID != gameEngine._user._ID) // if this is the actual instance's player
            { 
                foreach(UserData user in usersPlaying){      
                    if(user._userRole == UserRole.Player){  // don't send viewers positions
                        osc.sender.SendOSCPosition(user, 0, targetUser.oscEndPoint);
                        osc.sender.SendOSCPosition(user, 1, targetUser.oscEndPoint);
                        osc.sender.SendOSCPosition(user, 2, targetUser.oscEndPoint);
                    }
                }
            }
        }
    }

    public void SendAllPositionsToAudioSystem(List<UserData> usersPlaying, SoundHandler soundHandler)
    {
        foreach (UserData targetUser in usersPlaying)
        {
            if (targetUser._ID != gameEngine._user._ID) // if this is the actual instance's player
            { 
                foreach(UserData user in usersPlaying){      
                    if(user._userRole == UserRole.Player){  // don't send viewers positions
                        osc.sender.SendUserDataToAudioSystem(user, soundHandler.oscEndPoint);
                    }
                }
            }
        }
    }


    public void ShowConnexionState(){
        if (osc.initialized)
            oscToggle.color = new Color(0, 1, 0);
        else oscToggle.color = new Color(1, 0, 0);
    }


    public bool CheckPortAvailability(List<UserData> users, int requestedPort)
    {
        Debug.Log("Checking availability for :" +requestedPort);
        foreach(UserData user in users)
        {
            if (user.oscEndPoint.remotePort==requestedPort) return false;
        }
        return true;
    }

    public void RegisterUser(UserData _user, UserRole _userRole){
        osc.sender.RequestUserRegistation(_user, _userRole);
    }


    public void SendClientPositionGap()
    {
        foreach (UserData targetUser in gameEngine.userManager.usersPlaying) // we take each actual player one by one
        {
            osc.sender.SendCalibrationInfo(targetUser, gameEngine.userManager.usersPlaying); // we send for each of them the list of positiongaps
            // upon reception, each user has to adapt its own position to be centered
        }
    }

    public void EnvironmentChangeOrder(List<UserData> usersPlaying, string newEnv)
    {
        foreach (UserData user in usersPlaying)
            osc.sender.ChangeClientsEnvironment(user, newEnv);
    }


}
