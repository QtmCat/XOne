using System;
using System.Collections.Generic;
using System.Text;

namespace QtmCatFramework 
{
	public class SimpleMyJsonBinary
	{
		public static void WriteStrDict(System.IO.Stream stream, IList<string> dict)
		{
			//System.IO.MemoryStream ms = new System.IO.MemoryStream();
			WriteUIntSingle(stream, dict.Count);
			foreach (var d in dict)
			{
				var strdata = System.Text.Encoding.UTF8.GetBytes(d);
				WriteUIntSingle(stream, strdata.Length);
				stream.Write(strdata, 0, strdata.Length);
			}
			//return ms.ToArray();
		}

		public static List<string> ReadStrDict(System.IO.Stream stream)
		{
			List<string> list = new List<string>();
			int c = ReadIntSingle(stream);
			for (int i = 0; i < c; i++)
			{
				int slen = ReadIntSingle(stream);
				byte[] buf = new byte[slen];
				stream.Read(buf, 0, slen);
				string str = System.Text.Encoding.UTF8.GetString(buf, 0, buf.Length);
				list.Add(str);
			}
			return list;
		}

		/// <summary>
		/// ?json??????
		/// </summary>
		/// <param name="stream">????</param>
		/// <param name="node">json</param>
		/// <param name="pubdict">???????(??)????????????????????????????????</param>
		/// <param name="riseDictByKey">???key??????????false?</param>
		/// <param name="riseDictByString">???String???????????false?</param>
		public static void Write(System.IO.Stream stream, SimpleMyJson.IJsonNode node, IList<string> pubdict = null, bool riseDictByKey = false, bool riseDictByString = false)
		{
			List<string> localdict = new List<string>();
			System.IO.MemoryStream ms = new System.IO.MemoryStream();
			PackJson(ms, node, pubdict, localdict, riseDictByKey, riseDictByString);
			byte[] data = ms.ToArray();
			ms.Close();
			WriteStrDict(stream, localdict);
			stream.Write(data, 0, data.Length);
		}

		/// <summary>
		/// ???????Json
		/// </summary>
		/// <param name="stream">????</param>
		/// <param name="pubdict">???????(??),?????????????????????</param>
		/// <returns></returns>
		public static SimpleMyJson.IJsonNode Read(System.IO.Stream stream, IList<string> pubdict = null)
		{
			var list = ReadStrDict(stream);
			return UnPackJson(stream, pubdict, list);
		}

		static int GetFreeKey(IList<string> dict)
		{
			return dict.Count;
		}

		static int GetKey(IList<string> dict, string value)
		{
			if (dict == null) return -1;
			for (int i = 0; i < dict.Count; i++)
			{
				if (dict[i] == value)
					return i;
			}
			return -1;

		}
		static byte MakeStringTag(bool inDict, bool isPubDict, int keylength)
		{
			byte tag = 128 | 64;//stringtag
			if (inDict)
				tag |= 32;
			if (isPubDict)
				tag |= 16;

			tag |= (byte)(keylength);

			return tag;
		}

		static byte MakeNumberTag(bool isFloat, bool isBool, bool isNull, bool isNeg, int datalength)
		{
			byte tag = 128 | 0;//numbertag
			if (isFloat)
				tag |= 32;
			if (isBool)
				tag |= 16;
			if (isNull)
				tag |= 8;
			if (isNeg)
				tag |= 4;
			if (isFloat)
				tag |= (byte)(4 - 1);
			else if (!isBool && !isNull)
				tag |= (byte)(datalength - 1);
			return tag;
		}

		static byte MakeArrayTag(int arraycount, int bytelen)
		{
			byte tag = 0 | 0;//arraytag
			if (arraycount < 32)
			{
				tag |= 32;
				tag |= (byte)arraycount;
			}
			else
			{
				tag |= (byte)(bytelen - 1);
			}
			return tag;
		}

		static byte MakeObjectTag(int arraycount, int bytelen)
		{
			byte tag = 0 | 64;//objecttag
			if (arraycount < 32)
			{
				tag |= 32;
				tag |= (byte)arraycount;
			}
			else
			{
				tag |= (byte)(bytelen - 1);
			}
			return tag;
		}

