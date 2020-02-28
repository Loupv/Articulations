using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveOSC : MonoBehaviour {

    //public OSC osc;
    public GameEngine gameEngine;
    public UserManager userManager;
    public SendOSC sender;
    public OSC osc;
    int clientAnswersPending;

    // Use this for initialization
    public void StartListening()
    {

        if (gameEngine._userRole == UserRole.Player || gameEngine._userRole == UserRole.Viewer || gameEngine._userRole == UserRole.Playback || gameEngine._userRole == UserRole.Tracker)
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
            osc.SetAddressHandler("/StartAudioRecording", StartAudioRecording);
            osc.SetAddressHandler("/StopAudioRecording", StopAudioRecording);
            osc.SetAddressHandler("/StartPlayback", StartPlaybackOrder);
            osc.SetAddressHandler("/PausePlayback", PausePlaybackOrder);
            osc.SetAddressHandler("/StopPlayback", StopPlaybackOrder);

            //osc.SetAddressHandler("/AudioData", DebugTemp); // debugtest

        }
        if (gameEngine._userRole == UserRole.Server)
        {
            osc.SetAddressHandler("/PlayerRegistrationRequest", RegistationRequestedFromPlayer);
            osc.SetAddressHandler("/ClientHasLeft", ErasePlayerRequest);
            osc.SetAddressHandler("/ClientPlayerData", UpdateClientPosition);
            osc.SetAddressHandler("/AudioRecordingConfirmation", AudioRecordingConfirmed);
            osc.SetAddressHandler("/AudioRecordingStopped", AudioRecordingStopped);
        }

        osc.SetAddressHandler("/wek/outputs", WekinatorInfosReceived);

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

            bool available = gameEngine.networkManager.CheckAvailability(userManager, requestedPort, playerIP, gameEngine.gameData.runInLocal);

            if (available)
            {
                UserData user = userManager.AddNewUser(gameEngine, playerID, playerName, playerIP, requestedPort, role);
                if (role == UserRole.Player)
                {
                    sender.AddNewPlayerToClientsGames(playerID, playerName, userManager.usersPlaying, isPlayer, userManager.usersPlaying.Count - 1); // minus1 because server had already added user in list
                    sender.SendCalibrationInfo(user, userManager.usersPlaying);
                    if(gameEngine.networkManager.sendToAudioDevice) sender.InitGenerativeAudioSystem(user, user._registeredRank);

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

    void AudioRecordingConfirmed(OscMessage message){
        if(gameEngine.debugMode) Debug.Log("Received : " + message);
        clientAnswersPending ++;
        if(clientAnswersPending == gameEngine.userManager.CountPlayers()){
            Debug.Log("Everyone is recording");
            gameEngine.uiHandler.CountRecordTimeRemaining();
            clientAnswersPending = 0;
        }
    }

    void AudioRecordingStopped(OscMessage message){
        if(gameEngine.debugMode) Debug.Log("Received : " + message);
        clientAnswersPending ++;
        if(clientAnswersPending == gameEngine.userManager.CountPlayers()){
            Debug.Log("All recordings are stopped");
            gameEngine.uiHandler.CancelRecordTime();
            clientAnswersPending = 0;
            gameEngine.uiHandler.ActualizeGizmos(false, false);
        }
        if(gameEngine.uiHandler.autoRecordPerformance.isOn && gameEngine.uiHandler.recordAudioAfterScenario.isOn) 
            gameEngine.scenarioEvents.performanceRecorder.StopRecording();
        
    }

/*
    -------------------------------------
    -----------CLIENT FUNCTIONS----------
    -------------------------------------
 */

    void RegistrationConfirmed(OscMessage message)
    {
        int playerID = message.GetInt(1);

        if(playerID == gameEngine._user._ID && gameEngine.appState == AppState.WaitingForServer){
            if(gameEngine.debugMode) Debug.Log("Received : " + message);
            int sessionID = message.GetInt(0);
            int requestedPort = message.GetInt(2);
            string visualisationMode = message.GetString(3);
            int rank = message.GetInt(4);
            int recordAudio = message.GetInt(5);
            int recordLength = message.GetInt(6);
            gameEngine.EndStartProcess(sessionID, playerID, requestedPort, visualisationMode, rank, recordAudio, recordLength);
        }
    }

    // triggered for each osc position message received (3 per player)
    void UpdatePartnerPosition(OscMessage message)
    {
        if(gameEngine.debugMode) Debug.Log("Received : "+message);
        
        int playerID = message.GetInt(0);
        int registeredRank = message.GetInt(1);
        string playerPart="none";
        if(message.GetInt(2) == 0) playerPart = "Head";
        else if (message.GetInt(2) == 1) playerPart = "LeftHand";
        else if (message.GetInt(2) == 2) playerPart = "RightHand";
        float xPos = message.GetFloat(3);
        float yPos = message.GetFloat(4);
        float zPos = message.GetFloat(5);
        float xRot = message.GetFloat(6);
        float yRot = message.GetFloat(7);
        float zRot = message.GetFloat(8);
        float wRot = message.GetFloat(9);

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
            userManager.AddNewUser(gameEngine, playerID, playerName, "null", -1, userRole);
            gameEngine.userManager.ChangeVisualisationMode(gameEngine.currentVisualisationMode, gameEngine, false); // trigger the change mode again, to actualize to new players
        }
    }


    void RemovePlayerFromGame(OscMessage message)
    {
        if(gameEngine.debugMode) Debug.Log("Received : " + message);
        int playerID = message.GetInt(0);
        userManager.ErasePlayer(playerID);
    }

    void VisualisationModeChange(OscMessage message){
        Debug.Log("Received : " + message);
        string mode = message.GetString(0);
        int toFade = message.GetInt(1);
        userManager.ChangeVisualisationMode(mode, gameEngine, toFade == 1);
    }

    void EnvironmentChangedByServer(OscMessage message)
    {
        if (gameEngine.debugMode) Debug.Log("Received : " + message);

        string envType = message.GetString(0);
        int val = message.GetInt(1);

        if (envType == "mirror") gameEngine.scenarioEvents.ToggleMirror(val == 1);
        else if (envType == "sky") gameEngine.scenarioEvents.SetSkybox(val);
        else if (envType == "naoto") gameEngine.scenarioEvents.ToggleNaoto(val == 1);
    }

    void CalibrationChange(OscMessage message){
        Debug.Log(message);
        int ID = message.GetInt(0);
        foreach(UserData user in userManager.usersPlaying){
            if (user._ID == ID)
            {
                Vector3 calibVec = new Vector3(message.GetFloat(1), 0, message.GetFloat(2));

                if (userManager.me._ID == user._ID && gameEngine.useVRHeadset && calibVec != Vector3.zero)
                { // if me with headset
                    GameObject viveParent = GameObject.Find(userManager.viveSystemName);
                    GameObject camera = viveParent.transform.Find("Camera").gameObject;
                    viveParent.transform.position += user.calibrationPositionGap - new Vector3(camera.transform.position.x, 0, camera.transform.position.z) 
                        + gameEngine.scenarioEvents.calibrationTransforms[user._registeredRank].transform.position;
                }
                else
                { // if me without headset or not me
                    user.calibrationPositionGap = calibVec;
                }
                //break;
            }
        }
    }


    void StartPlaybackOrder(OscMessage message){
        if(gameEngine._userRole == UserRole.Playback) gameEngine.playbackManager.StartPlayback();
    }

    void PausePlaybackOrder(OscMessage message){
        if(gameEngine._userRole == UserRole.Playback) gameEngine.playbackManager.PausePlayback();
    }

    void StopPlaybackOrder(OscMessage message){
        if(gameEngine._userRole == UserRole.Playback) gameEngine.playbackManager.StopPlayback();
    }


    public void StartAudioRecording(OscMessage message){
        Debug.Log(message);
        int audioLenght = message.GetInt(0);
        if(userManager.me._userRole == UserRole.Player) gameEngine.soundHandler.Launch(audioLenght);
    }

    public void StopAudioRecording(OscMessage message){
        Debug.Log(message);
        if(userManager.me._userRole == UserRole.Player) gameEngine.soundHandler.Stop();
    }

    // i'm client, server has quit
    void GoInsane(OscMessage message)
    {
        Debug.Log("Received : " + message);
        Debug.Log("HAAAAAAAAAAAAAAA");
        //gameEngine.KillApp();
        gameEngine.Restart();
    }



    void UpdateTrailsVisualisation(OscMessage message){
        //
        int valueId = message.GetInt(0);
        float value = message.GetFloat(1);
        userManager.ChangeVisualisationParameter(valueId, value);
    }

    void WekinatorInfosReceived(OscMessage message)
    {
        Debug.Log(message);
    }

}
