using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;

public class AiManager : PlayerManager
{
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        object tempObj;
        //[进入攻击状态]
        if (changedProps.TryGetValue("state", out tempObj))
        {
            PlayerState state = (PlayerState)tempObj;
            if (state == PlayerState.Attack)
            {
                myState = state;
            }
        }
    }
    public override void InitInfo()
    {
        myHp = 0;
        myHeroId = Random.Range(0,3);
        myIdx = 1;
        myActorNumber = -1;
        myNickName = "Enemy";
        mySprite = specialSprites[1];
        myState = PlayerState.Ready;
    }

    public override void PlaceStar()
    {
        StartCoroutine(AiPlaceStar());
        
    }
    private IEnumerator AiPlaceStar()
    {
        yield return new WaitForSeconds(1);
        myStarPos = new Vector2(Random.Range(0, 8), Random.Range(0, 8));
        myState = PlayerState.PlaceComplete;
        myHp++;
    }

    public override void ChooseNum()
    {
        StartCoroutine(AiChooseNum());
    }
    private IEnumerator AiChooseNum()
    {
        yield return new WaitForSeconds(1);
        int res = Random.Range(1, 6);
        GameManager.Instance.selectNum = res;
    }

    public override void Attack()
    {
        StartCoroutine(AiAttack());
    }
    private IEnumerator AiAttack()
    {
        yield return new WaitForSeconds(1);
        myState = PlayerState.Attack;
        //选择
        var board = BoardManager.Instance.PickRandomAlive(0);
        GameManager.Instance.attackPos = board.gridPos.pos;
    }

}