		static void WriteStringDataDirect(System.IO.Stream stream, string str)
		{
			byte[] sdata = System.Text.Encoding.UTF8.GetBytes(str);
			byte tag = MakeStringTag(false, false, sdata.Length);
			stream.WriteByte(tag);
			stream.Write(sdata, 0, sdata.Length);
		}

		static void WriteStringDataDict(System.IO.Stream stream, bool isPubDict, int pid)
		{
			int bytelen = 1;
			int c = pid;
			while (c >= 0x100)
			{
				c /= 0x100;
				bytelen++;
			}
			byte tag = MakeStringTag(true, isPubDict, bytelen);
			stream.WriteByte(tag);
			byte[] buf = new byte[8];
			buf = BitConverter.GetBytes(pid);
			stream.Write(buf, 0, bytelen);
		}

		static string ReadString(System.IO.Stream stream, byte tagfirst, IList<string> pubdict, IList<string> localdict)
		{
			bool inDict = (tagfirst & 32) > 0;
			bool isPubDict = (tagfirst & 16) > 0;
			int keylength = tagfirst % 16;
			if (inDict)
			{
				byte[] buf = new byte[4];
				for (int i = 0; i < 4; i++)
				{
					buf[i] = 0;
				}
				stream.Read(buf, 0, keylength);
				int id = BitConverter.ToInt32(buf, 0);
				if (isPubDict)
				{
					return pubdict[id];
				}
				else
				{
					return localdict[id];
				}
			}
			else
			{
				byte[] buf = new byte[keylength];
				stream.Read(buf, 0, keylength);
				return System.Text.Encoding.UTF8.GetString(buf, 0, buf.Length);
			}

		}

		static void WriteFloatData(System.IO.Stream stream, float number)
		{
			stream.WriteByte(MakeNumberTag(true, false, false, false, 4));
			byte[] buf = BitConverter.GetBytes(number);
			stream.Write(buf, 0, 4);
		}

		static void WriteIntData(System.IO.Stream stream, int number)
		{
			int bytelen = 1;
			int sc = number;
			if (number < 0)
				sc *= -1;
			int c = sc;
			while (c >= 0x100)
			{
				c /= 0x100;
				bytelen++;
			}
			stream.WriteByte(MakeNumberTag(false, false, false, (number < 0), bytelen));
			byte[] buf = BitConverter.GetBytes(sc);
			stream.Write(buf, 0, bytelen);
		}


		static void WriteUIntSingle(System.IO.Stream stream, int number)
		{
			int bytelen = 1;
			int c = number;
			while (c >= 0x100)
			{
				c /= 0x100;
				bytelen++;
			}
			if (number < 128)
			{
				stream.WriteByte((byte)number);
			}
			else if (number < 31 * 256)
			{
				int high = number / 256;
				int low = number % 256;
				stream.WriteByte((byte)(128 | (byte)high));
				stream.WriteByte((byte)low);
			}
			else if (number < 15 * 256 * 256)
			{
				int high = number / 256 / 256;
				int midle = (number / 256) % 256;
				int low = (number % 256);

				stream.WriteByte((byte)(128 | 64 | (byte)high));
				stream.WriteByte((byte)midle);
				stream.WriteByte((byte)low);

			}
		}

		static int ReadIntSingle(System.IO.Stream stream)
		{
			byte t = (byte)stream.ReadByte();
			if ((t & 128) > 0)
			{
				if ((t & 64) > 0)
				{
					if ((t & 32) == 0)
					{
						byte h = (byte)(t % 32);
						byte m = (byte)stream.ReadByte();
						byte l = (byte)stream.ReadByte();
						return h * 256 * 256 + m * 256 + l;
					}
					else
					{
						throw new NotSupportedException();
					}
				}
				else
				{
					byte low = (byte)stream.ReadByte();
					return t % 64 * 256 + low;
				}
			}
			else
			{
				return t;
			}
		}

		static void WriteArrayCountHead(System.IO.Stream stream, int arraycount)
		{
			int bytelen = 1;
			int c = arraycount;
			while (c >= 0x100)
			{
				c /= 0x100;
				bytelen++;
			}
			stream.WriteByte(MakeArrayTag(arraycount, bytelen));
			if (arraycount >= 32)
			{
				byte[] buf = BitConverter.GetBytes(arraycount);
				stream.Write(buf, 0, bytelen);
			}
		}

