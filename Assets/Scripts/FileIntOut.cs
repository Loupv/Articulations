using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using CsvHelper;
using System.Globalization;

[SerializeField]
public class PerformanceFile
{
    public List<PerformanceLine> lines;

}

public class PerformanceLine{
    public int SessionID { get; set; }
    public double Time { get; set; }
    public Vector3 position { get; set; } 
}



public class FileInOut {

    [HideInInspector]
    public bool jsonDataInitialized = false;


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


    public PerformanceFile LoadPerformance(string fileName){
        
        if(Application.platform == RuntimePlatform.OSXPlayer) fileName = "/Resources/Data/"+fileName;
        string filePath = Application.dataPath +"/StreamingAssets/Recordings/"+ fileName;

        Debug.Log("Loading Performance File at "+filePath);

        using(var reader = new StreamReader(filePath))
        using(var csv = new CsvReader(reader))
        {

            var records = new PerformanceFile();
            records.lines = new List<PerformanceLine>();

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
                    position = new Vector3(float.Parse(csv.GetField(5), cul), 
                    float.Parse(csv.GetField(6), cul),
                    float.Parse(csv.GetField(7), cul))
                };
                Debug.Log(record.SessionID+", "+record.Time+", "+record.position);
                records.lines.Add(record);

            }
            return(records);
        }

    }



}
