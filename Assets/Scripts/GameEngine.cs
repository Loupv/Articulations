using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
//using UnityEditor.SceneManagement;


public struct AppConfig
{
    public int playerRole;
}


[System.Serializable]
public class GameData
{
    public int runInLocal;
    public string OSC_ServerIP, OSC_LocalIP;
    public int OSC_ServerPort, OSC_ClientPort;
    public int DebugMode;
}

public enum UserNetworkType
{
    Server, Client
}


public enum AppState
{
    Initializing, StartScreen, WaitingForServer, Home, Running
}


// send 
public class GameEngine : MonoBehaviour
{

    public CanvasHandler canvasHandler;
    public NetworkManager networkManager;

    public Dictionary<string, Vector3> pendingPositionsActualizations;
    public Dictionary<string, Quaternion> pendingRotationsActualizations;
    public List<UserData> usersPlaying;
    [HideInInspector]
    public GameObject _userGameObject;
    [HideInInspector]
    public UserData _user;

    public OSC osc;
    //private SendOSC sender;
    //private ReceiveOSC receiver;

    private JSONLoader jSONLoader;
    public UIHandler uiHandler;
    public GameData gameData;
    
    public GameObject playerPrefab, viewerPrefab, ViveSystemPrefab, LongTrailsPrefab, ShortTrailsPrefab;
    public List<GameObject> POVs;

    public PerformanceRecorder performanceRecorder;
    public UserNetworkType userNetworkType;
    public AppState appState;
    public int currentVisualisationMode = 1; // justHands
    private OSCEndPoint serverEndpoint;
    public bool useVRHeadset;
    public string viveSystemName = "[CameraRig]", 
        viveHeadName  = "Camera", 
        viveLeftHandName = "Controller (left)", 
        viveRightHandName = "Controller (right)";
    
    public bool debugMode = false;
    public GameObject debugPrefab;


    private void Start()
    {
        InitApplication();
    }


    // The very start of the program
    public void InitApplication()
    {
        Screen.fullScreen = false;
        appState = AppState.Initializing;
        
        canvasHandler = GetComponent<CanvasHandler>();
        uiHandler = GetComponentInChildren<UIHandler>();
        canvasHandler.ChangeCanvas("initCanvas");

        userNetworkType = UserNetworkType.Server;

        jSONLoader = new JSONLoader();
        gameData = jSONLoader.LoadGameData("/StreamingAssets/GameData.json");

        pendingPositionsActualizations = new Dictionary<string, Vector3>();
        pendingRotationsActualizations = new Dictionary<string, Quaternion>();
        
        if (gameData.runInLocal == 1) {
            uiHandler.OSCServerPortInput.text = "127.0.0.1";
            gameData.OSC_LocalIP = "127.0.0.1";
            gameData.OSC_ClientPort = UnityEngine.Random.Range(5555,8888);
        }
        else {
            uiHandler.OSCServerPortInput.text = gameData.OSC_ServerIP;
            gameData.OSC_LocalIP = CheckIp();
        }
        // adjust user's parameters
        uiHandler.SetPlayerNetworkType(0);
        
        // do we print sent and received messages
        if(gameData.DebugMode == 1){
             Instantiate(debugPrefab);
             debugMode = true;
        }

    }

    // Start the performance when button's pressed
    public void StartGame(int isPlayer)
    {
        int ID = UnityEngine.Random.Range(0, 10000); //TODO remplacer par le port

        if (isPlayer == 1) _userGameObject = Instantiate(playerPrefab);
        else  {
            _userGameObject = Instantiate(viewerPrefab);
            uiHandler.viewerController = _userGameObject.GetComponent<ViewerController>();
        }
        _user = _userGameObject.GetComponent<UserData>();
        _user.Init(this, ID, gameData.OSC_ServerIP, gameData.OSC_ServerPort, _userGameObject, isPlayer, 1);

        if (isPlayer == 1)
        {
            usersPlaying.Add(_user);

            pendingPositionsActualizations.Add(_user._ID + "Head", _user.head.transform.position);
            pendingPositionsActualizations.Add(_user._ID + "LeftHand", _user.leftHand.transform.position);
            pendingPositionsActualizations.Add(_user._ID + "RightHand", _user.rightHand.transform.position);
            pendingRotationsActualizations.Add(_user._ID + "Head", _user.head.transform.rotation);
            pendingRotationsActualizations.Add(_user._ID + "LeftHand", _user.leftHand.transform.rotation);
            pendingRotationsActualizations.Add(_user._ID + "RightHand", _user.rightHand.transform.rotation);
            
        }

        osc.receiver.userNetworkType = userNetworkType;
        
        serverEndpoint.ip = gameData.OSC_ServerIP;
        serverEndpoint.remotePort = gameData.OSC_ClientPort;

        if (userNetworkType == UserNetworkType.Server)
        {
            osc.inPort = gameData.OSC_ServerPort;
            osc.outPort = gameData.OSC_ClientPort;
            osc.outIP = uiHandler.address;
            osc.Init();
            appState = AppState.Running;
            networkManager.ShowConnexionState();
            canvasHandler.ChangeCanvas("serverCanvas");     
        }

        else if (userNetworkType == UserNetworkType.Client)
        {
            osc.inPort = gameData.OSC_ClientPort;
            osc.outPort = gameData.OSC_ServerPort;
            osc.outIP = uiHandler.address;
            osc.Init();
            appState = AppState.WaitingForServer;
            osc.sender.RequestUserRegistation(_user, gameData.OSC_ClientPort, isPlayer);
            canvasHandler.ChangeCanvas("waitingCanvas");
        }

    }


