using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    public int value;

    private void OnMouseOver()
    {
        //1.左键按下
        //2.此客户端状态为<选择数字>
        if (Input.GetMouseButtonDown(0) &&
            CustomProperties.GetLocalState() == PlayerManager.PlayerState.ChooseNumbers)
        {
            //调用客户端对应Player脚本 加数字 方法
            GameManager.Instance.players[CustomProperties.playerLocalIdx].AddNum(value);
        }
    }
}
