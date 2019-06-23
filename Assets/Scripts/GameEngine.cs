﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.XR;

//using UnityEditor.SceneManagement;





[System.Serializable]
public class GameData
{
    public int runInLocal;
    public string OSC_ServerIP, OSC_LocalIP = "";
    public int OSC_ServerPort, OSC_ClientPort;
    public string OSC_SoundHandlerIP ;
    public int useVr;
    public int saveFileFrequency;
    public int OSC_SoundHandlerPort ;
    public int audioRecordLength; // no use in client mode, chosen length is sent by server
    public int DebugMode;
    public int keepNamesVisibleForPlayers;
}

public enum UserRole
{
    Server, Player, Viewer, Tracker
}

public enum AppState
{
    Initializing, WaitingForServer, Running
}


// send 
public class GameEngine : MonoBehaviour
{

    public CanvasHandler canvasHandler;
    public NetworkManager networkManager;

    public UserManager userManager;

    
    [HideInInspector]
    public UserData _user;

    public OSC osc;
    //private SendOSC sender;
    //private ReceiveOSC receiver;

    private JSONLoader jSONLoader;
    
    public UIHandler uiHandler;
    public SoundHandler soundHandler;
    public ScenarioEvents scenarioEvents;
    public AudioRecordManager audioRecordManager;
    public SoundInstructionPlayer instructionPlayer;
    public GameData gameData;
    
    public GameObject ViveSystemPrefab;
    //public GameObject sceneGameObjects;
    public List<GameObject> POVs;

    //public GameObject mirror;

    public PerformanceRecorder performanceRecorder;
    public UserRole _userRole;
    public AppState appState;
    public string currentVisualisationMode = "1A", pendingVisualisationMode = "none";

    [HideInInspector]
    public bool useVRHeadset;
    public bool sendToAudioDevice;
    public string viveSystemName = "[CameraRig]", 
        viveHeadName  = "Camera", 
        viveLeftHandName = "Controller (left)", 
        viveRightHandName = "Controller (right)";
    
    public bool debugMode = false;//, weFade = false;
    public GameObject debugPrefab;
    public int targetFrameRate = 60;

    private void Start()
    {
        //if (!useVRHeadset) StartCoroutine(EnableDisableVRMode(false));
        //else StartCoroutine(EnableDisableVRMode(true));

        StartCoroutine(InitApplication());   
    }


    // The very start of the program
   // public void InitApplication()
    public IEnumerator InitApplication()
    {

        Screen.fullScreen = false;
        appState = AppState.Initializing;

        Application.targetFrameRate = targetFrameRate;

        canvasHandler = GetComponent<CanvasHandler>();
        uiHandler = GetComponentInChildren<UIHandler>();
        canvasHandler.ChangeCanvas("initCanvas");
        _userRole = UserRole.Server; // base setting
        
        
        

#if UNITY_ANDROID
        Debug.Log("tablet initializing");

        // game data json
        string filePath = Path.Combine(Application.streamingAssetsPath, "GameData.json");
        UnityWebRequest www = UnityWebRequest.Get(filePath);
        yield return www.SendWebRequest();
        string dataAsJson = www.downloadHandler.text;
        Debug.Log(dataAsJson);
        gameData = JsonUtility.FromJson<GameData>(dataAsJson);

        // scenario json
        filePath = Path.Combine(Application.streamingAssetsPath, "Scenarios.json");
        UnityWebRequest www = UnityWebRequest.Get(filePath);
        yield return www.SendWebRequest();
        dataAsJson = www.downloadHandler.text;
        scenarioEvents.scenarios = JsonUtility.FromJson<Scenario[]>(dataAsJson);
#else

        // load preferences file
        jSONLoader = new JSONLoader();
        gameData = jSONLoader.LoadGameData("/StreamingAssets/GameData.json");
        scenarioEvents.scenarios = jSONLoader.LoadScenarioList("/StreamingAssets/Scenarios.json").scenarios;

#endif


        gameData = uiHandler.AdjustBasicUIParameters(gameData, CheckIp()); // change UI and gameData depending on actual conditions
        uiHandler.PopulateScenariosDropdown(scenarioEvents.scenarios);

        useVRHeadset = (gameData.useVr ==1);
        StartCoroutine(EnableDisableVRMode(useVRHeadset));


        userManager.keepNamesVisibleForPlayers = (gameData.keepNamesVisibleForPlayers == 1);
        soundHandler.Init(gameData.OSC_SoundHandlerIP, gameData.OSC_SoundHandlerPort);

        // adjust user's parameters
        if(useVRHeadset) uiHandler.SetPlayerNetworkType(1);
        else uiHandler.SetPlayerNetworkType(0);
        
        // do we print sent and received messages
        if(gameData.DebugMode == 1){
            Instantiate(debugPrefab);
            debugMode = true;
        }
        yield return new WaitForSeconds(1);

        InvokeRepeating("TimedUpdate", 0.5f, 1f / targetFrameRate);
    }


