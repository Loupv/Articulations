using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasHandler : MonoBehaviour
{

    public GameObject initCanvas, gameCanvas, waitingCanvas, serverCanvas, viewerCanvas;

    public void ChangeCanvas(string canvasName)
    {
        if (canvasName == "initCanvas")
        {
            initCanvas.SetActive(true);
            gameCanvas.SetActive(false);
            viewerCanvas.SetActive(false);
            waitingCanvas.SetActive(false);
            serverCanvas.SetActive(false);
        }
        else if (canvasName == "gameCanvas")
        {
            initCanvas.SetActive(false);
            gameCanvas.SetActive(true);
            viewerCanvas.SetActive(false);
            waitingCanvas.SetActive(false);
            serverCanvas.SetActive(false);
        }
        else if (canvasName == "viewerCanvas")
        {
            initCanvas.SetActive(false);
            gameCanvas.SetActive(true);
            viewerCanvas.SetActive(true);
            waitingCanvas.SetActive(false);
            serverCanvas.SetActive(false);
        }
        else if (canvasName == "waitingCanvas")
        {
            initCanvas.SetActive(false);
            gameCanvas.SetActive(false);
            viewerCanvas.SetActive(false);
            waitingCanvas.SetActive(true);
            serverCanvas.SetActive(false);
        }
        else if(canvasName == "serverCanvas"){
            initCanvas.SetActive(false);
            gameCanvas.SetActive(true);
            viewerCanvas.SetActive(false);
            waitingCanvas.SetActive(false);
            serverCanvas.SetActive(true);
        }
        else Debug.Log("Wrong Canvas Name !");
    }

}
