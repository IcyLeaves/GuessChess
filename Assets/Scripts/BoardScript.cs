using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardScript : MonoBehaviour
{
    public Sprite spriteNothing;
    public Sprite spriteStar;
    public Sprite spriteDmg;
    public Sprite spriteDmgStar;
    public struct GridPoint
    {
        public Vector2 pos;
        public int playerIdx;
    }
    public GridPoint gridPos;

    private SpriteRenderer spriteRenderer;
    public enum BoardState
    {
        Nothing,Star,Damaged,DamagedStar
    }
    public BoardState boardState;



    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boardState = BoardState.Nothing;
    }
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(0))
        {
            //调用[本地端.本地方]方法
            GameManager.Instance.players[GameManager.Instance.localIdx].ClickGrid(this);
        }
    }

    public void ChangeSprite(BoardState state)
    {
        boardState = state;
        switch (state)
        {
            case BoardState.Star:
                spriteRenderer.sprite = spriteStar;
                break;
            case BoardState.Damaged:
                spriteRenderer.sprite = spriteDmg;
                break;
            case BoardState.DamagedStar:
                spriteRenderer.sprite = spriteDmgStar;
                break;
            default:
                break;
        }
    }
}
