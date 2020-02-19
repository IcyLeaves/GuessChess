using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardHover : MonoBehaviour
{
    public static CardHover Instance;

    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    // Update is called once per frame
    void Update()
    {
        FollowMouse();
    }

    private void FollowMouse()
    {
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (transform.position.x < 0)//在左边
        {
            transform.position = new Vector3(transform.position.x+3, transform.position.y+4, 0);
        }
        else //在右边
        {
            transform.position = new Vector3(transform.position.x - 3, transform.position.y + 4, 0);
        }
        
    }

    public void Activate(int heroId)
    {
        if (spriteRenderer.enabled) return;
        spriteRenderer.sprite = HeroManager.Instance.heroCardPrefabs[heroId].GetComponent<Image>().sprite;
        spriteRenderer.enabled = true;
    }

    public void Deactivate()
    {
        spriteRenderer.enabled = false;
    }
}
