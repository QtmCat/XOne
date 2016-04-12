using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace QtmCatFramework
{
	public class SpritePacker
	{
		[MenuItem ("Tools/AtlasMaker")]
		private static void MakeAtlas()
		{
			string spriteDir = Application.dataPath + "/Resources/Game/Prefabs/Sprites";		

			if (Directory.Exists(spriteDir))
			{
				Directory.Delete(spriteDir, true);
			}

			Directory.CreateDirectory(spriteDir);

			DirectoryInfo  rootDirInfo = new DirectoryInfo(Application.dataPath + "/Atlas");
			List<FileInfo> list        = new List<FileInfo>(); 

			foreach (DirectoryInfo dirInfo in rootDirInfo.GetDirectories()) 
			{
				foreach (FileInfo pngFile in dirInfo.GetFiles("*.png", SearchOption.AllDirectories)) 
				{
					list.Add(pngFile);
				}

				foreach (FileInfo pngFile in dirInfo.GetFiles("*.jpg", SearchOption.AllDirectories)) 
				{
					list.Add(pngFile);
				}
			}	

			Dictionary<string, GameObject> dirGameObjectDic = new Dictionary<string, GameObject>();

			for (int i = 0; i < list.Count; i++)
			{
				FileInfo   pngFile   = list[i];

				string     allPath   = pngFile.FullName;
				string     assetPath = allPath.Substring(allPath.IndexOf("Assets"));
				Sprite     sprite    = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

				if (sprite == null)
				{
					continue;
				}

				string     dir           = Regex.Match(assetPath, "Atlas\\\\(.+)\\\\").Groups[1].Value;
				GameObject dirGameObject = null;

				if (dirGameObjectDic.ContainsKey(dir))
				{
					dirGameObject = dirGameObjectDic[dir];
				}
				else
				{
					dirGameObject         = new GameObject(dir);
					dirGameObjectDic[dir] = dirGameObject;
				}

				GameObject go         = AUIManager.AddChild(dirGameObject);	
				go.name               = sprite.name;
				SpriteRenderer render = go.AddComponent<SpriteRenderer>();
				render.sprite         = sprite;

				EditorUtility.DisplayProgressBar
				(
					"?? Sprite", 
					string.Format("??? {0} / {1}, {2}", i, list.Count, sprite.name), 
					(i + 1) / (float) list.Count
				);
			}

			Dictionary<string, GameObject>.Enumerator etor = dirGameObjectDic.GetEnumerator();
			int count = 0;
			while (etor.MoveNext())
			{
				GameObject go   = etor.Current.Value;
				string     path = spriteDir + "/" + go.name + ".prefab";

				PrefabUtility.CreatePrefab(path.Substring(path.IndexOf ("Assets")), go);

				EditorUtility.DisplayProgressBar
				(
					"?? Prefab", 
					string.Format("??? {0} / {1}, {2}", count, dirGameObjectDic.Count, go.name), 
					(count++ + 1) / (float) dirGameObjectDic.Count
				);

				GameObject.DestroyImmediate(go);
			}

			EditorUtility.ClearProgressBar();
		}

		[MenuItem ("Tools/SetMyImage")]
		private static void SetMyImage()
		{
			DirectoryInfo  rootDirInfo = new DirectoryInfo(Application.dataPath + "/Resources/Game/Prefabs/");
			List<FileInfo> list        = new List<FileInfo>(); 

			foreach (FileInfo prefabFile in rootDirInfo.GetFiles("*.prefab", SearchOption.AllDirectories)) 
			{
				list.Add(prefabFile);
			}

			foreach (DirectoryInfo dirInfo in rootDirInfo.GetDirectories()) 
			{
				foreach (FileInfo prefabFile in dirInfo.GetFiles("*.prefab", SearchOption.AllDirectories)) 
				{
					list.Add(prefabFile);
				}
			}

			for (int i = 0; i < list.Count; i++)
			{
				FileInfo      prefabFile  = list[i];

				string        allPath     = prefabFile.FullName;
				string        assetPath   = allPath.Substring(allPath.IndexOf("Assets"));
				GameObject    go          = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

				List<MyImage> myImageList = new List<MyImage>();
				MyImage       myImage     = go.GetComponent<MyImage>();
				if (myImage != null)
				{
					myImageList.Add(myImage);
				}

				myImageList.AddRange (go.GetComponentsInChildren<MyImage> (true));

				foreach (MyImage image in myImageList)
				{
					if (image != null && image.sprite != null)
					{
						string path = AssetDatabase.GetAssetPath(image.sprite);
						string dir  = Regex.Match(path, "Atlas/(.+)/").Groups[1].Value;

						image.spritePrefabName = image.sprite.name;
						image.dirPrefabName    = dir;
						image.sprite           = null;

						EditorUtility.DisplayProgressBar
						(
							"Set MyImage", 
							string.Format("?? {0}", image.spritePrefabName), 
							(i + 1) / (float) list.Count
						);

						EditorUtility.SetDirty(image.gameObject);
					}
				}
			}

			EditorUtility.ClearProgressBar();
		}
	}
}