		static int ReadCountHead(System.IO.Stream stream, byte tagfirst)
		{
			bool b32 = (tagfirst & 32) > 0;
			if (!b32)
			{
				int blen = tagfirst % 32 + 1;
				byte[] buf = new byte[8];
				for (int i = 0; i < 8; i++)
				{
					buf[i] = 0;
				}
				stream.Read(buf, 0, blen);
				return BitConverter.ToInt32(buf, 0);
			}
			else
			{
				return tagfirst % 32;
			}
		}

		static void WriteObjectCountHead(System.IO.Stream stream, int arraycount)
		{
			int bytelen = 1;
			int c = arraycount;
			while (c >= 0x100)
			{
				c /= 0x100;
				bytelen++;
			}
			stream.WriteByte(MakeObjectTag(arraycount, bytelen));
			if (arraycount >= 32)
			{
				byte[] buf = BitConverter.GetBytes(arraycount);
				stream.Write(buf, 0, bytelen);
			}
		}

		static void PackJsonString(System.IO.Stream stream, string str, IList<string> pubdict, IList<string> localdict, bool riseDictByString)
		{
			if (str.Length < 2)
			{//????
				WriteStringDataDirect(stream, str);
			}
			else
			{
				int pid = GetKey(pubdict, str);
				if (pid >= 0)//????
				{
					WriteStringDataDict(stream, true, pid);
				}
				else //????
				{
					if (localdict.Contains(str) == false)
					{
						localdict.Add(str);
					}
					pid = GetKey(localdict, str);
					WriteStringDataDict(stream, false, pid);
				}
			}
		}

		static void PackJsonNumber(System.IO.Stream stream, SimpleMyJson.JsonNodeValueNumber number)
		{
			if (number.isNull)
			{
				stream.WriteByte(MakeNumberTag(false, false, true, false, 0));
			}
			else if (number.isBool)
			{
				stream.WriteByte(MakeNumberTag(false, true, number.AsBool(), false, 0));
			}
			else
			{
				string numstr = number.ToString();
				if (numstr.Contains(".") || numstr.Contains("e") || numstr.Contains("E"))
				{
					WriteFloatData(stream, (float)number.AsDouble());
				}
				else
				{
					WriteIntData(stream, number.AsInt());
				}
			}
		}

		static void PackJsonArray(System.IO.Stream stream, SimpleMyJson.JsonNodeArray array, IList<string> pubdict, IList<string> localdict, bool riseDictByKey, bool riseDictByString)
		{
			WriteArrayCountHead(stream, array.Count);
			for (int i = 0; i < array.Count; i++)
			{
				PackJson(stream, array[i], pubdict, localdict, riseDictByKey, riseDictByString);
			}
		}

		static void PackJsonObject(System.IO.Stream stream, SimpleMyJson.JsonNodeObject _object, IList<string> pubdict, IList<string> localdict, bool riseDictByKey, bool riseDictByString)
		{
			WriteObjectCountHead(stream, _object.Count);
			foreach (string key in _object.Keys)
			{
				if (key.Length < 2)
				{
					WriteStringDataDirect(stream, key);
				}
				else
				{
					int pid = GetKey(pubdict, key);
					if (pid >= 0)//????
					{
						WriteStringDataDict(stream, true, pid);
					}
					else //????
					{
						if (riseDictByKey)
						{
							pid = GetFreeKey(pubdict);
							pubdict.Add(key);
							WriteStringDataDict(stream, true, pid);
						}
						else
						{
							if (localdict.Contains(key) == false)
							{
								localdict.Add(key);
							}
							pid = GetKey(localdict, key);
							WriteStringDataDict(stream, false, pid);
						}
					}

				}
			}
			foreach (var item in _object)
			{
				PackJson(stream, item.Value, pubdict, localdict, riseDictByKey, riseDictByString);
			}
		}

