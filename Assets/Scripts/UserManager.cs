﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : MonoBehaviour
{

    public List<UserData> usersPlaying;
    public UserData me;
    public Dictionary<string, Vector3> pendingPositionsActualizations;
    public Dictionary<string, Quaternion> pendingRotationsActualizations;
    public GameObject playerPrefab, viewerPrefab, trackerPrefab, TrailRendererPrefab, SparkParticlesPrefab, particles3, particles4;


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

        me = user;

        return user;
    }


    public UserData AddNewUser(GameEngine gameEngine, int ID, string name, string address, int port, UserRole role, int rank)
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

        p.Init(gameEngine, rank, ID, name, address, port, go, false, role, userCol);
        usersPlaying.Add(p);

        if (role == UserRole.Player) {
            StoreUserParts(p);
        }
        return p;
    }



    public void ChangeVisualisationMode(int mode, GameEngine gameEngine) {


         // for clients
        if(me._userRole == UserRole.Server)
            gameEngine.osc.sender.SendVisualisationChange(mode, usersPlaying);


        if (mode == 0) { // other's hand visible, mine are not
            foreach (UserData user in usersPlaying) {
                if (user._ID == gameEngine._user._ID)
                    user.ChangeSkin(this, "noHands");
                else user.ChangeSkin(this, "justHands");
            }
            gameEngine.scenarioEvents.SetSkybox(0);
        }
        else if (mode == 1) { // basic condition, every spheres visible
            foreach (UserData user in usersPlaying) {
                user.ChangeSkin(this, "justHands");
            }
            gameEngine.scenarioEvents.SetSkybox(0);
        }
        else if (mode == 2) { // trails mode1
            foreach (UserData user in usersPlaying) {
                user.ChangeSkin(this, "shortTrails");
            }
        }
        else if (mode == 3) { // trails mode2
            foreach (UserData user in usersPlaying) {
                user.ChangeSkin(this, "longTrails");
            }
        }
        else if (mode == 4) // trails mode3
        {
            foreach (UserData user in usersPlaying)
            {
                if (user._ID == gameEngine._user._ID)
                    user.ChangeSkin(this, "noHands");
                else user.ChangeSkin(this, "shortTrails");
            }
        }
        else if (mode == 5) // one player has left hand visible, other player has right hand visible
        {
            foreach (UserData user in usersPlaying)
            {
                user.ChangeSkin(this, "onehand");
            }
        }
        else if (mode == 6) // trails mode4
        {
            foreach (UserData user in usersPlaying)
            {
                user.ChangeSkin(this, "particles3");
            }
        }
        else if (mode == 7) // trails mode5
        {
            foreach (UserData user in usersPlaying)
            {
                user.ChangeSkin(this, "particles4");
            }
        }
        else if (mode == 8) // trails mode5
        {
            gameEngine.scenarioEvents.ToggleMirror();
            gameEngine.scenarioEvents.SetSkybox(3);
        }
        else if (mode == 9) // trails mode5
        {
            foreach (UserData user in usersPlaying)
            {
                user.ChangeSkin(this, "particles4");
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
                if(me._userRole == UserRole.Server){
                    usersPlaying[i].head.transform.position = pendingPositionsActualizations[user._ID + "Head"]  + usersPlaying[i].calibrationPositionGap;
                    usersPlaying[i].leftHand.transform.position = pendingPositionsActualizations[user._ID + "LeftHand"]  + usersPlaying[i].calibrationPositionGap ;
                    usersPlaying[i].rightHand.transform.position = pendingPositionsActualizations[user._ID + "RightHand"]  + usersPlaying[i].calibrationPositionGap ;
                }
                else {
                    usersPlaying[i].head.transform.position = pendingPositionsActualizations[user._ID + "Head"];
                    usersPlaying[i].leftHand.transform.position = pendingPositionsActualizations[user._ID + "LeftHand"];
                    usersPlaying[i].rightHand.transform.position = pendingPositionsActualizations[user._ID + "RightHand"];
                }
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


    public void TranslateUser(){
        me.gameObject.transform.position += me.calibrationPositionGap;
    }


    public Vector3 GetCalibrationGap(int playerID){
        foreach(UserData user in usersPlaying){
            if(user._ID== playerID) return user.calibrationPositionGap;
        }
        return new Vector3();
    }


}
