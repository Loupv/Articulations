using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasHandler : MonoBehaviour
{

    public GameObject initCanvas, gameCanvas, waitingCanvas;

    public void ChangeCanvas(string canvasName)
    {
        if (canvasName == "initCanvas")
        {
            initCanvas.SetActive(true);
            gameCanvas.SetActive(false);
            waitingCanvas.SetActive(false);
        }
        else if (canvasName == "gameCanvas")
        {
            initCanvas.SetActive(false);
            gameCanvas.SetActive(true);
            waitingCanvas.SetActive(false);
        }
        else if (canvasName == "waitingCanvas")
        {
            initCanvas.SetActive(false);
            gameCanvas.SetActive(false);
            waitingCanvas.SetActive(true);

        }
        else Debug.Log("Wrong Canvas Name !");
    }

}