    // when server has agreed for client registration
    public void EndStartProcess(int playerID, int requestedPort, int visualisationMode)
    {
        if (userNetworkType == UserNetworkType.Client)
        {
            ChangeVisualisationMode(visualisationMode);
            Debug.Log(playerID+"registered on port "+requestedPort);
            appState = AppState.Running;
            networkManager.ShowConnexionState();

            if(_user._isPlayer == 1) canvasHandler.ChangeCanvas("gameCanvas");
            else if(_user._isPlayer == 0) canvasHandler.ChangeCanvas("viewerCanvas");
        }
    }



    private void Update()
    {
        // TODO fix game start with VR headset -> multiple cameras and displays
        if(Input.GetKeyDown("space") && appState == AppState.Initializing)
            StartGame(1);

        if (appState == AppState.Running)
        {
            UpdateGame();
        }
    }



    public void UpdateGame()
    {
        if(userNetworkType == UserNetworkType.Client){
            if(_user._isPlayer == 1) networkManager.SendOwnPosition(_user, serverEndpoint); // don't send if you're viewer
        }
        else if(userNetworkType == UserNetworkType.Server){
            networkManager.SendAllPositionsToClients(usersPlaying);
        }
        if(performanceRecorder.isRecording && !performanceRecorder.isPaused) performanceRecorder.SaveData(usersPlaying);
        ActualizePlayersPositions(); 
    }




    // adjust players positions from stored one
    public void ActualizePlayersPositions(){
        int i = 0;
        foreach (UserData user in usersPlaying)
        {
            if (user._ID != _user._ID && user._isPlayer == 1) // if it's not actual instance's player
            {
                usersPlaying[i].head.transform.position = pendingPositionsActualizations[user._ID + "Head"];
                usersPlaying[i].leftHand.transform.position = pendingPositionsActualizations[user._ID + "LeftHand"];
                usersPlaying[i].rightHand.transform.position = pendingPositionsActualizations[user._ID + "RightHand"];
                usersPlaying[i].head.transform.rotation = pendingRotationsActualizations[user._ID + "Head"];
                usersPlaying[i].leftHand.transform.rotation = pendingRotationsActualizations[user._ID + "LeftHand"];
                usersPlaying[i].rightHand.transform.rotation = pendingRotationsActualizations[user._ID + "RightHand"];
            }
            i++;
        }
    }


    public UserData AddOtherPlayer(int playerID, string address, int port, int isPlayer)
    {
        // check si dispo
        GameObject go = Instantiate(playerPrefab);
        UserData p = go.GetComponent<UserData>();
        p.Init(this, playerID, address, port, go, isPlayer, 0);
        usersPlaying.Add(p);
        if(isPlayer == 1){
            pendingPositionsActualizations.Add(playerID + "Head", p.head.transform.position);
            pendingPositionsActualizations.Add(playerID + "LeftHand", p.leftHand.transform.position);
            pendingPositionsActualizations.Add(playerID + "RightHand", p.rightHand.transform.position);
            pendingRotationsActualizations.Add(playerID + "Head", p.head.transform.rotation);
            pendingRotationsActualizations.Add(playerID + "LeftHand", p.leftHand.transform.rotation);
            pendingRotationsActualizations.Add(playerID + "RightHand", p.rightHand.transform.rotation);
        }
        return p;
    }

    // server's reaction to clienthasleft message
    public void ErasePlayer(int playerID)
    {
        pendingPositionsActualizations.Remove(playerID + "Head");
        pendingPositionsActualizations.Remove(playerID + "LeftHand");
        pendingPositionsActualizations.Remove(playerID + "RightHand");
        pendingRotationsActualizations.Remove(playerID + "Head");
        pendingRotationsActualizations.Remove(playerID + "LeftHand");
        pendingRotationsActualizations.Remove(playerID + "RightHand");

        foreach (UserData p in usersPlaying)
        {
            if (p._ID == playerID)
            {
                Destroy(p.gameObject);
                usersPlaying.Remove(p);
                Destroy(p);
                break;
            }
        }
    }

    // in the case we want to have local user differents from other players, we place the loops here
    public void ChangeVisualisationMode(int mode){

        if(mode == 0){
            foreach(UserData user in usersPlaying){
                if(user._ID == _user._ID)
                    user.ChangeSkin(this, "noHands");
                else user.ChangeSkin(this, "justHands");
            }
        }
        else if(mode == 1){
            foreach(UserData user in usersPlaying){
                user.ChangeSkin(this, "justHands");
            }
        }
        else if(mode == 2){
            foreach(UserData user in usersPlaying){
                user.ChangeSkin(this, "shortTrails");
            }
        }
        else if(mode == 3){
            foreach(UserData user in usersPlaying){
                user.ChangeSkin(this, "longTrails");
            }
        }
        currentVisualisationMode = mode;
    }


    public int ReturnPlayerRank(int n){ // n may be equal to 1 or 2 (player1 or 2) 
        int r = 0;
        int i = 0;
        foreach(UserData user in usersPlaying){
            if(user._isPlayer == 1) r +=1; // if we found a player thats number r
            if(r == n) return i; // if r was needed, return it
            i++;
        }
        Debug.Log("Player not found");
        return -1;
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
        if (userNetworkType == UserNetworkType.Client && osc.initialized)
            osc.sender.SendQuitMessage(userNetworkType);
        else if (userNetworkType == UserNetworkType.Server && osc.initialized){
            if(performanceRecorder.isRecording) performanceRecorder.SaveTofile();
            osc.sender.SendQuitMessage(userNetworkType); // TODO adapt if server
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

}
