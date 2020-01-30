using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{
    public static Hover Instance;

    private SpriteRenderer spriteRenderer;
    public enum HoverState
    {
        Star,Attack
    }
    public HoverState hoverState;
    public Sprite spriteStar;
    public Sprite spriteAttack;
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
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    public void Activate(HoverState state)
    {
        switch (state)
        {
            case HoverState.Star:
                spriteRenderer.sprite = spriteStar;
                break;
            case HoverState.Attack:
                spriteRenderer.sprite = spriteAttack;
                break;
            default:
                break;
        }
        spriteRenderer.enabled = true;
    }

    public void Deactivate()
    {
        spriteRenderer.enabled = false;
    }
}
