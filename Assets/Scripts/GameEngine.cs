using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
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
    Server, Player, Viewer
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
    public SoundHandler soundHandler;
    public GameData gameData;
    
    public GameObject playerPrefab, viewerPrefab, ViveSystemPrefab, LongTrailsPrefab, ShortTrailsPrefab;
    public List<GameObject> POVs;

    public PerformanceRecorder performanceRecorder;
    public UserRole _userRole;
    public AppState appState;
    public int currentVisualisationMode = 1; // justHands
    private OSCEndPoint serverEndpoint;
    public bool useVRHeadset, keepNamesVisibleForPlayers, sendToAudioDevice;
    public string viveSystemName = "[CameraRig]", 
        viveHeadName  = "Camera", 
        viveLeftHandName = "Controller (left)", 
        viveRightHandName = "Controller (right)";
    
    public bool debugMode = false;
    public GameObject debugPrefab;
    public int targetFrameRate = 60;
    private float tmpTime;

    private void Start()
    {
        InitApplication();
        
        Application.targetFrameRate = targetFrameRate;
        InvokeRepeating("TimedUpdate", 0.5f, 1f / targetFrameRate);  
          
    }


    // The very start of the program
    public void InitApplication()
    {
        Screen.fullScreen = false;
        appState = AppState.Initializing;
        
        canvasHandler = GetComponent<CanvasHandler>();
        uiHandler = GetComponentInChildren<UIHandler>();
        canvasHandler.ChangeCanvas("initCanvas");

        _userRole = UserRole.Server;

        jSONLoader = new JSONLoader();
        gameData = jSONLoader.LoadGameData("/StreamingAssets/GameData.json");

        pendingPositionsActualizations = new Dictionary<string, Vector3>();
        pendingRotationsActualizations = new Dictionary<string, Quaternion>();
        
        if (gameData.runInLocal == 1) {
            uiHandler.OSCServerAddressInput.text = "127.0.0.1";
            gameData.OSC_LocalIP = "127.0.0.1";
            
            gameData.OSC_ClientPort = UnityEngine.Random.Range(5555,8888);
        }
        else {
            uiHandler.OSCServerAddressInput.text = gameData.OSC_ServerIP;
            gameData.OSC_LocalIP = CheckIp();
        }

        soundHandler.oscEndPoint.ip = gameData.OSC_SoundHandlerIP;
        soundHandler.oscEndPoint.remotePort = gameData.OSC_SoundHandlerPort;

        // adjust user's parameters
        if(useVRHeadset)
                uiHandler.SetPlayerNetworkType(1);
        else 
        uiHandler.SetPlayerNetworkType(0);
        
        keepNamesVisibleForPlayers = (gameData.keepNamesVisibleForPlayers == 1);

        // do we print sent and received messages
        if(gameData.DebugMode == 1){
             Instantiate(debugPrefab);
             debugMode = true;
        }

    }

    // Start the performance when button's pressed
    public void StartGame()
    {
        int ID = UnityEngine.Random.Range(0, 10000); //TODO remplacer par le port

        if (_userRole == UserRole.Player) _userGameObject = Instantiate(playerPrefab);
        else  {
            _userGameObject = Instantiate(viewerPrefab);
            uiHandler.viewerController = _userGameObject.GetComponent<ViewerController>();
        }
        if (_userRole == UserRole.Server)
        {
            useVRHeadset = false;
        }

        _user = _userGameObject.GetComponent<UserData>();
        string tmpIp;
        if(gameData.runInLocal == 1) tmpIp = "127.0.0.1";
        else {
            tmpIp = uiHandler.OSCServerAddressInput.text;
            gameData.OSC_ServerIP = tmpIp;
        }

        string n = uiHandler.PlayerName.text;
        
        _user.Init(this, ID, n, tmpIp, gameData.OSC_ServerPort, _userGameObject, true, _userRole);

        if (_userRole == UserRole.Player)
        {
            usersPlaying.Add(_user);

            pendingPositionsActualizations.Add(_user._ID + "Head", _user.head.transform.position);
            pendingPositionsActualizations.Add(_user._ID + "LeftHand", _user.leftHand.transform.position);
            pendingPositionsActualizations.Add(_user._ID + "RightHand", _user.rightHand.transform.position);
            pendingRotationsActualizations.Add(_user._ID + "Head", _user.head.transform.rotation);
            pendingRotationsActualizations.Add(_user._ID + "LeftHand", _user.leftHand.transform.rotation);
            pendingRotationsActualizations.Add(_user._ID + "RightHand", _user.rightHand.transform.rotation);
            
        }

        osc.receiver.userRole = _userRole;
        
        serverEndpoint.ip = gameData.OSC_ServerIP;
        serverEndpoint.remotePort = gameData.OSC_ClientPort;

        if (_userRole == UserRole.Server)
        {
            osc.inPort = gameData.OSC_ServerPort;
            osc.outPort = gameData.OSC_ClientPort;
            osc.outIP = uiHandler.OSCServerAddressInput.text;
            osc.Init();
            print("OSC Server - Connexion initiation");
            appState = AppState.Running;
            networkManager.ShowConnexionState();
            canvasHandler.ChangeCanvas("serverCanvas");     
        }

        else if ((_userRole == UserRole.Player || _userRole == UserRole.Viewer))
        {
            osc.inPort = gameData.OSC_ClientPort;
            osc.outPort = gameData.OSC_ServerPort;
            osc.outIP = uiHandler.OSCServerAddressInput.text;
            osc.Init();
            print("OSC Connexion initiation to " + osc.outIP + " : " + osc.outPort + "/" + osc.inPort);
            appState = AppState.WaitingForServer;
            osc.sender.RequestUserRegistation(_user, _userRole);
            canvasHandler.ChangeCanvas("waitingCanvas");
        }

    }


    // when server has agreed for client registration
    public void EndStartProcess(int playerID, int requestedPort, int visualisationMode)
    {
        if (_userRole == UserRole.Player || _userRole == UserRole.Viewer)
        {
            ChangeVisualisationMode(visualisationMode);
            Debug.Log(playerID+"registered on port "+requestedPort);
            appState = AppState.Running;
            networkManager.ShowConnexionState();

            if(_user._userRole == UserRole.Player) canvasHandler.ChangeCanvas("gameCanvas");
            else if(_user._userRole == UserRole.Viewer) canvasHandler.ChangeCanvas("viewerCanvas");
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
        if(_userRole == UserRole.Player || _userRole == UserRole.Viewer){
            if(_user._userRole == UserRole.Player) networkManager.SendOwnPosition(_user, serverEndpoint); // don't send if you're viewer
        }
        else if(_userRole == UserRole.Server){
            networkManager.SendAllPositionsToClients(usersPlaying);
            if(sendToAudioDevice) networkManager.SendAllPositionsToAudioSystem(usersPlaying, soundHandler);
        }
        if(performanceRecorder.isRecording && !performanceRecorder.isPaused) performanceRecorder.SaveData(usersPlaying);
        ActualizePlayersPositions(); 
    }




    // adjust players positions from stored one
    public void ActualizePlayersPositions(){
        int i = 0;
        foreach (UserData user in usersPlaying)
        {
            if (user._ID != _user._ID && user._userRole == UserRole.Player) // if it's not actual instance's player
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


    public UserData AddOtherPlayer(int playerID, string playerName, string address, int port, UserRole role)
    {
        // check si dispo
        GameObject go = Instantiate(playerPrefab);
        UserData p = go.GetComponent<UserData>();

        p.Init(this, playerID, playerName, address, port, go, false, role);
        usersPlaying.Add(p);
        if(role == UserRole.Player){
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

    public void ChangeVisualisationParameter(int valueId, float value){
            
        if(valueId == 0){
            foreach(GameObject hand in GameObject.FindGameObjectsWithTag("HandParticleSystem")){
                hand.GetComponent<TrailRenderer>().time = value;
            }
        }
    }


    public int ReturnPlayerRank(int n){ // n may be equal to 1 or 2 (player1 or 2) 
        int r = 0;
        int i = 0;
        foreach(UserData user in usersPlaying){
            if(user._userRole == UserRole.Player) r +=1; // if we find a player thats number r
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
        if ((_userRole == UserRole.Player || _userRole == UserRole.Viewer) && osc.initialized)
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

}
