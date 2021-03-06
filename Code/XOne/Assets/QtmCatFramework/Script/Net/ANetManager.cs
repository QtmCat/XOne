using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace QtmCatFramework
{
	public static class ANetManager
	{

		private class InnerHttp : MonoBehaviour
		{
			public WWW         www;
			public Action<WWW> OnComplete;

			private IEnumerator Start()
			{
				while (!www.isDone) 
				{
					yield return www;
				}

				if (OnComplete != null)
				{
					OnComplete(www);
				}

				www.Dispose();
				Destroy(this.gameObject);
			}
		}

		/**
		 * args are groups of k-v parameters for Http get  
		 */
		public static void HttpGet(string url, Action<WWW> OnComplete, params object[] args)  
		{
			ADebug.Assert(OnComplete != null);

			string param;  

			if (args.Length > 0)  
			{  
				ADebug.Assert(args.Length % 2 == 0);

				param = "?";  
				for (int i = 0; i < args.Length; i += 2)
				{
					if (i > 0)
					{
						param += "&";
					}

					param += args[i] + "=" + args[i + 1];
				}
			}  
			else  
			{  
				param = "";  
			}  

			string request = url + param;
			ADebug.Log("HttpGet {0}", request);

			WWW www = new WWW(request);  

			InnerHttp innerHttp = new GameObject(www.url).AddComponent<InnerHttp>();
			innerHttp.www        = www;
			innerHttp.OnComplete = OnComplete;
		}  


		public static void HttpPost(string url, Action<WWW> OnComplete, Dictionary<string, object> formData)
		{
			ADebug.Assert(formData != null && formData.Count > 0, "Forgot set form data ?");

			WWWForm form = new WWWForm();

			foreach (KeyValuePair<string, object> kv in formData)
			{
				if (kv.Value is Byte[])
				{
					form.AddBinaryData(kv.Key, kv.Value as Byte[], "application/octet-stream");
				}
				else
				{
					form.AddField(kv.Key, kv.Value.ToString());
				}
			}

			ADebug.Log("HttpPost {0}", url);

			WWW www = new WWW(url, form);

			InnerHttp innerHttp = new GameObject(www.url).AddComponent<InnerHttp>();
			innerHttp.www        = www;
			innerHttp.OnComplete = OnComplete;
		}

//-------------------------------------------------------------------------------------------------
		private static Socket      socket;
		private static int         bufferSize    = 1024;
		private static byte[]      buffer        = new byte[bufferSize];
		private static List<byte>  receiveBuffer = new List<byte>(bufferSize);

		public static event Action OnGeneralMsgSend;

		public static void SocketConnect(string server, int port, Action OnConnected)
		{
			IPAddress  ipAddress  = Dns.GetHostEntry(server).AddressList[0];  
			IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);  

			socket                = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			socket.NoDelay        = true;

			ADebug.Log("[Socket start connecting]");

			socket.BeginConnect
			(
				ipEndPoint, 
				(IAsyncResult ar) => 
				{
					ADebug.Log("[Socket connected]");

					socket.EndSend(ar);
					OnConnected();
					StartReceived();
				}, 
				null // no need
			);
		}


		private static void StartReceived()
		{
			socket.BeginReceive
			(
				buffer,
				0,
				bufferSize,
				SocketFlags.None,
				Received,
				null
			);
		}

		private static void Received(IAsyncResult ar)
		{
			int read = socket.EndReceive(ar);

			if (read > 0)
			{
				byte[] bytes = new byte[read];
				Buffer.BlockCopy(buffer, 0 , bytes, 0, read);
				receiveBuffer.AddRange(bytes);
			}


			while (receiveBuffer.Count > 4) 
			{
				byte[] lenBytes = receiveBuffer.GetRange(0, 4).ToArray();
				int    len      = IPAddress.HostToNetworkOrder(BitConverter.ToInt32(lenBytes, 0));

				// one protocol data received
				if (receiveBuffer.Count - 4 >= len)
				{
					byte[]    dataBytes = receiveBuffer.GetRange(4, len).ToArray();
					NetStream stream    = new NetStream(dataBytes);
					byte      protocol  = stream.ReadByte();   // (1 byte)

					if (protocol == 0) // server redirect
					{
						int    redirPort = stream.ReadInt32();
						string redirHost = stream.ReadString8();

						socket.Disconnect(false);
						socket.Close();
						stream.Close();
						receiveBuffer.RemoveRange(0, len + 4);

						ADebug.Log("[Socket conneting redirection server " + redirHost+":"+redirPort.ToString()+"]");
						SocketConnect
						(
							redirHost, 
							redirPort, 
							() => 
							{
								SocketSend
								(

									(NetStream writer) =>
									{
										writer.WriteInt64(0);
										writer.WriteString16("");
									},

									() =>
									{
										ADebug.Log("[login request complete]");
									},

									0,
									0
								);
							}
						);

						return;
					}
					else 
					{
						// ADebug.Log("msgId:" + msgId);

						// dispatch hanlder
						// receiveStreamHanlders.Enqueue(messageHandlerDic[msgId]);
						// receiveStreamHanlders.Enqueue(stream);

						receiveBuffer.RemoveRange(0, len + 4);
					}
				}
				else 
				{
					// protocol data not complete
					break;
				}
			}

			// continue to receive listen
			StartReceived();
		}


		public static int gameServerInstanceID = 0;

		public static void SocketGameSend(Action<NetStream> OnSetStream, Action OnComplete)
		{
			OnGeneralMsgSend();
			SocketSend(OnSetStream, OnComplete, ANetManager.gameServerInstanceID);
		}

		public static void SocketSend(Action<NetStream> OnSetStream, Action OnComplete, int serverInstanceId = 0, int msgId = 0)
		{
			if (socket == null)
			{
				return;
			}

			ADebug.Assert(OnSetStream != null, "Please set send OnSetStream");
			ADebug.Assert(OnComplete  != null, "Please set send OnComplete");

			NetStream stream = new NetStream();

			OnSetStream(stream);

			stream.Seek(0);
			// real length
			stream.WriteInt32(stream.GetLength() - 4);

			byte[] buffer = stream.GetBuffer();

			socket.BeginSend
			(
				buffer,
				0,
				buffer.Length,
				SocketFlags.None,
				(IAsyncResult ar) =>
				{
					socket.EndSend(ar);
					stream.Close();
					OnComplete();
				},
				null // no need
			);
		}



		public static Dictionary<int, Action<NetStream>> instructionHandlerDic 	= new Dictionary<int, Action<NetStream>>();
		public static Dictionary<int, Action<NetStream>> moduleHandlerDic 		= new Dictionary<int, Action<NetStream>>();

		public static void RegisterInstructionHandler(int instructionId, Action<NetStream> OnStream)
		{
			ADebug.Assert(OnStream != null);
			instructionHandlerDic.Add(instructionId, OnStream);
		}

		public static void RemoveInstructionHandler(int instructionId)
		{
			instructionHandlerDic.Remove(instructionId);
		}

		public static void RegisterModuleHandler(int moduleId, Action<NetStream> OnStream)
		{
			ADebug.Assert(OnStream != null);
			moduleHandlerDic.Add(moduleId, OnStream);
		}

		public static void RemoveModuleHandler(int moduleId)
		{
			moduleHandlerDic.Remove(moduleId);
		}


		private static Queue<object> receiveStreamHanlders = new Queue<object>();
		private class NetReveiveAction : MonoBehaviour
		{
			void Update()
			{
				if (receiveStreamHanlders.Count > 0)
				{
					Action<NetStream> action = (Action<NetStream>) receiveStreamHanlders.Dequeue();
					NetStream         stream = (NetStream)         receiveStreamHanlders.Dequeue();

					action(stream);
					stream.Close();
				}
			}
		}

		public static void Init()
		{
			new GameObject("NetReceiveStream").AddComponent<NetReveiveAction>();
		}

	}

}
