using UnityEngine;
using System.Collections;
using QtmCatFramework;

public class Incubator : MonoBehaviour 
{
    public  ElementColor[] colorList;

    private ElementColor   preColor;

    void Start()
    {
        this.Init();
    }

    private void Init()
    {
        this.preColor = ElementColor.none;
    }

    public Element CreateElement()
    {
        GameObject obj      = AUIManager.InstantiatePrefab("Prefab/Element");
        Element    element  = obj.GetComponent<Element>();
        element.Setup (this.RandomColor(), ElementType.normal);

        return element;
    }

    private ElementColor RandomColor()
    {
        ElementColor color;

        while (true)
        {
            color = this.colorList[UnityEngine.Random.Range(0, this.colorList.Length)];
            if (this.preColor == ElementColor.none || this.preColor != color)
            {
                this.preColor = color;
                break;
            }
        }

        return color;
    }
}
