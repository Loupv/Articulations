﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{

    public GameEngine gameEngine;
    public Dropdown networkDropdown, userRoleDropdown;
    public InputField OSCInPortInput, OSCOutPortInput, OSCAddressInput;
    public GameObject clientObjectParent, serverObjectParent;
    public int OSCLocalPort, OSCRemotePort;
    public string address;

    public int isPlayer;

    public void SetPlayerRole()
    {
        if (userRoleDropdown.options[userRoleDropdown.value].text == "Player")
        {
            isPlayer = 1;
        }

        else if (userRoleDropdown.options[userRoleDropdown.value].text == "Viewer")
        {
            isPlayer = 0;
        }
    }

    public void SetPlayerNetworkType()
    {
        if (networkDropdown.options[networkDropdown.value].text == "Server")
        {
            gameEngine.userNetworkType = UserNetworkType.Server;
            if (clientObjectParent != null) clientObjectParent.gameObject.SetActive(false);
            if (serverObjectParent != null) serverObjectParent.gameObject.SetActive(true);
            SetPlayerRole();
            userRoleDropdown.gameObject.SetActive(true);

        }
        else if (networkDropdown.options[networkDropdown.value].text == "Client")
        {
            gameEngine.userNetworkType = UserNetworkType.Client;
            if(clientObjectParent != null) clientObjectParent.gameObject.SetActive(true);
            if (serverObjectParent != null) serverObjectParent.gameObject.SetActive(false);
            SetPlayerRole();
            userRoleDropdown.value= 0; // to prevent client/viewer case that is not handled atm
            userRoleDropdown.gameObject.SetActive(false);
        }
    }

    // does not change OSC object values, just the UI temporary ones
    public void ChangeOSCConfig()
    {
        int.TryParse(OSCInPortInput.text, out OSCLocalPort);
        int.TryParse(OSCOutPortInput.text, out OSCRemotePort);
        address = OSCAddressInput.text;
    }

    public void StartButtonPressed()
    {
        gameEngine.StartGame(isPlayer);
    }

    public void SwitchPortsNumbers()
    {
        int tmp = OSCLocalPort;
        OSCLocalPort = OSCRemotePort;
        OSCRemotePort = tmp;
        OSCInPortInput.text = OSCLocalPort.ToString();
        OSCOutPortInput.text = OSCRemotePort.ToString();
        ChangeOSCConfig();

    }


}
