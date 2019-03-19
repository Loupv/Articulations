using System.Collections;
using System.Net;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SendOSC : MonoBehaviour {

    public GameEngine gameEngine;
    private OscMessage message;

    public void RequestUserRegistation(OSC osc, int playerID, int requestedPort)
    {
        if (osc.initialized)
        {
            message = new OscMessage();
            message.address = "/PlayerRegistrationRequest";
            message.values.Add(playerID);
            message.values.Add(requestedPort);
            osc.OscPacketIO.RemoteHostName = endPoint.Address.ToString();
            osc.OscPacketIO.RemotePort = endPoint.Port;
            osc.Send(message);
            Debug.Log("Sending : " + message);
        }
    }

    public void SendRegistrationConfirmation(OSC osc, IPEndPoint endPoint, int playerID, int requestedPort)
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

    public void RefuseRegistration(OSC osc, IPEndPoint endPoint, int requestedPort)
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



    public void SendOSCPosition(OSC osc, IPEndPoint endPoint, string header, int playerID, int playerPart, Vector3 pos)
    {
        if (osc.initialized)
        {   
            message = new OscMessage();
            message.address = header;
            message.values.Add(playerID);
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





    public void SendCustomOSCMessage(OSC osc, IPEndPoint endPoint, string header)
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


    public void SendQuitMessage(OSC osc, IPEndPoint endPoint, UserNetworkType userNetworkType, UserData player)
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


    public void AddEveryPlayerToClientDict(OSC osc, IPEndPoint endPoint)
    {
        foreach (int playerID in gameEngine.IDsList)
        {
            message = new OscMessage();
            message.address = "/AddPlayerFromServer";
            message.values.Add(playerID);
            osc.OscPacketIO.RemoteHostName = endPoint.Address.ToString();
            osc.OscPacketIO.RemotePort = endPoint.Port;
            osc.Send(message);
            Debug.Log("Sending : " + message);
        }
          
    }

}
