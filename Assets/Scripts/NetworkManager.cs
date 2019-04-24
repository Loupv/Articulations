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

    // client only
    public void SendOwnPosition(UserData user, OSCEndPoint serverEndPoint)
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

}
