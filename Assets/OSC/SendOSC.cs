using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendOSC : MonoBehaviour {

    public OSC _osc;
    public GameEngine gameEngine;
    private OscMessage message;

    public SendOSC(OSC osc)
    {
        _osc = osc;
        gameEngine = GameObject.Find("GameEngine").GetComponent<GameEngine>();
    }

    public void RequestUserRegistation(int playerID, int requestedPort)
    {
        if (_osc.initialized)
        {
            message = new OscMessage();
            message.address = "/PlayerRegistrationRequest";
            message.values.Add(playerID);
            message.values.Add(requestedPort);
            _osc.Send(message);
            Debug.Log("Sending : " + message);
        }
    }

    public void SendRegistrationConfirmation(int playerID, int requestedPort)
    {
        if (_osc.initialized)
        {
            message = new OscMessage();
            message.address = "/RegistrationConfirmed";
            message.values.Add(playerID);
            message.values.Add(requestedPort);
            _osc.Send(message);
            Debug.Log("Sending : " + message);

            AddEveryPlayerToClientDict();
        }
    }

    public void RefuseRegistration(int requestedPort)
    {
        if (_osc.initialized)
        {
            message = new OscMessage();
            message.address = "/RegistrationRefused";
            message.values.Add(requestedPort);
            _osc.Send(message);
            Debug.Log("Sending : " + message);
        }
    }



    public void SendOSCPosition(string header, int playerID, int playerPart, Vector3 pos)
    {
        if (_osc.initialized)
        {   
            message = new OscMessage();
            message.address = header;
            message.values.Add(playerID);
            message.values.Add(playerPart);
            message.values.Add(pos.x);
            message.values.Add(pos.y);
            message.values.Add(pos.z);

            _osc.Send(message);
            //Debug.Log("Sending : " + message);
        }
    }





    public void SendCustomOSCMessage(string header)
    {
        if (_osc.initialized)
        {
            message = new OscMessage();
            message.address = header;
            _osc.Send(message);
            Debug.Log("Sending : " + message);
        }
    }


    public void SendQuitMessage(UserNetworkType userNetworkType, UserData player)
    {
        if(userNetworkType == UserNetworkType.Client)
        {
            message = new OscMessage();
            message.address = "/ClientHasLeft";
            message.values.Add(player._ID);
            _osc.Send(message);
            Debug.Log("Sending : " + message);
        }
        else if (userNetworkType == UserNetworkType.Server)
        {
            message = new OscMessage();
            message.address = "/ServerShutDown";
            _osc.Send(message);
            Debug.Log("Sending : " + message);
        }

    }


    public void AddEveryPlayerToClientDict()
    {
        foreach (int playerID in gameEngine.IDsList)
        {
                message = new OscMessage();
                message.address = "/AddPlayerFromServer";
                message.values.Add(playerID);
                _osc.Send(message);
                Debug.Log("Sending : " + message);
        }
          
    }

}
