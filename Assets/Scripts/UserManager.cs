using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : MonoBehaviour
{

    public List<UserData> usersPlaying;
    public Dictionary<string, Vector3> pendingPositionsActualizations;
    public Dictionary<string, Quaternion> pendingRotationsActualizations;
    public GameObject playerPrefab, viewerPrefab, trackerPrefab, LongTrailsPrefab, ShortTrailsPrefab;


    [HideInInspector]
    public GameObject _userGameObject;
    public NetworkManager networkManager;

    public bool keepNamesVisibleForPlayers;
    public Color userCol;

    // Start is called before the first frame update


    void Start()
    {
        pendingPositionsActualizations = new Dictionary<string, Vector3>();
        pendingRotationsActualizations = new Dictionary<string, Quaternion>();
        userCol = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }


    public UserData InitLocalUser(GameEngine gameEngine, int ID, string name, string address, int localPort, bool isMe, UserRole userRole) {

        if (userRole == UserRole.Player) _userGameObject = Instantiate(playerPrefab);
        else if (userRole == UserRole.Tracker) _userGameObject = Instantiate(trackerPrefab);
        else if (userRole == UserRole.Viewer || userRole == UserRole.Server) _userGameObject = Instantiate(viewerPrefab);

        if (userRole == UserRole.Viewer || userRole == UserRole.Tracker || userRole == UserRole.Server) {
            gameEngine.uiHandler.viewerController = _userGameObject.GetComponent<ViewerController>();
            gameEngine.uiHandler.viewerController.InitViewerController(isMe);
        }
        UserData user = _userGameObject.GetComponent<UserData>();
        user.Init(gameEngine, usersPlaying.Count, ID, name, address, localPort, _userGameObject, isMe, userRole, userCol);

        if (userRole == UserRole.Player)
        {
            usersPlaying.Add(user);
            StoreUserParts(user);
        }

        return user;
    }


    public UserData AddNewUser(GameEngine gameEngine, int ID, string name, string address, int port, UserRole role)
    {
        GameObject go;

        if (role == UserRole.Player) go = Instantiate(playerPrefab);
        else if (role == UserRole.Tracker)
        {
            go = Instantiate(trackerPrefab);
            go.GetComponent<ViewerController>().InitViewerController(false);
        }
        else if (role == UserRole.Viewer)
        {
            go = Instantiate(viewerPrefab);
            go.GetComponent<ViewerController>().InitViewerController(false);
        }
        else go = new GameObject(); // useless

        UserData p = go.GetComponent<UserData>();

        p.Init(gameEngine, usersPlaying.Count, ID, name, address, port, go, false, role, userCol);
        usersPlaying.Add(p);

        if (role == UserRole.Player) {
            StoreUserParts(p);
        }
        return p;
    }



    public void ChangeVisualisationMode(int mode, GameEngine gameEngine) {

        if (mode == 0) {
            foreach (UserData user in usersPlaying) {
                if (user._ID == gameEngine._user._ID)
                    user.ChangeSkin(this, "noHands");
                else user.ChangeSkin(this, "justHands");
            }
        }
        else if (mode == 1) {
            foreach (UserData user in usersPlaying) {
                user.ChangeSkin(this, "justHands");
            }
        }
        else if (mode == 2) {
            foreach (UserData user in usersPlaying) {
                user.ChangeSkin(this, "shortTrails");
            }
        }
        else if (mode == 3) {
            foreach (UserData user in usersPlaying) {
                user.ChangeSkin(this, "longTrails");
            }
        }
        gameEngine.currentVisualisationMode = mode;
    }

    public void ChangeVisualisationParameter(int valueId, float value) {

        if (valueId == 0) {
            foreach (GameObject hand in GameObject.FindGameObjectsWithTag("HandParticleSystem")) {
                hand.GetComponent<TrailRenderer>().time = value;
            }
        }
    }



    public void StoreUserParts(UserData user) {

        pendingPositionsActualizations.Add(user._ID + "Head", user.head.transform.position);
        pendingPositionsActualizations.Add(user._ID + "LeftHand", user.leftHand.transform.position);
        pendingPositionsActualizations.Add(user._ID + "RightHand", user.rightHand.transform.position);
        pendingRotationsActualizations.Add(user._ID + "Head", user.head.transform.rotation);
        pendingRotationsActualizations.Add(user._ID + "LeftHand", user.leftHand.transform.rotation);
        pendingRotationsActualizations.Add(user._ID + "RightHand", user.rightHand.transform.rotation);

    }



    // adjust players positions from stored one
    public void ActualizePlayersPositions(UserData me) {
        int i = 0;
        foreach (UserData user in usersPlaying)
        {
            if (user._ID != me._ID && user._userRole == UserRole.Player) // if it's not actual instance's player
            {
                usersPlaying[i].head.transform.position = pendingPositionsActualizations[user._ID + "Head"] - usersPlaying[i].calibrationPositionGap;
                usersPlaying[i].leftHand.transform.position = pendingPositionsActualizations[user._ID + "LeftHand"] - usersPlaying[i].calibrationPositionGap;
                usersPlaying[i].rightHand.transform.position = pendingPositionsActualizations[user._ID + "RightHand"] - usersPlaying[i].calibrationPositionGap;
                usersPlaying[i].head.transform.rotation = pendingRotationsActualizations[user._ID + "Head"];
                usersPlaying[i].leftHand.transform.rotation = pendingRotationsActualizations[user._ID + "LeftHand"];
                usersPlaying[i].rightHand.transform.rotation = pendingRotationsActualizations[user._ID + "RightHand"];
            }
            i++;
        }
    }



    public int ReturnPlayerRank(int n) { // n may be equal to 1 or 2 (player1 or 2) 
        int r = 0;
        int i = 0;
        foreach (UserData user in usersPlaying) {
            if (user._userRole == UserRole.Player) r += 1; // if we find a player thats number r
            if (r == n) return i; // if r was needed, return it
            i++;
        }
        Debug.Log("Player not found");
        return -1;
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

    public void CalibratePlayerTransform()
    {
        foreach (UserData user in usersPlaying) // we take each actual player one by one
        {
            if (user._userRole == UserRole.Player)
            {
                user.calibrationPositionGap = user.head.transform.position;
                networkManager.SendClientPositionGap(user, usersPlaying); // we send for each of them the list of positiongaps
                // upon reception, each user has to adapt its own position to be centered
            }
        }
    }
}
