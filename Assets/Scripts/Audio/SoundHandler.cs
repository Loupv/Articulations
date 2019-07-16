using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundHandler : MonoBehaviour
{
    public OSCEndPoint oscEndPoint;
    
    public void Init(string ip, int remotePort){
        oscEndPoint.ip = ip;
        oscEndPoint.remotePort = remotePort;
    }

}
