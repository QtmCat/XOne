using System;
using UnityEngine;

namespace QtmCatFramework
{
	public class EventIgnore : MonoBehaviour, ICanvasRaycastFilter
	{
		public Action DoAction;

		public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
		{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			if (Input.GetMouseButtonUp(0))
			{
				DoAction();
			}

#else
			if (Input.touchCount == 1)
			{
				Touch touch = Input.GetTouch(0);

				if (touch.phase == TouchPhase.Ended)
				{
					DoAction();		
				}
			}
#endif
			return false;
		}
	}
}



