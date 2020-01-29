using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    public int targetPlayerId = 0;
    public int value;
    private void OnMouseOver()
    {
        //1.左键按下 2.当前是你的回合
        if (Input.GetMouseButtonDown(0) && IsYourTurn())
        {
            //为玩家选择数字
            PickNum(GameManager.Instance.players[targetPlayerId]);
        }
    }
    private bool IsYourTurn()
    {
        return GameManager.Instance.currentPlayerIdx == targetPlayerId;
    }

    private void PickNum(PlayerManager player)
    {
        //玩家挑选数字加上
        if (player.myState == PlayerManager.PlayerState.ChooseNumbers)
            player.AddNum(value);
    }
}
