using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random=UnityEngine.Random;

public class AiManager : PlayerManager
{
    private void Start()
    {
        myId = 1;
    }
    public override void StartTurn()
    {
        //myState = PlayerState.ChooseNumbers;
        StartCoroutine(AddNum());
    }
    private IEnumerator AddNum()
    {
        //暂停1秒
        yield return new WaitForSeconds(1);
        //设定选择的数字
        int value = Random.Range(1, 6);
        //GameManager.Instance.AddNowNum(value);
        Debug.Log("+" + value);
        //结束我的回合
        //myState = PlayerState.OthersTurn;
        GameManager.Instance.OnCompleteTurn();
    }//!

    public override void PlaceStar()
    {
        //myState = PlayerState.PlaceStar;
        myStarPos = new Vector2(Random.Range(1, 9), Random.Range(1, 9));
        Debug.Log("Star placed in " + myStarPos);
        //GameManager.Instance.StarPlaced();
    }//!

    public override void Attack(int ammos)
    {
        //myState = PlayerState.Attack;
        StartCoroutine(AttackBoard(ammos));
    }
    private IEnumerator AttackBoard(int ammos)
    {
        for(int i=0;i<ammos;i++)
        {
            //暂停1秒
            yield return new WaitForSeconds(1);
            //选择
            var board=BoardManager.Instance.PickRandomAlive(0);
            Debug.Log("Attack at " + board.gridPos.pos);
            //如果攻击到了星星
            if (board.gridPos.pos == GameManager.Instance.players[1-myId].myStarPos)
            {
                //回调，将被点击方块改成受损星星
                board.ChangeSprite(BoardScript.BoardState.DamagedStar);
                //取消鼠标悬浮图标
                Hover.Instance.Deactivate();
                //通知GameManager游戏结束，我是赢家
                GameManager.Instance.Over(myId);
                yield break;
            }
            //回调，将被点击方块改成受损
            board.ChangeSprite(BoardScript.BoardState.Damaged);
        }
        
        //通知GameManager，开始新的Turn
        //myState = PlayerState.OthersTurn;
        GameManager.Instance.TurnBegin();
    }//!
}
