﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIHandler : MonoBehaviour
{

    public GameEngine gameEngine;
    public UserManager userManager;
    public NetworkManager networkManager;
    public FileInOut fileInOut;
    public PlaybackManager playbackManager;
    public ScenarioEvents scenarioEvents;
    public SendOSC sender;
    public Button networkButtonChoice1, networkButtonChoice2, networkButtonChoice3, networkButtonChoice4,networkButtonChoice5, 
        autoMode, manualMode, launchScenario, pauseScenario, stopScenario, stopAudioRecord;
    public Button FreeCam, POVPlayer1, POVPlayer2, POV3;
    public InputField OSCServerAddressInput, sessionIDInputBox, playbackSpeedInputField;
    public GameObject serverManualModeParent, serverAutoModeParent;
    public GameObject clientGOParent, playerNameTextBox, serverGOParent, playbackGOParent;
    public GameObject recordGizmo, pauseGizmo;
    public Dropdown scenarioDropDown, onlineOfflinePlaybackMode, switchPerformanceDataFile, switchPlaybackPlayer;
    public Sprite selectedButtonSprite, normalButtonSprite, playSprite, pauseSprite;
    public Toggle sendToAudioDeviceToggle, autoRecordPerformance, recordAudioAfterScenario;
    public Slider trailsDecaySlider, playbackSpeedSlider;
    public Text trailTime, conditionTimeRemaining, recordingTimeRemaining, currentConditionText, playbackTime, currentViz;
    private string scenarioMode;
    int tmpTimer;
    public Image playPauseButtonImage;


    void Start(){

        if(sendToAudioDeviceToggle.isOn) networkManager.sendToAudioDevice = true;
        else networkManager.sendToAudioDevice = false;

        fileInOut.PopulatePlaybackDataFileDropdown(switchPerformanceDataFile);
        ScenarioModeSwitch(1);

    }

    /* ------------------------------------------------ */
    /* ------ Methods triggered by UI Component ------- */
    /* ------------------------------------------------ */


    public void StartButtonPressed()
    {
        if(!(gameEngine._userRole == UserRole.Server && sessionIDInputBox.GetComponent<InputField>().text == "")) gameEngine.StartGame();
        else Debug.Log("Please enter session ID");
    }


    /* SERVER ONLY */
    public void EnvironmentChange(string env)
    {
        Debug.Log(env);
        // change env to server
        if (env == "sky") scenarioEvents.SetNextSkybox();
        else if (env == "mirror" || env == "mirrorAlt") scenarioEvents.ToggleMirror(!scenarioEvents.mirrorAct, 0, 0);
        else if (env == "naoto") scenarioEvents.ToggleNaoto(!scenarioEvents.naotoAct);

        // change env to clients
        networkManager.EnvironmentChangeOrder(userManager.usersPlaying, env);
    }
    public void ScenarioDropDownChanged(int value){
        scenarioEvents.currentScenario = value;
    }
    public void ChangeVisualisationMode(string i){
        userManager.ChangeVisualisationMode(i, gameEngine, false);
    }
    public void ToggleSendToAudioHandler(){
        networkManager.sendToAudioDevice = !networkManager.sendToAudioDevice;
    }
    public void TrailsDecaySliderChanged(int id){
        sender.SendTrailValueChange(id, trailsDecaySlider.value, userManager.usersPlaying);
        userManager.ChangeVisualisationParameter(id, trailsDecaySlider.value); // update also for server visualisation
        trailTime.text = "TrailTime : "+trailsDecaySlider.value;
    }


    // this method stores in each user data a gap vector that centers him back to the markers at the center of the field
    // for a client's instance it translates its own parent and leaves head and hands at 0 (to be used with vive locally)
    // for a client's other instance, it add the gap to each of the sphere
    // for the server it adds the gap during the update function (taking data from the pending position dictionnary) 

    public void CalibratePlayersPosition(){
        scenarioEvents.CalibratePlayersPositions(userManager.usersPlaying);
           //userManager.CalibratePlayerTransform();
    }

    /* PLAYBACK ONLY */
    public void ChangePlaybackSpeedSlider(){
        playbackManager.playbackSpeed = playbackSpeedSlider.value;
        playbackSpeedInputField.text = Math.Round(playbackSpeedSlider.value, 1).ToString().Replace(",",".");
    }
    public void ChangePlaybackSpeedTextField(){
        float tmpValue = float.Parse(playbackSpeedInputField.text.Replace(".",","));
        if(tmpValue > playbackSpeedSlider.maxValue) tmpValue = playbackSpeedSlider.maxValue;
        else if(tmpValue < playbackSpeedSlider.minValue) tmpValue = playbackSpeedSlider.minValue;
        playbackSpeedSlider.value = tmpValue;
        playbackManager.playbackSpeed = tmpValue;
    }

    /* VIEWER / SERVER */
    public void ChangeViewerPOV(int pov){
        
        int tmp = 0;

        switch(pov){

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
        
        default:
        POVPlayer1.image.sprite = normalButtonSprite;
        POVPlayer2.image.sprite = normalButtonSprite;
        break;
        
        }
        userManager.viewerController.UpdatePOV(pov, tmp);
    }

    



    /* -------------------------------------------------------------------- */
    /* Method triggered by script to de/activate or actualize UI components */
    /* -------------------------------------------------------------------- */


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
            if(scenarioEvents.timerPaused) pauseScenario.image.sprite = selectedButtonSprite;
            else pauseScenario.image.sprite = normalButtonSprite;
        }
        else if(value == 0){ // stop
            launchScenario.gameObject.SetActive(true);
            pauseScenario.gameObject.SetActive(false);
        }

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
        launchScenario.gameObject.SetActive(false);
        stopAudioRecord.gameObject.SetActive(true);
        recordingTimeRemaining.gameObject.SetActive(true);
        //tmpTimer = gameEngine.audioRecordManager.postScenarioRecordingLenght;
        tmpTimer = 0;
        InvokeRepeating("TimerUI", 0f,1f);
    }

    public void CancelRecordTime(){
        launchScenario.gameObject.SetActive(true);
        stopAudioRecord.gameObject.SetActive(false);
        recordingTimeRemaining.gameObject.SetActive(false);
        CancelInvoke("TimerUI");
    }

    void TimerUI(){
        recordingTimeRemaining.text = "Record Time : "+tmpTimer;
        tmpTimer++;
        //tmpTimer--;
        //if(tmpTimer < 0) CancelRecordTime();
    }


    public void FillServerIPField(int runInLocal, string serverIP){

        if (runInLocal == 1) {
            OSCServerAddressInput.text = "127.0.0.1";
        }
        else {
            OSCServerAddressInput.text = serverIP;
            
        }
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
            if(playbackManager.mode == PlaybackMode.Online) OSCServerAddressInput.gameObject.SetActive(true);
            else if(playbackManager.mode == PlaybackMode.Offline) OSCServerAddressInput.gameObject.SetActive(false);
            clientGOParent.gameObject.SetActive(false);
            serverGOParent.gameObject.SetActive(false);
            playbackGOParent.gameObject.SetActive(true);
        }
    }


    public void PlayPauseIconSwitch()
    {
        if (playPauseButtonImage.sprite == playSprite) playPauseButtonImage.sprite = pauseSprite;
        else playPauseButtonImage.sprite = playSprite;
    }

    public void ShowPlayerNoses(Toggle tog)
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
        {
            go.GetComponent<UserData>().head.transform.GetChild(1).gameObject.SetActive(tog.isOn);
        }
    }

    /*public void quitApp()
    {
        Application.Quit();
    }*/
}
