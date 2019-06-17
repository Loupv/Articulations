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
    public Button networkButtonChoice1, networkButtonChoice2, networkButtonChoice3, networkButtonChoice4, 
        autoMode, manualMode, launchScenario, pauseScenario;
    public Button FreeCam, POVPlayer1, POVPlayer2, POV3;
    public ViewerController viewerController;
    public InputField OSCServerAddressInput, PlayerName;
    public GameObject serverManualModeParent, serverAutoModeParent;
    public GameObject serverIPTextBox, playerNameTextBox, sessionIDInputBox;
    public GameObject recordGizmo, pauseGizmo;
    public Dropdown scenarioDropDown;
    public Sprite selectedButtonSprite, normalButtonSprite;
    public Toggle sendToAudioDeviceToggle, autoRecordPerformance;
    public Slider trailsDecaySlider;
    public Text trailTime;
    private string scenarioMode;


    void Start(){

        if(sendToAudioDeviceToggle.isOn) gameEngine.sendToAudioDevice = true;
        else gameEngine.sendToAudioDevice = false;
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
            if (serverIPTextBox != null) serverIPTextBox.gameObject.SetActive(false);
            //SetPlayerRole(1);
            networkButtonChoice1.image.sprite = selectedButtonSprite;
            networkButtonChoice2.image.sprite = normalButtonSprite;
            networkButtonChoice3.image.sprite = normalButtonSprite;
            networkButtonChoice4.image.sprite = normalButtonSprite;
            playerNameTextBox.gameObject.SetActive(false);
            serverIPTextBox.gameObject.SetActive(false);
            sessionIDInputBox.gameObject.SetActive(true);

        }
        else if (i == 1) // client
        {
            gameEngine._userRole = UserRole.Player;
            if(serverIPTextBox != null) serverIPTextBox.gameObject.SetActive(true);
            //SetPlayerRole(0);
            networkButtonChoice1.image.sprite = normalButtonSprite;
            networkButtonChoice2.image.sprite = selectedButtonSprite;
            networkButtonChoice3.image.sprite = normalButtonSprite;
            networkButtonChoice4.image.sprite = normalButtonSprite;
            playerNameTextBox.gameObject.SetActive(true);
            serverIPTextBox.gameObject.SetActive(true);
            sessionIDInputBox.gameObject.SetActive(false);
        }
        else if (i == 2) // viewer
        {
            gameEngine._userRole = UserRole.Viewer;
            if(serverIPTextBox != null) serverIPTextBox.gameObject.SetActive(true);
            //SetPlayerRole(0);
            networkButtonChoice1.image.sprite = normalButtonSprite;
            networkButtonChoice2.image.sprite = normalButtonSprite;
            networkButtonChoice3.image.sprite = selectedButtonSprite;
            networkButtonChoice4.image.sprite = normalButtonSprite;
            playerNameTextBox.gameObject.SetActive(false);
            serverIPTextBox.gameObject.SetActive(true);
            sessionIDInputBox.gameObject.SetActive(false);
        }
        else if (i == 3) // tracker
        {
            gameEngine._userRole = UserRole.Tracker;
            if (serverIPTextBox != null) serverIPTextBox.gameObject.SetActive(true);
            //SetPlayerRole(0);
            networkButtonChoice1.image.sprite = normalButtonSprite;
            networkButtonChoice2.image.sprite = normalButtonSprite;
            networkButtonChoice3.image.sprite = normalButtonSprite;
            networkButtonChoice4.image.sprite = selectedButtonSprite;
            playerNameTextBox.gameObject.SetActive(false);
            serverIPTextBox.gameObject.SetActive(true);
            sessionIDInputBox.gameObject.SetActive(false);
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


    public void ChangeVisualizationMode(string i){

        if (i == "3A" || i == "3B" || i == "3C"){
            trailsDecaySlider.gameObject.SetActive(true);
            //trailsDecaySlider.value = 
        }
        else trailsDecaySlider.gameObject.SetActive(false);

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


    public void quitApp()
    {
        Application.Quit();
    }
}
