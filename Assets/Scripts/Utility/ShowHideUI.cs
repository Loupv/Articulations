using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHideUI : MonoBehaviour
{
    public Camera camera;
    public bool visible = true;

    // Start is called before the first frame update
    void Start()
    {
        
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
        camera.cullingMask |= 1 << LayerMask.NameToLayer("UI");
        visible = true;
    }
    private void Hide() {
        camera.cullingMask &=  ~(1 << LayerMask.NameToLayer("UI"));
        visible = false;
    }

}
