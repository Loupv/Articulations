using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barycenter : MonoBehaviour
{

    public bool inited, activated;
    UserData player1, player2;
    GameObject heartp1, heartp2, heartmiddle;
    public int actualMode;

    // Start is called before the first frame update
    void Init()
    {
        UserManager userManager = GameObject.Find("UserManager").GetComponent<UserManager>();

        int i = 0;
        
        bool player1found = false, player2found = false;

        while(!(player1found && player2found)){
            if(!player1found){
                if(userManager.usersPlaying[i]._userRole == UserRole.Player || (userManager.usersPlaying[i]._userRole == UserRole.Playback && !userManager.usersPlaying[i]._isMe)) {
                    player1 = userManager.usersPlaying[i];
                    Debug.Log("Player 1 id :"+player1._ID);
                    player1found = true;
                }
                i+=1;
            }
            else{
                if(userManager.usersPlaying[i]._userRole == UserRole.Player || (userManager.usersPlaying[i]._userRole == UserRole.Playback && !userManager.usersPlaying[i]._isMe)) {
                    player2 = userManager.usersPlaying[i];
                    Debug.Log("Player 2 id :"+player2._ID);
                    player2found = true;
                }
                else i+=1;
            }
        }

        heartp1 = transform.Find("heartp1").gameObject;
        heartp2 = transform.Find("heartp2").gameObject;
        heartmiddle = transform.Find("heartmiddle").gameObject;

        inited = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(inited && activated){

            if(actualMode == 0){
                heartp1.transform.position = (player1.head.transform.position  +
                    player1.leftHand.transform.position  +
                    player1.rightHand.transform.position )/3;
                heartp2.transform.position = (player2.head.transform.position  +
                    player2.leftHand.transform.position  +
                    player2.rightHand.transform.position )/3;
                
            }
            else{
                heartmiddle.transform.position = (player1.head.transform.position + player2.head.transform.position +
                    player1.leftHand.transform.position + player2.leftHand.transform.position +
                    player1.rightHand.transform.position + player2.rightHand.transform.position )/6;

            }
        } 
    }

    public void Activate(int mode){
        if(!inited) Init();
        
        if(!activated) {
            activated = true;
        }

        if(mode == 0){
            heartp1.GetComponent<MeshRenderer>().enabled = true;
            heartp2.GetComponent<MeshRenderer>().enabled = true;
            heartmiddle.GetComponent<MeshRenderer>().enabled = false;
        }
        else{
            heartp1.GetComponent<MeshRenderer>().enabled = false;
            heartp2.GetComponent<MeshRenderer>().enabled = false;
            heartmiddle.GetComponent<MeshRenderer>().enabled = true;
        }

        actualMode = mode;
    }

    public void DeActivate(){
        heartp1.GetComponent<MeshRenderer>().enabled = false;
        heartp2.GetComponent<MeshRenderer>().enabled = false;
        heartmiddle.GetComponent<MeshRenderer>().enabled = false;
    }
}
