﻿using System.Collections;
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
    public int OSC_LocalPort, OSC_RemotePort;
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
    public List<UserData> usersPlaying;
    [HideInInspector]
    public GameObject _userGameObject;
    [HideInInspector]
    public UserData _user;

    public OSC osc;
    //private SendOSC sender;
    //private ReceiveOSC receiver;

    private JSONLoader jSONLoader;
    private UIHandler uiHandler;
    public GameData gameData;
    
    public GameObject playerPrefab;

    public UserNetworkType userNetworkType;
    public AppState appState;
    private OSCEndPoint serverEndpoint;
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

        uiHandler.OSCInPortInput.text = gameData.OSC_LocalPort.ToString();
        uiHandler.OSCOutPortInput.text = gameData.OSC_RemotePort.ToString();
        if (gameData.runInLocal == 1) uiHandler.OSCAddressInput.text = "127.0.0.1";
        else uiHandler.OSCAddressInput.text = gameData.OSC_ServerIP;
        
        // adjust user's parameters
        uiHandler.ChangeOSCConfig();
        uiHandler.SetPlayerNetworkType();
        uiHandler.SetPlayerRole();
        
        // do we print sent and received messages
        if(gameData.DebugMode == 1){
             Instantiate(debugPrefab);
             debugMode = true;
        }

    }

    // Start the performance when button's pressed
    public void StartGame(int isPlayer)
    {
        int ID = Random.Range(0, 10000); //TODO remplacer par le port

        _userGameObject = Instantiate(playerPrefab);
        _user = _userGameObject.GetComponent<UserData>();
        _user.Init(this, ID, gameData.OSC_ServerIP, gameData.OSC_LocalPort, _userGameObject, isPlayer, 1);

        if (isPlayer == 1)
        {
            usersPlaying.Add(_user);

            pendingPositionsActualizations.Add(_user._ID + "Head", _user.head.transform.position);
            pendingPositionsActualizations.Add(_user._ID + "LeftHand", _user.leftHand.transform.position);
            pendingPositionsActualizations.Add(_user._ID + "RightHand", _user.rightHand.transform.position);
        }

        osc.receiver.userNetworkType = userNetworkType;
        osc.inPort = uiHandler.OSCLocalPort;
        osc.outPort = uiHandler.OSCRemotePort;
        osc.outIP = uiHandler.address;
        osc.Init();

        serverEndpoint.ip = gameData.OSC_ServerIP;
        serverEndpoint.remotePort = gameData.OSC_RemotePort;

        if (userNetworkType == UserNetworkType.Server)
        {
            appState = AppState.Running;
            networkManager.ShowConnexionState();
            canvasHandler.ChangeCanvas("gameCanvas");     
        }

        else if (userNetworkType == UserNetworkType.Client)
        {
            appState = AppState.WaitingForServer;
            osc.sender.RequestUserRegistation(_user, gameData.OSC_RemotePort);
            canvasHandler.ChangeCanvas("waitingCanvas");
        }

    }


    // when server has agreed for client registration
    public void EndStartProcess(int playerID, int requestedPort)
    {
        if (userNetworkType == UserNetworkType.Client)
        {
            Debug.Log(playerID+"registered on port "+requestedPort);
            appState = AppState.Running;
            networkManager.ShowConnexionState();
            canvasHandler.ChangeCanvas("gameCanvas");
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
        if(userNetworkType == UserNetworkType.Client)
            networkManager.SendOwnPosition(_user, serverEndpoint);
        else 
            networkManager.SendAllPositionsToClients(usersPlaying);

        ActualizePlayersPositions();
        
    }




    // adjust players positions from stored one
    public void ActualizePlayersPositions(){
        int i = 0;
        foreach (UserData user in usersPlaying)
        {
            if (user._ID != _user._ID) // if it's not actual instance's player
            {
                usersPlaying[i].head.transform.position = pendingPositionsActualizations[user._ID + "Head"];
                usersPlaying[i].leftHand.transform.position = pendingPositionsActualizations[user._ID + "LeftHand"];
                usersPlaying[i].rightHand.transform.position = pendingPositionsActualizations[user._ID + "RightHand"];
            }
            i++;
        }
    }


    public UserData AddOtherPlayer(int playerID, string address, int port)
    {
        // check si dispo
        GameObject go = Instantiate(playerPrefab);
        UserData p = go.GetComponent<UserData>();
        p.Init(this, playerID, address, port, go, 1, 0);
        usersPlaying.Add(p);

        pendingPositionsActualizations.Add(playerID + "Head", p.head.transform.position);
        pendingPositionsActualizations.Add(playerID + "LeftHand", p.leftHand.transform.position);
        pendingPositionsActualizations.Add(playerID + "RightHand", p.rightHand.transform.position);
        return p;
    }

    // server's reaction to clienthasleft message
    public void ErasePlayer(int playerID)
    {
        pendingPositionsActualizations.Remove(playerID + "Head");
        pendingPositionsActualizations.Remove(playerID + "LeftHand");
        pendingPositionsActualizations.Remove(playerID + "RightHand");

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
        else if (userNetworkType == UserNetworkType.Server && osc.initialized)
            osc.sender.SendQuitMessage(userNetworkType); // TODO adapt if server
        Debug.Log("Closing Game Engine...");
    }

}
