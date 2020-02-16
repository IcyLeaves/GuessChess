using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DarkPanelScript : MonoBehaviour
{
    public Image image;
    public bool isHide;
    public GameObject tempPanel;

    public Sprite hideSprite;
    public Sprite appearSprite;

    private void Start()
    {
        isHide = false;
    }
    public void OnHideBtnClick(Image btnImage)
    {
        if(!isHide)//如果当前是显示状态
        {
            isHide = !isHide;//改变状态
            image.color = new Color(0, 0, 0, 0);//透明度为0
            btnImage.sprite = hideSprite;//改为“隐藏”图标
            tempPanel.SetActive(false);
        }
        else
        {
            isHide = !isHide;//改变状态
            image.color = new Color(0, 0, 0, 100f/225f);//透明度为100
            btnImage.sprite = appearSprite;//改为“显示”图标
            tempPanel.SetActive(true);
        }
    }
    public void OnCloseBtnClick()
    {
        Destroy(tempPanel);
        gameObject.SetActive(false);
    }

}
