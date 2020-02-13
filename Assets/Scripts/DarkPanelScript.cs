using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DarkPanelScript : MonoBehaviour
{
    public Image image;
    public bool isHide;

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
            image.raycastTarget = false;//可以穿透
            btnImage.sprite = hideSprite;//改为“隐藏”图标
        }
        else
        {
            isHide = !isHide;//改变状态
            image.color = new Color(0, 0, 0, 100f/225f);//透明度为100
            image.raycastTarget = true;//不可以穿透
            btnImage.sprite = appearSprite;//改为“显示”图标
        }
    }
    public void OnCloseBtnClick()
    {
        gameObject.SetActive(false);
    }

}
