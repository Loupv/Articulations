using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveOSC : MonoBehaviour {

    //public OSC osc;
    public GameEngine gameEngine;
    public OSC _osc;

    public ReceiveOSC(OSC osc)
    {
        _osc = osc;
        gameEngine = GameObject.Find("GameEngine").GetComponent<GameEngine>();
    }

    // Use this for initialization
    public void StartListening () {
        _osc.SetAddressHandler("/PlayerRegistrationRequest", RegistationRequestedFromPlayer);
        _osc.SetAddressHandler("/RegistrationConfirmed", RegistrationConfirmed);
        _osc.SetAddressHandler("/PlayerPosition", UpdatePartnerPosition);
        _osc.SetAddressHandler("/ClientHasLeft", ErasePlayerRequest);
        _osc.SetAddressHandler("/ServerShutDown", GoInsane);
        _osc.SetAddressHandler("/AddPlayerFromServer", AddPlayerFromServer);
    }
	
    // server side
    void RegistationRequestedFromPlayer(OscMessage message)
    {
        Debug.Log("Received : " + message);
        if (gameEngine.userNetworkType == UserNetworkType.Server)
        {
            int playerID = message.GetInt(0);
            int requestedPort = message.GetInt(1);

            bool portAvailable = gameEngine.networkManager.CheckPortAvailability(requestedPort);

            if (portAvailable)
            {
                gameEngine.AddOtherPlayer(playerID);
                _osc.sender.SendRegistrationConfirmation(playerID, requestedPort);
            }
            else _osc.sender.RefuseRegistration(requestedPort);
        }
    }


    // client side
    void RegistrationConfirmed(OscMessage message)
    {
        Debug.Log("Received : " + message);
        int playerID = message.GetInt(0);
        int requestedPort = message.GetInt(1);
        gameEngine.EndStartProcess(playerID, requestedPort);
    }



    // triggered for each osc position message received (3 per player)
    void UpdatePartnerPosition(OscMessage message)
    {
        int playerID = message.GetInt(0);

        string playerPart="none";
        if(message.GetInt(1) == 0) playerPart = "Head";
        else if (message.GetInt(1) == 1) playerPart = "LeftHand";
        else if (message.GetInt(1) == 2) playerPart = "RightHand";

        float xPos = message.GetFloat(2);
        float yPos = message.GetFloat(3);
        float zPos = message.GetFloat(4);

        if (playerID != gameEngine._user._ID)
        {
            //Debug.Log("Receiving " + playerID + playerPart + " : " + new Vector3(xPos, yPos, zPos));
            gameEngine.pendingPositionsActualizations[playerID + playerPart] = new Vector3(xPos, yPos, zPos);
        }
    }


    void AddPlayerFromServer(OscMessage message)
    {
        Debug.Log("Received : " + message);
        if (gameEngine.userNetworkType == UserNetworkType.Client)
        {
            int playerID = message.GetInt(0);
            gameEngine.AddOtherPlayer(playerID);
        }
    }

    void ErasePlayerRequest(OscMessage message)
    {
        Debug.Log("Received : " + message);
        int playerID = message.GetInt(0);
        gameEngine.ErasePlayer(playerID);
        // TODO something to suppress this instance
    }

    // server has quit
    void GoInsane(OscMessage message)
    {
        Debug.Log("Received : " + message);
        Debug.Log("HAAAAAAAAAAAAAAA");
        gameEngine.KillApp();
    }

}
