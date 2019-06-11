using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveOSC : MonoBehaviour {

    //public OSC osc;
    public GameEngine gameEngine;
    public UserManager userManager;
    public SendOSC sender;
    public OSC osc;
    public UserRole userRole;

    // Use this for initialization
    public void StartListening()
    {

        if (gameEngine._userRole == UserRole.Player || gameEngine._userRole == UserRole.Viewer || userRole == UserRole.Tracker)
        {
            osc.SetAddressHandler("/RegistrationConfirmed", RegistrationConfirmed);
            osc.SetAddressHandler("/ServerShutDown", GoInsane);
            osc.SetAddressHandler("/AddPlayerToGame", AddPlayerToGame);
            osc.SetAddressHandler("/RemovePlayerFromGame", RemovePlayerFromGame);
            osc.SetAddressHandler("/VisualisationModeChange", VisualisationModeChange);
            osc.SetAddressHandler("/PlayerData", UpdatePartnerPosition);
            osc.SetAddressHandler("/TrailsParameterChange", UpdateTrailsVisualisation);
            osc.SetAddressHandler("/EnvironmentChange", EnvironmentChangedByServer);
            osc.SetAddressHandler("/CalibrationChange", CalibrationChange);
            

            //osc.SetAddressHandler("/AudioData", DebugTemp); // debugtest

        }
        if (userRole == UserRole.Server)
        {
            osc.SetAddressHandler("/PlayerRegistrationRequest", RegistationRequestedFromPlayer);
            osc.SetAddressHandler("/ClientHasLeft", ErasePlayerRequest);
            osc.SetAddressHandler("/ClientPlayerData", UpdateClientPosition);
        }

    }
	
    void DebugTemp(OscMessage message){
        Debug.Log(message);
    }


/*
    -------------------------------------
    -----------SERVER FUNCTIONS----------
    -------------------------------------
 */


    // server side
    void RegistationRequestedFromPlayer(OscMessage message)
    {
        if(gameEngine.debugMode) Debug.Log("Received : " + message);

        if (gameEngine._userRole == UserRole.Server)
        {
            int playerID = message.GetInt(0);

            int requestedPort = message.GetInt(1);
            string playerIP = message.GetString(2);

            UserRole role;
            int isPlayer = message.GetInt(3);
            if (isPlayer == 1) role = UserRole.Player;
            else if (isPlayer == 2) role = UserRole.Tracker;
            else role = UserRole.Viewer;

            string playerName = message.GetString(4);

            bool portAvailable = gameEngine.networkManager.CheckPortAvailability(userManager.usersPlaying, requestedPort);

            if (portAvailable || gameEngine.gameData.runInLocal == 0)
            {
                UserData user = userManager.AddNewUser(gameEngine, playerID, playerName, playerIP, requestedPort, role, userManager.usersPlaying.Count);
                if (role == UserRole.Player)
                {
                    sender.AddNewPlayerToClientsGames(playerID, playerName, userManager.usersPlaying, isPlayer, userManager.usersPlaying.Count - 1); // minus1 because server had already added user in list
                    sender.SendCalibrationInfo(user, userManager.usersPlaying);
                }
                sender.SendRegistrationConfirmation(user);
            }
            else sender.RefuseRegistration(playerIP, requestedPort);
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
        float xRot = message.GetFloat(5);
        float yRot = message.GetFloat(6);
        float zRot = message.GetFloat(7);
        float wRot = message.GetFloat(8);

        if (playerID != gameEngine._user._ID)
        {
            userManager.pendingPositionsActualizations[playerID + playerPart] = new Vector3(xPos, yPos, zPos);
            userManager.pendingRotationsActualizations[playerID + playerPart] = new Quaternion(xRot, yRot, zRot, wRot);
        }
    }

    void ErasePlayerRequest(OscMessage message)
    {
        if(gameEngine.debugMode) Debug.Log("Received : " + message);
        int playerID = message.GetInt(0);
        userManager.ErasePlayer(playerID);
        osc.sender.RemovePlayerInClientsGame(playerID, userManager.usersPlaying);
    }


/*
    -------------------------------------
    -----------CLIENT FUNCTIONS----------
    -------------------------------------
 */

    void RegistrationConfirmed(OscMessage message)
    {
        if(message.GetInt(0) == gameEngine._user._ID && gameEngine.appState == AppState.WaitingForServer){
            if(gameEngine.debugMode) Debug.Log("Received : " + message);
            int playerID = message.GetInt(0);
            int requestedPort = message.GetInt(1);
            string visualisationMode = message.GetString(2);
            int rank = message.GetInt(3);
            gameEngine.EndStartProcess(playerID, requestedPort, visualisationMode, rank);
        }
    }

    // triggered for each osc position message received (3 per player)
    void UpdatePartnerPosition(OscMessage message)
    {
        if(gameEngine.debugMode) Debug.Log("Received : "+message);
        
        int playerID = message.GetInt(0);
        string playerPart="none";
        if(message.GetInt(1) == 0) playerPart = "Head";
        else if (message.GetInt(1) == 1) playerPart = "LeftHand";
        else if (message.GetInt(1) == 2) playerPart = "RightHand";
        float xPos = message.GetFloat(2);
        float yPos = message.GetFloat(3);
        float zPos = message.GetFloat(4);
        float xRot = message.GetFloat(5);
        float yRot = message.GetFloat(6);
        float zRot = message.GetFloat(7);
        float wRot = message.GetFloat(8);

        if (playerID != gameEngine._user._ID) // if is not me
        {
            userManager.pendingPositionsActualizations[playerID + playerPart] = new Vector3(xPos, yPos, zPos) + userManager.GetCalibrationGap(playerID);
            userManager.pendingRotationsActualizations[playerID + playerPart] = new Quaternion(xRot, yRot, zRot, wRot);
        }
    }

    
    void AddPlayerToGame(OscMessage message)
    {
        if(gameEngine.debugMode) Debug.Log("Received : " + message);
        int playerID = message.GetInt(0);

        UserRole userRole;
        int playerRole = message.GetInt(1);
        if (playerRole == 1) userRole = UserRole.Player;
        else if (playerRole == 2) userRole = UserRole.Tracker;
        else userRole = UserRole.Viewer;

        string playerName = message.GetString(2);
        int rank = message.GetInt(3);
        
        if ((gameEngine._userRole == UserRole.Player || gameEngine._userRole == UserRole.Viewer || gameEngine._userRole == UserRole.Tracker) && playerID != gameEngine._user._ID)
        {
            Debug.Log(playerID+" vs "+gameEngine._user._ID);
            userManager.AddNewUser(gameEngine, playerID, playerName, "null", -1, userRole, rank);
        }
    }

    // client side
    void RemovePlayerFromGame(OscMessage message)
    {
        if(gameEngine.debugMode) Debug.Log("Received : " + message);
        int playerID = message.GetInt(0);
        userManager.ErasePlayer(playerID);
    }

    void VisualisationModeChange(OscMessage message){
        Debug.Log("Received : " + message);
        string mode = message.GetString(0);

        userManager.ChangeVisualisationMode(mode, gameEngine, gameEngine.weFade);
    }

    void EnvironmentChangedByServer(OscMessage message)
    {
        if (gameEngine.debugMode) Debug.Log("Received : " + message);

        string envType = message.GetString(0);

        if (envType == "mirror") gameEngine.scenarioEvents.ToggleMirror();
        else if (envType == "sky") gameEngine.scenarioEvents.SetNextSkybox();
    }

    void CalibrationChange(OscMessage message){
        Debug.Log(message);
        int ID = message.GetInt(0);
        foreach(UserData user in userManager.usersPlaying){
            if(user._ID == ID){ 
                Vector3 calibVec = new Vector3(message.GetFloat(1), 0, message.GetFloat(2));
                user.calibrationPositionGap = calibVec;
                if(user._ID == userManager.me._ID) // if we receive our own gap, we translate the parent and not the childs
                    user.gameObject.transform.position += user.calibrationPositionGap;
                
                break;
            }
        }
    }

    // server has quit
    void GoInsane(OscMessage message)
    {
        Debug.Log("Received : " + message);
        Debug.Log("HAAAAAAAAAAAAAAA");
        gameEngine.KillApp();
    }



    void UpdateTrailsVisualisation(OscMessage message){
        //
        int valueId = message.GetInt(0);
        float value = message.GetFloat(1);
        userManager.ChangeVisualisationParameter(valueId, value);
    }

}
