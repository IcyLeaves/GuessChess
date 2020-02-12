using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random=UnityEngine.Random;

public class CardPanelScript : MonoBehaviour
{
    public int numOfCards = 3;

    private int[] heroIdxs;

    private void Start()
    {
        heroIdxs = new int[numOfCards];
        RandomHeroCards();
        for (int i= 0; i<numOfCards; i++)
        {
            var g = Instantiate(HeroManager.Instance.heroCardPrefabs[heroIdxs[i]]);
            var _i = i;
            g.transform.SetParent(gameObject.transform);
            g.GetComponent<Button>().onClick.AddListener(delegate()
            {
                GameManager.Instance.OnHeroCardClick(heroIdxs[_i]);
            });
        }
    }

    private void RandomHeroCards()
    {
        //数字列表
        List<int> nums = new List<int>();
        for(int i=0;i< HeroManager.Instance.heroCardPrefabs.Length;i++)
        {
            nums.Add(i);
        }
        //抽一次去掉一个数字
        for(int i=0;i<numOfCards;i++)
        {
            var idx = Random.Range(0, nums.Count);
            heroIdxs[i]=nums[idx];
            nums.RemoveAt(idx);
        }
    }
}
