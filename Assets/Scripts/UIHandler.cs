using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{

    public GameEngine gameEngine;
    public Button networkButtonChoice1, networkButtonChoice2, networkButtonChoice3;
    public Button FreeCam, POVPlayer1, POVPlayer2, POV3;
    public ViewerController viewerController;
    public InputField OSCServerAddressInput, PlayerName;
    public GameObject clientObjectParent, userRoleButton;
    public GameObject recordGizmo, pauseGizmo;
    public int OSCServerPort, OSCClientPort;
    public Sprite selectedButtonSprite, normalButtonSprite, lockedButtonSprite;
    public Toggle sendToAudioDeviceToggle;
    public Slider trailsDecaySlider;
    public Text trailTime;


    void Start(){

        if(sendToAudioDeviceToggle.isOn) gameEngine.sendToAudioDevice = true;
        else gameEngine.sendToAudioDevice = false;

    }

    public void SetPlayerNetworkType(int i) // 0 for server, 1 for client
    {
        if (i == 0) // server
        {
            gameEngine._userRole = UserRole.Server;
            if (clientObjectParent != null) clientObjectParent.gameObject.SetActive(false);
            //SetPlayerRole(1);
            networkButtonChoice1.image.sprite = selectedButtonSprite;
            networkButtonChoice2.image.sprite = normalButtonSprite;
            networkButtonChoice3.image.sprite = normalButtonSprite;
            clientObjectParent.gameObject.SetActive(false);
            //serverObjectParent.gameObject.SetActive(true);
            //userRoleButton.gameObject.SetActive(false);

        }
        else if (i == 1) // client
        {
            gameEngine._userRole = UserRole.Player;
            if(clientObjectParent != null) clientObjectParent.gameObject.SetActive(true);
            //SetPlayerRole(0);
            networkButtonChoice1.image.sprite = normalButtonSprite;
            networkButtonChoice2.image.sprite = selectedButtonSprite;
            networkButtonChoice3.image.sprite = normalButtonSprite;
            clientObjectParent.gameObject.SetActive(true);
            //serverObjectParent.gameObject.SetActive(false);
            //userRoleButton.gameObject.SetActive(true);
        }
        else if (i == 2) // client
        {
            gameEngine._userRole = UserRole.Viewer;
            if(clientObjectParent != null) clientObjectParent.gameObject.SetActive(true);
            //SetPlayerRole(0);
            networkButtonChoice1.image.sprite = normalButtonSprite;
            networkButtonChoice2.image.sprite = normalButtonSprite;
            networkButtonChoice3.image.sprite = selectedButtonSprite;
            clientObjectParent.gameObject.SetActive(true);
            //serverObjectParent.gameObject.SetActive(false);
            //userRoleButton.gameObject.SetActive(true);
        }
    }


/* 
    public void SetPlayerRole(int i) // 0 for player, 1 for viewer
    {
        if (i == 0) // player
        {
            gameEngine._userRole = UserRole.Player;
            roleButtonChoice1.image.sprite = selectedButtonSprite;
            roleButtonChoice2.image.sprite = normalButtonSprite;
        }

        else if (i == 1) // viewer
        {
            gameEngine._userRole = UserRole.Viewer;
            roleButtonChoice1.image.sprite = normalButtonSprite;
            roleButtonChoice2.image.sprite = selectedButtonSprite;
        }
    }*/


    public void StartButtonPressed()
    {
        gameEngine.StartGame();
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
        
        int tmp = 0;

        switch(pov){
		case 0: // free mode
        FreeCam.image.sprite = selectedButtonSprite;
        POVPlayer1.image.sprite = normalButtonSprite;
        POVPlayer2.image.sprite = normalButtonSprite;
        POV3.image.sprite = normalButtonSprite;
		break;

		case 1:
        tmp = gameEngine.ReturnPlayerRank(1);
		if(tmp != -1){
            FreeCam.image.sprite = normalButtonSprite;
            POVPlayer1.image.sprite = selectedButtonSprite;
            POVPlayer2.image.sprite = normalButtonSprite;
            POV3.image.sprite = normalButtonSprite;
        }
		break;

		case 2:
        tmp = gameEngine.ReturnPlayerRank(2);
		if(tmp != -1){
            FreeCam.image.sprite = normalButtonSprite;
            POVPlayer1.image.sprite = normalButtonSprite;
            POVPlayer2.image.sprite = selectedButtonSprite;
            POV3.image.sprite = normalButtonSprite;
        }
        break;

        case 3:
		FreeCam.image.sprite = normalButtonSprite;
        POVPlayer1.image.sprite = normalButtonSprite;
        POVPlayer2.image.sprite = normalButtonSprite;
        POV3.image.sprite = selectedButtonSprite;
		break;
        }
        viewerController.UpdatePOV(pov, tmp);
    }

    public void ToggleSendToAudioHandler(){
        gameEngine.sendToAudioDevice = !gameEngine.sendToAudioDevice;
    }

    // Is a server function // triggered by UI button
    public void TrailsDecaySliderChanged(int id){
        gameEngine.osc.sender.SendTrailValueChange(id, trailsDecaySlider.value, gameEngine.usersPlaying);
        gameEngine.ChangeVisualisationParameter(id, trailsDecaySlider.value); // update also for server visualisation
        trailTime.text = "TrailTime : "+trailsDecaySlider.value;
    }


    public void CalibratePlayersPosition(){
        
    }


}
