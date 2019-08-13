using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIHandler : MonoBehaviour
{

    public GameEngine gameEngine;
    public UserManager userManager;
    public ScenarioEvents scenarioEvents;
    public Button networkButtonChoice1, networkButtonChoice2, networkButtonChoice3, networkButtonChoice4,networkButtonChoice5, 
        autoMode, manualMode, launchScenario, pauseScenario;
    public Button FreeCam, POVPlayer1, POVPlayer2, POV3;
    public ViewerController viewerController;
    public InputField OSCServerAddressInput, sessionIDInputBox;
    public GameObject serverManualModeParent, serverAutoModeParent;
    public GameObject clientGOParent, playerNameTextBox, serverGOParent, playbackGOParent;
    public GameObject recordGizmo, pauseGizmo;
    public Dropdown scenarioDropDown, onlineOfflinePlaybackMode, switchPerformanceDataFile, switchPlaybackPlayer;
    public Sprite selectedButtonSprite, normalButtonSprite;
    public Toggle sendToAudioDeviceToggle, autoRecordPerformance, recordAudioAfterScenario;
    public Slider trailsDecaySlider;
    public Text trailTime, conditionTimeRemaining, recordingTimeRemaining, currentConditionText, playbackTime, currentViz;
    private string scenarioMode;
    int tmpTimer;


    void Start(){

        if(sendToAudioDeviceToggle.isOn) gameEngine.sendToAudioDevice = true;
        else gameEngine.sendToAudioDevice = false;

        gameEngine.fileInOut.PopulatePlaybackDataFileDropdown(switchPerformanceDataFile);
        ScenarioModeSwitch(1);
    }

    public GameData AdjustBasicUIParameters(GameData gameData, string tmpIp){

        if (gameData.runInLocal == 1) {
            OSCServerAddressInput.text = "127.0.0.1";
            gameData.OSC_LocalIP = "127.0.0.1";
            gameData.OSC_ClientPort = UnityEngine.Random.Range(5555,8888);
        }
        else {
            OSCServerAddressInput.text = gameData.OSC_ServerIP;
            gameData.OSC_LocalIP = tmpIp;
        }
        return gameData;

    }

    public void SetPlayerNetworkType(int i) // 0 for server, 1 for client
    {
        if (i == 0) // server
        {
            gameEngine._userRole = UserRole.Server;
            networkButtonChoice1.image.sprite = selectedButtonSprite;
            networkButtonChoice2.image.sprite = normalButtonSprite;
            networkButtonChoice3.image.sprite = normalButtonSprite;
            networkButtonChoice4.image.sprite = normalButtonSprite;
            networkButtonChoice5.image.sprite = normalButtonSprite;
            playerNameTextBox.gameObject.SetActive(false);
            OSCServerAddressInput.gameObject.SetActive(false);
            clientGOParent.gameObject.SetActive(false);
            serverGOParent.gameObject.SetActive(true);
            playbackGOParent.gameObject.SetActive(false);

        }
        else if (i == 1) // client
        {
            gameEngine._userRole = UserRole.Player;
            networkButtonChoice1.image.sprite = normalButtonSprite;
            networkButtonChoice2.image.sprite = selectedButtonSprite;
            networkButtonChoice3.image.sprite = normalButtonSprite;
            networkButtonChoice4.image.sprite = normalButtonSprite;
            networkButtonChoice5.image.sprite = normalButtonSprite;
            playerNameTextBox.gameObject.SetActive(true);
            OSCServerAddressInput.gameObject.SetActive(true);
            clientGOParent.gameObject.SetActive(true);
            serverGOParent.gameObject.SetActive(false);
            playbackGOParent.gameObject.SetActive(false);
        }
        else if (i == 2) // viewer
        {
            gameEngine._userRole = UserRole.Viewer;
            networkButtonChoice1.image.sprite = normalButtonSprite;
            networkButtonChoice2.image.sprite = normalButtonSprite;
            networkButtonChoice3.image.sprite = selectedButtonSprite;
            networkButtonChoice4.image.sprite = normalButtonSprite;
            networkButtonChoice5.image.sprite = normalButtonSprite;
            playerNameTextBox.gameObject.SetActive(false);
            OSCServerAddressInput.gameObject.SetActive(true);
            clientGOParent.gameObject.SetActive(true);
            serverGOParent.gameObject.SetActive(false);
            playbackGOParent.gameObject.SetActive(false);
        }
        else if (i == 3) // tracker
        {
            gameEngine._userRole = UserRole.Tracker;
            networkButtonChoice1.image.sprite = normalButtonSprite;
            networkButtonChoice2.image.sprite = normalButtonSprite;
            networkButtonChoice3.image.sprite = normalButtonSprite;
            networkButtonChoice4.image.sprite = selectedButtonSprite;
            networkButtonChoice5.image.sprite = normalButtonSprite;
            playerNameTextBox.gameObject.SetActive(false);
            OSCServerAddressInput.gameObject.SetActive(true);
            clientGOParent.gameObject.SetActive(true);
            serverGOParent.gameObject.SetActive(false);
            playbackGOParent.gameObject.SetActive(false);
        }
        else if (i == 4) // playback
        {
            gameEngine._userRole = UserRole.Playback;
            networkButtonChoice1.image.sprite = normalButtonSprite;
            networkButtonChoice2.image.sprite = normalButtonSprite;
            networkButtonChoice3.image.sprite = normalButtonSprite;
            networkButtonChoice4.image.sprite = normalButtonSprite;
            networkButtonChoice5.image.sprite = selectedButtonSprite;
            playerNameTextBox.gameObject.SetActive(false);
            if(gameEngine.playbackManager.mode == 0) OSCServerAddressInput.gameObject.SetActive(true);
            else if(gameEngine.playbackManager.mode == 1) OSCServerAddressInput.gameObject.SetActive(false);
            clientGOParent.gameObject.SetActive(false);
            serverGOParent.gameObject.SetActive(false);
            playbackGOParent.gameObject.SetActive(true);
        }
    }

    // server function
    public void EnvironmentChange(string env)
    {
        // change env to server
        if (env == "sky") gameEngine.scenarioEvents.SetNextSkybox();
        else if (env == "mirror") gameEngine.scenarioEvents.ToggleMirror();

        // change env to clients
        gameEngine.networkManager.EnvironmentChangeOrder(userManager.usersPlaying, env);
    }

    
    public void ScenarioDropDownChanged(int value){
        gameEngine.scenarioEvents.currentScenario = value;
    }


    // switch server UI between automated and manual mode
    public void ScenarioModeSwitch(int value){
        if(value == 1){
            scenarioMode = "Auto";
            serverAutoModeParent.SetActive(true);
            serverManualModeParent.SetActive(false);
            manualMode.image.sprite = normalButtonSprite;
            autoMode.image.sprite = selectedButtonSprite;
        }
        else if(value == 2){
            scenarioMode = "Manual";
            serverAutoModeParent.SetActive(false);
            serverManualModeParent.SetActive(true);
            manualMode.image.sprite = selectedButtonSprite;
            autoMode.image.sprite = normalButtonSprite;
        }
    }

    public void ToggleScenarioButton(int value){
        if(value == 1){ // start
            launchScenario.gameObject.SetActive(false);
            pauseScenario.gameObject.SetActive(true);
        }
        else if(value == 2){ // pause
            if(scenarioEvents.timerPaused) pauseScenario.image.sprite = normalButtonSprite;
            else pauseScenario.image.sprite = selectedButtonSprite;
        }
        else if(value == 0){ // stop
            launchScenario.gameObject.SetActive(true);
            pauseScenario.gameObject.SetActive(false);
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
        if(!(gameEngine._userRole == UserRole.Server && sessionIDInputBox.GetComponent<InputField>().text == "")) gameEngine.StartGame();
        else Debug.Log("Please enter session ID");
    }


    public void ChangeVisualisationMode(string i){
        // for server
        userManager.ChangeVisualisationMode(i, gameEngine, false);
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
        tmp = userManager.ReturnPlayerRank(1);
		if(tmp != -1){
            FreeCam.image.sprite = normalButtonSprite;
            POVPlayer1.image.sprite = selectedButtonSprite;
            POVPlayer2.image.sprite = normalButtonSprite;
            POV3.image.sprite = normalButtonSprite;
        }
		break;

		case 2:
        tmp = userManager.ReturnPlayerRank(2);
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
        gameEngine.osc.sender.SendTrailValueChange(id, trailsDecaySlider.value, userManager.usersPlaying);
        userManager.ChangeVisualisationParameter(id, trailsDecaySlider.value); // update also for server visualisation
        trailTime.text = "TrailTime : "+trailsDecaySlider.value;
    }


    // this method stores in each user data a gap vector that centers him back to the markers at the center of the field
    // for a client's instance it translates its own parent and leaves head and hands at 0 (to be used with vive locally)
    // for a client's other instance, it add the gap to each of the sphere
    // for the server it adds the gap during the update function (taking data from the pending position dictionnary) 

    public void CalibratePlayersPosition(){
        scenarioEvents.CalibratePlayersPositions();
           //userManager.CalibratePlayerTransform();
    }


    public void PopulateScenariosDropdown(Scenario[] scenarios){
        scenarioDropDown.ClearOptions();

        foreach(Scenario scenario in scenarios){
            string cond = "";
            foreach(string s in scenario.conditions) cond += s;
            //scenarioDropDown.options.Add(new Dropdown.OptionData("Scenario"+scenario.scenarioId));
            scenarioDropDown.options.Add(new Dropdown.OptionData("Scenario-"+cond));
        }
    }

    public void CountRecordTimeRemaining(){
        recordingTimeRemaining.gameObject.SetActive(true);
        tmpTimer = gameEngine.audioRecordManager.postScenarioRecordingLenght;
        InvokeRepeating("TimerUI", 0f,1f);
    }

    public void CancelRecordTime(){
        recordingTimeRemaining.gameObject.SetActive(false);
        CancelInvoke("TimerUI");
    }

    void TimerUI(){
        recordingTimeRemaining.text = "Record Time Remaining : "+tmpTimer;
        tmpTimer--;
        if(tmpTimer < 0) CancelRecordTime();
    }


    public void quitApp()
    {
        Application.Quit();
    }
}
