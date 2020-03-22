using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Help : MonoBehaviour
{
    public struct HeroInfo
    {
        public int id;
        public int wins;
        public int all;
        public HeroInfo(int i,int w,int a)
        {
            id = i;
            wins = w;
            all = a;
        }
    }
    private List<HeroInfo> heroes=new List<HeroInfo>();

    public GameObject infoPanel;
    public TMP_Text infoText;
    public Image cardImage;
    public List<Sprite> cardSprites;


    private void Start()
    {
        ReadWins();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Close();
    }
    private void ReadWins()
    {
        string filename = Application.dataPath + @"\Statistics\wins.txt";
        string[] strs = File.ReadAllLines(filename);//将filename路径的txt读取为string数组
        foreach(var str in strs)
        {
            string[] arr=str.Split(' ');
            heroes.Add(new HeroInfo(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2])));
        }
    }
    public void Back()
    {
        SceneManager.LoadScene("Launcher");
    }
    
    public void Close()
    {
        if(infoPanel.activeSelf)
        infoPanel.SetActive(false);
    }

    public void OpenInfo(int cardIdx)
    {
        cardImage.sprite = cardSprites[cardIdx];
        infoText.text = "胜场/总场数：" + heroes[cardIdx].wins + " / " + heroes[cardIdx].all;
        infoPanel.SetActive(true);
    }
}
