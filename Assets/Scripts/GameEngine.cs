using System;
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
    public int showNamesAboveHead;
}

public enum UserRole
{
    Server, Player, Viewer, Tracker, Playback
}

public enum AppState
{
    Initializing, WaitingForServer, Running
}


// send 
public class GameEngine : MonoBehaviour
{

    [HideInInspector] public UserData _user;
    [HideInInspector] public UserManager userManager;
    [HideInInspector] public ScenarioEvents scenarioEvents;
    [HideInInspector] public FileInOut fileInOut;
    [HideInInspector] public OSC osc;
    [HideInInspector] public NetworkManager networkManager;
    [HideInInspector] public CanvasHandler canvasHandler;
    [HideInInspector] public UIHandler uiHandler;
    [HideInInspector] public SoundHandler soundHandler;
    [HideInInspector] public PlaybackManager playbackManager;
    [HideInInspector] public Clock clock;

    public GameData gameData;    
    public GameObject ViveSystemPrefab;
    //public GameObject sceneGameObjects;
    public List<GameObject> POVs;

    //public GameObject mirror;

    public UserRole _userRole;
    public AppState appState;
    public string currentVisualisationMode = "1A", pendingVisualisationMode = "none";

    [HideInInspector]
    public bool useVRHeadset;

    
    public bool debugMode = false;//, weFade = false;
    public GameObject debugPrefab;
    public int targetFrameRate = 60;

    private void Start()
    {
        StartCoroutine(InitApplication());   
    }


    // The very start of the program
    public IEnumerator InitApplication()
    {
        // basic initialization
        Screen.fullScreen = false;
        appState = AppState.Initializing;
        Application.targetFrameRate = targetFrameRate;

        userManager =           GetComponentInChildren<UserManager>();
        playbackManager =       GetComponentInChildren<PlaybackManager>();
        scenarioEvents =        GetComponentInChildren<ScenarioEvents>();

        networkManager =        (NetworkManager)FindObjectOfType(typeof(NetworkManager));
        osc =                   networkManager.osc;
        
        uiHandler =             (UIHandler)FindObjectOfType(typeof(UIHandler));
        canvasHandler =         uiHandler.GetComponentInChildren<CanvasHandler>();
        
        fileInOut =             (FileInOut)FindObjectOfType(typeof(FileInOut));
        soundHandler =          (SoundHandler)FindObjectOfType(typeof(SoundHandler));
        clock =                 (Clock)FindObjectOfType(typeof(Clock));

        canvasHandler.ChangeCanvas("initCanvas");
        _userRole = UserRole.Server; // base setting
        
        // load jsons
        fileInOut.LoadPreferencesFiles(this);

        if (gameData.runInLocal == 1) {
            gameData.OSC_LocalIP = "127.0.0.1";
        }
        else {
            gameData.OSC_LocalIP = CheckIp();
        }

        // change UI's server IP field
        uiHandler.FillServerIPField(gameData.runInLocal, gameData.OSC_ServerIP); 
        
        userManager.keepNamesVisibleForPlayers = (gameData.showNamesAboveHead == 1);

        
        // adjust user's parameters
        useVRHeadset = (gameData.useVr ==1);
        StartCoroutine(EnableDisableVRMode(useVRHeadset));
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
        if(clock != null) clock.SetSceneStartTs();

        string tmpIp;
        if(gameData.runInLocal == 1){ 
            tmpIp = gameData.OSC_LocalIP;
            if(_userRole != UserRole.Server && !(_userRole == UserRole.Playback && playbackManager.mode == PlaybackMode.Offline)) 
                gameData.OSC_ClientPort = UnityEngine.Random.Range(5555,8888);
        }
        else {
            tmpIp = uiHandler.OSCServerAddressInput.text;
            gameData.OSC_ServerIP = tmpIp; // put back written address into gamedata object
        }

        string uiChosenName = uiHandler.playerNameTextBox.GetComponentInChildren<UnityEngine.UI.InputField>().text;

        if (_userRole == UserRole.Server || _userRole == UserRole.Playback)
        {
            useVRHeadset = false;
            StartCoroutine(EnableDisableVRMode(false));
        }

        _user = userManager.InitLocalUser(this, ID, uiChosenName, tmpIp, gameData.OSC_ServerPort, true, _userRole);

        networkManager.InitNetwork(_userRole, gameData, uiHandler.OSCServerAddressInput.text, playbackManager.mode);

        if(_userRole != UserRole.Playback) userManager.ChangeVisualisationMode("0", this, false);


        soundHandler.Init(gameData.OSC_SoundHandlerIP, gameData.OSC_SoundHandlerPort, _userRole, uiHandler.recordAudioAfterScenario.isOn, gameData.audioRecordLength);


        if (_userRole == UserRole.Server)
        {
            // load every scenario stored in scenario json
            scenarioEvents.performanceRecorder.sessionID = int.Parse(uiHandler.sessionIDInputBox.text);
            uiHandler.PopulateScenariosDropdown(scenarioEvents.scenarios);
            appState = AppState.Running;
            
            canvasHandler.ChangeCanvas("serverCanvas");
            // sonification

        }

        else if(_userRole == UserRole.Playback){
            fileInOut.LoadPerformance(fileInOut.performanceDataFiles[uiHandler.switchPerformanceDataFile.value], playbackManager);

            if(playbackManager.mode == PlaybackMode.Online) // online
            {
                appState = AppState.WaitingForServer;
                InvokeRepeating("AskForRegistration",0f,1f);
                canvasHandler.ChangeCanvas("waitingCanvas");
            }
            else if(playbackManager.mode == PlaybackMode.Offline){
                appState = AppState.Running;
                canvasHandler.ChangeCanvas("playbackCanvasOff");
                userManager.AddNewUser(this, 776, "", osc.outIP, osc.outPort, UserRole.Playback);
                userManager.AddNewUser(this, 777, "", osc.outIP, osc.outPort, UserRole.Playback);
                playbackManager.StartPlayback();
            }
        }

        else
        {
            appState = AppState.WaitingForServer;
            InvokeRepeating("AskForRegistration",0f,1f);
            canvasHandler.ChangeCanvas("waitingCanvas");
        }
    }


