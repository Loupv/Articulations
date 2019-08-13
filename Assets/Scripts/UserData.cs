using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public struct OSCEndPoint{
    public string ip;
    public int remotePort;
}



public class UserData : MonoBehaviour
{

    public UserRole _userRole;
    //public OSC osc;
    public int _ID;
    public string _playerName;
    public GameObject playerGameObject, head, leftHand, rightHand;//, leftHold, rightHold;
    public UnityEngine.UI.Text headText;
    public OSCEndPoint oscEndPoint;
    public Vector3 calibrationPositionGap;
    public Quaternion calibrationRotationGap;
    public int _registeredRank;


    public void InitUserData(GameEngine gameEngine, int rank, int ID, string playerName, string address, int localPort, GameObject pGameObject, bool isMe, UserRole userRole)
    {
        
        _ID = ID;

        _userRole = userRole;
        _playerName = playerName;
        _registeredRank = rank;

        oscEndPoint = new OSCEndPoint();
        oscEndPoint.ip = address;
        oscEndPoint.remotePort = localPort;

        if(userRole == UserRole.Player || (!isMe && userRole == UserRole.Playback)){
            head = pGameObject.transform.Find("Head").gameObject;
            leftHand = pGameObject.transform.Find("LeftHand").gameObject;
            rightHand = pGameObject.transform.Find("RightHand").gameObject;
        }


        if(gameEngine._userRole == UserRole.Player && !gameEngine.userManager.keepNamesVisibleForPlayers) // mask UI for players when not wanted
        {
            headText = head.transform.Find("Canvas").Find("Text").GetComponent<UnityEngine.UI.Text>();
            headText.text = _playerName;
            headText.gameObject.SetActive(false);
        }
        else if(head != null){
            headText = head.transform.Find("Canvas").Find("Text").GetComponent<UnityEngine.UI.Text>();
            headText.text = _playerName;
            if(gameEngine._userRole == UserRole.Playback) headText.gameObject.SetActive(false); // tmp for screen record
        }


        // things that change depending on the kind of player we try to instantiate
        // PLAYER //
        if (_userRole == UserRole.Player)
        {            
            
            if(!gameEngine.useVRHeadset) Debug.Log("Not looking for Vive System");

            PlaceUserPartsInScene(gameEngine, gameEngine.useVRHeadset, pGameObject, isMe); 
            
            pGameObject.name = "Player" + _ID.ToString();
            playerGameObject = pGameObject;
        }
        // TRACKER //
        else if (_userRole == UserRole.Tracker)
        {
            pGameObject.name = "Tracker" + _ID.ToString();
            PlaceUserPartsInScene(gameEngine, false, pGameObject, isMe);
        }
        // PLAYBACK //
        else if (userRole == UserRole.Playback)
        {            
            PlaceUserPartsInScene(gameEngine, gameEngine.useVRHeadset, pGameObject, isMe); 
            if(isMe) pGameObject.name = "Playback" + _ID.ToString();
            else pGameObject.name = "PlaybackPlayer" + _ID.ToString();
            playerGameObject = pGameObject;
        }
        // VIEWER //
        else{
            pGameObject.name = "Viewer" + _ID.ToString();
            PlaceUserPartsInScene(gameEngine, false, pGameObject, isMe);
        }



        calibrationPositionGap = new Vector3();
        calibrationRotationGap = new Quaternion();


    }


    void PlaceUserPartsInScene(GameEngine gameEngine, bool vr, GameObject pGameObject, bool isMe){

        GameObject parent;

        if(vr && isMe){

            parent = Instantiate(gameEngine.ViveSystemPrefab);
            GameObject cam = parent.transform.Find(gameEngine.viveHeadName).gameObject;
            cam.tag = "MainCamera";
            Camera.main.gameObject.SetActive(false);
            //
            Camera camera = cam.GetComponent<Camera>();
            camera.cullingMask = ~(1 << 9);
         //   head.layer = 9;
            //
            head.transform.parent = cam.transform;
            leftHand.transform.parent = parent.transform.Find(gameEngine.viveLeftHandName).gameObject.transform;
            rightHand.transform.parent = parent.transform.Find(gameEngine.viveRightHandName).gameObject.transform;
        
            SetPlayerPosition(Vector3.zero,Vector3.zero,Vector3.zero);

            pGameObject.transform.parent = parent.transform;

        }
        else{
            parent = GameObject.Find("Players");
            if(parent == null) {
                parent = new GameObject();
                parent.name = "Players";
            }

        }
        pGameObject.transform.parent = parent.transform;  

    }


