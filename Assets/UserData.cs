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


    public void Init(int ID, string address, int localPort, GameObject pGameObject, string viveSystemName, int isPlayer, int isLocalPlayer)
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
            GameObject parent = GameObject.Find(viveSystemName);

            if(isLocalPlayer == 1 && parent != null){ // if Init is launched at startup by this instance's player
                head.transform.parent = parent.transform.Find("ViveHead").gameObject.transform;
                leftHand.transform.parent = parent.transform.Find("ViveLeftHand").gameObject.transform;
                rightHand.transform.parent = parent.transform.Find("ViveRightHand").gameObject.transform;
                pGameObject.transform.parent = parent.transform;
            }
            else{ // else we store the player under an empty named Players
                Debug.Log("Vive System not Found");
                
                parent = GameObject.Find("Players");
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
