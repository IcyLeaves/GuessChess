using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public int cols = 8;
    public int rows = 8;
    public GameObject floorTile;
    public Transform playerOneCornerPoint;
    public Transform playerTwoCornerPoint;

    private Transform boardHolder;

    private void Start()
    {
        BoardSetup();
    }
    /// <summary>
    /// 外墙和地板的生成函数
    /// </summary>
    void BoardSetup()
    {
        boardHolder = new GameObject("Board").transform;

        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector3 pos = new Vector3(playerOneCornerPoint.position.x + x, playerOneCornerPoint.position.y + y, 0f);
                GameObject instance = Instantiate(floorTile, pos, Quaternion.identity) as GameObject;
                instance.transform.SetParent(boardHolder);

                pos= new Vector3(playerTwoCornerPoint.position.x + x, playerTwoCornerPoint.position.y + y, 0f);
                instance = Instantiate(floorTile, pos, Quaternion.identity) as GameObject;
                instance.transform.SetParent(boardHolder);
            }

        }
    }
}
