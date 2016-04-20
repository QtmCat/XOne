using UnityEngine;
using System.Collections;

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
        this.preColor = ElementColor.None;
    }

    public Element CreateElement()
    {
		// TODO: AResource.Load<GameObject>("Prefab/Element"); 
		// TODO: Instantiate 有封裝接口
        GameObject obj     = Instantiate(Resources.Load ("Prefab/Element")) as GameObject;
        Element    element = obj.GetComponent<Element>();
        element.Setup (this.RandomColor(), ElementType.Normal);

        return element;
    }

    private ElementColor RandomColor()
    {
        ElementColor color;

        while (true)
        {
            color = this.colorList[UnityEngine.Random.Range(0, this.colorList.Length)];
            if (this.preColor == ElementColor.None || this.preColor != color)
            {
                this.preColor = color;
                break;
            }
        }

        return color;
    }
}
