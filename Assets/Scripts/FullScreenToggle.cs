using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class FullScreenToggle : MonoBehaviour
{
    static string fullScreenPrefKey = "isFullScreen";
    private Toggle toggle;
    // Start is called before the first frame update
    void Start()
    {
        int defaultSetting = 1;
        toggle = GetComponent<Toggle>();
        if (PlayerPrefs.HasKey(fullScreenPrefKey))
        {
            defaultSetting = PlayerPrefs.GetInt(fullScreenPrefKey);
            toggle.isOn = defaultSetting==1?true:false;
        }
        ChangeResolution(toggle.isOn);
    }
    public void ChangeResolution(bool isFullScreen)
    {
        if (isFullScreen)
            Screen.SetResolution(1920, 1080, true);
        else
            Screen.SetResolution(1600, 900, false);
    }
    public void SetToggle()
    {
        ChangeResolution(toggle.isOn);
        PlayerPrefs.SetInt(fullScreenPrefKey, toggle.isOn ? 1 : 0);
    }
}
