using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHideUI : MonoBehaviour
{
    public Camera cam;
    public bool visible = true;
    public bool init;

    // Start is called before the first frame update
    void Awake()
    {
        if (cam == null) cam = GameObject.Find("CanvasCamera").GetComponent<Camera>();
        init = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("h")){
            //camera.cullingMask ^= 1 << LayerMask.NameToLayer("SomeLayer");
            if(visible) Hide();
            else Show();
        }
    }

    private void Show(){
        cam.cullingMask |= 1 << LayerMask.NameToLayer("UI");
        visible = true;
    }
    private void Hide() {
        cam.cullingMask &=  ~(1 << LayerMask.NameToLayer("UI"));
        visible = false;
    }

}
