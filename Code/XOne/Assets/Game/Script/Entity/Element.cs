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

    void Awake()
    {
        this.Init();
    }

    void Start()
    {
        
    }

    public override void Update()
    {
        base.Update();
    }

    public void Init()
    {
        this.CreateState(StateType.idle);
        this.CreateState(StateType.ani);

        this.SetState(StateType.idle);
    }

    public void Setup(ElementColor color, ElementType type)
    {
        this.color = color;
        this.type  = type;
        this.Populate();
    }

    private void Populate()
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

		// TODO:
		// 为什么不用系统的自己的Color，而是用了一个枚举做索引
		// 或者是不需要Color，直接使用Image，或是动画的属性
        this.image.color = colorList[(int) this.color];
    }

    public void ResetPos(Action Callback)
    {
        this.SetState(StateType.ani);

        DOTween.Sequence()
               .Append(this.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutBack))
               .AppendInterval(0f)
               .AppendCallback
		        (
		            () =>
		            {
		                this.SetState(StateType.idle);
		                if (Callback != null)
		                {
		                    Callback();
		                }
		            }
		        );
    }

    public void Drop(float duration, Action Callback)
    {
        this.SetState(StateType.ani);

        this.transform.DOLocalMove(Vector3.zero, duration).SetEase(Ease.Linear).OnComplete
		(
			() =>
	        {
	            this.SetState(StateType.idle);
	            if (Callback != null)
	            {
	                Callback();
	            }
	        }
		);
    }

    public void DropWithBounce(float duration, Action Callback)
    {
        this.SetState(StateType.ani);

        Sequence sequence = DOTween.Sequence();

        sequence.Append(this.transform.DOLocalMove(new Vector3(0, -10, 0), duration).SetEase(Ease.Linear));
        sequence.Append(this.transform.DOLocalMove(new Vector3(0, 5, 0), 0.05f).SetEase(Ease.Linear));
        sequence.Append(this.transform.DOLocalMove(new Vector3(0, -2, 0), 0.05f).SetEase(Ease.Linear));
        sequence.Append(this.transform.DOLocalMove(new Vector3(0, 0, 0), 0.05f).SetEase(Ease.Linear));
        sequence.AppendCallback
        (
            () =>
            {
                this.SetState(StateType.idle);
                if (Callback != null)
                {
                    Callback();
                }
            }
        );

        //this.transform.DOLocalMove(Vector3.zero, duration).SetEase(Ease.OutBounce).OnComplete
        //(
        //    () =>
        //    {
        //        this.SetState(StateType.idle);
        //        if (Callback != null)
        //        {
        //            Callback();
        //        }
        //    }
        //);
    }

    public void Crash()
    {
        Destroy(this.gameObject);
    }

    public void SetColor(ElementColor color)
    {
        this.color = color;
        this.Populate();
    }

    public void SetType(ElementType type)
    {
        this.type = type;
        this.Populate();
    }

    public ElementColor color { private set; get; }
    public ElementType  type  { private set; get; }

    public enum StateType : int
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
