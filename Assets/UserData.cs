using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public struct OSCEndPoint{
    string ip;
    int remotePort;
}



public class UserData : MonoBehaviour
{

    public int _isPlaying;
    //public OSC osc;
    public int _ID;
    public GameObject playerGameObject, head, leftHand, rightHand;
    public OSCEndPoint oscEndPoint;


    public UserData(int ID, GameData gameData, GameObject playerPrefab, GameObject parent, int isPlaying, bool animatorMode)
    {
        _ID = ID;
        _isPlaying = isPlaying;

        oscEndPoint = new OSCEndPoint();
        oscEndPoint.ip = gameData.OSC_IP;
        oscEndPoint.remotePort = gameData.OSC_LocalPort;
        

        if (isPlaying == 0)
        {
            playerGameObject = Instantiate(playerPrefab) as GameObject;
            playerGameObject.transform.position += new Vector3(0, Random.Range(-2f, 1.5f), 0);
            playerGameObject.transform.parent = parent.transform;

            head = playerGameObject.transform.Find("Head").gameObject;
            leftHand = playerGameObject.transform.Find("LeftHand").gameObject;
            rightHand = playerGameObject.transform.Find("RightHand").gameObject;

            playerGameObject.name = "Player" + _ID.ToString();

            if (animatorMode) playerGameObject.GetComponent<Animator>().SetTrigger("isLocalPlayer");
            else Destroy(playerGameObject.GetComponent<Animator>());

        }

    }


}
