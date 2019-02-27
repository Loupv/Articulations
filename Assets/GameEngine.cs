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
    public string OSC_IP;
    public int OSC_ServerOutPort, OSC_ClientOutPort;
}

public enum UserNetworkType
{
    Server, Client
}

public enum UserRole
{
    Player, Viewer
}

public enum AppState
{
    Initializing, StartScreen, WaitingForServer, Home, Running
}


// send 
public class GameEngine : MonoBehaviour
{


    public OSC osc;
    public SendOSC sendOSC;
    public ReceiveOSC receiveOSC;
    public CanvasHandler canvasHandler;

    public Dictionary<string, Vector3> pendingPositionsActualizations;
    public List<int> IDsList;
    public List<PlayerData> players;
    //public int _playerID;
    public PlayerData player;

    private JSONLoader jSONLoader;
    private GameData gameData;

    public Image oscToggle;
    public Text sentMessage;
    public GameObject playerPrefab;
    public GameObject playerParent;

    public UserNetworkType userNetworkType;
    public UserRole userRole;
    public AppState appState;


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
        canvasHandler.ChangeCanvas("initCanvas");

        userNetworkType = UserNetworkType.Server;
        userRole = UserRole.Player;

        jSONLoader = new JSONLoader();
        gameData = jSONLoader.LoadGameData("/StreamingAssets/GameData.json");
        pendingPositionsActualizations = new Dictionary<string, Vector3>();
       
    }

    // Start the performance
    public void StartGame()
    {
        InitConnexion(userNetworkType, userRole);

        if (userRole == UserRole.Player)
        {
            int ID = Random.Range(0, 10000);
            player = new PlayerData(ID, playerPrefab, playerParent, true);
            players.Add(player);
            IDsList.Add(player._ID);
            pendingPositionsActualizations.Add(player._ID + "Head", player.head.transform.position);
            pendingPositionsActualizations.Add(player._ID + "LeftHand", player.leftHand.transform.position);
            pendingPositionsActualizations.Add(player._ID + "RightHand", player.rightHand.transform.position);
        }
        //else _playerID = -1;


        if (userNetworkType == UserNetworkType.Server) {
            appState = AppState.Running;
            canvasHandler.ChangeCanvas("gameCanvas");
        }
        else if (userNetworkType == UserNetworkType.Client)
        {
            appState = AppState.WaitingForServer;
            sendOSC.RegisterOverNetwork(player._ID);
            canvasHandler.ChangeCanvas("waitingCanvas");
        }

    }


    // when server has agreed for client player registration
    public void EndStartProcess()
    {
        appState = AppState.Running;
        canvasHandler.ChangeCanvas("gameCanvas");
    }



    private void Update()
    {
        if (appState == AppState.Running)
        {
            UpdateGame();
        }
    }


    public void InitConnexion(UserNetworkType userNetworkType, UserRole userRole)
    {

        if (gameData.runInLocal == 0)
        {
            osc.outIP = gameData.OSC_IP;
        }
        if (userNetworkType == UserNetworkType.Server)
        {
            osc.inPort = gameData.OSC_ClientOutPort;
            osc.outPort = gameData.OSC_ServerOutPort;
        }
        else if (userNetworkType == UserNetworkType.Client)
        {
            osc.inPort = gameData.OSC_ServerOutPort;
            osc.outPort = gameData.OSC_ClientOutPort;
        }

        osc.Init();
        if (osc.initialized) oscToggle.color = new Color(0, 1, 0);
        else oscToggle.color = new Color(1, 0, 0);
    }



    public void UpdateGame()
    {
    
        int i = 0;
        foreach (int playerID in IDsList)
        { 
            if (playerID == player._ID) { // if this is the actual instance's player
                Debug.Log("Sending local infos :" + players[i].playerGameObject.name);
                sendOSC.SendOSCPosition("/PlayerPosition", player._ID, 0, players[i].head.transform.position);
                sendOSC.SendOSCPosition("/PlayerPosition", player._ID, 1, players[i].leftHand.transform.position);
                sendOSC.SendOSCPosition("/PlayerPosition", player._ID, 2, players[i].rightHand.transform.position);
            } 
            else // update every other player on the network
            {
                Debug.Log("Receiving external infos :" + players[i].playerGameObject.name);
                players[i].head.transform.position = pendingPositionsActualizations[playerID+"Head"];
                players[i].leftHand.transform.position = pendingPositionsActualizations[playerID + "LeftHand"];
                players[i].rightHand.transform.position = pendingPositionsActualizations[playerID + "RightHand"];
            }
            i++;

        }
    }





    public void AddOtherPlayer(int playerID, GameObject parent)
    {
        if (!IDsList.Contains(playerID))
        {
            //int ID = Random.Range(0, 10000);
            PlayerData p = new PlayerData(playerID, playerPrefab, parent, false);
            players.Add(p);
            IDsList.Add(playerID);
            pendingPositionsActualizations.Add(playerID + "Head", p.head.transform.position);
            pendingPositionsActualizations.Add(playerID + "LeftHand", p.leftHand.transform.position);
            pendingPositionsActualizations.Add(playerID + "RightHand", p.rightHand.transform.position);

        }
    }


    public void ErasePlayer(int playerID)
    {
       //PlayerGOs.Remove(GameObject.Find("Player" + playerID.ToString()));
        pendingPositionsActualizations.Remove(playerID + "Head");
        pendingPositionsActualizations.Remove(playerID + "LeftHand");
        pendingPositionsActualizations.Remove(playerID + "RightHand");

        foreach(PlayerData p in players)
        {
            Debug.Log(p._ID + ", " + playerID);

            if(p._ID == playerID)
            {
                Debug.Log("found");
                Destroy(p.playerGameObject);
                players.Remove(p);
                Destroy(p);
                IDsList.Remove(playerID);
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

    void OnApplicationQuit()
    {
        if (userNetworkType == UserNetworkType.Client) 
            sendOSC.SendQuitMessage(userNetworkType, player);
        Debug.Log("Closing Game Engine...");
    }


}
