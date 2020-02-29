using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random=UnityEngine.Random;



public class CardPanelScript : MonoBehaviour
{
    public static CardPanelScript Instance;
    private int[] heroIdxs;

  

    private void Start()
    {
        Instance = this;
        heroIdxs = HeroManager.Instance.myHeroCardIdxs;
        for (int i= 0; i<HeroManager.Instance.numOfCards; i++)
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





}
