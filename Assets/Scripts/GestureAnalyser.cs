using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://stackoverflow.com/questions/22583391/peak-signal-detection-in-realtime-timeseries-data/53614452#53614452

public class GestureAnalyser : MonoBehaviour
{

    public bool inited = false;

    GameObject p1Head, p1LeftHand, p1RightHand, p2Head, p2LeftHand, p2RightHand;
    Rigidbody p1HeadRg, p1LeftHandRg, p1RightHandRg, p2HeadRg, p2LeftHandRg, p2RightHandRg;

    List<Vector3> p1HPositionSequence, p1LHPositionSequence,p1RHPositionSequence,
        p2HPositionSequence,p2LHPositionSequence,p2RHPositionSequence;

    List<float> p1LHVelocityMagnitudeSequence, p1RHVelocityMagnitudeSequence, p2LHVelocityMagnitudeSequence, p2RHVelocityMagnitudeSequence;

    OSC osc;

    public int bufferSize = 30;
    public float frameRate = 60;
    public int activityBufferSize = 5;

    float p1LHSpeed;
    public float activityThreshold = 0.002f;
    public bool p1HeadActivity, p1LeftHandActivity,p1RightHandActivity,
        p2HeadActivity, p2LeftHandActivity,p2RightHandActivity;


    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    void Init(){
        if (GameObject.Find("UserManager").GetComponent<UserManager>().usersPlaying.Count < 2) return;
            
        UserData player1 = new UserData(), player2 = new UserData();
        UserManager userManager = GameObject.Find("UserManager").GetComponent<UserManager>();
        NetworkManager networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        osc = GameObject.Find("OSC").GetComponent<OSC>();

        int i = 0;
            
        bool player1found = false, player2found = false;

        while(!(player1found && player2found)){
            if(!player1found){
                if(userManager.usersPlaying[i]._userRole == UserRole.Player || (userManager.usersPlaying[i]._userRole == UserRole.Playback && !userManager.usersPlaying[i]._isMe)) {
                    player1 = userManager.usersPlaying[i];
                    Debug.Log("Player 1 id :"+player1._ID);
                    if(player1.head.GetComponent<Rigidbody>() == null) player1.head.AddComponent<Rigidbody>();
                    if(player1.leftHand.GetComponent<Rigidbody>() == null) player1.leftHand.AddComponent<Rigidbody>();
                    if(player1.rightHand.GetComponent<Rigidbody>() == null) player1.rightHand.AddComponent<Rigidbody>();
                    player1found = true;
                }
                i+=1;
            }
            else{
                if(userManager.usersPlaying[i]._userRole == UserRole.Player || (userManager.usersPlaying[i]._userRole == UserRole.Playback  && !userManager.usersPlaying[i]._isMe)) {
                    player2 = userManager.usersPlaying[i];
                    Debug.Log("Player 2 id :"+player2._ID);
                    player2found = true;
                }
                else i+=1;
            }
        }

        p1Head = player1.head;
        p1LeftHand = player1.leftHand;
        p1RightHand = player1.rightHand;

        p2Head = player2.head;
        p2LeftHand = player2.leftHand;
        p2RightHand = player2.rightHand;

        p1HeadRg = p1Head.GetComponent<Rigidbody>();
        p1LeftHandRg = p1LeftHand.GetComponent<Rigidbody>();
        p1RightHandRg = p1RightHand.GetComponent<Rigidbody>();

        p2HeadRg = p2Head.GetComponent<Rigidbody>();
        p2LeftHandRg = p2LeftHand.GetComponent<Rigidbody>();
        p2RightHandRg = p2RightHand.GetComponent<Rigidbody>();

        p1HPositionSequence = new List<Vector3>();
        p1LHPositionSequence = new List<Vector3>();
        p1RHPositionSequence = new List<Vector3>();
        p2HPositionSequence = new List<Vector3>();
        p2LHPositionSequence = new List<Vector3>();
        p2RHPositionSequence = new List<Vector3>();


        p1LHVelocityMagnitudeSequence = new List<float>();
        p1RHVelocityMagnitudeSequence = new List<float>();
        p2LHVelocityMagnitudeSequence = new List<float>();
        p2RHVelocityMagnitudeSequence = new List<float>();
            
        inited = true;

        InvokeRepeating("UpdateGestureAnalysis", 0, 1/frameRate);
        InvokeRepeating("DetectActivity", 1, 1);
    }

    
    void Update()
    {
        if (inited == false)
        {
            Init();
        }
    }

    void UpdateGestureAnalysis()
    {
        FillPositionSequences();
        ComputeSpeed();

        osc.sender.SendWekinatorInfos(p1LeftHandActivity);
    }

    void FillPositionSequences(){

        p1LHPositionSequence.Add(p1LeftHand.transform.position);
        if(p1LHPositionSequence.Count > activityBufferSize) p1LHPositionSequence.RemoveAt(0);
 
    }

    void ComputeSpeed(){
        if(p1LHPositionSequence.Count>2){
            
            float dist = Vector3.Distance(p1LHPositionSequence[p1LHPositionSequence.Count - 1], p1LHPositionSequence[p1LHPositionSequence.Count - 2]);
            p1LHSpeed = dist / (1/frameRate);
            p1LHVelocityMagnitudeSequence.Add(p1LHSpeed);
            if (p1LHVelocityMagnitudeSequence.Count > activityBufferSize) p1LHVelocityMagnitudeSequence.RemoveAt(0);
            Debug.Log(dist+", "+p1LHSpeed);
        }
    }


    void DetectActivity(){
        if (inited)
        {
            if (Mathf.Max(p1LHVelocityMagnitudeSequence.ToArray()) > activityThreshold) p1LeftHandActivity = true;
            else p1LeftHandActivity = false;
        }
    }




}
