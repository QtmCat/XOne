using System;
using UnityEngine;
using UnityEngine.UI;

namespace QtmCatFramework
{
	public class UIDepth : MonoBehaviour
	{
		public int    order;
		public bool   isUI = true;

		void Start() 
		{
			if (isUI)
			{
				Canvas canvas = this.GetComponent<Canvas>();

				if( canvas == null)
				{
					canvas = this.gameObject.AddComponent<Canvas>();
					this.gameObject.AddComponent<GraphicRaycaster>();
				}

				canvas.overrideSorting = true;
				canvas.sortingOrder    = order;
			}
			else
			{
				Renderer[] renders = this.GetComponentsInChildren<Renderer>(true);

				foreach (Renderer renderer in renders)
				{
					renderer.sortingOrder = order;
				}
			}
		}
	}
}

