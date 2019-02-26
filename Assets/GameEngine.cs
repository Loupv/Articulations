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
    public string networkID;
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
    public List<GameObject> players;
    public int _playerID;

    private JSONLoader jSONLoader;
    private GameData gameData;

    public Image oscToggle;
    public Text sentMessage;
    public Object playerPrefab;
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
            _playerID = Random.Range(0, 10000);
            InitPlayer(_playerID, playerParent);
            GameObject.Find("Player" + _playerID).GetComponent<Animator>().SetTrigger("isLocalPlayer");
        }
        else _playerID = -1;


        if (userNetworkType == UserNetworkType.Server) {
            appState = AppState.Running;
            canvasHandler.ChangeCanvas("gameCanvas");
        }
        else if (userNetworkType == UserNetworkType.Client)
        {
            appState = AppState.WaitingForServer;
            sendOSC.RegisterOverNetwork(_playerID);
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
        // différencier serveur et client

        // ici on doit juste envoyer les position du joueur / confusion thisPlayer et all players
        // si serveur on doit envoyer toutes les positions
        // si client on doit juste envoyer la sienne

        int i = 0;
        foreach (int playerID in IDsList)
        { 
            if (playerID == _playerID) { // if this is the actual instance's player
                Debug.Log("Sending local infos :" + players[i]);
                sendOSC.SendOSCPosition("/PlayerPosition", _playerID, 0, players[i].transform.GetChild(0).position);
                sendOSC.SendOSCPosition("/PlayerPosition", _playerID, 1, players[i].transform.GetChild(1).position);
                sendOSC.SendOSCPosition("/PlayerPosition", _playerID, 2, players[i].transform.GetChild(2).position);
            } 
            else // update every other player on the network
            {
                Debug.Log("Receiving external infos :" + players[i]);
                players[i].transform.GetChild(0).position = pendingPositionsActualizations[playerID+"Head"];
                players[i].transform.GetChild(1).position = pendingPositionsActualizations[playerID + "LeftHand"];
                players[i].transform.GetChild(2).position = pendingPositionsActualizations[playerID + "RightHand"];
            }
            i++;

        }
    }



    public void InitPlayer(int playerID, GameObject parent)
    {
        if (!IDsList.Contains(playerID))
        {
            GameObject player = Instantiate(playerPrefab) as GameObject;
            player.transform.position += new Vector3(0,Random.Range(-2f, 1.5f),0);
            player.transform.parent = playerParent.transform;

            GameObject head = player.transform.Find("Head").gameObject;
            GameObject leftHand = player.transform.Find("LeftHand").gameObject;
            GameObject rightHand = player.transform.Find("RightHand").gameObject;

            player.name = "Player" + playerID.ToString();
            players.Add(player);

            //PlayerGOs.Add(player);
            IDsList.Add(playerID);
            pendingPositionsActualizations.Add(playerID+"Head", head.transform.position);
            pendingPositionsActualizations.Add(playerID + "LeftHand", leftHand.transform.position);
            pendingPositionsActualizations.Add(playerID + "RightHand", rightHand.transform.position);

        }
    }

    public void AddOtherPlayer(int playerID, GameObject parent)
    {
        if (!IDsList.Contains(playerID))
        {
            GameObject player = Instantiate(playerPrefab) as GameObject;
            player.transform.parent = playerParent.transform;
            Destroy(player.GetComponent<Animator>());
            GameObject head = player.transform.Find("Head").gameObject;
            GameObject leftHand = player.transform.Find("LeftHand").gameObject;
            GameObject rightHand = player.transform.Find("RightHand").gameObject;

            player.name = "Player" + playerID.ToString();
            players.Add(player);

            //PlayerGOs.Add(player);
            IDsList.Add(playerID);
            pendingPositionsActualizations.Add(playerID + "Head", head.transform.position);
            pendingPositionsActualizations.Add(playerID + "LeftHand", leftHand.transform.position);
            pendingPositionsActualizations.Add(playerID + "RightHand", rightHand.transform.position);

        }
    }


    public void ErasePlayer(int playerID)
    {
       //PlayerGOs.Remove(GameObject.Find("Player" + playerID.ToString()));
        pendingPositionsActualizations.Remove(playerID + "Head");
        pendingPositionsActualizations.Remove(playerID + "LeftHand");
        pendingPositionsActualizations.Remove(playerID + "RightHand");
        Destroy(GameObject.Find("Player" + playerID.ToString()));
        IDsList.Remove(playerID);
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
            sendOSC.SendQuitMessage(userNetworkType, _playerID);
        Debug.Log("Closing Game Engine...");
    }


}
