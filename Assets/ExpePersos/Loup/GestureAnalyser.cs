using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://stackoverflow.com/questions/22583391/peak-signal-detection-in-realtime-timeseries-data/53614452#53614452

public class GestureAnalyser : MonoBehaviour
{

    public bool inited = false;


    public UserData p1, p2;
    GameObject p1Head, p1LeftHand, p1RightHand;

    List<Vector3> p1HPositionSequence, p1LHPositionSequence, p1RHPositionSequence;    
    List<float> p1HVelocityMagnitudeSequence, p1LHVelocityMagnitudeSequence, p1RHVelocityMagnitudeSequence;
    public bool p1HeadActivity, p1LeftHandActivity,p1RightHandActivity;
    public float p1Volume;

    OSC osc;


    public int bufferSize = 30;
    public float frameRate = 60;
    public int activityBufferSize = 5;

    public float p1HSpeed, p1LHSpeed, p1RHSpeed;
    public float activityThreshold = 0.002f;

    public float leftHandHeightRatio, rightHandHeightRatio;
    float p1handsMaxHeight, p1handsMinHeight;



    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    void Init(){
        if (GameObject.Find("UserManager").GetComponent<UserManager>().usersPlaying.Count < 2) return;
            
        UserManager userManager = GameObject.Find("UserManager").GetComponent<UserManager>();
        NetworkManager networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        osc = GameObject.Find("OSC").GetComponent<OSC>();

        int i = 0;
            
        bool player1found = false, player2found = false;

        while(!(player1found && player2found)){
            if(!player1found){
                if(userManager.usersPlaying[i]._userRole == UserRole.Player || (userManager.usersPlaying[i]._userRole == UserRole.Playback && !userManager.usersPlaying[i]._isMe)) {
                    p1 = userManager.usersPlaying[i];
                    Debug.Log("Player 1 id :"+ p1._ID);
                    player1found = true;
                }
                i+=1;
            }
            else{
                if(userManager.usersPlaying[i]._userRole == UserRole.Player || (userManager.usersPlaying[i]._userRole == UserRole.Playback  && !userManager.usersPlaying[i]._isMe)) {
                    p2 = userManager.usersPlaying[i];
                    Debug.Log("Player 2 id :"+p2._ID);
                    player2found = true;
                }
                else i+=1;
            }
        }

        p1Head = p1.head;
        p1LeftHand = p1.leftHand;
        p1RightHand = p1.rightHand;

        p1HPositionSequence = new List<Vector3>();
        p1LHPositionSequence = new List<Vector3>();
        p1RHPositionSequence = new List<Vector3>();

        p1HVelocityMagnitudeSequence = new List<float>();
        p1LHVelocityMagnitudeSequence = new List<float>();
        p1RHVelocityMagnitudeSequence = new List<float>();

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
        UpdateHeightRatios();
        p1Volume = CalculatePrismVolume(p1Head.transform,p1LeftHand.transform,p1RightHand.transform);
        osc.sender.SendWekinatorInfos(p1LeftHandActivity);
    }

    void FillPositionSequences(){

        p1HPositionSequence.Add(p1Head.transform.position);
        if(p1HPositionSequence.Count > activityBufferSize) p1HPositionSequence.RemoveAt(0);

        p1LHPositionSequence.Add(p1LeftHand.transform.position);
        if (p1LHPositionSequence.Count > activityBufferSize) p1LHPositionSequence.RemoveAt(0);

        p1RHPositionSequence.Add(p1RightHand.transform.position);
        if (p1RHPositionSequence.Count > activityBufferSize) p1RHPositionSequence.RemoveAt(0);
    }