    // Start the performance when button's pressed
    public void StartGame()
    {
        int ID = UnityEngine.Random.Range(0, 10000); 

        string tmpIp;
        if(gameData.runInLocal == 1) tmpIp = "127.0.0.1";
        else {
            tmpIp = uiHandler.OSCServerAddressInput.text;
            gameData.OSC_ServerIP = tmpIp;
        }

        string n = uiHandler.PlayerName.text;

        if (_userRole == UserRole.Server)
        {
            performanceRecorder.sessionID = int.Parse(uiHandler.sessionIDInputBox.text);
            useVRHeadset = false;
            StartCoroutine(EnableDisableVRMode(false));
            //EnableDisableVRMode(useVRHeadset);
        }
        _user = userManager.InitLocalUser(this, ID, n, tmpIp, gameData.OSC_ServerPort, true, _userRole);

        networkManager.InitNetwork(_userRole, gameData, uiHandler.OSCServerAddressInput.text);

        uiHandler.ChangeVisualizationMode("0"); // TODO get this out of ui handler

        if (_userRole == UserRole.Server)
        {
            appState = AppState.Running;
            //networkManager.ShowConnexionState();
            canvasHandler.ChangeCanvas("serverCanvas");
            audioRecordManager.recordPostScenarioAudio = uiHandler.recordAudioAfterScenario.isOn;
            audioRecordManager.postScenarioRecordingLenght = gameData.audioRecordLength;
        }
        else
        {
            appState = AppState.WaitingForServer;
            networkManager.RegisterUSer(_user, _userRole);
            canvasHandler.ChangeCanvas("waitingCanvas");
            //if (useVRHeadset) StartCoroutine(EnableDisableVRMode(true));
            foreach(GameObject model in GameObject.FindGameObjectsWithTag("SteamModel"))
            {
                model.SetActive(false);
            }
        }
    }


    // when server has agreed for client registration
    // player/tracker 
    public void EndStartProcess(int sessionID, int playerID, int requestedPort, string visualisationMode, int rank, int recordAudio, int recordLength)
    {
        if (_userRole == UserRole.Player || _userRole == UserRole.Viewer || _userRole == UserRole.Tracker)
        {
            uiHandler.ChangeVisualizationMode(visualisationMode);
            Debug.Log(playerID+"registered on port "+requestedPort);
            appState = AppState.Running;
            //networkManager.ShowConnexionState();
            _user._registeredRank = rank;
            performanceRecorder.sessionID = sessionID;

            audioRecordManager.recordPostScenarioAudio = (recordAudio == 1);
    
            if(audioRecordManager.recordPostScenarioAudio)  
                audioRecordManager.InitAudioRecorder(performanceRecorder.sessionID, recordLength);

            if(_user._userRole == UserRole.Player) canvasHandler.ChangeCanvas("gameCanvas");
            else if(_user._userRole == UserRole.Viewer || _userRole == UserRole.Tracker) canvasHandler.ChangeCanvas("viewerCanvas");
        }
    }



    private void TimedUpdate()
    {
        // initialization step  
        if(Input.GetKeyDown("space") && appState == AppState.Initializing)
            StartGame();
        if(Input.GetKeyDown(KeyCode.Tab) && appState == AppState.Initializing)
        {
            int t;
            if(_userRole == UserRole.Server) t = 1;
            else t= 0;
            uiHandler.SetPlayerNetworkType(t);
        }
        


        if (appState == AppState.Running)
        {
            UpdateGame();
        }
    }



    public void UpdateGame()
    {
        
        if(_userRole == UserRole.Player){
            networkManager.SendOwnPosition(_user, networkManager.serverEndpoint); // don't send if you're viewer
        }
        else if(_userRole == UserRole.Server){
            networkManager.SendAllPositionsToClients(userManager.usersPlaying);
            if(sendToAudioDevice) networkManager.SendAllPositionsToAudioSystem(userManager.usersPlaying, soundHandler);
        }

        
        
        userManager.ActualizePlayersPositions(_user); 
    }


    public void KillApp()
    {
        Application.Quit();
#if UnityEditor
        EditorApplication.ExecuteMenuItem("Edit/Play");
#endif
    }


    public void OnApplicationQuit()
    {
        if ((_userRole == UserRole.Player || _userRole == UserRole.Viewer || _userRole == UserRole.Tracker) && osc.initialized)
            osc.sender.SendQuitMessage(_userRole);
        else if (_userRole == UserRole.Server && osc.initialized){
            if(performanceRecorder.isRecording) performanceRecorder.SaveTofile();
            osc.sender.SendQuitMessage(_userRole); // TODO adapt if server
        }

        StartCoroutine(EnableDisableVRMode(false));

        Debug.Log("Closing Game Engine...");
    }


    public static string CheckIp()
    {
        string localIP;
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            localIP = endPoint.Address.ToString();
        }
        return localIP;
    }


    public IEnumerator EnableDisableVRMode(bool bEnable)
    {
        #if UNITY_64 || UNITY_EDITOR_64
        if (true == bEnable)
        {
            yield return new WaitForEndOfFrame();
            UnityEngine.XR.XRSettings.LoadDeviceByName("OpenVR");
            yield return new WaitForEndOfFrame();
            UnityEngine.XR.XRSettings.enabled = true;
            Debug.Log("VR switched On");
        }
        else
        {
            yield return new WaitForEndOfFrame();
            UnityEngine.XR.XRSettings.LoadDeviceByName("None");
            yield return new WaitForEndOfFrame();
            UnityEngine.XR.XRSettings.enabled = false;
            Debug.Log("VR switched Off");
        }

        yield return new WaitForEndOfFrame();

        Debug.Log("<color=yellow>UnityEngine.XR.XRSettings.enabled = " + UnityEngine.XR.XRSettings.enabled + "</color>");
        Debug.Log("<color=yellow>UnityEngine.XR.XRSettings.loadedDeviceName = " + UnityEngine.XR.XRSettings.loadedDeviceName + "</color>");
        #endif
    }


}
