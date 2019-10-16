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
    public double TimeStamp {get;set;}
    public double Time { get; set; }
    public string Condition{get; set;}
    public Vector3 p1HeadPosition { get; set; } 
    public Vector3 p1LeftHandPosition { get; set; } 
    public Vector3 p1RightHandPosition { get; set; } 
    public Quaternion p1HeadRotation { get; set; } 
    public Quaternion p1LeftHandRotation { get; set; } 
    public Quaternion p1RightHandRotation { get; set; } 
    public Vector3 p2HeadPosition { get; set; } 
    public Vector3 p2LeftHandPosition { get; set; } 
    public Vector3 p2RightHandPosition { get; set; } 
    public Quaternion p2HeadRotation { get; set; } 
    public Quaternion p2LeftHandRotation { get; set; } 
    public Quaternion p2RightHandRotation { get; set; } 
}



public class FileInOut : MonoBehaviour {

    [HideInInspector]
    public bool jsonDataInitialized = false;
    string performanceFilePath;
    GameEngine gameEngine;
    PlaybackManager playbackManager;
    public List<string> performanceDataFiles;


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

    public void PopulatePlaybackDataFileDropdown(UnityEngine.UI.Dropdown dropdown){
        
        dropdown.ClearOptions();

        string[] files = Directory.GetFiles(Application.dataPath +"/StreamingAssets/Recordings/");

        foreach(string file in files){
            if (!file.Contains(".meta") && !file.Contains(".DS_Store")) {
                dropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData(file.Replace(Application.dataPath +"/StreamingAssets/Recordings/", "")));
                performanceDataFiles.Add(file);
            }
        }
        
    }


    public void LoadPerformance(string fileName, PlaybackManager pm){
        
        //if(Application.platform == RuntimePlatform.OSXPlayer) fileName = "/Resources/Data/"+fileName;
        performanceFilePath = fileName;

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
                    TimeStamp = csv.GetField<double>(1),
                    Time = csv.GetField<double>(2),
                    Condition = csv.GetField<string>(4),
                    p1HeadPosition = new Vector3(float.Parse(csv.GetField(5), cul), 
                    float.Parse(csv.GetField(6), cul),
                    float.Parse(csv.GetField(7), cul)),
                    p1HeadRotation = new Quaternion(float.Parse(csv.GetField(8), cul), 
                    float.Parse(csv.GetField(9), cul),
                    float.Parse(csv.GetField(10), cul),
                    float.Parse(csv.GetField(11), cul)),
                    p1LeftHandPosition = new Vector3(float.Parse(csv.GetField(12), cul), 
                    float.Parse(csv.GetField(13), cul),
                    float.Parse(csv.GetField(14), cul)),
                    p1LeftHandRotation = new Quaternion(float.Parse(csv.GetField(15), cul), 
                    float.Parse(csv.GetField(16), cul),
                    float.Parse(csv.GetField(17), cul),
                    float.Parse(csv.GetField(18), cul)),
                    p1RightHandPosition = new Vector3(float.Parse(csv.GetField(19), cul), 
                    float.Parse(csv.GetField(20), cul),
                    float.Parse(csv.GetField(21), cul)),
                    p1RightHandRotation = new Quaternion(float.Parse(csv.GetField(22), cul), 
                    float.Parse(csv.GetField(23), cul),
                    float.Parse(csv.GetField(24), cul),
                    float.Parse(csv.GetField(25), cul)),

                    p2HeadPosition = new Vector3(float.Parse(csv.GetField(26), cul), 
                    float.Parse(csv.GetField(27), cul),
                    float.Parse(csv.GetField(28), cul)),
                    p2HeadRotation = new Quaternion(float.Parse(csv.GetField(29), cul), 
                    float.Parse(csv.GetField(30), cul),
                    float.Parse(csv.GetField(31), cul),
                    float.Parse(csv.GetField(32), cul)),
                    p2LeftHandPosition = new Vector3(float.Parse(csv.GetField(33), cul), 
                    float.Parse(csv.GetField(34), cul),
                    float.Parse(csv.GetField(35), cul)),
                    p2LeftHandRotation = new Quaternion(float.Parse(csv.GetField(36), cul), 
                    float.Parse(csv.GetField(37), cul),
                    float.Parse(csv.GetField(38), cul),
                    float.Parse(csv.GetField(39), cul)),
                    p2RightHandPosition = new Vector3(float.Parse(csv.GetField(40), cul), 
                    float.Parse(csv.GetField(41), cul),
                    float.Parse(csv.GetField(42), cul)),
                    p2RightHandRotation = new Quaternion(float.Parse(csv.GetField(43), cul), 
                    float.Parse(csv.GetField(44), cul),
                    float.Parse(csv.GetField(45), cul),
                    float.Parse(csv.GetField(46), cul))
                };
                if(record != null){
                    //Debug.Log(record.SessionID+", "+record.Time+", "+record.p1HeadPosition);
                    playbackManager.performanceFile.lines.Add(record);
                }

            }
        }
        playbackManager.performanceMaxTime = (float)(playbackManager.performanceFile.lines[playbackManager.performanceFile.lines.Count-1].Time/1000);
    }

}
