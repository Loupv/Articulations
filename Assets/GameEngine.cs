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

    public Dictionary<string, Vector3> playersPositions;
    public List<int> IDsList;

    public OSC osc;
    public SendOSC sendOSC;
    public ReceiveOSC receiveOSC;
    public CanvasHandler canvasHandler;

    public List<GameObject> PlayerGOs;
    //public List<GameObject> OtherPlayers;
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

        playersPositions = new Dictionary<string, Vector3>();

    }

    // Start the performance
    public void StartGame()
    {
        InitConnexion(userNetworkType, userRole);


        if (userRole == UserRole.Player)
        {
            _playerID = Random.Range(0, 10000);
            AddPlayer(_playerID, playerParent);
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
        if (PlayerGOs != null && PlayerGOs.Count > 0)
        {
            foreach (GameObject player in PlayerGOs)
            {

                Transform headTransform = player.transform.GetChild(0);
                Transform leftHandTransform = player.transform.GetChild(1);
                Transform rightHandTransform = player.transform.GetChild(2);

                sendOSC.SendOSCPosition("/PlayerPosition", _playerID, 0, headTransform.position);
                sendOSC.SendOSCPosition("/PlayerPosition", _playerID, 1, leftHandTransform.position);
                sendOSC.SendOSCPosition("/PlayerPosition", _playerID, 2, rightHandTransform.position);

            }
        }

        // ici on doit ajuster les positions de tout le monde
        if (playersPositions.Count > 0)
        {
            foreach (KeyValuePair<string, Vector3> player in playersPositions)
            {
                // update other players positions
            }
        }

    }



    public void AddPlayer(int playerID, GameObject parent)
    {
        if (!IDsList.Contains(playerID))
        {
            GameObject player = Instantiate(playerPrefab) as GameObject;
            player.transform.parent = playerParent.transform;

            GameObject head = player.transform.Find("Head").gameObject;
            GameObject leftHand = player.transform.Find("LeftHand").gameObject;
            GameObject rightHand = player.transform.Find("RightHand").gameObject;

            player.name = "Player" + playerID.ToString();
            head.name = "Player" + playerID.ToString() + "Head";
            leftHand.name = "Player" + playerID.ToString() + "LeftHand";
            rightHand.name = "Player" + playerID.ToString() + "RightHand";

            PlayerGOs.Add(player);
            IDsList.Add(playerID);
            playersPositions.Add(head.name, head.transform.position);
            playersPositions.Add(leftHand.name, leftHand.transform.position);
            playersPositions.Add(rightHand.name, rightHand.transform.position);

            //head.transform.parent = parent.transform;
            //leftHand.transform.parent = parent.transform;
            //rightHand.transform.parent = parent.transform;
        }
    }

    public void ErasePlayer(int playerID)
    {
        playersPositions.Remove("Player" + playerID.ToString() + "Head");
        playersPositions.Remove("Player" + playerID.ToString() + "LeftHand");
        playersPositions.Remove("Player" + playerID.ToString() + "RightHand");
        PlayerGOs.Remove(GameObject.Find("Player" + playerID.ToString()));
        Destroy(GameObject.Find("Player" + playerID.ToString()));
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
