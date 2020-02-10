using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    public int cols = 8;
    public int rows = 8;
    public GameObject floorTile;
    public Transform playerOneCornerPoint;
    public Transform playerTwoCornerPoint;

    private void Start()
    {
        Instance = this;
        BoardSetup();

    }
    /// <summary>
    /// 外墙和地板的生成函数
    /// </summary>
    void BoardSetup()
    {

        for (int y = 0; y <rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                //确定位置
                Vector3 pos = new Vector3(playerOneCornerPoint.position.x + x, playerOneCornerPoint.position.y - y, 0f);
                GameObject instance = Instantiate(floorTile, pos, Quaternion.identity) as GameObject;
                //设置父对象
                instance.transform.SetParent(playerOneCornerPoint);
                //设置脚本中的初始值
                instance.GetComponent<BoardScript>().gridPos.pos = new Vector2(x, y);
                instance.GetComponent<BoardScript>().gridPos.playerIdx = 0;


                pos = new Vector3(playerTwoCornerPoint.position.x + x, playerTwoCornerPoint.position.y - y, 0f);
                instance = Instantiate(floorTile, pos, Quaternion.identity) as GameObject;
                instance.transform.SetParent(playerTwoCornerPoint);
                instance.GetComponent<BoardScript>().gridPos.pos = new Vector2(x, y);
                instance.GetComponent<BoardScript>().gridPos.playerIdx = 1;
            }

        }
    }

    //获得随机存活方块
    public BoardScript PickRandomAlive(int playerId)
    {
        List<BoardScript> liveBoard = SelectAllAlive(playerId);

        int idx = Random.Range(0, liveBoard.Count);
        return liveBoard[idx];
    }
    //获取指定位置方块
    public BoardScript GetPosBoard(int playerId, Vector2 pos)
    {
        Transform selectedBoard = playerId == 0 ? playerOneCornerPoint : playerTwoCornerPoint;
        return selectedBoard.GetChild((int)(pos.y * cols + pos.x)).GetComponent<BoardScript>();
    }
    private List<BoardScript> SelectAllAlive(int playerId)
    {
        Transform selectedBoard = playerId == 0 ? playerOneCornerPoint : playerTwoCornerPoint;
        List<BoardScript> tmpBoard = new List<BoardScript>();
        for (int i = 0; i < selectedBoard.childCount; i++)
        {
            var child = selectedBoard.GetChild(i).gameObject.GetComponent<BoardScript>();
            if (child.boardState == BoardScript.BoardState.Nothing ||
                child.boardState == BoardScript.BoardState.Star)
                tmpBoard.Add(child);
        }
        return tmpBoard;
    }
}
