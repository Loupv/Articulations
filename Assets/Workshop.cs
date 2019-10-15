using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Workshop : MonoBehaviour
{

    /*
    - Link to user parts

    - Examples on how to change the environment / lights

    - Eyesweb informations accessible, examples

        Personnel
            Kinetic Energy
            Hauteur des mains / tête (0 à 1)
            Activité des deux mains / gauche / droite

        Collaboratif
            Position du barycentre
            Distance des têtes
            Distance minimale entre les sphères de l’un et l’autre

    - Possibilité d'envoyer un ordre OSC custom côté server / et de le recevoir côté client

    
    
    
    
     */

    public OSC osc;
    public List<UserData> _usersPlaying;
    bool initialized;


    // Start is called before the first frame update
    void Init(List<UserData> usersPlaying)
    {
        StartListening();
        _usersPlaying = usersPlaying;
        initialized = true;
    }




    // Update is called once per frame
    void Update()
    {
        foreach(UserData user in _usersPlaying){

            // do blablabla

        }

        
    }



    /*
    ----------------------------- OSC----------------------------
     */

    /*
    OSC SEND
    */

    void SendCustomOrder(){
        OscMessage customMessage = new OscMessage();
        OSCEndPoint oscEndPoint = new OSCEndPoint();

        customMessage.address = "/WorkshopOrder";
        customMessage.values.Add(0);
        customMessage.values.Add("string");
        
        osc.sender.SendCustomMessage(customMessage, oscEndPoint);
    }

    /*
    OSC RECEIVE
     */
    void StartListening(){
        osc.SetAddressHandler("/WorkshopOrder", OrderReceived);
    }
    
    void OrderReceived(OscMessage message){
        int i = message.GetInt(0);
        string s = message.GetString(1);
        Debug.Log(message);
    }


    


}
