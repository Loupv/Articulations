using System.Collections;
using System.Net;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SendOSC : MonoBehaviour {

    public GameEngine gameEngine;
    private OscMessage message;
    public OSC osc;

    public void RequestUserRegistation(UserData userData)
    {
        if (osc.initialized)
        {
            message = new OscMessage();
            message.address = "/PlayerRegistrationRequest";
            message.values.Add(userData._ID);
            message.values.Add(userData.oscEndPoint.remotePort);
            
            osc.OscPacketIO.RemoteHostName = userData.oscEndPoint.ip;
            osc.OscPacketIO.RemotePort = endPoint.Port;
            osc.Send(message);
            Debug.Log("Sending : " + message);
        }
    }

    public void SendRegistrationConfirmation(int playerID, int requestedPort)
    {
        if (osc.initialized)
        {
            message = new OscMessage();
            message.address = "/RegistrationConfirmed";
            message.values.Add(playerID);
            message.values.Add(requestedPort);
            osc.OscPacketIO.RemoteHostName = endPoint.Address.ToString();
            osc.OscPacketIO.RemotePort = endPoint.Port;
            osc.Send(message);
            Debug.Log("Sending : " + message);

            AddEveryPlayerToClientDict(osc, endPoint);
        }
    }

    public void RefuseRegistration(int requestedPort)
    {
        if (osc.initialized)
        {
            message = new OscMessage();
            message.address = "/RegistrationRefused";
            message.values.Add(requestedPort);
            osc.OscPacketIO.RemoteHostName = endPoint.Address.ToString();
            osc.OscPacketIO.RemotePort = endPoint.Port;
            osc.Send(message);
            Debug.Log("Sending : " + message);
        }
    }



    public void SendOSCPosition(UserData userData, int playerPart)
    {
        Vector3 pos;

        if (osc.initialized)
        {   
            message = new OscMessage();
            message.address =  "/PlayerPosition";
            message.values.Add(userData.playerID);
            if(playerPart == 0) pos = userData.head;
            if(playerPart == 1) pos = userData.leftHand;
            if(playerPart == 2) pos = userData.rightHand;
            message.values.Add(playerPart);
            message.values.Add(pos.x);
            message.values.Add(pos.y);
            message.values.Add(pos.z);

            osc.OscPacketIO.RemoteHostName = endPoint.Address.ToString();
            osc.OscPacketIO.RemotePort = endPoint.Port;
            osc.Send(message);

            Debug.Log("Sending position : " + osc.OscPacketIO.IsOpen() + ", " + osc.localPort+", "+ osc.remotePort+", "+osc.outIP);
        }
    }





    public void SendCustomOSCMessage(string header)
    {
        if (osc.initialized)
        {
            message = new OscMessage();
            message.address = header;
            osc.OscPacketIO.RemoteHostName = endPoint.Address.ToString();
            osc.OscPacketIO.RemotePort = endPoint.Port;
            osc.Send(message);
            Debug.Log("Sending : " + message);
        }
    }


    public void SendQuitMessage(UserNetworkType userNetworkType, UserData player)
    {
        osc.OscPacketIO.RemoteHostName = endPoint.Address.ToString();
        osc.OscPacketIO.RemotePort = endPoint.Port;

        if (userNetworkType == UserNetworkType.Client)
        {
            message = new OscMessage();
            message.address = "/ClientHasLeft";
            message.values.Add(player._ID);
            osc.Send(message);
            Debug.Log("Sending : " + message);
        }
        else if (userNetworkType == UserNetworkType.Server)
        {
            message = new OscMessage();
            message.address = "/ServerShutDown";
            osc.Send(message);
            Debug.Log("Sending : " + message);
        }

    }


    public void AddEveryPlayerToClientDict(UserData userData)
    {
        foreach (int playerID in gameEngine.IDsList)
        {
            message = new OscMessage();
            message.address = "/AddPlayerFromServer";
            message.values.Add(playerID);
            osc.OscPacketIO.RemoteHostName = userData.oscEndPoint.Address.ToString();
            osc.OscPacketIO.RemotePort = userData.oscEndPoint.Port;
            osc.Send(message);
            Debug.Log("Sending : " + message);
        }
          
    }

}
