using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{

    public GameEngine gameEngine;
    public Dropdown networkDropdown, userRoleDropdown;
    public InputField OSCInPortInput, OSCOutPortInput, OSCAddressInput;
    public int OSCInPort, OSCOutPort;
    public string address;

    private int isPlayer;

    public void SetPlayerRole()
    {   
        if(userRoleDropdown.options[userRoleDropdown.value].text == "Player")
            isPlayer = 0;
        else if(userRoleDropdown.options[userRoleDropdown.value].text == "Viewer")
            isPlayer = 1;
    }

    public void SetPlayerNetworkType()
    {
        if (networkDropdown.options[networkDropdown.value].text == "Server")
            gameEngine.userNetworkType = UserNetworkType.Server;
        else if (networkDropdown.options[networkDropdown.value].text == "Client")
            gameEngine.userNetworkType = UserNetworkType.Client;
    }

    public void ChangeOSCConfig()
    {
        int.TryParse(OSCInPortInput.text, out OSCInPort);
        int.TryParse(OSCOutPortInput.text, out OSCOutPort);
        address = OSCAddressInput.text;
    }

    public void StartButtonPressed()
    {
        gameEngine.StartGame(isPlayer);
    }

    public void SwitchPortsNumbers()
    {
        int tmp = OSCInPort;
        OSCInPort = OSCOutPort;
        OSCOutPort = tmp;
        OSCInPortInput.text = OSCInPort.ToString();
        OSCOutPortInput.text = OSCOutPort.ToString();
        ChangeOSCConfig();

    }


}
