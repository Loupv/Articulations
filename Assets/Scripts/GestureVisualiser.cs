using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// https://ilkinulas.github.io/development/unity/2016/04/30/cube-mesh-in-unity3d.html

public class UserPerformanceData{
    public PlayerPartInfos head, leftHand, rightHand;
}

public class PlayerPartInfos{

    public float speed;
    public float acceleration;
    public float jerk;
    public float curvature;
    public float smoothness;
    public float contraction;

}

public class GestureVisualiser : MonoBehaviour
{

    public bool drawContraction = true, debugText = true, initialised;
    public List<UserData> usersPlaying;
    public List<UserPerformanceData> usersPerformanceData;
    public GameObject[] p1ObjectsToTrack, p2ObjectsToTrack;
    public GameObject meshDrawer1, meshDrawer2;
    private EyeswebOSC eyeswebOSC;
    private DrawShape shapeDrawer1,shapeDrawer2;
    public Text[] p1DebugText, p2DebugText;
	


    void Start(){

        try{
            usersPlaying = FindObjectOfType<GameEngine>().userManager.usersPlaying;
        }
        catch{
            Debug.Log("Pas trouvé...");
        }
        usersPerformanceData = new List<UserPerformanceData>();

        shapeDrawer1 = meshDrawer1.GetComponent<DrawShape>();
        shapeDrawer2 = meshDrawer2.GetComponent<DrawShape>();

        eyeswebOSC = GetComponentInChildren<EyeswebOSC>();

        if(usersPlaying != null) // if we're in main scene
            
            if(usersPlaying.Count > 0){
                p1ObjectsToTrack = new GameObject[3];
                p1ObjectsToTrack[0] = usersPlaying[0].head;
                p1ObjectsToTrack[1] = usersPlaying[0].leftHand;
                p1ObjectsToTrack[2] = usersPlaying[0].rightHand;
            }
            if(usersPlaying.Count > 0){
                p2ObjectsToTrack = new GameObject[3];
                p2ObjectsToTrack[0] = usersPlaying[1].head;
                p2ObjectsToTrack[1] = usersPlaying[1].leftHand;
                p2ObjectsToTrack[2] = usersPlaying[1].rightHand;
            }

        usersPerformanceData.Add(new UserPerformanceData());
        usersPerformanceData.Add(new UserPerformanceData());
        usersPerformanceData[0].head = new PlayerPartInfos();
        usersPerformanceData[0].leftHand = new PlayerPartInfos();
        usersPerformanceData[0].rightHand = new PlayerPartInfos();
        usersPerformanceData[1].head = new PlayerPartInfos();
        usersPerformanceData[1].leftHand = new PlayerPartInfos();
        usersPerformanceData[1].rightHand = new PlayerPartInfos();
         

        eyeswebOSC.Init(p1ObjectsToTrack, p2ObjectsToTrack);

        initialised = true;
    }

    void Update(){
        if(initialised){
            if(drawContraction) {
                DrawContraction();
            }
            if(debugText) DebugText();
        }
    }


    void DrawContraction(){
        shapeDrawer1.DrawContraction(p1ObjectsToTrack[0].transform,p1ObjectsToTrack[1].transform,p1ObjectsToTrack[2].transform);                        
        shapeDrawer2.DrawContraction(p2ObjectsToTrack[0].transform,p2ObjectsToTrack[1].transform,p2ObjectsToTrack[2].transform);
    }

    void DebugText(){

        p1DebugText[0].text = "p1speed : "+usersPerformanceData[0].leftHand.speed.ToString();
        p1DebugText[1].text = "p1acc : "+usersPerformanceData[0].leftHand.acceleration.ToString();
        p1DebugText[2].text = "p1jerk : "+usersPerformanceData[0].leftHand.jerk.ToString();
        p1DebugText[3].text = "p1curv : "+usersPerformanceData[0].leftHand.curvature.ToString();
        p1DebugText[4].text = "p1smooth : "+usersPerformanceData[0].leftHand.smoothness.ToString();

        p1DebugText[5].text = "p1speed : "+usersPerformanceData[0].rightHand.speed.ToString();
        p1DebugText[6].text = "p1acc : "+usersPerformanceData[0].rightHand.acceleration.ToString();
        p1DebugText[7].text = "p1jerk : "+usersPerformanceData[0].rightHand.jerk.ToString();
        p1DebugText[8].text = "p1curv : "+usersPerformanceData[0].rightHand.curvature.ToString();
        p1DebugText[9].text = "p1smooth : "+usersPerformanceData[0].rightHand.smoothness.ToString();

        p2DebugText[0].text = "p2speed : "+usersPerformanceData[1].leftHand.speed.ToString();
        p2DebugText[1].text = "p2acc : "+usersPerformanceData[1].leftHand.acceleration.ToString();
        p2DebugText[2].text = "p2jerk : "+usersPerformanceData[1].leftHand.jerk.ToString();
        p2DebugText[3].text = "p2curv : "+usersPerformanceData[1].leftHand.curvature.ToString();
        p2DebugText[4].text = "p2smooth : "+usersPerformanceData[1].leftHand.smoothness.ToString();

        p2DebugText[5].text = "p2speed : "+usersPerformanceData[1].rightHand.speed.ToString();
        p2DebugText[6].text = "p2acc : "+usersPerformanceData[1].rightHand.acceleration.ToString();
        p2DebugText[7].text = "p2jerk : "+usersPerformanceData[1].rightHand.jerk.ToString();
        p2DebugText[8].text = "p2curv : "+usersPerformanceData[1].rightHand.curvature.ToString();
        p2DebugText[9].text = "p2smooth : "+usersPerformanceData[1].rightHand.smoothness.ToString();

    }


}
