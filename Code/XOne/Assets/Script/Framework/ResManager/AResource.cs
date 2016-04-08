using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif 


namespace QtmCat
{
	public static class AResource
	{
		private static Dictionary<string, object> dicPrefab = new Dictionary<string, object>();

		public static T Load<T>(string path) where T : UnityEngine.Object
		{
			object obj;
			if (dicPrefab.TryGetValue(path, out obj))
			{
				return obj as T;
			}

			obj = Resources.Load<T>(path);
			dicPrefab.Add(path, obj);

			return obj as T;
		}


		public static IEnumerator LoadAsync<T> (string path) where T : UnityEngine.Object
		{
			if (dicPrefab.ContainsKey(path))
			{
				yield return null;
			}
			else
			{
				ResourceRequest request = Resources.LoadAsync<T> (path);
				yield return request;

				T t = request.asset as T;
				dicPrefab.Add(path, t as object);

				yield return t;
			}
		}

		public static void LoadAllSpriteFromPrefab(string prefabName)
		{
			string     path   = prefabName;
			GameObject prefab = AResource.Load<GameObject>(path);
			ADebug.Assert(prefab != null, "Can not find Prefab [{0}]", path);

			SpriteRenderer[] renders = prefab.GetComponentsInChildren<SpriteRenderer>(true);
			foreach (SpriteRenderer render in renders)
			{
				dicPrefab.Add(path + render.name, render.sprite);
			}
		}

		public static Sprite LoadSpriteFromPrefab(string prefabName, string spriteName)
		{
			string path = prefabName;
			string key  = path + spriteName;


			object obj;
			if (dicPrefab.TryGetValue(key, out obj))
			{
				return obj as Sprite;
			}


			GameObject prefab = Load<GameObject>(path);


			#if UNITY_EDITOR
			if (prefab == null)
			{
			//				EditorUtility.DisplayDialog("提示", "找皮神prefab不存在啊", "好哒不要找王磊");
			}
			#endif

			ADebug.Assert(prefab != null, "Can not find Prefab [{0}]", path);


			SpriteRenderer[] renders = prefab.GetComponentsInChildren<SpriteRenderer>(true);
			foreach (SpriteRenderer render in renders)
			{
				if (render.name == spriteName)
				{
					dicPrefab.Add(key, render.sprite);
					return render.sprite;
				}
			}


			#if UNITY_EDITOR
			//			EditorUtility.DisplayDialog("提示", "Sprite找不到, 试试使用Tools下面的AtlasMaker", "好哒");
			#endif

			ADebug.Assert(false, "Can not find Sprite with [{0}]", spriteName);

			return null;
		}

		public static IEnumerator LoadSpriteFromPrefabAnsy(string prefabName, string spriteName)
		{
			string path = prefabName;
			string key  = path + spriteName;


			object obj;
			if (dicPrefab.TryGetValue(key, out obj))
			{
				yield return obj as Sprite;
			}

			IEnumerator etor = LoadAsync<GameObject>(path);
			yield return ACoroutineManager.StartCoroutineTask(etor);	


			GameObject prefab = etor.Current as GameObject;
			ADebug.Assert(prefab != null, "Can not find Prefab [{0}]", path);


			SpriteRenderer[] renders = prefab.GetComponentsInChildren<SpriteRenderer>(true);
			foreach (SpriteRenderer render in renders)
			{
				if (render.name == spriteName)
				{
					dicPrefab.Add(key, render.sprite);
					yield return render.sprite;
					yield break;
				}
			}

			ADebug.Assert(false, "Can not find Sprite in {0} with [{1}]", prefabName, spriteName);
		}
	}
}

