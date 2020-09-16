using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotProduct : MonoBehaviour
{
    public bool inited, activated;
    UserData player1, player2;
    

    void Start(){
        if(!inited) Init();
        
        if(!activated) {
            activated = true;
        }

    }

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

        inited = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(inited && activated){

            Debug.Log(Vector3.Dot(player1.head.transform.rotation.eulerAngles.normalized, player2.head.transform.rotation.eulerAngles.normalized));
        } 
    }

}
