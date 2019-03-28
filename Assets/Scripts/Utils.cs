using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static string GetIpFromInt(int i, string localIP){
        int index = localIP.LastIndexOf(".");
        if(index>0) {
            string ip = localIP.Substring(0, index)+"."+i.ToString();
            return ip;
            }
        else return "null";
    }

    public static int GetLastIntFromIp(string ip){
        int index = ip.LastIndexOf(".");
        
        if(index>0){ 
            string tmp = ip.Substring(index+1, ip.Length-index-1);
            int.TryParse(tmp, out int r);
            return r;

        }
        else return -1;
    }
}
