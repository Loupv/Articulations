using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{

    public GameEngine gameEngine;
    public Dropdown networkDropdown, userRoleDropdown;


    public void SetPlayerRole()
    {   
        if(userRoleDropdown.options[userRoleDropdown.value].text == "Player")
            gameEngine.userRole = UserRole.Player;
        else if(userRoleDropdown.options[userRoleDropdown.value].text == "Viewer")
            gameEngine.userRole = UserRole.Viewer;
    }

    public void SetPlayerNetworkType()
    {
        if (networkDropdown.options[networkDropdown.value].text == "Server")
            gameEngine.userNetworkType = UserNetworkType.Server;
        else if (networkDropdown.options[networkDropdown.value].text == "Client")
            gameEngine.userNetworkType = UserNetworkType.Client;
    }

    public void StartButtonPressed()
    {
        gameEngine.StartGame();
    }


}
