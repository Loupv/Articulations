using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public struct OSCEndPoint{
    public string ip;
    public int remotePort;
}



public class UserData : MonoBehaviour
{

    public int _isPlayer;
    //public OSC osc;
    public int _ID;
    public GameObject playerGameObject, head, leftHand, rightHand;
    public OSCEndPoint oscEndPoint;


    public void Init(GameEngine gameEngine, int ID, string address, int localPort, GameObject pGameObject, int isPlayer, int isLocalPlayer)
    {
        
        _ID = ID;
        _isPlayer = isPlayer;

        oscEndPoint = new OSCEndPoint();
        oscEndPoint.ip = address;
        oscEndPoint.remotePort = localPort;

        head = pGameObject.transform.Find("Head").gameObject;
        leftHand = pGameObject.transform.Find("LeftHand").gameObject;
        rightHand = pGameObject.transform.Find("RightHand").gameObject;
        
        //pGameObject.transform.position += new Vector3(0, Random.Range(-2f, 1.5f), 0);
        
        if (isPlayer == 1) // if player and not just viewer
        {

            if(isLocalPlayer == 1 && gameEngine.findVive){ // if Init is launched at startup by this instance's player
                
                GameObject parent = GameObject.Instantiate(gameEngine.ViveSystemPrefab);
                GameObject camera = parent.transform.Find(gameEngine.viveHeadName).gameObject;
                head.transform.parent = camera.transform;
                leftHand.transform.parent = parent.transform.Find(gameEngine.viveLeftHandName).gameObject.transform;
                rightHand.transform.parent = parent.transform.Find(gameEngine.viveRightHandName).gameObject.transform;
                head.transform.position = Vector3.zero;
                leftHand.transform.position = Vector3.zero;
                rightHand.transform.position = Vector3.zero;
                pGameObject.transform.parent = parent.transform;

                Camera.main.gameObject.SetActive(false);
                camera.tag = "MainCamera";
                

            }
            else{ // else we store the player under an empty named Players
                Debug.Log("Vive System Missing");
                
                GameObject parent = GameObject.Find("Players");
                if(parent == null) {
                    parent = new GameObject();
                    parent.name = "Players";
                }
                pGameObject.transform.parent = parent.transform;                
            }

            pGameObject.name = "Player" + _ID.ToString();
            playerGameObject = pGameObject;
            Color col = new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f));
            head.GetComponent<MeshRenderer>().materials[0].color = col;
            leftHand.GetComponent<MeshRenderer>().materials[0].color = col;
            rightHand.GetComponent<MeshRenderer>().materials[0].color = col;

        }
        else{
            pGameObject.name = "Viewer" + _ID.ToString();
            head.SetActive(false);
            leftHand.SetActive(false);
            rightHand.SetActive(false);
        }
    }


}
