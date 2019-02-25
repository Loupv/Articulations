using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveOSC : MonoBehaviour {

    public OSC osc;
    public GameEngine gameEngine;

    // Use this for initialization
    public void StartListening () {
        osc.SetAddressHandler("/PlayerRegistrationRequest", RegisterPlayer);
        osc.SetAddressHandler("/RegistrationConfirmed", RegistrationConfirmed);
        osc.SetAddressHandler("/PlayerPosition", UpdatePartnerPosition);
        osc.SetAddressHandler("/ClientHasLeft", ErasePlayerRequest);
        osc.SetAddressHandler("/ServerShutDown", GoInsane);
        osc.SetAddressHandler("/AddPlayerFromServer", AddPlayerFromServer);
    }
	

    void RegisterPlayer(OscMessage message)
    {
        Debug.Log("Received : " + message);
        if (gameEngine.userNetworkType == UserNetworkType.Server)
        {
            int playerID = message.GetInt(0);
            gameEngine.AddPlayer(playerID, gameEngine.playerParent);
            osc.sender.ConfirmRegistration(playerID);
        }
    }



    void RegistrationConfirmed(OscMessage message)
    {
        Debug.Log("Received : " + message);
        if (gameEngine.userNetworkType == UserNetworkType.Client)
        {
            gameEngine.EndStartProcess();

        }
    }


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
            gameEngine.playersPositions[playerID + playerPart] = new Vector3(xPos, yPos, zPos);
    }


    void AddPlayerFromServer(OscMessage message)
    {
        Debug.Log("Received : " + message);
        if (gameEngine.userNetworkType == UserNetworkType.Client)
        {
            int playerID = message.GetInt(0);
            gameEngine.AddPlayer(playerID, gameEngine.playerParent);
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
