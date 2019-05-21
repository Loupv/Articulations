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
    public int OSC_SoundHandlerPort ;
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
    public GameData gameData;
    
    public GameObject ViveSystemPrefab;
    public GameObject sceneGameObjects;
    public List<GameObject> POVs;

    public PerformanceRecorder performanceRecorder;
    public UserRole _userRole;
    public AppState appState;
    public int currentVisualisationMode = 1; // justHands
    
    public bool useVRHeadset, sendToAudioDevice;
    public string viveSystemName = "[CameraRig]", 
        viveHeadName  = "Camera", 
        viveLeftHandName = "Controller (left)", 
        viveRightHandName = "Controller (right)";
    
    public bool debugMode = false;
    public GameObject debugPrefab;
    public int targetFrameRate = 60;

    private void Start()
    {
        if(!useVRHeadset) StartCoroutine(EnableDisableVRMode(false));
        else StartCoroutine(EnableDisableVRMode(true));
        //VRSettings.LoadDeviceByName("None");
        Application.targetFrameRate = targetFrameRate;
        //InitApplication();
        StartCoroutine(InitApplication());
        InvokeRepeating("TimedUpdate", 0.5f, 1f / targetFrameRate);    
    }


    // The very start of the program
   // public void InitApplication()
    public IEnumerator InitApplication()
    {

        Screen.fullScreen = false;
        appState = AppState.Initializing;
        
        canvasHandler = GetComponent<CanvasHandler>();
        uiHandler = GetComponentInChildren<UIHandler>();
        canvasHandler.ChangeCanvas("initCanvas");
        _userRole = UserRole.Server; // base setting
        
        // load preferences file
        jSONLoader = new JSONLoader();
        gameData = jSONLoader.LoadGameData("/StreamingAssets/GameData.json");
        gameData = uiHandler.AdjustBasicUIParameters(gameData, CheckIp()); // change UI and gameData depending on actual conditions

#if UNITY_ANDROID
        Debug.Log("tablet initializing");
        string filePath = Path.Combine(Application.streamingAssetsPath, "GameData.json");
        UnityWebRequest www = UnityWebRequest.Get(filePath);
        yield return www.SendWebRequest();
        string dataAsJson = www.downloadHandler.text;
        Debug.Log(dataAsJson);
        gameData = JsonUtility.FromJson<GameData>(dataAsJson);
        gameData = uiHandler.AdjustBasicUIParameters(gameData, CheckIp());
#endif

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

    }


    // Start the performance when button's pressed
    public void StartGame()
    {
        int ID = UnityEngine.Random.Range(0, 10000); //TODO remplacer par le port

        string tmpIp;
        if(gameData.runInLocal == 1) tmpIp = "127.0.0.1";
        else {
            tmpIp = uiHandler.OSCServerAddressInput.text;
            gameData.OSC_ServerIP = tmpIp;
        }

        string n = uiHandler.PlayerName.text;

        if (_userRole == UserRole.Server) useVRHeadset = false;

        _user = userManager.InitLocalUser(this, ID, n, tmpIp, gameData.OSC_ServerPort, true, _userRole);

        networkManager.InitNetwork(_userRole, gameData, uiHandler.OSCServerAddressInput.text);
        
        if (_userRole == UserRole.Server)
        {
            appState = AppState.Running;
            networkManager.ShowConnexionState();
            canvasHandler.ChangeCanvas("serverCanvas");     
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
    public void EndStartProcess(int playerID, int requestedPort, int visualisationMode)
    {
        if (_userRole == UserRole.Player || _userRole == UserRole.Viewer || _userRole == UserRole.Tracker)
        {
            userManager.ChangeVisualisationMode(visualisationMode, this);
            Debug.Log(playerID+"registered on port "+requestedPort);
            appState = AppState.Running;
            networkManager.ShowConnexionState();

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

        if(performanceRecorder.isRecording && !performanceRecorder.isPaused) performanceRecorder.SaveData(userManager.usersPlaying);
        
        userManager.ActualizePlayersPositions(_user); 
    }


    public void KillApp()
    {
        Application.Quit();
#if UnityEditor
            if(Application.isEditor) EditorApplication.ExecuteMenuItem("Edit/Play");
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
        if (true == bEnable)
        {
            yield return new WaitForEndOfFrame();
            UnityEngine.XR.XRSettings.LoadDeviceByName("OpenVR");
            yield return new WaitForEndOfFrame();
            UnityEngine.XR.XRSettings.enabled = true;
        }
        else
        {
            yield return new WaitForEndOfFrame();
            UnityEngine.XR.XRSettings.LoadDeviceByName("None");
            yield return new WaitForEndOfFrame();
            UnityEngine.XR.XRSettings.enabled = false;
        }

        yield return new WaitForEndOfFrame();

        Debug.Log("<color=yellow>UnityEngine.XR.XRSettings.enabled = " + UnityEngine.XR.XRSettings.enabled + "</color>");
        Debug.Log("<color=yellow>UnityEngine.XR.XRSettings.loadedDeviceName = " + UnityEngine.XR.XRSettings.loadedDeviceName + "</color>");
    }

}
