using UnityEngine;
using System.Collections;
using QtmCatFramework;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG;
using DG.Tweening;

[ExecuteInEditMode]
public class MapCell : StateMachine, IPointerDownHandler, IPointerEnterHandler
{
    public bool isShow;                 // 是否显示

    public bool isNest;                 // 是否生产Element

    public IceLevel iceLevel;           // 冰块级别 (0: 无)

    public StoneLevel stoneLevel;       // 石块级别 (0: 无)

    public GameObject selectPanel;

    public GameObject elementPanel;

    public GameObject[] iceList;

    public GameObject[] stoneList;



    private Button button;

	void Start ()
    {
        this.Init ();
    }

    void Update ()
    {

    }

    private void Init ()
    {
        this.Finalize ();
    }

    public void Setup (int colIndex, int rowIndex, Element element)
    {
        this.colIndex   = colIndex;
        this.rowIndex   = rowIndex;
        this.element    = element;

        this.element.transform.parent           = this.elementPanel.transform;
        this.element.transform.localScale       = Vector3.one;
        this.element.transform.localPosition    = Vector3.zero;
        this.element.transform.localRotation    = Quaternion.identity;
    }

    private void Finalize ()
    {
        
    }

    public void SetElement (Element element)
    {
        this.element                    = element;
        this.element.transform.parent   = this.transform;
    }

    public void SetSelected (bool selected)
    {
        this.selected                   = selected;
        this.selectPanel.SetActive (this.selected);
    }

    public void OnPointerDown (PointerEventData eventData)
    {
        MapCellManager.Instance.SetPointerMapCell (this);
    }

    public void OnPointerEnter (PointerEventData eventData)
    {
        
    }

    public int colIndex { private set; get; }

    public int rowIndex { private set; get; }

    public bool selected { private set; get; }

    public Element element { set; get; }
}

public enum IceLevel
{
    None = 0,
    One,
    Double,
    Triple
}

public enum StoneLevel
{
    None = 0,
    One,
    Double,
    Triple
}
