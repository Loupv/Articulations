using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{

    public GameEngine gameEngine;
    public Button networkButtonChoice1, networkButtonChoice2, roleButtonChoice1, roleButtonChoice2;
    public Button FreeCam, POVPlayer1, POVPlayer2, POV3;
    public ViewerController viewerController;
    public InputField OSCServerPortInput;
    public GameObject clientObjectParent, userRoleButton;
    public GameObject recordGizmo, pauseGizmo;
    public int OSCServerPort, OSCClientPort;
    public string address;
    public Sprite selectedButtonSprite, normalButtonSprite, lockedButtonSprite;

    public int isPlayer;


    public void SetPlayerNetworkType(int i) // 0 for server, 1 for client
    {
        if (i == 0) // server
        {
            gameEngine.userNetworkType = UserNetworkType.Server;
            if (clientObjectParent != null) clientObjectParent.gameObject.SetActive(false);
            SetPlayerRole(1);
            networkButtonChoice1.image.sprite = selectedButtonSprite;
            networkButtonChoice2.image.sprite = normalButtonSprite;
            clientObjectParent.gameObject.SetActive(false);
            userRoleButton.gameObject.SetActive(false);

        }
        else if (i == 1) // client
        {
            gameEngine.userNetworkType = UserNetworkType.Client;
            if(clientObjectParent != null) clientObjectParent.gameObject.SetActive(true);
            SetPlayerRole(0);
            networkButtonChoice1.image.sprite = normalButtonSprite;
            networkButtonChoice2.image.sprite = selectedButtonSprite;
            clientObjectParent.gameObject.SetActive(true);
            userRoleButton.gameObject.SetActive(true);
        }
    }



    public void SetPlayerRole(int i) // 0 for player, 1 for viewer
    {
        if (i == 0) // player
        {
            isPlayer = 1;
            roleButtonChoice1.image.sprite = selectedButtonSprite;
            roleButtonChoice2.image.sprite = normalButtonSprite;
        }

        else if (i == 1) // viewer
        {
            isPlayer = 0;
            roleButtonChoice1.image.sprite = normalButtonSprite;
            roleButtonChoice2.image.sprite = selectedButtonSprite;
        }
    }


    public void StartButtonPressed()
    {
        gameEngine.StartGame(isPlayer);
    }


    public void ChangeVisualizationMode(int i){
        gameEngine.osc.sender.SendVisualisationChange(i, gameEngine.usersPlaying);
        gameEngine.ChangeVisualisationMode(i);
    }

    
    public void ActualizeGizmos(bool isRecording, bool isPaused){
        if(isRecording && !isPaused){
            recordGizmo.SetActive(true);
            pauseGizmo.SetActive(false);
        }
        else if(isRecording && isPaused){    
            recordGizmo.SetActive(false);
            pauseGizmo.SetActive(true);
        }
        else if(!isRecording){    
            recordGizmo.SetActive(false);
            pauseGizmo.SetActive(false);
        }
    }


    public void ChangeViewerPOV(int pov){
        
        
        switch(pov){
		case 0: // free mode
        FreeCam.image.sprite = selectedButtonSprite;
        POVPlayer1.image.sprite = normalButtonSprite;
        POVPlayer2.image.sprite = normalButtonSprite;
        POV3.image.sprite = normalButtonSprite;
		break;

		case 1:
        FreeCam.image.sprite = normalButtonSprite;
        POVPlayer1.image.sprite = selectedButtonSprite;
        POVPlayer2.image.sprite = normalButtonSprite;
        POV3.image.sprite = normalButtonSprite;
		break;

		case 2:
		FreeCam.image.sprite = normalButtonSprite;
        POVPlayer1.image.sprite = normalButtonSprite;
        POVPlayer2.image.sprite = selectedButtonSprite;
        POV3.image.sprite = normalButtonSprite;
		break;

        case 3:
		FreeCam.image.sprite = normalButtonSprite;
        POVPlayer1.image.sprite = normalButtonSprite;
        POVPlayer2.image.sprite = normalButtonSprite;
        POV3.image.sprite = selectedButtonSprite;
		break;
        }
        viewerController.UpdatePOV(pov);
    }

}
