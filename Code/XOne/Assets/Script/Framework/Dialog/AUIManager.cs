using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace QtmCat 
{
	public class AUIManager : MonoBehaviour
	{
		/**
		 * 队列对话框，持有对话框的prefab和创建回调函数 
		 **/
		private class QueuedDialog
		{
			public Dialog         prefab;
			public Action<Dialog> onCreated;
		}

		public  static AUIManager instance;

		// 根据对话框DialogName属性，缓存所有打开的对话框
		public  static Dictionary<string, Dialog> openedDialogs  = new Dictionary<string, Dialog>();
		// 持有所有队列提示框
		private static List<QueuedDialog>         queuedTips     = new List<QueuedDialog>();

		public GameObject     uiRoot;
		public Camera         uiCamera;

		// 屏幕像素尺寸宽
		public int            screenWidth;
		// 屏幕像素尺寸搞
		public int            screenHeight;
		// 缓存系统事件对象
		public EventSystem    eventSystem;

		private AUIManager() {}

		void Awake()
		{
			ADebug.Assert(this.uiRoot   != null);
			ADebug.Assert(this.uiCamera != null);
			instance = this;
		}

		/**
		 * 根据在场景的排序，获得所有打开并激活的对话框
		 * */
		public static List<Dialog> GetOpenedActivedDialog()
		{
			List<Dialog> list = new List<Dialog>();

			foreach (KeyValuePair<string, Dialog> kv in openedDialogs)
			{
				if (kv.Value.container.activeSelf) 
				{
					list.Add(kv.Value);
				}
			}

			list.Sort
			(
				(Dialog a, Dialog b) =>
				{
					return a.container.transform.GetSiblingIndex() - b.container.transform.GetSiblingIndex();
				}
			);

			return list;
		}

		/**
		 * 根据在场景的排序，获得所有对话框
		 * */
		public static List<Dialog> GetOpenedOrderedDialog()
		{
			List<Dialog> list = new List<Dialog>();

			foreach (KeyValuePair<string, Dialog> kv in openedDialogs)
			{
				list.Add(kv.Value);
			}

			list.Sort
			(
				(Dialog a, Dialog b) =>
				{
					return a.container.transform.GetSiblingIndex() - b.container.transform.GetSiblingIndex();
				}
			);

			return list;
		}

		/**
		 * 根据对话框的名字，设置对话框的激活状态
		 * */
		public static Dialog SetDialogActive(string name, bool isActive)
		{
			return SetDialogActive(openedDialogs[name], isActive);
		}

		/**
		 * 根据对话框对象，设置激活状态，如果对话框是animator模式会自动执行绑定animator
		 * */
		public static Dialog SetDialogActive(Dialog dialog, bool isActive)
		{
			ADebug.Assert(dialog != null);

			dialog.container.SetActive(isActive);

			bool hasAnimation;

			if (isActive)
			{
				hasAnimation  = dialog.openTransition == Dialog.TransitionStyle.animation;
			}
			else
			{
				hasAnimation  = dialog.closeTransition == Dialog.TransitionStyle.animation;
			}

			if (hasAnimation)
			{
				Animator animator = dialog.GetComponent<Animator>();
				if (animator != null)
				{
					string animationName;

					if (isActive)
					{
						animationName = dialog.openAnimation.name;
					}
					else
					{
						animationName = dialog.closeAnimation.name;
					}

					// we set play animation name 
					// disenabled and update 0 for play first frame will not dispear dialog on beginning
					// and next frame play normal
					animator.Play(animationName);
					animator.enabled = false;
					animator.Update(0f);

					DOTween.Sequence()
						.AppendInterval(0)
						.AppendCallback
						(
							() =>
							{
								animator.enabled = true;
							}
						)
						.Play();
				}
			}

			return dialog;
		}

		/**
		 * 显示提示框，根据提示框prefab
		 * 提示框，将会在属性 autoCloseTime后自动关闭
		 * */
		public static Dialog ShowTip(Dialog prefab)
		{
			ADebug.Assert(prefab != null);
			ADebug.Assert(prefab.autoCloseTime > 0);
			Dialog dialog = SpawnDialog(prefab);

			dialog.AddOpened
			(
				(Dialog d) =>
				{
					DOTween.Sequence()
						.PrependInterval(prefab.autoCloseTime)
						.AppendCallback
						(
							() =>
							{
								CloseDialog(d);
							}
						);
				}
			);

			return dialog;
		}

		/**
		 * 根据提示框路径显示提示框
		 * */
		public static Dialog ShowTip(string prefabFile)
		{
			Dialog prefab = AResource.Load<Dialog>(prefabFile);
			ADebug.Assert(prefab != null);

			return ShowTip(prefab);
		}

		private static bool isQueuedTipShowing = false;

		/**
		 * 显示队列提示框，每次调用提示框会进入队列依次显示
		 * */
		public static void ShowQueuedTip(Dialog prefab, Action<Dialog> onCreated = null) 
		{
			ADebug.Assert(prefab != null);

			if (isQueuedTipShowing) 
			{
				QueuedDialog qd = new QueuedDialog();
				qd.prefab       = prefab;
				qd.onCreated   += onCreated;
				queuedTips.Add(qd);
			} 
			else 
			{
				isQueuedTipShowing = true;

				ShowTip(prefab)
					.AddOpened
					(
						(Dialog d) =>
						{
							if (onCreated != null)
							{
								onCreated(d);
							}
						}
					)

					.AddClosed
					(
						(Dialog d) =>
						{				
							isQueuedTipShowing = false;
							if (queuedTips.Count > 0)
							{
								QueuedDialog qd = queuedTips[0];
								queuedTips.RemoveAt(0);
								ShowQueuedTip(qd.prefab, qd.onCreated);						
							} 
						}
					);
			}
		}

		/**
		 * 根据提示框路径现实提示框
		 * */
		public static void ShowQueuedTip(string prefabFile, Action<Dialog> onCreated = null) 
		{
			Dialog prefab = AResource.Load<Dialog>(prefabFile);
			ADebug.Assert(prefab != null);

			ShowQueuedTip(prefab, onCreated);
		}

		/**
		 * 打开对话框
		 * 
		 * parent    对话框父类
		 * onCreated prefab实例化后调用
		 * */
		public static Dialog OpenDialog(Dialog prefab, GameObject parent = null, Action<Dialog> onCreated = null)
		{
			ADebug.Assert(prefab != null);
			ADebug.Assert(prefab.autoCloseTime == 0);

			if (parent == null) 
			{
				parent = instance.uiCamera.gameObject;
			}

			return SpawnDialog(prefab, parent, onCreated);
		}

		/**
		 * 根据对话框路径打开
		 * */
		public static Dialog OpenDialog(string prefabFile, GameObject parent = null, Action<Dialog> onCreated = null)
		{
			Dialog prefab = AResource.Load<Dialog>(prefabFile);
			ADebug.Assert(prefab != null);

			if (parent == null)
			{
				parent = instance.uiCamera.gameObject;
			}

			return OpenDialog(prefab, parent, onCreated);
		}

		/**
		 * 根据名字关闭对话框
		 * */
		public static void CloseDialog(string name)
		{
			Dialog dialog;
			if (!openedDialogs.TryGetValue(name, out dialog))
			{
				return;
			}

			CloseDialog(dialog);
		}

		/**
		 * 关闭所有对话框，清空提示框队列
		 * */
		public static void CloseAllDialogs()
		{
			Dictionary<string, Dialog>.Enumerator enumerator = openedDialogs.GetEnumerator();
			while (enumerator.MoveNext())
			{
				CloseDialog(enumerator.Current.Value);
			}

			openedDialogs.Clear();
			queuedTips.Clear();
		}

		/**
		 * 根据对话框对象，关闭对话框，并执行关闭动画，移除缓存, 执行回调函数
		 * */
		public static void CloseDialog(Dialog dialog)
		{
			ADebug.Assert(dialog != null);

			if (!openedDialogs.ContainsKey(dialog.dialogName))
			{
				return;
			}

			dialog.isClosing = true;
			dialog.OnBeginClose();

			Vector2 fullScreenDimensions = instance.GetComponent<RectTransform>().sizeDelta;

			switch (dialog.closeTransition) 
			{
				case Dialog.TransitionStyle.animation:
					{ 
						Animator animator = dialog.GetComponent<Animator>();
						animator.enabled  = true;
						animator.Play(dialog.closeAnimation.name);				
					}   break;

				case Dialog.TransitionStyle.none:
					{
						DOTween.Sequence()
							.Append(dialog.transform.DOScale(1.0f, 0.1f))
							.AppendCallback
							(
								() =>
								{
									dialog.OnCloseComplete();
								}
							)
							.Play();
					}	break;

				case Dialog.TransitionStyle.zoom:
					{
						dialog.gameObject.transform.DOScale(Vector3.zero, dialog.closeDuration)
							.SetEase(dialog.closeEase)
							.OnComplete
							(
								() => 
								{
									dialog.OnCloseComplete();
								}
							);

					} break;

				case Dialog.TransitionStyle.slide_left:
					{
						dialog.gameObject.transform.DOLocalMoveX(-fullScreenDimensions.x - dialog.width / 2, dialog.closeDuration)
							.SetEase(dialog.closeEase)
							.OnComplete
							(
								() =>
								{
									dialog.OnCloseComplete();
								}
							);										
					} break;


				case Dialog.TransitionStyle.slide_right:
					{
						dialog.gameObject.transform.DOLocalMoveX(fullScreenDimensions.x + dialog.width / 2, dialog.closeDuration)
							.SetEase(dialog.closeEase)
							.OnComplete
							(
								() =>
								{
									dialog.OnCloseComplete();
								}
							);										
					} break;


				case Dialog.TransitionStyle.slide_top:
					{
						dialog.gameObject.transform.DOLocalMoveY(fullScreenDimensions.y + dialog.height / 2, dialog.closeDuration)
							.SetEase(dialog.closeEase)
							.OnComplete
							(
								() =>
								{
									dialog.OnCloseComplete();
								}
							);										
					} break;

				case Dialog.TransitionStyle.slide_bottom:
					{
						dialog.gameObject.transform.DOLocalMoveY(-fullScreenDimensions.y - dialog.height / 2, dialog.closeDuration)
							.SetEase(dialog.closeEase)
							.OnComplete
							(
								() =>
								{
									dialog.OnCloseComplete();
								}
							);										
					} break;
			}

			if (dialog.scrim)
			{				
				Image image = dialog.scrim.GetComponent<Image>();
				DOTween.ToAlpha
				(
					()  => image.color,
					(c) => image.color = c,
					0f, 
					0.618f
				);
			}

			openedDialogs.Remove(dialog.dialogName);
		}

		/**
		 * 判断对话框是否已经打开状态
		 * */
		public static bool isDialogOpen(string dialogName)
		{
			return openedDialogs.ContainsKey(dialogName);
		}

		/**
		 * 根据名字，直接销毁对话框，不调用回调函数
		 * */
		public static void DestroyDialog(string name, bool immediate)
		{
			Dialog dialog;
			if (!openedDialogs.TryGetValue(name, out dialog))
			{
				return;
			}

			DestroyDialog(dialog, immediate);
		}

		/**
		 * 根据对象，直接销毁对话框
		 * immediate 是否立即销毁对象内存
		 * */
		public static void DestroyDialog(Dialog dialog, bool immediate = false)
		{
			ADebug.Assert(dialog != null);

			if (!openedDialogs.ContainsKey(dialog.dialogName))
			{
				return;
			}

			openedDialogs.Remove(dialog.dialogName);

			if (immediate)
			{
				DestroyImmediate(dialog.container);
			}
			else
			{
				Destroy(dialog.container);
			}

			// UnityEngine.Resources.UnloadUnusedAssets();
		}

		/**
		 * 内部函数，生成对话框
		 * */
		private static Dialog SpawnDialog(Dialog prefab, GameObject parent = null, Action<Dialog> onCreated = null) 
		{
			ADebug.Assert(prefab != null);

			Dialog d;
			if (openedDialogs.TryGetValue(prefab.dialogName, out d))
			{
				// 对话框已经被打开
				
				if (openedDialogs.Count == 1)
				{
					ADebug.Log("Dialog [{0}] has already opened, what the fuck have you done !!!", prefab.dialogName);
				}
				else
				{
					// 如果有多个打开对话框，则关闭此对话框之前的，并激活此对话框 

					List<Dialog> list = GetOpenedOrderedDialog();

					for (int i = list.Count - 1; i >= 0; i--)
					{
						if (list[i].dialogName == prefab.dialogName)
						{
							list[i].gameObject.SetActive(true);
							return list[i];
						}

						CloseDialog(list[i].dialogName);
					}
				}

				return d;
			}

			GameObject container;
			// 生成对话框容器
			if (parent == null)
			{
				container = AddChild(instance.uiCamera.gameObject);
			}
			else 
			{
				container = AddChild(parent);
			}

			// 计算屏幕大小
			Vector2 fullScreenDimensions   = instance.GetComponent<RectTransform>().sizeDelta;

			container.transform.localScale = new Vector3(fullScreenDimensions.x / AUIManager.instance.screenWidth, fullScreenDimensions.y / AUIManager.instance.screenHeight, 1.0f);

			// 创建屏蔽层
			GameObject scrim                  = AddChild(container); 
			scrim.transform.localPosition     = new Vector3(0, 0, 10f);

			GameObject go                     = AddChild(container, prefab.gameObject);
			container.name                    = go.name + "(DialogContainer)"; 
			container.transform.localPosition = new Vector3(0, 0,  openedDialogs.Count * -100);
			scrim.name                        = "(DialogScrim)";

			Dialog dialog                     = go.GetComponent<Dialog>();
			dialog.container                  = container;
			dialog.scrim                      = scrim;

			dialog.isOpenning = true;

			if (onCreated != null)
			{
				// 回调创建函数
				onCreated(dialog);
			}

			// 如果需要模糊对话框背景
			if (dialog.isUseBlur)
			{
				GameObject blur             = AddChild(container);
				blur.name                   = "(blur)"; 
				blur.transform.SetAsFirstSibling();

				dialog.rawImage             = blur.AddComponent<RawImage>();
				dialog.rawImage.color       = new Color(dialog.rawImage.color.r, dialog.rawImage.color.g, dialog.rawImage.color.b, 0f);
				dialog.rawImage.GetComponent<RectTransform>().sizeDelta = fullScreenDimensions;

				BlurEffect.rawImage         = dialog.rawImage;

				DOTween.ToAlpha
				(
					()  => dialog.rawImage.color,
					(c) => dialog.rawImage.color = c,
					1f, 
					1.0f
				);
			}

			// 创建遮挡层
			Image image;
			if (prefab.isModal)
			{
				image       = AddImage(scrim, AResource.Load<Sprite>("Scrim"));
				image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
				DOTween.ToAlpha
				(
					()  => image.color,
					(c) => image.color = c,
					1f, 
					0.45f
				);
			} 
			else 
			{
				image = AddImage(scrim, null);
				image.color = new Color(0, 0, 0, 0);
			}

			// 设置遮挡层大小
			image.GetComponent<RectTransform>().sizeDelta = fullScreenDimensions;

			// 是否需要点击窗口外部关闭对话框
			if (dialog.isCloseOnFocusOutside)
			{
				scrim.AddComponent<Button>().onClick.AddListener
				(
					() => 
					{
						CloseDialog(dialog);
					}
				);
			}

			// 事件是否能穿透对话框
			if (dialog.isTouchFallThrough)
			{
				CanvasGroup group    = scrim.AddComponent<CanvasGroup>();
				group.interactable   = false;
				group.blocksRaycasts = false;
			}

			// 匹配执行打开动画
			switch (dialog.openTransition) 
			{
				case Dialog.TransitionStyle.animation:
					{
						dialog.transform.localPosition = new Vector3
							(
								prefab.openPosOffsetX + dialog.transform.localPosition.x, 
								prefab.openPosOffsetY + dialog.transform.localPosition.y, 
								dialog.transform.localPosition.z
							);

						AnimationEvent aEvent1 = new AnimationEvent();
						aEvent1.time = dialog.openAnimation.length;
						aEvent1.functionName = "OnOpenComplete";
						dialog.openAnimation.events = null;	
						dialog.openAnimation.AddEvent(aEvent1);


						if (dialog.closeTransition == Dialog.TransitionStyle.animation)
						{
							AnimationEvent aEvent2 = new AnimationEvent();
							aEvent2.time = dialog.closeAnimation.length;
							aEvent2.functionName = "OnCloseComplete";
							dialog.closeAnimation.events = null;
							dialog.closeAnimation.AddEvent(aEvent2);
						}

						Animator animator = dialog.gameObject.GetComponent<Animator>();
						if (animator == null)
						{
							animator = dialog.gameObject.AddComponent<Animator>();
							animator.runtimeAnimatorController = dialog.controllerAnimation;
						}
						// we set play animation name 
						// disenabled and update 0 for play first frame will not dispear dialog on beginning
						// and next frame play normal
						animator.Play(dialog.openAnimation.name);
						animator.enabled = false;
						animator.Update(0f);

						DOTween.Sequence()
							.AppendInterval(0)
							.AppendCallback
							(
								() =>
								{
									animator.enabled = true;
								}
							)
							.Play();
					} break;

				case Dialog.TransitionStyle.none:
					{ 

						Vector3 pos                = go.transform.localPosition;
						go.transform.localPosition = new Vector3(pos.x + prefab.openPosOffsetX, pos.y + prefab.openPosOffsetY, pos.z);

						DOTween.Sequence()
							.AppendInterval(0.1f)
							.AppendCallback
							(
								() => 
								{
									dialog.OnOpenComplete();
								}
							)
							.Play();

					} break;

				case Dialog.TransitionStyle.zoom:
					{
						go.transform.localScale    = Vector3.zero;
						Vector3 pos                = go.transform.localPosition;
						go.transform.localPosition = new Vector3(prefab.openPosOffsetX + pos.x, prefab.openPosOffsetY + pos.y, 0f);
						go.transform.DOScale(Vector3.one, prefab.openDuration)
							.SetEase(prefab.openEase)
							.OnComplete
							(
								() => 
								{
									dialog.OnOpenComplete();
								}
							);
					} break;

				case Dialog.TransitionStyle.slide_left:
					{
						Vector3 pos                = go.transform.localPosition;
						go.transform.localPosition = new Vector3(-fullScreenDimensions.x - dialog.width / 2, pos.y + prefab.openPosOffsetY, pos.z);
						go.transform.DOLocalMoveX(pos.x + prefab.openPosOffsetX, prefab.openDuration)
							.SetEase(prefab.openEase)
							.OnComplete
							(
								() =>
								{
									dialog.OnOpenComplete();
								}
							);										
					} break;


				case Dialog.TransitionStyle.slide_right:
					{
						Vector3 pos                = go.transform.localPosition;
						go.transform.localPosition = new Vector3(fullScreenDimensions.x + dialog.width / 2, pos.y + prefab.openPosOffsetY, pos.z);
						go.transform.DOLocalMoveX(pos.x + prefab.openPosOffsetX, prefab.openDuration)
							.SetEase(prefab.openEase)
							.OnComplete
							(
								() =>
								{
									dialog.OnOpenComplete();
								}
							);										
					} break;


				case Dialog.TransitionStyle.slide_top:
					{
						Vector3 pos                = go.transform.localPosition;
						go.transform.localPosition = new Vector3(pos.x + prefab.openPosOffsetX, fullScreenDimensions.y + dialog.height / 2, pos.z);
						go.transform.DOLocalMoveY(pos.y + prefab.openPosOffsetY, prefab.openDuration)
							.SetEase(prefab.openEase)
							.OnComplete
							(
								() =>
								{
									dialog.OnOpenComplete();
								}
							);										
					} break;

				case Dialog.TransitionStyle.slide_bottom:
					{
						Vector3 pos                = go.transform.localPosition;
						go.transform.localPosition = new Vector3(pos.x + prefab.openPosOffsetX, -fullScreenDimensions.y - dialog.height / 2, pos.z);
						go.transform.DOLocalMoveY(pos.y + prefab.openPosOffsetY, prefab.openDuration)
							.SetEase(prefab.openEase)
							.OnComplete
							(
								() =>
								{
									dialog.OnOpenComplete();
								}
							);										
					} break;
			}

			// 添加到打开缓存
			openedDialogs.Add(prefab.dialogName, dialog);

			return dialog;
		}

		/**
		 * 添加MyImage对象到go对象
		 * */
		public static MyImage AddImage(GameObject go, Sprite sprite)
		{
			MyImage image   = go.AddComponent<MyImage>();
			image.sprite    = sprite;
			//		    image.material  = AResource.Load<Material>(AFrameworkConst.SPRITE_DEFAULT_MAT);
			//            image.SetNativeSize();

			return image;
		}

		/**
		 * 实例化prefab到UIRoot上
		 * */
		public static GameObject InstantiatePrefabToUIRoot(string prefabFile)
		{
			ADebug.Assert(prefabFile != null);
			GameObject prefab = AResource.Load<GameObject>(prefabFile);
			ADebug.Assert(prefab != null);

			return AddChild(instance.uiRoot, prefab);
		}

		/**
		 * 实例化prefab到UICamera上
		 * */
		public static GameObject InstantiatePrefabToUICamera(string prefabFile)
		{
			ADebug.Assert(prefabFile != null);
			GameObject prefab = AResource.Load<GameObject>(prefabFile);
			ADebug.Assert(prefab != null);

			return AddChild(instance.uiCamera.gameObject, prefab);
		}

		/**
		 * 实例化prefab并指定父类
		 * */
		public static GameObject InstantiatePrefab(GameObject parent, string prefabFile)
		{
			ADebug.Assert(prefabFile != null);
			ADebug.Assert(parent     != null);
			GameObject prefab = AResource.Load<GameObject>(prefabFile);

			//			if (prefab == null) 	
			//			{
			//				prefab = AResource.Load<GameObject>(AGameConst.BUILDING_PREFAB_PATH + "FoodFact");
			//				ADebug.LogError("InstantiatePrefab not found prefab = {0}", prefabFile);
			//			}

			ADebug.Assert(prefab != null, "InstantiatePrefab not found prefab = {0}", prefabFile);

			return AddChild(parent, prefab);
		}

		/**
		 * 利用协程异步实例化prefab，在实例化完成时候回调callback
		 * */
		public static void InstantiatePrefabAsyn(GameObject parent, string prefabFile, Action<GameObject> callback)
		{
			ADebug.Assert(prefabFile != null);
			ADebug.Assert(parent     != null);
			ADebug.Assert(callback   != null);

			ACoroutineManager.StartCoroutineTask
			(
				AResource.LoadAsync<GameObject>(prefabFile), 
				(object obj) => 
				{		
					ADebug.Assert(obj != null, "InstantiatePrefabAsyn not found prefabFile = {0}", prefabFile);
					callback(AddChild(parent, obj as GameObject));
				}
			);
		}

		/**
		 * 利用协程异步实例化prefab到UICamera上，完成回调callback
		 * */
		public static void InstantiatePrefabToUICameraAsyn(string prefabFile, Action<GameObject> callback)
		{
			ADebug.Assert(prefabFile != null);
			ADebug.Assert(callback   != null);

			InstantiatePrefabAsyn(instance.uiCamera.gameObject, prefabFile, callback);
		}

		/**
		 * 利用协程异步实例化prefab到UIRoot上，完成回调callback
		 * */
		public static void InstantiatePrefabToUIRootAsyn(string prefabFile, Action<GameObject> callback)
		{
			ADebug.Assert(prefabFile != null);
			ADebug.Assert(callback   != null);

			InstantiatePrefabAsyn(instance.uiRoot.gameObject, prefabFile, callback);
		}

		/**
		 * 指定父类添加一个生成的GameObject
		 * */
		public static GameObject AddChild(GameObject parent)
		{
			ADebug.Assert(parent != null);

			GameObject go           = new GameObject();
			Transform  transform    = go.transform;
			transform.SetParent(parent.transform);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale    = Vector3.one;
			go.layer                = parent.layer;

			return go;
		}

		/**
		 * 指定父类实例化一个prefab
		 * */
		public static GameObject AddChild(GameObject parent, GameObject prefab)
		{
			GameObject go = UnityEngine.Object.Instantiate(prefab) as GameObject;
			ADebug.Assert(go != null && parent != null);

			Transform transform     = go.transform;
			transform.SetParent(parent.transform);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale    = Vector3.one;
			go.layer                = parent.layer;

			return go;
		}

		/**
		 * GameObject添加Collider
		 * */
		public static T AddCollider<T>(GameObject go, bool isDynamic) where T : Collider
		{
			T local = go.AddComponent<T>();
			if (isDynamic && (go.GetComponent<Rigidbody>() == null))
			{
				Rigidbody rigidbody   = go.AddComponent<Rigidbody>();
				rigidbody.isKinematic = true;
				rigidbody.useGravity  = false;
			}

			return local;
		}
	}
}