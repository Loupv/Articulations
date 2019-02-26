using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendOSC : MonoBehaviour {

    public OSC osc;
    public GameEngine gameEngine;
    private OscMessage message;



    public void RegisterOverNetwork(int playerID)
    {
        if (osc.initialized)
        {
            message = new OscMessage();
            message.address = "/PlayerRegistrationRequest";
            message.values.Add(playerID);
            osc.Send(message);
            Debug.Log("Sending : " + message);
        }
    }

    public void ConfirmRegistration(int playerID)
    {
        if (osc.initialized)
        {
            message = new OscMessage();
            message.address = "/RegistrationConfirmed";
            message.values.Add(playerID);
            osc.Send(message);
            Debug.Log("Sending : " + message);

            AddEveryPlayerToClientDict();
        }
    }



    public void SendOSCPosition(string header, int playerID, int playerPart, Vector3 pos)
    {
        if (osc.initialized)
        {   
            message = new OscMessage();
            message.address = header;
            message.values.Add(playerID);
            message.values.Add(playerPart);
            message.values.Add(pos.x);
            message.values.Add(pos.y);
            message.values.Add(pos.z);

            osc.Send(message);
            //Debug.Log("Sending : " + message);
        }
    }





    public void SendCustomOSCMessage(string header)
    {
        if (osc.initialized)
        {
            message = new OscMessage();
            message.address = header;
            osc.Send(message);
            Debug.Log("Sending : " + message);
        }
    }


    public void SendQuitMessage(UserNetworkType userNetworkType, int playerID)
    {
        if(userNetworkType == UserNetworkType.Client)
        {
            message = new OscMessage();
            message.address = "/ClientHasLeft";
            message.values.Add(playerID);
            osc.Send(message);
            Debug.Log("Sending : " + message);
        }
        else if (userNetworkType == UserNetworkType.Server)
        {
            message = new OscMessage();
            message.address = "/ServerShutDown";
            osc.Send(message);
            Debug.Log("Sending : " + message);
        }

    }


    public void AddEveryPlayerToClientDict()
    {
        foreach (int playerID in gameEngine.IDsList)
        {
                message = new OscMessage();
                message.address = "/AddPlayerFromServer";
                message.values.Add(playerID);
                osc.Send(message);
                Debug.Log("Sending : " + message);
        }
          
    }

}
