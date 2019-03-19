using System;
using System.Net;
using System.Collections.Generic;

using UnityEngine;

public class NetworkManager : MonoBehaviour
{

    public OSC osc;
 
    public string serverAddress;
    public Dictionary<int, UDPPacketIO> IpPairs;



    // Start is called before the first frame update
    void Start()
    {

    }


    


    public void SendPlayerPosition(UserData user, List<UserData> users)
    {
        int i = 0;
        foreach (UserData p in users)
        {

            if (user._ID == p._ID)
            { // if this is the actual instance's player
                Debug.Log("Sending local infos :" + users[i].playerGameObject.name);
                osc.sender.SendOSCPosition(p, 0);
                osc.sender.SendOSCPosition(p, 1);
                osc.sender.SendOSCPosition(p, 2);
            }

            i++;

        }
    }

    public bool CheckPortAvailability(int requestedPort)
    {
        foreach(IPEndPoint endPoint in IpPairs.Values)
        {
            if (endPoint.Port==requestedPort) return false;
        }
        return true;
    }


    public void FinishUserRegistration(int playerID, IPEndPoint endPoint)
    {
        IpPairs.Add(playerID, endPoint);
    }
}
