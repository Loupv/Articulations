using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugScreen : MonoBehaviour
{
         string myLog;
         Queue myLogQueue = new Queue();
     
         void Start(){
         }
     
         void OnEnable () {
             Application.logMessageReceived += HandleLog;
         }
         
         void OnDisable () {
             Application.logMessageReceived -= HandleLog;
         }
     
         void HandleLog(string logString, string stackTrace, LogType type){
             myLog = logString;
            if(type != LogType.Warning){
             string newString = "\n [" + type + "] : " + myLog;
             myLogQueue.Enqueue(newString);
             if (type == LogType.Exception)
             {
                 newString = "\n" + stackTrace;
                 myLogQueue.Enqueue(newString);
             }
             myLog = string.Empty;

             if(myLogQueue.Count > 10) myLogQueue.Dequeue();

             foreach(string mylog in myLogQueue){
                 myLog += mylog;
             }
            }
         }
     
         void OnGUI () {
             GUILayout.Label(myLog);
         }
     }



