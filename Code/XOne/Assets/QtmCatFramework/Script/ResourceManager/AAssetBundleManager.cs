using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace QtmCatFramework
{
	public static class AAssetBundleManager
	{
		// A dictionary to hold the AssetBundle references
		private static Dictionary<string, AssetBundle> dictAssetBundle = new Dictionary<string, AssetBundle>();

		public static string url     = "";
		public static int    version = 1;

		// Get an AssetBundle
		public static AssetBundle GetAssetBundle(string assetBundleName) 
		{
			AssetBundle ab;
			if (dictAssetBundle.TryGetValue(assetBundleName, out ab))
			{
				return ab;
			}

			return null;
		}

		// Unload an AssetBundle
		public static void Unload(string assetBundleName, bool isAllObjects)
		{
			AssetBundle ab;
			if (dictAssetBundle.TryGetValue(assetBundleName, out ab))
			{
				ab.Unload(isAllObjects);
				dictAssetBundle.Remove(assetBundleName);
			}
			else 
			{
				ADebug.LogError("Unload AssetBundle failed, not found {0}", assetBundleName);
			}
		}


		// Download an AssetBundle
		public static IEnumerator downloadAssetBundle(string assetBundleName, Action<WWW, string> OnProgress)
		{
			string path = url + assetBundleName + version.ToString();

			using(WWW www = WWW.LoadFromCacheOrDownload(path, version))
			{
				OnProgress(www, assetBundleName);

				yield return www;

				if (www.error != null)
				{
					ADebug.LogError ("WWW download AssetBundle [{0}] fialed : {1}", assetBundleName, www.error);
				}
				else
				{
					AssetBundle ab;
					if (dictAssetBundle.TryGetValue(assetBundleName, out ab))
					{
						ab.Unload(true);
						dictAssetBundle.Remove(assetBundleName);
					}

					dictAssetBundle.Add(assetBundleName, www.assetBundle);

					www.Dispose();
				}
			}
		}


		public static T LoadAsset<T>(string assetBundleName, string assetName) where T : UnityEngine.Object
		{
			AssetBundle ab;
			if (dictAssetBundle.TryGetValue(assetBundleName, out ab))
			{
				return ab.LoadAsset<T>(assetName);
			}

			return null;
		}


		public static T LoadAsset<T>(string assetName) where T : UnityEngine.Object
		{
			Dictionary<string, AssetBundle>.Enumerator etor = dictAssetBundle.GetEnumerator();

			while (etor.MoveNext())
			{
				AssetBundle ab = etor.Current.Value;
				if (ab.Contains(assetName))
				{
					return ab.LoadAsset<T>(assetName);
				}
			}

			return null;
		}


		public static AssetBundleRequest LoadAssetAsync<T>(string assetBundleName, string assetName) where T : UnityEngine.Object
		{
			AssetBundle ab;
			if (dictAssetBundle.TryGetValue(assetBundleName, out ab))
			{
				return ab.LoadAssetAsync<T>(assetName);
			}

			return null;
		}


		public static AssetBundleRequest LoadAssetAsync<T>(string assetName) where T : UnityEngine.Object
		{
			Dictionary<string, AssetBundle>.Enumerator etor = dictAssetBundle.GetEnumerator();

			while (etor.MoveNext())
			{
				AssetBundle ab = etor.Current.Value;
				if (ab.Contains(assetName))
				{
					return ab.LoadAssetAsync<T>(assetName);
				}
			}

			return null;
		}
	}
}



