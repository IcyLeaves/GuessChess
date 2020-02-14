using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HeroIconScript : MonoBehaviour
{
    public Sprite spriteDisable;
    public Sprite spriteIdle;
    public Sprite spriteHover;
    public Sprite spritePress;
    public Sprite spritePassive;

    private SpriteRenderer spriteRenderer;
    public enum IconState
    {
        Disable, Idle,Enable, Hover, Press,Passive
    }
    public IconState iconState;
    public bool isPassive;
    public int heroId;
    private float hovertime=0;


    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        iconState = IconState.Disable;//初始不可按
    }
    private void OnMouseOver()
    {
        if(EventSystem.current.IsPointerOverGameObject() == false)
        {
            hovertime += Time.deltaTime * 1;
            if(hovertime>=1.5)
            {
                CardHover.Instance.Activate(heroId);
            }
            ChangeSprite(IconState.Hover);
            //左键按下
            if (Input.GetMouseButton(0))
            {
                ChangeSprite(IconState.Press);
            }
            //1.左键松开
            //2.此按钮为可点击状态
            if (Input.GetMouseButtonUp(0) &&
                iconState != IconState.Disable)
            {
                //禁用数字按钮
                ChangeSprite(IconState.Disable);
                //Do something...
            }
        }
    }
    private void OnMouseExit()
    {
        hovertime = 0;
        CardHover.Instance.Deactivate();
        ChangeSprite(IconState.Idle);
    }

    public void ChangeSprite(IconState state)
    {
        if (iconState == IconState.Passive) return;
        switch (state)
        {
            case IconState.Disable:
                iconState = state;
                spriteRenderer.sprite = spriteDisable;
                break;
            case IconState.Enable:
                iconState = state;
                spriteRenderer.sprite = spriteIdle;
                break;
            case IconState.Idle:
                if (iconState != IconState.Disable)
                {
                    iconState = state;
                    spriteRenderer.sprite = spriteIdle;
                }
                break;
            case IconState.Hover:
                if (iconState != IconState.Disable)
                {
                    iconState = state;
                    spriteRenderer.sprite = spriteHover;
                }
                break;
            case IconState.Press:
                if (iconState != IconState.Disable)
                {
                    iconState = state;
                    spriteRenderer.sprite = spritePress;
                }
                break;
            default:
                break;
        }
    }
}
