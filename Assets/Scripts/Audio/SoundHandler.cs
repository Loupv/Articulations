using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SoundHandler : MonoBehaviour
{
    public GameEngine gameEngine;
    public OSCEndPoint oscEndPoint;

    public AudioSource[] instructions;
    public AudioSource recordAudioSource;
    

    public bool isRecording, recordPostScenarioAudio;
    public bool micConnected = false;  
    private string[] microphoneName;
    private int frequencyRate = 44100;
    int sessionID;
    private int sampleRate = 0;
    public string audioDirPath;
    public int postScenarioRecordingLenght, recordingTimeLeft;


    public void Init(string ip, int remotePort, UserRole userRole, bool recordAudioAfterScenario, int audioRecordLength){

        if (userRole == UserRole.Server)
        {
            recordPostScenarioAudio = recordAudioAfterScenario;
            postScenarioRecordingLenght = audioRecordLength;
        }

        oscEndPoint.ip = ip;
        oscEndPoint.remotePort = remotePort;
    }


    
    /* PLAYING */

    public void PlayInstructions(int i)
    {
        if(instructions != null && instructions[i] != null)
            instructions[i].Play();
    }




    /* RECORDING */

    public void InitAudioRecorder(int id, int audioRecordTime)
    {
        sessionID = id;

        audioDirPath = Application.dataPath + "/StreamingAssets/SoundRecords/"+System.DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss")+"_S"+sessionID+"_P" + (gameEngine.userManager.me._registeredRank+1) ;

        microphoneName = new string[Microphone.devices.Length];
        
        postScenarioRecordingLenght = audioRecordTime;
        
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
                recordAudioSource.clip = Microphone.Start (null, true, postScenarioRecordingLenght, frequencyRate); 
                isRecording = true; 
                
                PlayInstructions(0);
                Debug.Log("Audio Recording Started!");
                gameEngine.osc.sender.RecordAudioConfirmation();
            }  
        } 
        else {  
            Debug.Log("Microphone not connected!");  
        }  

        //if(postScenarioRecordingLenght > 0) Invoke("Stop", postScenarioRecordingLenght); // invoke stop when time is passed
    }


    // client - triggered by order from server
    public void Stop ()
    {
        recordAudioSource.clip = SavWav.TrimSilence (recordAudioSource.clip, 0);
        string fileName = "S"+sessionID+"_"+System.DateTime.Now.ToString("MM-dd-yyyy_hh-mm");
        isRecording = false; 
        SavWav.Save (audioDirPath, fileName, recordAudioSource.clip);
        gameEngine.osc.sender.AudioRecordHasStopped();
        Microphone.End (null);
        PlayInstructions(1);
        Debug.Log("Audio Recording Stopped !");  
	}

}
