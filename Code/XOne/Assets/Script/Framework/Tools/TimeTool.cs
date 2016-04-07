using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace QtmCat
{
	public static class TimeTool
	{
//		public const int kNumSecondsInDay = 0x15180;

		public static DateTime epoch = Convert.ToDateTime("1/1/1970 0:00:00 AM");

		public static bool TryBeforeTimestamp(long timestamp)
		{
			return (CurrentUnix() < timestamp);
		}

		public static object BeforeTimestamp(object[] args)
		{
			return TryBeforeTimestamp(Convert.ToInt64(args[0]));
		}

		public static bool TryBeyondTimestamp(long timestamp)
		{
			return (CurrentUnix() > timestamp);
		}

		public static object BeyondTimestamp(object[] args)
		{
			return TryBeyondTimestamp(Convert.ToInt64(args[0]));
		}

		public static long CurrentUnix()
		{
			TimeSpan span   = DateTime.Now.ToUniversalTime().Subtract(epoch);
			double num      = (((((span.Days * 0x18) + span.Hours) * 60) + span.Minutes) * 60) + span.Seconds;
			return Convert.ToInt64(num);
		}

		public static long CurrentUnixWithFeatureOffset(string offsetName)
		{
			return CurrentUnix();
		}

		public static long DayEndSeconds()
		{
			return (DayStartSeconds() + 0x15180L);
		}

		public static long DayStartSeconds()
		{
			TimeSpan span = (TimeSpan)(DateTime.Now.Date.ToUniversalTime() - epoch);
			return Convert.ToInt64(span.TotalSeconds);
		}

		public static long GetTimestamp(string dateTimeString)
		{
			TimeSpan span   = Convert.ToDateTime(dateTimeString).ToUniversalTime().Subtract(epoch);
			double num      = (((((span.Days * 0x18) + span.Hours) * 60) + span.Minutes) * 60) + span.Seconds;
			return Convert.ToInt64(num);
		}

		public static object GetTimestamp(object[] args)
		{
			return GetTimestamp((string)args[0]);
		}

		public static string MillisecondsAsFormattedString(int timeInMilliseconds)
		{
			int num             = timeInMilliseconds % 0x3e8;
			int timeInSeconds   = timeInMilliseconds / 0x3e8;
			string str          = SecondsAsFormattedString(timeInSeconds);
			if (num > 0)
			{
				str = str + num + "ms";
			}
			return str;
		}

		public static string SecondsAsFormattedString(int timeInSeconds)
		{
			if (timeInSeconds < 0)
			{
				timeInSeconds = 0;
			}

			int hours       = timeInSeconds / (60 * 60);
			int leftSeconds = timeInSeconds % (60 * 60);
			int minutes     = leftSeconds / 60;
			int seconds     = leftSeconds % 60;

			string formatTime = AddZeroPrefix(hours) + ":" + AddZeroPrefix(minutes) + ":" + AddZeroPrefix(seconds);
			return (formatTime);
		}

		public static string AddZeroPrefix(int number)
		{
			if (number < 10)
			{
				return "0" + number;
			}
			else
			{
				return "" + number;
			}
		}

		public static float SecondsUntil(long to)
		{
			TimeSpan span   = DateTime.Now.ToUniversalTime().Subtract(epoch);
			double num      = (((((span.Days * 0x18) + span.Hours) * 60) + span.Minutes) * 60) + span.Seconds;
			return ((to - Convert.ToInt64(num)) - (((float)span.Milliseconds) / 1000f));
		}

		public static float SecondsUntilTomorrow(string featureOffsetName = null)
		{
			DateTime time = UnixTimeStampToDateTime((double)CurrentUnixWithFeatureOffset(featureOffsetName));
			TimeSpan span = (TimeSpan)(time.AddDays(1.0).Date - time);
			return (float)span.TotalSeconds;
		}

		public static float SecondsUntilWithFeatureOffset(string offsetName, long to)
		{
			return SecondsUntil(to);
		}

		public static long TimeZoneOffset()
		{
			TimeSpan utcOffset  = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
			double num          = (((((utcOffset.Days * 0x18) + utcOffset.Hours) * 60) + utcOffset.Minutes) * 60) + utcOffset.Seconds;
			return -Convert.ToInt64(num);
		}

		public static object TimeZoneOffset(object[] args)
		{
			return TimeZoneOffset();
		}

		public static DateTime ToLocalDateTime(long unixTimestamp)
		{
			TimeSpan span = TimeSpan.FromSeconds((double)unixTimestamp);
			return epoch.Add(span).ToLocalTime();
		}

		public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
		{
			DateTime time = new DateTime(0x7b2, 1, 1, 0, 0, 0, 0);
			return time.AddSeconds(unixTimeStamp).ToLocalTime();
		}
	}
}
