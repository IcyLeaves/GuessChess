using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class PlayerNameInputField : MonoBehaviour
{
    static string playerNamePrefKey = "PlayerName";
    // Start is called before the first frame update
    void Start()
    {
        string defaultName = "";
        InputField _inputField = GetComponent<InputField>();
        if(_inputField!=null)
        {
            if(PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                _inputField.text = defaultName;
            }
        }
        PhotonNetwork.LocalPlayer.NickName = defaultName;
    }

    public void SetPlayerName(string value)
    {
        PhotonNetwork.LocalPlayer.NickName = value + " ";
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
}