		static void PackJson(System.IO.Stream stream, SimpleMyJson.IJsonNode node, IList<string> pubdict, IList<string> localdict, bool riseDictByKey, bool riseDictByString)
		{
			if (node is SimpleMyJson.JsonNodeValueString)
			{
				string v = node.AsString();
				if (riseDictByString && v != null && v.Length > 1 && pubdict.Contains(v) == false)
				{
					pubdict.Add(v);
				}
				PackJsonString(stream, v, pubdict, localdict, riseDictByString);
			}
			else if (node is SimpleMyJson.JsonNodeValueNumber)
			{
				PackJsonNumber(stream, node as SimpleMyJson.JsonNodeValueNumber);
			}
			else if (node is SimpleMyJson.JsonNodeArray)
			{
				PackJsonArray(stream, node as SimpleMyJson.JsonNodeArray, pubdict, localdict, riseDictByKey, riseDictByString);
			}
			else if (node is SimpleMyJson.JsonNodeObject)
			{
				PackJsonObject(stream, node as SimpleMyJson.JsonNodeObject, pubdict, localdict, riseDictByKey, riseDictByString);
			}
		}

		static SimpleMyJson.IJsonNode UnPackJsonNumber(System.IO.Stream stream, byte tagfirst)
		{
			SimpleMyJson.JsonNodeValueNumber number = new SimpleMyJson.JsonNodeValueNumber();
			bool isFloat = (tagfirst & 32) > 0;
			bool isBool = (tagfirst & 16) > 0;
			bool isNull = (tagfirst & 8) > 0;
			bool isNeg = (tagfirst & 4) > 0;
			int blen = tagfirst % 4 + 1;
			byte[] buf = new byte[4];
			for (int i = 0; i < 4; i++)
			{
				buf[i] = 0;
			}
			if (isBool)
			{
				number.SetBool(isNull);
			}
			else if (isNull)
			{
				number.SetNull();
			}
			else if (isFloat)
			{
				stream.Read(buf, 0, blen);
				number.value = BitConverter.ToSingle(buf, 0);
			}
			else
			{
				stream.Read(buf, 0, blen);
				int v = BitConverter.ToInt32(buf, 0);
				number.value = isNeg ? -v : v;
			}

			return number;
		}

		static SimpleMyJson.IJsonNode UnPackJsonString(System.IO.Stream stream, byte tagfirst, IList<string> pubdict, IList<string> localdict)
		{
			SimpleMyJson.JsonNodeValueString str = new SimpleMyJson.JsonNodeValueString();
			str.value = ReadString(stream, tagfirst, pubdict, localdict);
			return str;
		}

		static SimpleMyJson.IJsonNode UnPackJsonArray(System.IO.Stream stream, byte tagfirst, IList<string> pubdict, IList<string> localdict)
		{
			SimpleMyJson.JsonNodeArray array = new SimpleMyJson.JsonNodeArray();
			int count = ReadCountHead(stream, tagfirst);
			for (int i = 0; i < count; i++)
			{
				array.Add(UnPackJson(stream, pubdict, localdict));
			}
			return array;
		}

		static SimpleMyJson.IJsonNode UnPackJsonObject(System.IO.Stream stream, byte tagfirst, IList<string> pubdict, IList<string> localdict)
		{
			SimpleMyJson.JsonNodeObject _object = new SimpleMyJson.JsonNodeObject();
			int count = ReadCountHead(stream, tagfirst);
			List<string> keys = new List<string>();
			for (int i = 0; i < count; i++)
			{
				byte ft = (byte)stream.ReadByte();
				keys.Add(ReadString(stream, ft, pubdict, localdict));
			}
			for (int i = 0; i < count; i++)
			{
				_object.Add(keys[i], UnPackJson(stream, pubdict, localdict));
			}
			return _object;
		}

		static SimpleMyJson.IJsonNode UnPackJson(System.IO.Stream stream, IList<string> pubdict, IList<string> localdict)
		{
			byte b = (byte)stream.ReadByte();
			bool t1 = (b & 128) > 0;
			bool t2 = (b & 64) > 0;

			if (t1 && !t2)//number
			{
				return UnPackJsonNumber(stream, b);
			}
			else if (t1 && t2)//string
			{
				return UnPackJsonString(stream, b, pubdict, localdict);
			}
			else if (!t1 && !t2)//array
			{
				return UnPackJsonArray(stream, b, pubdict, localdict);
			}
			else//object
			{
				return UnPackJsonObject(stream, b, pubdict, localdict);
			}
		}
	}
}