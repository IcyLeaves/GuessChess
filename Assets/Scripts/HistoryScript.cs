using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryScript : MonoBehaviour
{
    public static HistoryScript Instance;
    public Sprite[] myNumsSprite;
    public Sprite[] hisNumsSprite;
    public GameObject airObject;

    private Vector2 nowLocation;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    public void CreateHistory(bool isLocal, int val)
    {
        //val=0为问号
        var g=Instantiate(airObject, gameObject.transform);
        g.transform.localPosition = nowLocation;
        nowLocation += new Vector2(0.5f, 0);
        if (isLocal)
        {
            g.GetComponent<SpriteRenderer>().sprite = myNumsSprite[val];
        }
        else
        {
            g.GetComponent<SpriteRenderer>().sprite = hisNumsSprite[val];
        }
    }

    public void EmptyHistory()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }
        nowLocation = Vector2.zero;
    }
}
