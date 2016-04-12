using System;
using UnityEngine;

namespace QtmCatFramework
{
	public class EventIgnore : MonoBehaviour, ICanvasRaycastFilter
	{
		public Action action;

		public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
		{
			#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			if (Input.GetMouseButtonDown(0))
			{
				action();
			}

			#else
			if (Input.touchCount == 1)
			{
			Touch touch = Input.GetTouch(0);

			if (touch.phase == TouchPhase.Began)
			{
			action();		
			}
			}
			#endif
			return false;
		}
	}
}

