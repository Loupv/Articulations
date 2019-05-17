using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : MonoBehaviour
{

    public List<UserData> usersPlaying;
    public Dictionary<string, Vector3> pendingPositionsActualizations;
    public Dictionary<string, Quaternion> pendingRotationsActualizations;
    public GameObject playerPrefab, viewerPrefab, LongTrailsPrefab, ShortTrailsPrefab;
    
    [HideInInspector]
    public GameObject _userGameObject;

    public bool keepNamesVisibleForPlayers;

    // Start is called before the first frame update
    

    void Start()
    {
        pendingPositionsActualizations = new Dictionary<string, Vector3>();
        pendingRotationsActualizations = new Dictionary<string, Quaternion>();
    }


    public UserData InitLocalUser(GameEngine gameEngine, int ID, string name, string address, int localPort, bool isMe, UserRole userRole){
        
        if (userRole == UserRole.Player) _userGameObject = Instantiate(playerPrefab);
        else if(userRole == UserRole.Viewer || userRole == UserRole.Server)  _userGameObject = Instantiate(viewerPrefab);

        if (userRole == UserRole.Viewer) {
            gameEngine.uiHandler.viewerController = _userGameObject.GetComponent<ViewerController>();
            gameEngine.uiHandler.viewerController.InitViewerController(isMe);
        }
        UserData user = _userGameObject.GetComponent<UserData>();
        user.Init(gameEngine, ID, name, address, localPort, _userGameObject, isMe, userRole);

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

        if(role == UserRole.Player) go = Instantiate(playerPrefab);
        else if(role == UserRole.Viewer){
            go = Instantiate(viewerPrefab);
            go.GetComponent<ViewerController>().InitViewerController(false);
        } 
        else go = new GameObject(); // useless

        UserData p = go.GetComponent<UserData>();

        p.Init(gameEngine, ID, name, address, port, go, false, role);
        usersPlaying.Add(p);

        if(role == UserRole.Player){
            StoreUserParts(p);
        }
        return p;
    }



public void ChangeVisualisationMode(int mode, GameEngine gameEngine){

        if(mode == 0){
            foreach(UserData user in usersPlaying){
                if(user._ID == gameEngine._user._ID)
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
        gameEngine.currentVisualisationMode = mode;
    }

    public void ChangeVisualisationParameter(int valueId, float value){
            
        if(valueId == 0){
            foreach(GameObject hand in GameObject.FindGameObjectsWithTag("HandParticleSystem")){
                hand.GetComponent<TrailRenderer>().time = value;
            }
        }
    }



    public void StoreUserParts(UserData user){

        pendingPositionsActualizations.Add(user._ID + "Head", user.head.transform.position);
        pendingPositionsActualizations.Add(user._ID + "LeftHand", user.leftHand.transform.position);
        pendingPositionsActualizations.Add(user._ID + "RightHand", user.rightHand.transform.position);
        pendingRotationsActualizations.Add(user._ID + "Head", user.head.transform.rotation);
        pendingRotationsActualizations.Add(user._ID + "LeftHand", user.leftHand.transform.rotation);
        pendingRotationsActualizations.Add(user._ID + "RightHand", user.rightHand.transform.rotation);

    }


    
    // adjust players positions from stored one
    public void ActualizePlayersPositions(UserData me){
        int i = 0;
        foreach (UserData user in usersPlaying)
        {
            if (user._ID != me._ID && user._userRole == UserRole.Player) // if it's not actual instance's player
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

}
