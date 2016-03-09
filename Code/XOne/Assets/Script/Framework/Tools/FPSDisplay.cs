using UnityEngine;
using System.Collections;

namespace QtmCat
{
	public class FPSDisplay : MonoBehaviour
	{

		float deltaTime = 0.0f;

		void Update()
		{
			deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
		}

		void OnGUI()
		{
			int w = Screen.width, h = Screen.height;

			GUIStyle style = new GUIStyle();

			Rect   rect            = new Rect(0, 0, w, h);
			style.alignment        = TextAnchor.MiddleLeft;
			style.fontSize         = h * 3 / 100;
			style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
			float  msec            = deltaTime * 1000.0f;
			float  fps             = 1.0f / deltaTime;
			//string text            = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);

			GUI.Label(rect, string.Format("({0:0.} fps)", fps), style);
		}
	}
}