    void ComputeSpeed(){
        if(p1HPositionSequence.Count>2){       
            float dist = Vector3.Distance(p1HPositionSequence[p1HPositionSequence.Count - 1], p1HPositionSequence[p1HPositionSequence.Count - 2]);
            p1HSpeed = dist / (1/frameRate);
            p1HVelocityMagnitudeSequence.Add(p1LHSpeed);
            if (p1HVelocityMagnitudeSequence.Count > activityBufferSize) p1HVelocityMagnitudeSequence.RemoveAt(0);
        }
        if (p1LHPositionSequence.Count > 2)
        {
            float dist = Vector3.Distance(p1LHPositionSequence[p1LHPositionSequence.Count - 1], p1LHPositionSequence[p1LHPositionSequence.Count - 2]);
            p1LHSpeed = dist / (1 / frameRate);
            p1LHVelocityMagnitudeSequence.Add(p1LHSpeed);
            if (p1LHVelocityMagnitudeSequence.Count > activityBufferSize) p1LHVelocityMagnitudeSequence.RemoveAt(0);
        }
        if (p1RHPositionSequence.Count > 2)
        {
            float dist = Vector3.Distance(p1RHPositionSequence[p1RHPositionSequence.Count - 1], p1RHPositionSequence[p1RHPositionSequence.Count - 2]);
            p1RHSpeed = dist / (1 / frameRate);
            p1RHVelocityMagnitudeSequence.Add(p1RHSpeed);
            if (p1RHVelocityMagnitudeSequence.Count > activityBufferSize) p1RHVelocityMagnitudeSequence.RemoveAt(0);
        }

    }


    void DetectActivity(){
        if (inited)
        {
            if (Mathf.Max(p1LHVelocityMagnitudeSequence.ToArray()) > activityThreshold) p1LeftHandActivity = true;
            else p1LeftHandActivity = false;

            if (Mathf.Max(p1RHVelocityMagnitudeSequence.ToArray()) > activityThreshold) p1RightHandActivity = true;
            else p1RightHandActivity = false;

            if (Mathf.Max(p1HVelocityMagnitudeSequence.ToArray()) > activityThreshold) p1HeadActivity = true;
            else p1HeadActivity = false;
        }
    }


    void UpdateHeightRatios()
    {
        if (p1.leftHand.transform.position.y > p1handsMaxHeight || p1.rightHand.transform.position.y > p1handsMaxHeight)
        {
            p1handsMaxHeight = Mathf.Max(p1.leftHand.transform.position.y, p1.rightHand.transform.position.y);
        }

        if (p1.leftHand.transform.position.y < p1handsMinHeight || p1.rightHand.transform.position.y < p1handsMinHeight)
        {
            if (Mathf.Min(p1.leftHand.transform.position.y, p1.rightHand.transform.position.y) != 0)
                p1handsMinHeight = Mathf.Min(p1.leftHand.transform.position.y, p1.rightHand.transform.position.y);
        }

        leftHandHeightRatio = (p1.leftHand.transform.position.y - p1handsMinHeight) / (p1handsMaxHeight - p1handsMinHeight);
        rightHandHeightRatio = (p1.rightHand.transform.position.y - p1handsMinHeight) / (p1handsMaxHeight - p1handsMinHeight);
    }



    // copied from drawShape script
    private float CalculatePrismVolume(Transform s1, Transform s2, Transform s3)
    {


        //float baseVolume = 0;// base volume = aire du triangle au sol * hauteur => aire du triangle = sqr(s(s-a)(s-b)(s-c) où s est le demipérimètre (formule Héron) hauteur = min(s1.y,s2.y,s3.y)
        //float headVolume = 0; // trouver comment calculer la différence avec la partie supérieure du triangle
        float d1 = new Vector3(s1.position.x - s2.position.x, 0, s1.position.z - s2.position.z).magnitude;
        float d2 = new Vector3(s2.position.x - s3.position.x, 0, s2.position.z - s3.position.z).magnitude;
        float d3 = new Vector3(s3.position.x - s1.position.x, 0, s3.position.z - s1.position.z).magnitude;


        float demiperimeter = (d1 + d2 + d3) / 2;
        float alpha1 = Mathf.Abs(demiperimeter - d1);
        float alpha2 = Mathf.Abs(demiperimeter - d2);
        float alpha3 = Mathf.Abs(demiperimeter - d3);

        // here we do max(0.1f, diff) to avoid when diff = 0 => volume = 0 and contracton is inf
        float prismVolume = Mathf.Sqrt(demiperimeter * Mathf.Max(0.1f, alpha1) * Mathf.Max(0.1f, alpha2) * Mathf.Max(0.1f, alpha3)) * Mathf.Max(s1.position.y, s2.position.y, s3.position.y);

        /*if(prismVolume == 0){ 
			Debug.Log(d1+", "+d2+", "+d3+", "+demiperimeter);
			UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Pause");
		}*/
        return prismVolume;
    }
}
