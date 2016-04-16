using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG;
using DG.Tweening;

public class Element : MonoBehaviour 
{
    public Image image;

    public Image selectImage;

    void Start ()
    {

    }

    void Update ()
    {

    }

    public void Setup (ElementColor color, ElementType type)
    {
        this.color  = color;
        this.type   = type;
        this.Finalize ();
    }

    private void Finalize ()
    {
        Color[] colorList   = new Color[] { Color.clear, Color.red, Color.green, Color.blue, new Color(1, 0, 1, 1), Color.yellow };
        this.image.color    = colorList[(int)this.color];
    }

    public void Reset ()
    {
        this.transform.DOLocalMove (Vector3.zero, 0.5f).SetEase (Ease.OutBack);
    }

    public void SetColor (ElementColor color)
    {
        this.color = color;
        this.Finalize ();
    }

    public void SetType (ElementType type)
    {
        this.type = type;
        this.Finalize ();
    }

    public ElementColor color { private set; get; }

    public ElementType type { private set; get; }
}

public enum ElementColor
{
    None = 0,
    Red,
    Green,
    Blue,
    Purple,
    Yellow,
}

public enum ElementType
{
    Normal = 0,
    Horizontal, // 水平
    Vertical,   // 垂直
    Boom,       // 炸弹
    Super,
}