    // when server has agreed for client registration
    // player/tracker 
    public void EndStartProcess(int sessionID, int playerID, int requestedPort, string visualisationMode, int rank, int recordAudio, int recordLength)
    {
        CancelInvoke("AskForRegistration");

        if (_userRole == UserRole.Player || _userRole == UserRole.Viewer || _userRole == UserRole.Tracker || _userRole == UserRole.Playback)
        {
            userManager.ChangeVisualisationMode(visualisationMode, this, false);
            Debug.Log(playerID+"registered on port "+requestedPort);
            appState = AppState.Running;
            //networkManager.ShowConnexionState();
            _user._registeredRank = rank;
            scenarioEvents.performanceRecorder.sessionID = sessionID;

            soundHandler.recordPostScenarioAudio = (recordAudio == 1);
    
            if(soundHandler.recordPostScenarioAudio)  
                soundHandler.InitAudioRecorder(scenarioEvents.performanceRecorder.sessionID, recordLength);

            if(_user._userRole == UserRole.Player) 
                canvasHandler.ChangeCanvas("gameCanvas");
            else if(_user._userRole == UserRole.Viewer || _userRole == UserRole.Tracker) 
                canvasHandler.ChangeCanvas("viewerCanvas");
            else if(_user._userRole == UserRole.Playback){
                canvasHandler.ChangeCanvas("playbackCanvasOn");
                if(playbackManager.mode == PlaybackMode.Offline) playbackManager.StartPlayback(); // if online, we wait for server order
            } 
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
        
        // main game loop
        if (appState == AppState.Running)
        {
            networkManager.NetworkGameLoop(_user, userManager, playbackManager, soundHandler);
            userManager.ActualizePlayersPositions(_user); 
        }

        if(Input.GetKeyDown(KeyCode.Escape) && appState != AppState.Initializing) Restart();
    }

    // triggered by invoke
    void AskForRegistration(){
        networkManager.RegisterUser(_user, _userRole);
    }



    public void KillApp()
    {
        Application.Quit();
#if UnityEditor
        EditorApplication.ExecuteMenuItem("Edit/Play");
#endif
    }

    public void Restart() {
        OnApplicationQuit();
        Debug.Log("Restart");
        
        if(_userRole == UserRole.Playback){
            playbackManager.StopPlayback();
        } 
        else if (_userRole == UserRole.Server){
            osc.Close();
        }
        else if(_userRole == UserRole.Player){
            if(soundHandler.isRecording) soundHandler.Stop();
        }
        
        Camera.main.transform.parent = GameObject.Find("--------- Scene Objects ------------").transform;
        userManager.EraseAllPlayers();
        userManager = new UserManager();
        osc.sender.SendQuitMessage(_userRole);
        StopAllCoroutines();
        StartCoroutine(InitApplication());
        
        networkManager.eyeswebOSC.gameObject.SetActive(true);
        networkManager.eyeswebOSC.initialized = false;

        osc.Close();

        /*string[] endings = new string[]{
            "exe", "x86", "x86_64", "app"
        };
        string executablePath = Application.dataPath + "/..";
        foreach (string file in System.IO.Directory.GetFiles(executablePath)) {
            foreach (string ending in endings) {
                if (file.ToLower ().EndsWith ("." + ending)) {
                    System.Diagnostics.Process.Start (executablePath + file);
                    Application.Quit ();
                    return;
                }
            }
                
        }*/
    }


    public void OnApplicationQuit()
    {
        if ((_userRole == UserRole.Player || _userRole == UserRole.Viewer || _userRole == UserRole.Tracker || _userRole == UserRole.Playback) && osc.initialized)
            osc.sender.SendQuitMessage(_userRole);
        else if (_userRole == UserRole.Server && osc.initialized){
            if(scenarioEvents.performanceRecorder.isRecording) scenarioEvents.performanceRecorder.StopRecording();
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
