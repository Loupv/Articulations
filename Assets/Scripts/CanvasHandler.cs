using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasHandler : MonoBehaviour
{

    public GameObject initCanvas, waitingCanvas, gameCanvas, serverCanvas, viewerCanvas,playbackCanvasOn, playbackCanvasOff;

    public void ChangeCanvas(string canvasName)
    {
        if (canvasName == "initCanvas")
        {
            initCanvas.SetActive(true);
            gameCanvas.SetActive(false);
            viewerCanvas.SetActive(false);
            waitingCanvas.SetActive(false);
            serverCanvas.SetActive(false);
            playbackCanvasOn.SetActive(false);
            playbackCanvasOff.SetActive(false);
        }
        else if (canvasName == "gameCanvas")
        {
            initCanvas.SetActive(false);
            gameCanvas.SetActive(true);
            viewerCanvas.SetActive(false);
            waitingCanvas.SetActive(false);
            serverCanvas.SetActive(false);
            playbackCanvasOn.SetActive(false);
            playbackCanvasOff.SetActive(false);
        }
        else if (canvasName == "viewerCanvas")
        {
            initCanvas.SetActive(false);
            gameCanvas.SetActive(true);
            viewerCanvas.SetActive(true);
            waitingCanvas.SetActive(false);
            serverCanvas.SetActive(false);
           playbackCanvasOn.SetActive(false);
            playbackCanvasOff.SetActive(false);
        }
        else if (canvasName == "waitingCanvas")
        {
            initCanvas.SetActive(false);
            gameCanvas.SetActive(false);
            viewerCanvas.SetActive(false);
            waitingCanvas.SetActive(true);
            serverCanvas.SetActive(false);
            playbackCanvasOn.SetActive(false);
            playbackCanvasOff.SetActive(false);
        }
        else if(canvasName == "serverCanvas"){
            initCanvas.SetActive(false);
            gameCanvas.SetActive(true);
            viewerCanvas.SetActive(false);
            waitingCanvas.SetActive(false);
            serverCanvas.SetActive(true);
            playbackCanvasOn.SetActive(false);
            playbackCanvasOff.SetActive(false);
        }
        else if(canvasName == "playbackCanvasOff"){
            initCanvas.SetActive(false);
            gameCanvas.SetActive(true);
            viewerCanvas.SetActive(true);
            waitingCanvas.SetActive(false);
            serverCanvas.SetActive(false);
            playbackCanvasOn.SetActive(false);
            playbackCanvasOff.SetActive(true);
        }
        else if(canvasName == "playbackCanvasOn"){
            initCanvas.SetActive(false);
            gameCanvas.SetActive(true);
            viewerCanvas.SetActive(false);
            waitingCanvas.SetActive(false);
            serverCanvas.SetActive(false);
            playbackCanvasOn.SetActive(true);
            playbackCanvasOff.SetActive(false);
        }
        else Debug.Log("Wrong Canvas Name !");
    }

}
