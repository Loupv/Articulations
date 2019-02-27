﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{

    public int _OSCListenPort;

    public int _ID;
    public GameObject playerGameObject, head, leftHand, rightHand;

    public PlayerData(int ID, GameObject playerPrefab, GameObject parent, bool animatorMode)
    {
        _ID = ID;
        playerGameObject = Instantiate(playerPrefab) as GameObject;
        playerGameObject.transform.position += new Vector3(0, Random.Range(-2f, 1.5f), 0);
        playerGameObject.transform.parent = parent.transform;

        head = playerGameObject.transform.Find("Head").gameObject;
        leftHand = playerGameObject.transform.Find("LeftHand").gameObject;
        rightHand = playerGameObject.transform.Find("RightHand").gameObject;

        playerGameObject.name = "Player" + _ID.ToString();

        if(animatorMode) playerGameObject.GetComponent<Animator>().SetTrigger("isLocalPlayer");
        else Destroy(playerGameObject.GetComponent<Animator>());
    }


}
