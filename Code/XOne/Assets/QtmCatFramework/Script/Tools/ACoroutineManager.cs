using UnityEngine;using System.Collections;using System.Collections.Generic;using System;namespace QtmCatFramework{	public static class ACoroutineManager 	{		private class  InnerCoroutine : MonoBehaviour {}		private static InnerCoroutine coroutine;		static ACoroutineManager()		{			if (coroutine == null)			{				coroutine = new GameObject("ACoroutineManager").AddComponent<InnerCoroutine>();			}		}		public static Coroutine StartCoroutineTask(IEnumerator routine)		{			return coroutine.StartCoroutine(routine);		}   		private static IEnumerator StartInnerCoroutine(IEnumerator routine, Action<object> callback)		{			yield return StartCoroutineTask(routine);			callback(routine.Current);		}		/**
		 * Will cast routine Current  pass to callback
		 */		public static void StartCoroutineTask(IEnumerator routine, Action<object> callback)		{			StartCoroutineTask(StartInnerCoroutine(routine, callback));		}		public static void StopCoroutineTask(IEnumerator routine)		{			coroutine.StopCoroutine(routine);		}		public static void StopAllCoroutineTask()		{			coroutine.StopAllCoroutines();		}	}}