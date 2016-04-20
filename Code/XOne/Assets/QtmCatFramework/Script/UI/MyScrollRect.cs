using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG;
using DG.Tweening;
using System;

namespace QtmCatFramework
{
	public class MyScrollRect : ScrollRect 
	{
		private RectTransform  viewRect;
		private int            touchNum = 0;


		public override void OnBeginDrag (PointerEventData eventData)
		{
			if(Input.touchCount > 1) 
			{
				return;
			}

			base.OnBeginDrag(eventData);
		}

		public override void OnDrag (PointerEventData eventData)
		{
			if (Input.touchCount > 1)
			{
				touchNum = Input.touchCount;
				//this.MultipDrag();
				return;
			}
			else if(Input.touchCount == 1 && touchNum > 1)
			{
				touchNum = Input.touchCount;
				base.OnBeginDrag(eventData);
				return;
			}

			base.OnDrag(eventData);
		}


		protected override void Start()
		{
			base.Start();

			this.viewRect = AUIManager.Instance.uiRoot.GetComponent<RectTransform>();
			this.GetComponent<RectTransform>().sizeDelta = this.viewRect.sizeDelta;
		}


		private float preX;
		private float preY;

		private void Update()
		{
			if (Input.touchCount == 2)
			{
				Touch   t1   = Input.GetTouch(0);
				Touch   t2   = Input.GetTouch(1);

				Vector3 p1   = t1.position;
				Vector3 p2   = t2.position;

				float   newX = Mathf.Abs(p1.x - p2.x);
				float   newY = Mathf.Abs(p1.y - p2.y);

				if (t1.phase == TouchPhase.Began || t2.phase == TouchPhase.Began)
				{
					preX = newX;
					preY = newY;
				}
				else if (t1.phase == TouchPhase.Moved && t2.phase == TouchPhase.Moved)
				{	
					RectTransform rt    = base.content;
					float         scale = (newX + newY - preX - preY) / (rt.rect.width * 0.25f) + rt.localScale.x;

					if (scale > 1.0f && scale < 2.5f)
					{		
						float ratio   = scale / rt.localScale.x;

						rt.localScale = new Vector3(scale, scale, 0);

						float maxX    = base.content.rect.width  * scale / 2 - this.viewRect.rect.width  / 2;
						float minX    = -maxX;

						float maxY    = base.content.rect.height * scale / 2 - this.viewRect.rect.height / 2;
						float minY    = -maxY;

						Vector3 pos   = rt.position * ratio;

						if (pos.x > maxX)
						{
							pos.x = maxX;
						}
						else if (pos.x < minX)
						{
							pos.x = minX;
						}

						if (pos.y > maxY)
						{
							pos.y = maxY;
						}
						else if (pos.y < minY)
						{
							pos.y = minY;
						}

						rt.position = pos;
					}
				}

				preX = newX;
				preY = newY;
			}
		}

//		private Vector3 originPos;
//		private Vector3 originScale;
//		private bool    isNeedMoveBack = false;
		public void MoveToUI(GameObject who, Transform to)
		{
//			if (!this.isNeedMoveBack)
//			{
//				this.originPos   = base.content.transform.position;
//				this.originScale = base.content.localScale;
//			}
//
//			this.isNeedMoveBack = true;
			//float scale         = CityMapManager.Instance.scaleForMoveToUI / base.content.localScale.x;

			//base.content.DOScale(CityMapManager.Instance.scaleForMoveToUI, CityMapManager.Instance.scaleTime).SetEase(CityMapManager.Instance.scaleTransition);

			//float x = to.position.x - who.transform.position.x * scale + base.content.position.x * scale;
			//float y = to.position.y - who.transform.position.y * scale + base.content.position.y * scale;

			//base.content.transform.DOMove(new Vector3(x, y, 0), CityMapManager.Instance.moveTime)
			//	.SetEase(CityMapManager.Instance.moveTransition);
		}

		public void MoveBack()
		{
//			base.content.DOScale(this.originScale.x, CityMapManager.Instance.scaleBackTime).SetEase(CityMapManager.Instance.scaleBackTransition);
//			base.content.transform.DOMove(this.originPos, CityMapManager.Instance.moveBackTime)
//				.SetEase(CityMapManager.Instance.moveBackTransition)
//				.OnComplete
//				(
//					() => 
//					{
//						isNeedMoveBack = false;
//					}
//				);
		}

		public void MoveToCenter(GameObject from, GameObject center, TweenCallback callback = null)
		{
			Vector3  pos        = from.transform.position;
			Vector3  contentPos = base.content.transform.position;


			// RectTransform rt    = from.GetComponent<RectTransform>();

			contentPos.x       -= pos.x - center.transform.position.x;
			contentPos.y       -= pos.y - center.transform.position.y;

			//			float   portWidthHalf     = this.viewRect.rect.width  / 2;
			//			float   portHeightHalf    = this.viewRect.rect.height / 2;
			//									  
			//			float   contentWidthHalf  = base.content.rect.width  * base.content.localScale.x / 2;
			//			float   contentHeightHalf = base.content.rect.height * base.content.localScale.y / 2;	
			//
			//			float maxX  = contentWidthHalf  - portWidthHalf;
			//			float minX  = -maxX;
			//			
			//			float maxY  = contentHeightHalf - portHeightHalf;
			//			float minY  = -maxY;
			//
			//			if (contentPos.x > maxX)
			//			{
			//				contentPos.x = maxX;
			//			}
			//			else if (contentPos.x < minX)
			//			{
			//				contentPos.x = minX;
			//			}
			//			
			//			if (contentPos.y > maxY)
			//			{
			//				contentPos.y = maxY;
			//			}
			//			else if (contentPos.y < minY)
			//			{
			//				contentPos.y = minY;
			//			}

			float duration = UnityEngine.Mathf.Sqrt(pos.x * pos.x + pos.y * pos.y) / 1912.0f;

			base.content.transform.DOMove(contentPos, duration)
				                  .SetEase(Ease.OutSine)
				                  .OnComplete(callback);		
		}
	}
}
