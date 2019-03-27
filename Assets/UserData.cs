using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public struct OSCEndPoint{
    public string ip;
    public int remotePort;
}



public class UserData : MonoBehaviour
{

    public int _isPlaying;
    //public OSC osc;
    public int _ID;
    public GameObject playerGameObject, head, leftHand, rightHand;
    public OSCEndPoint oscEndPoint;


    public void Init(int ID, string address, int localPort, GameObject pGameObject, GameObject parent, int isPlaying, bool animatorMode)
    {
        
        _ID = ID;
        _isPlaying = isPlaying;

        oscEndPoint = new OSCEndPoint();
        oscEndPoint.ip = address;
        oscEndPoint.remotePort = localPort;

        
        pGameObject.transform.position += new Vector3(0, Random.Range(-2f, 1.5f), 0);
        pGameObject.transform.parent = parent.transform;

        head = pGameObject.transform.Find("Head").gameObject;
        leftHand = pGameObject.transform.Find("LeftHand").gameObject;
        rightHand = pGameObject.transform.Find("RightHand").gameObject;

        pGameObject.name = "Player" + _ID.ToString();
    
        if (isPlaying == 0)
        {
            if (animatorMode) pGameObject.GetComponent<Animator>().SetTrigger("isLocalPlayer");
            else Destroy(pGameObject.GetComponent<Animator>());
            playerGameObject = pGameObject;

        }
    }


}
