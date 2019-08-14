using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AudioRecordManager : MonoBehaviour
{
    public GameEngine gameEngine;
    public bool isRecording, recordPostScenarioAudio;
    private AudioSource audioSource;
    public bool micConnected = false;  
    private string[] microphoneName;
    private int frequencyRate = 44100;
    int sessionID;
    private int sampleRate = 0;
    public string audioDirPath;
    public int postScenarioRecordingLenght, recordingTimeLeft;

    public void InitAudioRecorder(int id, int audioRecordTime)
    {
        sessionID = id;

        audioDirPath = Application.dataPath + "/StreamingAssets/SoundRecords/P" + gameEngine.userManager.me._registeredRank + "_" + System.DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss");

        microphoneName = new string[Microphone.devices.Length];
       
        postScenarioRecordingLenght = audioRecordTime;
        audioSource = GetComponent<AudioSource>();

        sampleRate = AudioSettings.outputSampleRate;

        //Check if there is at least one microphone connected  
        if (Microphone.devices.Length <= 0) {  
                //Throw a warning message at the console if there isn't  
                Debug.LogWarning ("Microphone not connected!");  
        } else { 
                micConnected = true; 
                Debug.Log("Microphone ready"); 
        }
    }
 
    public void Launch (int recordLength)
    {
        postScenarioRecordingLenght = recordLength;
        recordingTimeLeft = postScenarioRecordingLenght;

        if (!Directory.Exists (audioDirPath)) {   
                //if it doesn't, create it
                Directory.CreateDirectory (audioDirPath);
        }

        if (micConnected) {  
            //If the audio from any microphone isn't being captured  
            if (!Microphone.IsRecording (null)) {   
                //Start recording and store the audio captured from the microphone at the AudioClip in the AudioSource  
                audioSource.clip = Microphone.Start (null, true, postScenarioRecordingLenght, frequencyRate); 
                isRecording = true; 
                
                Debug.Log("Audio Recording Started!");
                gameEngine.osc.sender.RecordAudioConfirmation();
            }  
        } 
        else {  
            Debug.Log("Microphone not connected!");  
        }  

        if(postScenarioRecordingLenght > 0) Invoke("Stop", postScenarioRecordingLenght); // invoke stop when time is passed
    }


    public void Stop ()
    {
        audioSource.clip = SavWav.TrimSilence (audioSource.clip, 0);
        string fileName = "S"+sessionID+"_"+System.DateTime.Now.ToString("MM-dd-yyyy_hh-mm");
        isRecording = false; 
        SavWav.Save (audioDirPath, fileName, audioSource.clip);
        gameEngine.osc.sender.AudioRecordHasStopped();
        Microphone.End (null);
        gameEngine.instructionPlayer.PlayInstructions(1);
        Debug.Log("Audio Recording Stopped !");  
	}
}
