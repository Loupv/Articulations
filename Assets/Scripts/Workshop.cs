using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Workshop : MonoBehaviour
{

    /*
     * Workshop is handles as :
     *
     *  clients are autonomous from server, they receive all gesture qualities from server and use it as they want
     *  it's made easy to log in/off quickly and implement new things
     *  server stays running, visual changes are only done on client side


    Example of things to work with :
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
    public GameEngine gameEngine;
    public EyeswebOSC eyeswebOSC;
    public List<UserData> _usersPlaying;
    bool initialized;

    UserPerformanceData player1PerformanceData, player2PerformanceData;
    SharedPerformanceData sharedPerformanceData;

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

        // Actualize gesture data
        player1PerformanceData = eyeswebOSC.GetUserPerformanceData(1);
        player2PerformanceData = eyeswebOSC.GetUserPerformanceData(2);
        sharedPerformanceData = eyeswebOSC.GetSharedPerformanceData();


        // actualize things on players
        //if (initialized)
        {
            foreach (UserData user in _usersPlaying)
            {
                // do blablabla
                //user.leftHand.GetComponentInChildren<ParticleSystem>().startSpeed = Mathf.Sin((float)gameEngine.clock.GetTimeSinceSceneStart() * Mathf.PI);
                //user.leftHand.GetComponentInChildren<ParticleSystem>().startSize = Mathf.Sin((float)gameEngine.clock.GetTimeSinceSceneStart() * Mathf.PI);

                
            }

            Debug.Log(player1PerformanceData.handsVelocityMean);
        }


        // modify environment parameters

        //gameEngine.scenarioEvents.SetTimeOfDay(12); // 24h format
        // visual FXs


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
