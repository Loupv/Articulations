﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;




public class JSONLoader {

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

}
