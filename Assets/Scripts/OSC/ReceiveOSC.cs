using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveOSC : MonoBehaviour {

    //public OSC osc;
    public GameEngine gameEngine;
    public SendOSC sender;
    public OSC osc;
    public UserNetworkType userNetworkType;

    // Use this for initialization
    public void StartListening()
    {

        if (userNetworkType == UserNetworkType.Client)
        {
            osc.SetAddressHandler("/RegistrationConfirmed", RegistrationConfirmed);
            osc.SetAddressHandler("/ServerShutDown", GoInsane);
            osc.SetAddressHandler("/AddPlayerToGame", AddPlayerToGame);
            osc.SetAddressHandler("/RemovePlayerFromGame", RemovePlayerFromGame);
            
            osc.SetAddressHandler("/PlayerPosition", UpdatePartnerPosition);

        }
        if (userNetworkType == UserNetworkType.Server)
        {
            osc.SetAddressHandler("/PlayerRegistrationRequest", RegistationRequestedFromPlayer);
            osc.SetAddressHandler("/ClientHasLeft", ErasePlayerRequest);
            osc.SetAddressHandler("/ClientPlayerPosition", UpdateClientPosition);
        }

    }
	
    // server side
    void RegistationRequestedFromPlayer(OscMessage message)
    {
        if(gameEngine.debugMode) Debug.Log("Received : " + message);

        if (gameEngine.userNetworkType == UserNetworkType.Server)
        {
            int playerID = message.GetInt(0);

            int requestedPort = message.GetInt(1);
            string playerIP = gameEngine.GetIpFromInt(message.GetInt(2));

            bool portAvailable = gameEngine.networkManager.CheckPortAvailability(gameEngine.usersPlaying, requestedPort);

            if (portAvailable)
            {
                UserData user = gameEngine.AddOtherPlayer(playerID, playerIP, requestedPort);
                sender.AddNewPlayerToClientsGames(playerID, gameEngine.usersPlaying);
                sender.SendRegistrationConfirmation(user);
            }
            else sender.RefuseRegistration(playerIP, requestedPort);
        }
    }


    // client side
    void RegistrationConfirmed(OscMessage message)
    {
        if(message.GetInt(0) == gameEngine._user._ID && gameEngine.appState == AppState.WaitingForServer){
            if(gameEngine.debugMode) Debug.Log("Received : " + message);
            int playerID = message.GetInt(0);
            int requestedPort = message.GetInt(1);
            gameEngine.EndStartProcess(playerID, requestedPort);
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

        if (playerID != gameEngine._user._ID)
        {
            //Debug.Log("Receiving " + playerID + playerPart + " : " + new Vector3(xPos, yPos, zPos));
            gameEngine.pendingPositionsActualizations[playerID + playerPart] = new Vector3(xPos, yPos, zPos);
        }
    }

    void UpdateClientPosition(OscMessage message)
    {
        int playerID = message.GetInt(0);
        if(gameEngine.debugMode) Debug.Log("Received : " + message);
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


    void AddPlayerToGame(OscMessage message)
    {
        if(gameEngine.debugMode) Debug.Log("Received : " + message);
        int playerID = message.GetInt(0);
        //int port = message.GetInt(1);
        if (gameEngine.userNetworkType == UserNetworkType.Client && playerID != gameEngine._user._ID)
        {
            Debug.Log(playerID+" vs "+gameEngine._user._ID);
            gameEngine.AddOtherPlayer(playerID, "null", -1);
        }
    }

    // client side
    void RemovePlayerFromGame(OscMessage message)
    {
        if(gameEngine.debugMode) Debug.Log("Received : " + message);
        int playerID = message.GetInt(0);
        gameEngine.ErasePlayer(playerID);
    }

    // server side
    void ErasePlayerRequest(OscMessage message)
    {
        if(gameEngine.debugMode) Debug.Log("Received : " + message);
        int playerID = message.GetInt(0);
        gameEngine.ErasePlayer(playerID);
        osc.sender.RemovePlayerInClientsGame(playerID, gameEngine.usersPlaying);
    }

    // server has quit
    void GoInsane(OscMessage message)
    {
        Debug.Log("Received : " + message);
        Debug.Log("HAAAAAAAAAAAAAAA");
        gameEngine.KillApp();
    }

}
