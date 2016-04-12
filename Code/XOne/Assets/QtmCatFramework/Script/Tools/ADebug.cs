using System;

namespace QtmCatFramework
{
	public static class ADebug
	{
		public static void Log(string format, params object[] args)
		{
			#if UNITY_EDITOR
			UnityEngine.Debug.Log(string.Format(format, args));
			#endif
		}

		public static void LogError(string format, params object[] args)
		{
			#if UNITY_EDITOR
			UnityEngine.Debug.LogError(string.Format(format, args));
			#endif
		}

		public static void Assert(bool condition, string failedReason = "", params object[] args) 
		{
			#if UNITY_EDITOR
			UnityEngine.Debug.Assert(condition);
			if (!condition)
			{
				throw new Exception("Assert failed, " + string.Format(failedReason, args));
			}
			#endif
		}

	}
}