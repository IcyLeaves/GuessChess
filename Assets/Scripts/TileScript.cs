using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public int targetNumber;

    private Transform parent;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        tileState = TileState.Disable;
        targetNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        parent = transform.parent;
    }
    private void OnMouseOver()
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
            tileState !=TileState.Disable)
        {
            for(int i=0;i< parent.childCount;i++)
            {
                parent.GetChild(i).GetComponent<TileScript>().ChangeSprite(TileState.Disable);
            }
            //调用客户端对应Player脚本 加数字 方法
            GameManager.Instance.players[CustomProperties.playerLocalIdx].AddNum(value);
        }
    }
    private void OnMouseExit()
    {
        ChangeSprite(TileState.Idle);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //1.按钮的激活与否只跟自己的状态有关
        if (targetPlayer.ActorNumber != targetNumber) return;
        object tempObj;
        //[玩家状态]
        if (changedProps.TryGetValue("state", out tempObj))
        {
            PlayerManager.PlayerState state = (PlayerManager.PlayerState)tempObj;
            switch (state)
            {
                case PlayerManager.PlayerState.ChooseNumbers:
                        ChangeSprite(TileState.Enable);
                    break;
                default:
                    ChangeSprite(TileState.Disable);
                    break;
            }
        }
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
