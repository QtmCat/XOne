using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG;
using DG.Tweening;
using System;
using QtmCatFramework;

public class Element : StateMachine
{
    public Image image;

    public Image selectImage;

    void Start()
    {
        this.Init();
    }

    public override void Update()
    {
        base.Update();
    }

    private void Init()
    {
        this.CreateState((int) StateType.idle);
        this.CreateState((int) StateType.ani);

        this.SetState((int) StateType.idle);
    }

    public void Setup(ElementColor color, ElementType type)
    {
        this.color = color;
        this.type  = type;
        this.Finalize();
    }

	// TODO: 和object的方法冲突
    private void Finalize()
    {
        Color[] colorList = new Color[] 
		{
			Color.clear,
			Color.red,
			Color.green,
			Color.blue, 
			new Color(1, 0, 1, 1),
			Color.yellow 
		};

        this.image.color   = colorList[(int) this.color];
    }

    public void ResetPos(Action Callback)
    {
        this.SetState((int) StateType.ani);
        this.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutBack).OnComplete
		(
			() =>
       	    {
	            this.SetState((int) StateType.idle);
	            if (Callback != null)
	            {
	                Callback();
	            }
        	}
		);
    }

    public void Drop(float duration, Action Callback)
    {
        this.SetState((int) StateType.ani);
        this.transform.DOLocalMove(Vector3.zero, duration).SetEase(Ease.OutBack).OnComplete
		(
			() =>
	        {
	            this.SetState((int) StateType.idle);
	            if (Callback != null)
	            {
	                Callback();
	            }
	        }
		);
    }

    public void Crash()
    {
        Destroy(this.gameObject);
    }

    public void SetColor(ElementColor color)
    {
        this.color = color;
        this.Finalize();
    }

    public void SetType(ElementType type)
    {
        this.type = type;
        this.Finalize();
    }

    public ElementColor color { private set; get; }
    public ElementType  type  { private set; get; }

    public enum StateType
    {
        idle = 0,
        ani,
    }
}

public enum ElementColor
{
    none = 0,
    red,
    green,
    blue,
    purple,
    yellow,
}

public enum ElementType
{
    normal = 0,
    horizontal, // 水平
    vertical,   // 垂直
    boom,       // 炸弹
    super,
}
