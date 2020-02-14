using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class TileScript : MonoBehaviourPunCallbacks
{
    public Sprite spriteDisable;
    public Sprite spriteIdle;
    public Sprite spriteHover;
    public Sprite spritePress;

    private SpriteRenderer spriteRenderer;
    public enum TileState
    {
        Disable, Enable, Idle, Hover, Press
    }
    public TileState tileState;
    public int value;


    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        tileState = TileState.Disable;//初始不可按
    }
    private void OnMouseOver()
    {
        if(EventSystem.current.IsPointerOverGameObject() == false)
        {
            ChangeSprite(TileState.Hover);
            //左键按下
            if (Input.GetMouseButton(0))
            {
                ChangeSprite(TileState.Press);
            }
            //1.左键松开
            //2.此按钮为可点击状态
            if (Input.GetMouseButtonUp(0) &&
                tileState != TileState.Disable)
            {
                //禁用数字按钮
                ChangeSprite(TileState.Disable);
                foreach (var tile in GameManager.Instance.tiles)
                {
                    tile.ChangeSprite(TileState.Disable);
                }
                //调用[本地端.本地方]的重载ChooseNumber
                GameManager.Instance.players[GameManager.Instance.localIdx].ChooseNum(value);
            }
        }
    }
    private void OnMouseExit()
    {
        ChangeSprite(TileState.Idle);
    }

    public void ChangeSprite(TileState state)
    {
        switch (state)
        {
            case TileState.Disable:
                tileState = state;
                spriteRenderer.sprite = spriteDisable;
                break;
            case TileState.Enable:
                tileState = state;
                spriteRenderer.sprite = spriteIdle;
                break;
            case TileState.Idle:
                if (tileState != TileState.Disable)
                {
                    tileState = state;
                    spriteRenderer.sprite = spriteIdle;
                }
                break;
            case TileState.Hover:
                if (tileState != TileState.Disable)
                {
                    tileState = state;
                    spriteRenderer.sprite = spriteHover;
                }
                break;
            case TileState.Press:
                if (tileState != TileState.Disable)
                {
                    tileState = state;
                    spriteRenderer.sprite = spritePress;
                }
                break;
            default:
                break;
        }
    }
}
