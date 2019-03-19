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
    public int OSC_LocalPort, OSC_OutPort;
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
    public List<int> IDsList;
    public List<UserData> usersPlaying;
    //public int _playerID;
    public UserData _user;

    public OSC osc;
    //private SendOSC sender;
    //private ReceiveOSC receiver;

    private JSONLoader jSONLoader;
    private UIHandler uiHandler;
    public GameData gameData;
    public Image oscToggle;
    public Text sentMessage;
    public GameObject playerPrefab;
    public GameObject playerParent;

    public UserNetworkType userNetworkType;
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
        uiHandler = GetComponentInChildren<UIHandler>();
        userNetworkType = UserNetworkType.Server;

        jSONLoader = new JSONLoader();
        gameData = jSONLoader.LoadGameData("/StreamingAssets/GameData.json");

        uiHandler.OSCInPortInput.text = gameData.OSC_InPort.ToString();
        uiHandler.OSCOutPortInput.text = gameData.OSC_OutPort.ToString();

        if (gameData.runInLocal == 1) uiHandler.OSCAddressInput.text = "127.0.0.1";
        else uiHandler.OSCAddressInput.text = gameData.OSC_IP;
        uiHandler.ChangeOSCConfig();
        uiHandler.SetPlayerNetworkType();
        uiHandler.SetPlayerRole();

        pendingPositionsActualizations = new Dictionary<string, Vector3>();

    }

    // Start the performance
    public void StartGame(int isPlayer)
    {

        int ID = Random.Range(0, 10000);
        _user = new UserData(ID, playerPrefab, playerParent, isPlayer, true);

        if (isPlayer == 0) // if true
        {
            usersPlaying.Add(_user);
            IDsList.Add(_user._ID);

            pendingPositionsActualizations.Add(_user._ID + "Head", _user.head.transform.position);
            pendingPositionsActualizations.Add(_user._ID + "LeftHand", _user.leftHand.transform.position);
            pendingPositionsActualizations.Add(_user._ID + "RightHand", _user.rightHand.transform.position);
        }


        if (userNetworkType == UserNetworkType.Server)
        {
            appState = AppState.Running;
            canvasHandler.ChangeCanvas("gameCanvas");

            osc.Init(userNetworkType);

            if (networkManager.StartServerListener(uiHandler.OSCLocalPort))
                oscToggle.color = new Color(0, 1, 0);
            else oscToggle.color = new Color(1, 0, 0);


        }
        else if (userNetworkType == UserNetworkType.Client)
        {
            if (networkManager.AddNewPairingService(_user, uiHandler.OSCLocalPort, uiHandler.address, userNetworkType))
                oscToggle.color = new Color(0, 1, 0);
            else oscToggle.color = new Color(1, 0, 0);

            appState = AppState.WaitingForServer;
            osc.sender.RequestUserRegistation(osc, _user._ID, osc.remotePort);
            canvasHandler.ChangeCanvas("waitingCanvas");
        }

    }


    // when server has agreed for client player registration
    public void EndStartProcess(int playerID, int requestedPort)
    {
        //networkManager.FinishUserRegistration(playerID, requestedPort);

        if (userNetworkType == UserNetworkType.Client)
        {
            appState = AppState.Running;
            canvasHandler.ChangeCanvas("gameCanvas");
        }
    }



    private void Update()
    {
        if (appState == AppState.Running)
        {
            UpdateGame();
        }
    }






    public void UpdateGame()
    {

        networkManager.SendPlayerPosition(_user, usersPlaying);

        int i = 0;
        foreach (int playerID in IDsList)
        {
            if (playerID != _user._ID) // if it's not actual instance's player
            {
                Debug.Log("Receiving external infos :" + usersPlaying[i].playerGameObject.name);
                usersPlaying[i].head.transform.position = pendingPositionsActualizations[playerID + "Head"];
                usersPlaying[i].leftHand.transform.position = pendingPositionsActualizations[playerID + "LeftHand"];
                usersPlaying[i].rightHand.transform.position = pendingPositionsActualizations[playerID + "RightHand"];
            }
            i++;

        }
    }




    public UserData AddOtherPlayer(int playerID, string address, int port)
    {

        if (!IDsList.Contains(playerID))
        {
            //int ID = Random.Range(0, 10000);
            UserData p = new UserData(playerID, playerPrefab, playerParent, 0, false);
            usersPlaying.Add(p);
            IDsList.Add(playerID);
            pendingPositionsActualizations.Add(playerID + "Head", p.head.transform.position);
            pendingPositionsActualizations.Add(playerID + "LeftHand", p.leftHand.transform.position);
            pendingPositionsActualizations.Add(playerID + "RightHand", p.rightHand.transform.position);
            return p;
        }
        else return null;
    }

    public void ErasePlayer(int playerID)
    {
        //PlayerGOs.Remove(GameObject.Find("Player" + playerID.ToString()));
        pendingPositionsActualizations.Remove(playerID + "Head");
        pendingPositionsActualizations.Remove(playerID + "LeftHand");
        pendingPositionsActualizations.Remove(playerID + "RightHand");

        foreach (UserData p in usersPlaying)
        {
            if (p._ID == playerID)
            {
                Destroy(p.playerGameObject);
                usersPlaying.Remove(p);
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


    public void OnApplicationQuit()
    {
        if (userNetworkType == UserNetworkType.Client)
            osc.sender.SendQuitMessage(osc, userNetworkType, _user);
        Debug.Log("Closing Game Engine...");
    }


}
