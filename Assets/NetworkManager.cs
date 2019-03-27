using System;
using System.Net;
using System.Collections.Generic;

using UnityEngine;

public class NetworkManager : MonoBehaviour
{

    public OSC osc;
    public GameEngine gameEngine;
    public string serverAddress;
    public Dictionary<int, OSCEndPoint> IpPairs;    

    // client only
    public void SendOwnPosition(UserData user, OSCEndPoint serverEndPoint)
    {        
        osc.sender.SendClientOSCPosition(user, 0);
        osc.sender.SendClientOSCPosition(user, 1);
        osc.sender.SendClientOSCPosition(user, 2);
    }

    // server only
    public void SendAllPositionsToClients(List<UserData> users)
    {
        //int i = 0;
        foreach (UserData targetUser in users)
        {
            if (targetUser._ID != gameEngine._user._ID)
            { // if this is the actual instance's player
                foreach(UserData user in users){              
                    osc.sender.SendOSCPosition(user, 0, targetUser.oscEndPoint);
                    osc.sender.SendOSCPosition(user, 1, targetUser.oscEndPoint);
                    osc.sender.SendOSCPosition(user, 2, targetUser.oscEndPoint);
                }
            }

           // i++;

        }
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


    /* public void FinishUserRegistration(int playerID, IPEndPoint endPoint)
    {
        IpPairs.Add(playerID, endPoint);
    }*/
}
