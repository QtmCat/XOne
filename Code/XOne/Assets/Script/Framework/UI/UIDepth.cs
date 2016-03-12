using System;
using UnityEngine;
using UnityEngine.UI;

namespace QtmCat
{
	public class UIDepth : MonoBehaviour
	{
		public int order;

		void Start() 
		{
			Canvas canvas = this.GetComponent<Canvas>();

			if( canvas == null){
				canvas = this.gameObject.AddComponent<Canvas>();
				this.gameObject.AddComponent<GraphicRaycaster>();
			}

			canvas.overrideSorting = true;
			canvas.sortingOrder    = order;
		}
	}
}

