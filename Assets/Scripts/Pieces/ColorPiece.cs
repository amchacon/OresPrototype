using UnityEngine;
using System.Collections.Generic;

public class ColorPiece : MonoBehaviour
{
    public enum ColorType
    {
        YELLOW,
        PURPLE,
        RED,
        BLUE,
        GREEN,
        PINK
    };

    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;
        public Sprite sprite;
    };

    public ColorSprite[] colorSprites;

    private ColorType color;

    public ColorType Color
    {
        get { return color; }
        set { SetColor(value); }
    }

    public int NumColors => colorSprites.Length;

    [SerializeField] private SpriteRenderer sprite = null;

    private Dictionary<ColorType, Sprite> colorSpriteDict;

    void Awake()
    {
        colorSpriteDict = new Dictionary<ColorType, Sprite>();
        for (int i = 0; i < colorSprites.Length; i++)
        {
            if (!colorSpriteDict.ContainsKey(colorSprites[i].color))
            {
                colorSpriteDict.Add(colorSprites[i].color, colorSprites[i].sprite);
            }
        }
    }

    public void SetColor(ColorType newColor)
    {
        color = newColor;
        if (colorSpriteDict.ContainsKey(newColor))
        {
            sprite.sprite = colorSpriteDict[newColor];
        }
    }
}
