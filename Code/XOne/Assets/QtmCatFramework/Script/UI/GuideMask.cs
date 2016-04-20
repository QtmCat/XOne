using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using DG;
using DG.Tweening;


namespace QtmCatFramework
{
	[ExecuteInEditMode]
	public class GuideMask : MonoBehaviour
	{
		public float fade      = 1.0f;
		public float intensity = 1.0f;

		[Space(30)]
		public RectTransform   top;
		public RectTransform   bottom;
		public RectTransform   left;
		public RectTransform   right;
		public RectTransform   eventArea;
		public Image           image;


		public static GuideMask instance;


		public static GuideMask SetRectCenter(Vector2 center, Vector2 size, Action DoAction, bool isUseVignette = true)
		{
			GameObject go   = AUIManager.InstantiatePrefabToUICamera("GuideMask");
			GuideMask  mask = go.GetComponent<GuideMask>();

			GuideMask.instance = mask;

			go.transform.SetAsLastSibling();

			mask.SetRectCenterInner
			(
				center,
				size,
				isUseVignette
			);

			mask.eventArea.gameObject.GetComponent<EventIgnore>().DoAction += 
			() =>
			{
				DoAction();
				DestroyImmediate(go);
			};

			return mask;
		}


		private void SetRectCenterInner(Vector2 center, Vector2 size, bool isUseVignette = true)
		{           
			RectTransform   root     = AUIManager.instance.uiRoot.GetComponent<RectTransform>();
			Vector2         half     = root.sizeDelta * root.localScale.x / 2;
			Vector2         halfSize = size           * root.localScale.x / 2;


			top.sizeDelta            = root.sizeDelta;
			bottom.sizeDelta         = root.sizeDelta;
			left.sizeDelta           = root.sizeDelta;
			right.sizeDelta          = root.sizeDelta;


			Vector2 tl               = new Vector2(center.x - halfSize.x, center.y + halfSize.y);
			Vector2 br               = new Vector2(center.x + halfSize.x, center.y - halfSize.y);

			top.position             = new Vector3(0, tl.y + half.y, 0);
			bottom.position          = new Vector3(0, br.y - half.y, 0);

			left.position            = new Vector3(tl.x - half.x, br.y + halfSize.y, 0);
			right.position           = new Vector3(br.x + half.x, br.y + halfSize.y, 0);

			left.sizeDelta           = new Vector2(left.sizeDelta.x,  size.y);
			right.sizeDelta          = new Vector2(right.sizeDelta.x, size.y);

			eventArea.position       = new Vector3(center.x, center.y, 0);
			eventArea.sizeDelta      = size;

			image.gameObject.SetActive(isUseVignette);

			if (isUseVignette)
			{
				Vector2 imageSize = image.GetComponent<RectTransform>().sizeDelta = root.sizeDelta;

				image.material.SetFloat("_width",     0);
				image.material.SetFloat("_heigth",    0);
				image.material.SetFloat("_fade",      0);
				image.material.SetFloat("_intensity", 0);

				image.material.SetFloat("_offsetX",   -eventArea.position.x / half.x);
				image.material.SetFloat("_offsetY",   -eventArea.position.y / half.y);



				image.material.DOFloat(size.x / imageSize.x + 0.85f,  "_width",  0.3f).SetEase(Ease.OutBack);
				image.material.DOFloat(size.y / imageSize.y + 0.5f,  "_heigth", 0.3f).SetEase(Ease.OutBack);

				image.material.DOFloat(fade,       "_fade",      0.3f).SetEase(Ease.OutSine);
				image.material.DOFloat(intensity,  "_intensity", 0.3f).SetEase(Ease.OutSine);
			}
		}
	}
}

