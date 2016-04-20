using UnityEngine;
using System.Collections;
using QtmCatFramework;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG;
using DG.Tweening;
using System;

[ExecuteInEditMode]
public class MapCell : StateMachine, IPointerDownHandler, IPointerEnterHandler
{
    public bool         isExist;     // 是否存在

    public bool         isNest;      // 是否生产Element

    public IceLevel     iceLevel;    // 冰块级别 (0: 无)

    public StoneLevel   stoneLevel;  // 石块级别 (0: 无)

    public GameObject   selectPanel;

    public GameObject   elementPanel;

    public GameObject[] iceList;

    public GameObject[] stoneList;



    private float width;

	void Start()
    {
        this.Init ();
    }

    private void Init()
    {
        this.width      = this.GetComponent<RectTransform>().rect.width;

        this.Populate();
    }

    public void Setup(int colIndex, int rowIndex, Element element)
    {
        this.colIndex   = colIndex;
        this.rowIndex   = rowIndex;
        this.element    = element;

		this.element.transform.SetParent(this.elementPanel.transform, true);
        this.element.transform.localScale       = Vector3.one;
        this.element.transform.localPosition    = Vector3.zero;
        this.element.transform.localRotation    = Quaternion.identity;
    }

    private void Populate()
    {
        
    }

    public void SetElement(Element element)
    {
        this.element = element;
        if (this.element != null)
        {
            this.element.transform.SetParent(this.elementPanel.transform, true);
        }
    }

    public void SetHatchElement(Element element)
    {
        this.element = element;

        this.element.transform.SetParent(this.elementPanel.transform, true);
        this.element.transform.localScale       = Vector3.one;
        this.element.transform.localPosition    = this.transform.up * this.width;
        this.element.transform.localRotation    = Quaternion.identity;
    }

    public void ResetElementPos(Action Callback = null)
    {
        this.element.ResetPos(Callback);
    }

    public void Drop(Action Callback)
    {
        float duration = Vector3.Distance(this.element.transform.localPosition, Vector3.zero) / this.width * 0.5f;
        this.element.Drop(duration, Callback);
    }

    public void CrashElement()
    {
        this.element.Crash();
        this.element = null;
    }

    public bool isSameColor(MapCell other)
    {
        if (this.element == null || other.element == null)
        {
            return false;
        }

        if (this.element.GetCurStateId() != (int) Element.StateType.idle || other.element.GetCurStateId() != (int) Element.StateType.idle)
        {
            return false;
        }

        return this.element.color == other.element.color;
    }

    public bool isCoundDrop()
    {
        return this.element == null;
    }

    public bool isCoundHatch()
    {
        return this.element == null;
    }

    public void SetSelected(bool selected)
    {
        this.selected = selected;
        this.selectPanel.SetActive(this.selected);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MapCellManager.instance.SetPointerMapCell(this);
    }

    public void OnPointerEnter (PointerEventData eventData)
    {
        
    }

    public int     colIndex { private set; get; }

    public int     rowIndex { private set; get; }

    public bool    selected { private set; get; }

    public Element element  { set; get; }

    public delegate bool CrashTestHandler(MapCell mapCell);
	// TODO: Func<MapCell, boll> CrashTestHandler;
}

public enum IceLevel
{
    none = 0,
    one,
    two,
    three
}

public enum StoneLevel
{
    none = 0,
    one,
    two,
    three
}
