using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoBehaviour
{
    private float _spriteHeight;

    public float SpriteHeight
    {
        get { return _spriteHeight; }
    }

    private float _spriteWidth;

    public float SpriteWidth
    {
        get { return _spriteWidth; }
    }

    public Sprite[] sprites;

    public Sprite indicatorSprite;

    

    private List<GameObject> _piecesToCopy;

    public List<GameObject> PiecesToCopy
    {
        get { return _piecesToCopy; }

    }

    public GameObject Indicator;


    // Start is called before the first frame update
    void Start()
    {
        _piecesToCopy = new List<GameObject>();
        _spriteWidth = sprites[0].rect.width / 100.0f;
        _spriteHeight = sprites[0].rect.height / 100.0f;
        GameObject g;
        SpriteRenderer sr;
        foreach (var curSprite in sprites)
        {
            g = new GameObject();
            sr = g.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
            sr.sprite = curSprite;
            g.SetActive(false);
            _piecesToCopy.Add(g);
        }
        Indicator = new GameObject();
        sr = Indicator.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        sr.sprite = indicatorSprite;
        Indicator.transform.localScale = new Vector3(sprites[0].rect.width / sr.sprite.rect.width, sprites[0].rect.height / sr.sprite.rect.height, 1);
        Indicator.SetActive(false);
        GameObject.Find("Board").GetComponent<BoardManager>().Init();
    }
}
