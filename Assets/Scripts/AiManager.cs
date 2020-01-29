using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random=UnityEngine.Random;

public class AiManager : PlayerManager
{
    public override void StartTurn()
    {
        myState = PlayerState.ChooseNumbers;
        StartCoroutine(AddNum());
    }
    public IEnumerator AddNum()
    {
        //暂停value秒
        yield return new WaitForSeconds(1);
        //设定房间的当前数字
        int value = Random.Range(1, 6);
        GameManager.Instance.AddNowNum(value);
        Debug.Log("+" + value);
        //结束我的回合
        myState = PlayerState.OthersTurn;
        GameManager.Instance.OnCompleteTurn();
    }

}
