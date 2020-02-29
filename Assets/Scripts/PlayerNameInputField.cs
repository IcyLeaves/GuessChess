using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerNameInputField : MonoBehaviour
{
    static string playerNamePrefKey = "PlayerName";
    // Start is called before the first frame update
    void Start()
    {
        string defaultName = "";
        TMP_InputField _inputField = GetComponent<TMP_InputField>();
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
