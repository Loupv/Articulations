using System;
using System.Net;
using System.Collections.Generic;

using UnityEngine;

public class NetworkManager : MonoBehaviour
{

    public OSC osc;
    //public GameObject OSCPrefab, networkParent;
    public GameEngine gameEngine;

    public string serverAddress;
    public Dictionary<int, UDPPacketIO> IpPairs;



    // Start is called before the first frame update
    void Start()
    {
        //networkParent = this.gameObject;
        IpPairs = new Dictionary<int, UDPPacketIO>();
        osc = gameEngine.osc;
    }


    public bool StartServerListener(int localPort)
    {
        try
        {
            UDPPacketIO udpPacket = new UDPPacketIO(IPAddress.Any.ToString(), 0, localPort);
            //IpPairs.Add(user._ID, endPoint);
            IpPairs.Add(udpPacket);
            osc.receiver.StartListening(osc, UserNetworkType.Server);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("cannot create server listener hombre");
            Debug.LogWarning(e);
            return false;
        }

    }



    public bool AddNewPairingService(UserData user, int userPort, string address, UserNetworkType userNetworkType)
    {
        try
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(address), userPort);
            IpPairs.Add(user._ID, endPoint);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("cannot create endpoint hombre");
            Debug.LogWarning(e);
            return false;
        }

    }



    public void SendPlayerPosition(UserData user, List<UserData> users)
    {
        int i = 0;
        foreach (UserData p in users)
        {

            if (user._ID == p._ID)
            { // if this is the actual instance's player
                Debug.Log("Sending local infos :" + users[i].playerGameObject.name);
                gameEngine.osc.sender.SendOSCPosition(osc, IpPairs[user._ID], "/PlayerPosition", user._ID, 0, users[i].head.transform.position);
                gameEngine.osc.sender.SendOSCPosition(osc, IpPairs[user._ID], "/PlayerPosition", user._ID, 1, users[i].leftHand.transform.position);
                gameEngine.osc.sender.SendOSCPosition(osc, IpPairs[user._ID], "/PlayerPosition", user._ID, 2, users[i].rightHand.transform.position);
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
