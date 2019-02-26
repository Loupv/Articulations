using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveOSC : MonoBehaviour {

    public OSC osc;
    public GameEngine gameEngine;

    // Use this for initialization
    public void StartListening () {
        osc.SetAddressHandler("/PlayerRegistrationRequest", AnswerToRegistationRequest);
        osc.SetAddressHandler("/RegistrationConfirmed", RegistrationConfirmed);
        osc.SetAddressHandler("/PlayerPosition", UpdatePartnerPosition);
        osc.SetAddressHandler("/ClientHasLeft", ErasePlayerRequest);
        osc.SetAddressHandler("/ServerShutDown", GoInsane);
        osc.SetAddressHandler("/AddPlayerFromServer", AddPlayerFromServer);
    }
	
    // server side
    void AnswerToRegistationRequest(OscMessage message)
    {
        Debug.Log("Received : " + message);
        if (gameEngine.userNetworkType == UserNetworkType.Server)
        {
            int playerID = message.GetInt(0);
            gameEngine.AddOtherPlayer(playerID, gameEngine.playerParent);
            osc.sender.ConfirmRegistration(playerID);
        }
    }


    // client side
    void RegistrationConfirmed(OscMessage message)
    {
        Debug.Log("Received : " + message);
        if (gameEngine.userNetworkType == UserNetworkType.Client)
        {
            gameEngine.EndStartProcess();

        }
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

        if (playerID != gameEngine._playerID)
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
            gameEngine.AddOtherPlayer(playerID, gameEngine.playerParent);
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