    public void ChangeSkin(UserManager userManager, string skin){

        //leftHand.transform.position = new Vector3();
        //rightHand.transform.position = new Vector3();

        if (skin == "noHands"){
            RemovePlayerHold();
            leftHand.GetComponent<MeshRenderer>().enabled = false;            
            rightHand.GetComponent<MeshRenderer>().enabled = false;
            head.GetComponent<MeshRenderer>().enabled = true;
        }
        else if(skin == "all"){
            RemovePlayerHold();
            leftHand.GetComponent<MeshRenderer>().enabled = true;            
            rightHand.GetComponent<MeshRenderer>().enabled = true;
            head.GetComponent<MeshRenderer>().enabled = true;
        }
        else if(skin == "longTrails"){
            leftHand.GetComponent<MeshRenderer>().enabled = false;            
            rightHand.GetComponent<MeshRenderer>().enabled = false;
            head.GetComponent<MeshRenderer>().enabled = false;
            ReplacePlayerHold(userManager.SparkParticlesPrefab);
         
        }
        else if(skin == "trails"){
            leftHand.GetComponent<MeshRenderer>().enabled = false;            
            rightHand.GetComponent<MeshRenderer>().enabled = false;
            head.GetComponent<MeshRenderer>().enabled = false;
            ReplacePlayerHold(userManager.TrailRendererPrefab);            
        }
        else if(skin == "particles3"){
            leftHand.GetComponent<MeshRenderer>().enabled = false;            
            rightHand.GetComponent<MeshRenderer>().enabled = false;
            head.GetComponent<MeshRenderer>().enabled = false;
            ReplacePlayerHold(userManager.particles3);            
        }
        else if(skin == "particles4"){
            leftHand.GetComponent<MeshRenderer>().enabled = false;            
            rightHand.GetComponent<MeshRenderer>().enabled = false;
            head.GetComponent<MeshRenderer>().enabled = false;
            ReplacePlayerHold(userManager.particles4);            
        }
        else if (skin == "onehand")
        {
            RemovePlayerHold();
            if (_registeredRank % 2 == 1)
            {
                leftHand.GetComponent<MeshRenderer>().enabled = false;
                rightHand.GetComponent<MeshRenderer>().enabled = true;
            }
            else if (_registeredRank % 2 == 0)
            {
                leftHand.GetComponent<MeshRenderer>().enabled = true;
                rightHand.GetComponent<MeshRenderer>().enabled = false;
            }
            head.GetComponent<MeshRenderer>().enabled = true;
        }
        else if (skin == "nothing")
        {
            RemovePlayerHold();
            head.GetComponent<MeshRenderer>().enabled = false;
            leftHand.GetComponent<MeshRenderer>().enabled = false;
            rightHand.GetComponent<MeshRenderer>().enabled = false;
        }

    }



    void RemovePlayerHold(){
        foreach(Transform t in leftHand.transform){
            Destroy(t.gameObject);
        }
        foreach(Transform t in rightHand.transform){
            Destroy(t.gameObject);
        }
        foreach (Transform t in head.transform)
        {
            Destroy(t.gameObject);
        }
    }


    void ReplacePlayerHold(GameObject hold){
        
        RemovePlayerHold();

        GameObject tmp1 = Instantiate(hold);
        GameObject tmp2 = Instantiate(hold);
        GameObject tmp3 = Instantiate(hold);
        tmp1.transform.parent = leftHand.transform;
        tmp2.transform.parent = rightHand.transform;
        tmp3.transform.parent = head.transform;
        tmp1.transform.position = leftHand.transform.position;
        tmp2.transform.position = rightHand.transform.position;
        tmp3.transform.position = head.transform.position;
        tmp3.layer = 9;
    }


    public void SetPlayerPosition(Vector3 headPosition, Vector3 leftHandPosition, Vector3 rightHandPosition){
        head.transform.position = headPosition;
        leftHand.transform.position = leftHandPosition;
        rightHand.transform.position = rightHandPosition;
    }
    public void SetPlayerRotation(Quaternion headRotation, Quaternion leftHandRotation, Quaternion rightHandRotation){
        head.transform.rotation = headRotation;
        leftHand.transform.rotation = leftHandRotation;
        rightHand.transform.rotation = rightHandRotation;
    }

    
    /*public void CalibratePlayerTransform(Vector3 centeredPosition, GameObject sceneGameObjects)
    {
        sceneGameObjects.transform.position = this.transform.position;
        calibrationPositionGap = centeredPosition - this.transform.position;
        //calibrationRotationGap = this.transform.rotation.SetLookRotation(lookAt);
    }*/

}
