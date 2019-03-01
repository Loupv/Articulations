using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{

    public Dictionary<int, int> OSCPairs;
    //public GameObject OSCPrefab, networkParent;
    public GameEngine gameEngine;

    // Start is called before the first frame update
    void Start()
    {
        //networkParent = this.gameObject;
        OSCPairs = new Dictionary<int, int>();
    }


    public bool AddNewPairingService(UserData user, int inPort, int outPort, string address, UserNetworkType userNetworkType)
    {

        bool initState = InitConnexion(user, inPort, outPort, address, userNetworkType);

        return initState;
    }



    public bool InitConnexion(UserData user, int inPort, int outPort, string adress, UserNetworkType userNetworkType)
    {
        user.osc = new OSC();
        user.osc.outIP = adress;
        user.osc.inPort = inPort;
        user.osc.outPort = outPort;

        user.osc.Init();
        if (user.osc.initialized) return true;
        return false;
    }



    public void SendPlayerPosition(UserData player, List<UserData> players)
    {
        int i = 0;
        foreach (UserData p in players)
        {

            if (player._ID == p._ID)
            { // if this is the actual instance's player
                Debug.Log("Sending local infos :" + players[i].playerGameObject.name);
                player.osc.sender.SendOSCPosition("/PlayerPosition", player._ID, 0, players[i].head.transform.position);
                player.osc.sender.SendOSCPosition("/PlayerPosition", player._ID, 1, players[i].leftHand.transform.position);
                player.osc.sender.SendOSCPosition("/PlayerPosition", player._ID, 2, players[i].rightHand.transform.position);
            }

            i++;

        }
    }

    public bool CheckPortAvailability(int requestedPort)
    {
        foreach(int port in OSCPairs.Values)
        {
            if (port==requestedPort) return false;
        }
        return true;
    }


    public void FinishUserRegistration(int playerID, int requestedPort)
    {
        OSCPairs.Add(playerID, requestedPort);
    }
}
