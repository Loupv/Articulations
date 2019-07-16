using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using CsvHelper;
using System.Globalization;


public class PerformanceFile
{
    public List<PerformanceLine> lines;

}

public class PerformanceLine{
    public int SessionID { get; set; }
    public double Time { get; set; }
    public Vector3 p1HeadPosition { get; set; } 
    public Vector3 p1LeftHandPosition { get; set; } 
    public Vector3 p1RightHandPosition { get; set; } 
    public Vector3 p2HeadPosition { get; set; } 
    public Vector3 p2LeftHandPosition { get; set; } 
    public Vector3 p2RightHandPosition { get; set; } 
}



public class FileInOut : MonoBehaviour {

    [HideInInspector]
    public bool jsonDataInitialized = false;
    string performanceFilePath;
    PlaybackManager playbackManager;


    public GameData LoadGameData(string jsonName)
    {
        if(Application.platform == RuntimePlatform.OSXPlayer) jsonName = "/Resources/Data/"+jsonName;
        string filePath = Application.dataPath + jsonName;

        Debug.Log("Loading Json at "+filePath);
        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            GameData gameData = JsonUtility.FromJson<GameData>(dataAsJson);
            Debug.Log("GameData JSON loaded successfuly");
            return gameData;
        }
        else
        {
            Debug.Log("JSON File not found");
            return null;
        }

    }


    public void LoadPreferencesFiles(GameEngine gameEngine){

#if UNITY_ANDROID
        Debug.Log("tablet initializing");

        // game data json
        string filePath = Path.Combine(Application.streamingAssetsPath, "GameData.json");
        UnityWebRequest www = UnityWebRequest.Get(filePath);
        yield return www.SendWebRequest();
        string dataAsJson = www.downloadHandler.text;
        Debug.Log(dataAsJson);
        gameData = JsonUtility.FromJson<GameData>(dataAsJson);

        // scenario json
        filePath = Path.Combine(Application.streamingAssetsPath, "Scenarios.json");
        UnityWebRequest www = UnityWebRequest.Get(filePath);
        yield return www.SendWebRequest();
        dataAsJson = www.downloadHandler.text;
        scenarioEvents.scenarios = JsonUtility.FromJson<Scenario[]>(dataAsJson);
#else
        // load preferences file
        gameEngine.gameData = LoadGameData("/StreamingAssets/GameData.json");
        gameEngine.scenarioEvents.scenarios = LoadScenarioList("/StreamingAssets/Scenarios.json").scenarios;
#endif

    }


    public ScenarioList LoadScenarioList(string jsonName)
    {
        if(Application.platform == RuntimePlatform.OSXPlayer) jsonName = "/Resources/Data/"+jsonName;
        string filePath = Application.dataPath + jsonName;

        Debug.Log("Loading Json at "+filePath);
        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            ScenarioList scenarioList = JsonUtility.FromJson<ScenarioList>(dataAsJson);
            Debug.Log("Scenarios JSON loaded successfuly");
            return scenarioList;
        }
        else
        {
            Debug.Log("JSON File not found");
            return null;
        }

    }


    public void LoadPerformance(string fileName, PlaybackManager pm){
        
        if(Application.platform == RuntimePlatform.OSXPlayer) fileName = "/Resources/Data/"+fileName;
        performanceFilePath = Application.dataPath +"/StreamingAssets/Recordings/"+ fileName;

        playbackManager = pm;

        Invoke("LoadPerfData",0f);

    }


    void LoadPerfData(){

        Debug.Log("Loading Performance File at "+performanceFilePath);
        StreamReader reader = new StreamReader(performanceFilePath);

        using(CsvReader csv = new CsvReader(reader))
        {

            playbackManager.performanceFile = new PerformanceFile();
            playbackManager.performanceFile.lines = new List<PerformanceLine>();

            csv.Read();
            csv.ReadHeader();
            csv.Configuration.Delimiter= ";";
            //csv.Configuration.MissingFieldFound = null;

            NumberFormatInfo cul = new CultureInfo("en-US").NumberFormat;
            while(csv.Read())
            {
                PerformanceLine record = new PerformanceLine(){
                    SessionID = csv.GetField<int>(0),
                    Time = csv.GetField<double>(2),
                    p1HeadPosition = new Vector3(float.Parse(csv.GetField(5), cul), 
                    float.Parse(csv.GetField(6), cul),
                    float.Parse(csv.GetField(7), cul)),
                    p1LeftHandPosition = new Vector3(float.Parse(csv.GetField(11), cul), 
                    float.Parse(csv.GetField(12), cul),
                    float.Parse(csv.GetField(13), cul)),
                    p1RightHandPosition = new Vector3(float.Parse(csv.GetField(17), cul), 
                    float.Parse(csv.GetField(18), cul),
                    float.Parse(csv.GetField(19), cul)),
                    p2HeadPosition = new Vector3(float.Parse(csv.GetField(23), cul), 
                    float.Parse(csv.GetField(24), cul),
                    float.Parse(csv.GetField(25), cul)),
                    p2LeftHandPosition = new Vector3(float.Parse(csv.GetField(29), cul), 
                    float.Parse(csv.GetField(30), cul),
                    float.Parse(csv.GetField(31), cul)),
                    p2RightHandPosition = new Vector3(float.Parse(csv.GetField(35), cul), 
                    float.Parse(csv.GetField(36), cul),
                    float.Parse(csv.GetField(37), cul))
                };
                if(record != null){
                    //Debug.Log(record.SessionID+", "+record.Time+", "+record.p1HeadPosition);
                    playbackManager.performanceFile.lines.Add(record);
                }

            }
        }
    }

}
