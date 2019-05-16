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


    public void Init(GameEngine gameEngine, int ID, string playerName, string address, int localPort, GameObject pGameObject, bool isMe, UserRole userRole)
    {
        
        _ID = ID;

        _userRole = userRole;
        _playerName = playerName; 

        oscEndPoint = new OSCEndPoint();
        oscEndPoint.ip = address;
        oscEndPoint.remotePort = localPort;

        head = pGameObject.transform.Find("Head").gameObject;
        leftHand = pGameObject.transform.Find("LeftHand").gameObject;
        rightHand = pGameObject.transform.Find("RightHand").gameObject;
        
        //pGameObject.transform.position += new Vector3(0, Random.Range(-2f, 1.5f), 0);

        // things that change depending on this instance's mode
        if((gameEngine._userRole == UserRole.Player && gameEngine.userManager.keepNamesVisibleForPlayers) // if we're a player and we decided to keep UI
        || ((gameEngine._userRole == UserRole.Viewer || gameEngine._userRole == UserRole.Server) && _userRole == UserRole.Player)) // if we're a viewer and we instantiate a plyer
        {
            headText = head.transform.Find("Canvas").Find("Text").GetComponent<UnityEngine.UI.Text>();
            headText.text = _playerName;
        }
        else if(gameEngine._userRole == UserRole.Player && !gameEngine.userManager.keepNamesVisibleForPlayers) // mask UI for players when not wanted
        {
            headText = head.transform.Find("Canvas").Find("Text").GetComponent<UnityEngine.UI.Text>();
            headText.text = _playerName;
            headText.gameObject.SetActive(false);
        }


        // things that change depending on the kind of player we try to instantiate
        // PLAYER //
        if (_userRole == UserRole.Player)
        {            
            
            if(!gameEngine.useVRHeadset) Debug.Log("Not looking for Vive System");

            PlaceUserPartsInScene(gameEngine, gameEngine.useVRHeadset, pGameObject, isMe); 
            
            pGameObject.name = "Player" + _ID.ToString();
            playerGameObject = pGameObject;

            Color col = new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f));
            head.GetComponent<MeshRenderer>().materials[0].color = col;
            leftHand.GetComponent<MeshRenderer>().materials[0].color = col;
            rightHand.GetComponent<MeshRenderer>().materials[0].color = col;
        }
        
        // VIEWER //
        else{
            pGameObject.name = "Viewer" + _ID.ToString();
            PlaceUserPartsInScene(gameEngine, false, pGameObject, isMe);

            head.SetActive(false);
            leftHand.SetActive(false);
            rightHand.SetActive(false);
        }
        
    }


    void PlaceUserPartsInScene(GameEngine gameEngine, bool vr, GameObject pGameObject, bool isMe){

        GameObject parent;

        if(vr && isMe){

            parent = Instantiate(gameEngine.ViveSystemPrefab);
            GameObject cam = parent.transform.Find(gameEngine.viveHeadName).gameObject;
            cam.tag = "MainCamera";
            Camera.main.gameObject.SetActive(false);

            head.transform.parent = cam.transform;
            leftHand.transform.parent = parent.transform.Find(gameEngine.viveLeftHandName).gameObject.transform;
            rightHand.transform.parent = parent.transform.Find(gameEngine.viveRightHandName).gameObject.transform;
        
            head.transform.position = Vector3.zero;
            leftHand.transform.position = Vector3.zero;
            rightHand.transform.position = Vector3.zero;
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


    public void ChangeSkin(GameEngine gameEngine, string skin){
        if(skin == "noHands"){
            RemovePlayerHold();
            leftHand.GetComponent<MeshRenderer>().enabled = false;            
            rightHand.GetComponent<MeshRenderer>().enabled = false;
            //leftHold.SetActive(false);
            //rightHold.SetActive(false);
        }
        else if(skin == "justHands"){
            RemovePlayerHold();
            leftHand.GetComponent<MeshRenderer>().enabled = true;            
            rightHand.GetComponent<MeshRenderer>().enabled = true;
            //leftHold.SetActive(false);
            //rightHold.SetActive(false);
        }
        else if(skin == "longTrails"){
            leftHand.GetComponent<MeshRenderer>().enabled = false;            
            rightHand.GetComponent<MeshRenderer>().enabled = false;          
            ReplacePlayerHold(gameEngine.LongTrailsPrefab);
        }
        else if(skin == "shortTrails"){
            leftHand.GetComponent<MeshRenderer>().enabled = false;            
            rightHand.GetComponent<MeshRenderer>().enabled = false;
            ReplacePlayerHold(gameEngine.ShortTrailsPrefab);            
        }
    }


    void RemovePlayerHold(){
        foreach(Transform t in leftHand.transform){
            Destroy(t.gameObject);
        }
        foreach(Transform t in rightHand.transform){
            Destroy(t.gameObject);
        }
    }
    void ReplacePlayerHold(GameObject hold){
        
        RemovePlayerHold();

        GameObject tmp1 = Instantiate(hold);
        GameObject tmp2 = Instantiate(hold);
        tmp1.transform.parent = leftHand.transform;
        tmp2.transform.parent = rightHand.transform;        
        tmp1.transform.position = leftHand.transform.position;
        tmp2.transform.position = rightHand.transform.position;
    }

}
