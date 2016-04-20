using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace QtmCatFramework
{
	public class AUIManager : MonoBehaviour
	{
		private class QueuedDialog
		{
			public Dialog         prefab;
			public Action<Dialog> onCreated;
		}

		public  static AUIManager Instance;
		public  static Dialog     HUD;

		public  static Dictionary<string, Dialog> OpenedDialogs = new Dictionary<string, Dialog>();
		private static List<QueuedDialog>         QueuedTips    = new List<QueuedDialog>();

		public GameObject     uiRoot;
		public Camera         uiCamera;
		public int            screenWidth;
		public int            screenHeight;
		public EventSystem    eventSystem;

		private AUIManager() { }

		void Awake()
		{
			ADebug.Assert(this.uiRoot   != null);
			ADebug.Assert(this.uiCamera != null);
			Instance = this;
		}

		public static List<Dialog> GetOpenedActivedDialog()
		{
			List<Dialog> list = new List<Dialog>();

			foreach (KeyValuePair<string, Dialog> kv in OpenedDialogs)
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


		public static List<Dialog> GetOpenedOrderedDialog()
		{
			List<Dialog> list = new List<Dialog>();

			foreach (KeyValuePair<string, Dialog> kv in OpenedDialogs)
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

		public static Dialog SetDialogActive(string name, bool isActive)
		{
			return SetDialogActive(OpenedDialogs[name], isActive);
		}

		public static Dialog SetDialogActive(Dialog dialog, bool isActive)
		{
			ADebug.Assert(dialog != null);

			dialog.container.SetActive(isActive);

			bool hasAnimation;

			if (isActive)
			{
				hasAnimation = dialog.openTransition  == Dialog.TransitionStyle.animation;
			}
			else
			{
				hasAnimation = dialog.closeTransition == Dialog.TransitionStyle.animation;
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

					// we set play animation playerName 
					// disenabled and update 0 for play first frame will not dispear dialog on beginning
					// and next frame play normalSprite
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

		public static Dialog ShowTip(string prefabFile)
		{
			Dialog prefab = AResource.Load<Dialog>(prefabFile);
			ADebug.Assert(prefab != null);

			return ShowTip(prefab);
		}

		private static bool isQueuedTipShowing = false;

		public static void ShowQueuedTip(Dialog prefab, Action<Dialog> onCreated = null)
		{
			ADebug.Assert(prefab != null);

			if (isQueuedTipShowing)
			{
				QueuedDialog qd = new QueuedDialog();
				qd.prefab = prefab;
				qd.onCreated += onCreated;
				QueuedTips.Add(qd);
			}
			else
			{
				isQueuedTipShowing = true;

				ShowTip(prefab).AddOpened
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

										if (QueuedTips.Count > 0)
										{
											QueuedDialog qd = QueuedTips[0];
											QueuedTips.RemoveAt(0);
											ShowQueuedTip(qd.prefab, qd.onCreated);
										}
									}
								);
			}
		}

		public static void ShowQueuedTip(string prefabFile, Action<Dialog> onCreated = null)
		{
			Dialog prefab = AResource.Load<Dialog>(prefabFile);
			ADebug.Assert(prefab != null);

			ShowQueuedTip(prefab, onCreated);
		}

		public static Dialog OpenDialog(Dialog prefab, GameObject parent = null, Action<Dialog> onCreated = null)
		{
			ADebug.Assert(prefab != null);
			ADebug.Assert(prefab.autoCloseTime == 0);

			if (parent == null)
			{
				parent = Instance.uiCamera.gameObject;
			}

			return SpawnDialog(prefab, parent, onCreated);
		}

		public static Dialog OpenDialog(string prefabFile, GameObject parent = null, Action<Dialog> onCreated = null)
		{
			Dialog prefab = AResource.Load<Dialog>(prefabFile);
			ADebug.Assert(prefab != null);

			if (parent == null)
			{
				parent = Instance.uiCamera.gameObject;
			}

			return OpenDialog(prefab, parent, onCreated);
		}

		public static void CloseDialog(string name)
		{
			Dialog dialog;

			if (OpenedDialogs.TryGetValue(name, out dialog))
			{
				CloseDialog(dialog);
			}
		}

		public static void CloseAllDialogs(params string[] dialogNames)
		{
			Dictionary<string, Dialog>.Enumerator enumerator = OpenedDialogs.GetEnumerator();
			while (enumerator.MoveNext())
			{
				foreach (string name in dialogNames)
				{
					if (name == enumerator.Current.Value.dialogName)
					{
						goto Label_next;
					}
				}

				CloseDialog(enumerator.Current.Value);

				Label_next:;
			}

			OpenedDialogs.Clear();
			QueuedTips.Clear();
		}

		public static void CloseDialog(Dialog dialog)
		{
			ADebug.Assert(dialog != null);

			if (!OpenedDialogs.ContainsKey(dialog.dialogName))
			{
				return;
			}

			dialog.isClosing = true;
			dialog.OnBeginClose();

			Vector2 fullScreenDimensions = Instance.GetComponent<RectTransform>().sizeDelta;

			switch (dialog.closeTransition)
			{
				case Dialog.TransitionStyle.animation:
				{
					Animator animator = dialog.GetComponent<Animator>();
					animator.enabled  = true;
					animator.Play(dialog.closeAnimation.name);
				} break;

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
				} break;

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

				image.DOFade
				(
					0f,
					0.618f
				).SetEase(Ease.OutSine);
			}

			if (dialog.isFadeOut)
			{
				foreach (Image image in dialog.GetComponentsInChildren<Image>())
				{
					image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);

					image.DOFade
					(
						0f,
						dialog.fadeOutTime
					).SetEase(Ease.OutSine);

				}

				foreach (Text text in dialog.GetComponentsInChildren<Text>())
				{
					text.color = new Color(text.color.r, text.color.g, text.color.b, 1f);

					text.DOFade
					(
						0f,
						dialog.fadeOutTime
					).SetEase(Ease.OutSine);
				}
			}

			OpenedDialogs.Remove(dialog.dialogName);
		}

		public static bool isDialogOpen(string dialogName)
		{
			return OpenedDialogs.ContainsKey(dialogName);
		}


		public static void DestroyDialog(string name, bool immediate)
		{
			Dialog dialog;
			if (OpenedDialogs.TryGetValue(name, out dialog))
			{
				DestroyDialog(dialog, immediate);
			}
		}

		public static void DestroyDialog(Dialog dialog, bool immediate = false)
		{
			ADebug.Assert(dialog != null);

			if (OpenedDialogs.ContainsKey(dialog.dialogName))
			{
				OpenedDialogs.Remove(dialog.dialogName);

				if (immediate)
				{
					DestroyImmediate(dialog.container);
				}
				else
				{
					Destroy(dialog.container);
				}

				UnityEngine.Resources.UnloadUnusedAssets();
			}
		}

		private static Dialog SpawnDialog(Dialog prefab, GameObject parent = null, Action<Dialog> onCreated = null)
		{
			ADebug.Assert(prefab != null);

			Dialog d;
			if (OpenedDialogs.TryGetValue(prefab.dialogName, out d))
			{

				if (OpenedDialogs.Count == 1)
				{
					ADebug.Log("Dialog [{0}] has already opened, what the fuck have you done !!!", prefab.dialogName);
				}
				else
				{
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
			if (parent == null)
			{
				container = AddChild(Instance.uiCamera.gameObject);
			}
			else
			{
				container = AddChild(parent);
			}


			Vector2 fullScreenDimensions   = Instance.GetComponent<RectTransform>().sizeDelta;
			container.transform.localScale = new Vector3(fullScreenDimensions.x / AUIManager.Instance.screenWidth, fullScreenDimensions.y / AUIManager.Instance.screenHeight, 1.0f);


			GameObject scrim = AddChild(container);
			scrim.transform.localPosition = new Vector3(0, 0, 10f);
			scrim.name = "(DialogScrim)";



			GameObject go     = AddChild(container, prefab.gameObject);
			container.name    = go.name + "(DialogContainer)";
			RectTransform crt = container.AddComponent<RectTransform>();
			crt.localPosition = new Vector3(0, 0, OpenedDialogs.Count * -100);
			crt.sizeDelta     = Vector2.zero;



			Dialog dialog     = go.GetComponent<Dialog>();
			dialog.container  = container;
			dialog.scrim      = scrim;
			dialog.isOpenning = true;

			if (onCreated != null)
			{
				onCreated(dialog);
			}

			if (dialog.isUseBlur)
			{
				GameObject blur                                         = AddChild(container);
				blur.name                                               = "(blur)";
				blur.transform.SetAsFirstSibling();

				dialog.rawImage                                         = blur.AddComponent<RawImage>();
				dialog.rawImage.color                                   = new Color(dialog.rawImage.color.r, dialog.rawImage.color.g, dialog.rawImage.color.b, 0f);
				dialog.rawImage.GetComponent<RectTransform>().sizeDelta = new Vector2(AUIManager.Instance.screenWidth, AUIManager.Instance.screenHeight);

				BlurEffect.rawImage                                     = dialog.rawImage;

				dialog.rawImage.DOFade
				(
					1f,
					1f
				).SetEase(Ease.OutSine);

				dialog.blur = blur;
			}



			Image image;
			if (prefab.isModal)
			{
				image       = AddImage(scrim, AResource.Load<Sprite>("Sprite/Scrim"));
				image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);

				image.DOFade
				(
					1f,
					0.618f
				).SetEase(Ease.OutSine);
			}
			else
			{
				image       = AddImage(scrim, null);
				image.color = new Color(0, 0, 0, 0);
			}

			image.GetComponent<RectTransform>().sizeDelta = new Vector2(AUIManager.Instance.screenWidth, AUIManager.Instance.screenHeight);

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


			if (dialog.isTouchFallThrough)
			{
				CanvasGroup group    = scrim.AddComponent<CanvasGroup>();
				group.interactable   = false;
				group.blocksRaycasts = false;
			}


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

					AnimationEvent aEvent1      = new AnimationEvent();
					aEvent1.time                = dialog.openAnimation.length;
					aEvent1.functionName        = "OnOpenComplete";
					dialog.openAnimation.events = null;
					dialog.openAnimation.AddEvent(aEvent1);


					if (dialog.closeTransition == Dialog.TransitionStyle.animation)
					{
						AnimationEvent aEvent2       = new AnimationEvent();
						aEvent2.time                 = dialog.closeAnimation.length;
						aEvent2.functionName         = "OnCloseComplete";
						dialog.closeAnimation.events = null;
						dialog.closeAnimation.AddEvent(aEvent2);
					}

					Animator animator = dialog.gameObject.GetComponent<Animator>();
					if (animator == null)
					{
						animator = dialog.gameObject.AddComponent<Animator>();
						animator.runtimeAnimatorController = dialog.controllerAnimation;
					}

					// we set play animation playerName 
					// disenabled and update 0 for play first frame will not dispear dialog on beginning
					// and next frame play normalSprite
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

					Vector3 pos = go.transform.localPosition;
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

			if (dialog.isFadeIn)
			{
				foreach (Image image1 in dialog.GetComponentsInChildren<Image>())
				{
					image1.color = new Color(image1.color.r, image1.color.g, image1.color.b, 0f);

					image1.DOFade
					(
						1f,
						dialog.fadeInTime
					).SetEase(Ease.OutSine);
				}


				foreach (Text text in dialog.GetComponentsInChildren<Text>())
				{
					text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);

					text.DOFade
					(
						1f,
						dialog.fadeInTime
					).SetEase(Ease.OutSine);
				}
			}

			OpenedDialogs.Add(prefab.dialogName, dialog);

			return dialog;
		}

		public static MyImage AddImage(GameObject go, Sprite sprite)
		{
			MyImage image = go.AddComponent<MyImage>();
			image.sprite  = sprite;

			return image;
		}

		public static GameObject InstantiatePrefabToUIRoot(string prefabFile)
		{
			ADebug.Assert(prefabFile != null);
			GameObject prefab = AResource.Load<GameObject>(prefabFile);
			ADebug.Assert(prefab != null);

			return AddChild(Instance.uiRoot, prefab);
		}

		public static GameObject InstantiatePrefabToUICamera(string prefabFile)
		{
			ADebug.Assert(prefabFile != null);
			GameObject prefab = AResource.Load<GameObject>(prefabFile);
			ADebug.Assert(prefab != null);

			return AddChild(Instance.uiCamera.gameObject, prefab);
		}

		public static GameObject InstantiatePrefab(GameObject parent, string prefabFile)
		{
			ADebug.Assert(prefabFile != null);
			ADebug.Assert(parent != null);
			GameObject prefab = AResource.Load<GameObject>(prefabFile);

			ADebug.Assert(prefab != null, "InstantiatePrefab not found prefab = {0}", prefabFile);

			return AddChild(parent, prefab);
		}

		public static void InstantiatePrefabAsyn(GameObject parent, string prefabFile, Action<GameObject> callback)
		{
			ADebug.Assert(prefabFile != null);
			ADebug.Assert(parent != null);
			ADebug.Assert(callback != null);

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

		public static void InstantiatePrefabToUICameraAsyn(string prefabFile, Action<GameObject> callback)
		{
			ADebug.Assert(prefabFile != null);
			ADebug.Assert(callback != null);

			InstantiatePrefabAsyn(Instance.uiCamera.gameObject, prefabFile, callback);
		}

		public static void InstantiatePrefabToUIRootAsyn(string prefabFile, Action<GameObject> callback)
		{
			ADebug.Assert(prefabFile != null);
			ADebug.Assert(callback != null);

			InstantiatePrefabAsyn(Instance.uiRoot.gameObject, prefabFile, callback);
		}

		public static GameObject AddChild(GameObject parent)
		{
			ADebug.Assert(parent != null);

			GameObject go           = new GameObject();
			Transform transform     = go.transform;
			transform.SetParent(parent.transform);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale    = Vector3.one;
			go.layer                = parent.layer;

			return go;
		}

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

		public static T AddCollider<T>(GameObject go, bool isDynamic) where T : Collider
		{
			T local = go.AddComponent<T>();
			if (isDynamic && (go.GetComponent<Rigidbody>() == null))
			{
				Rigidbody rigidbody = go.AddComponent<Rigidbody>();
				rigidbody.isKinematic = true;
				rigidbody.useGravity = false;
			}

			return local;
		}

		public static void FlyText(Vector3 position, string str)
		{
			GameObject go    = AUIManager.AddChild(AUIManager.Instance.uiCamera.gameObject);
			go.name          = "FlyText";
			Text       text  = go.AddComponent<Text>();

			text.text        = str;
			text.color       = Color.green;
			text.fontSize    = 60;
			text.fontStyle   = FontStyle.Bold;
			text.font        = AResource.Load<Font>("Font/SimHei");
			text.alignment   = TextAnchor.UpperCenter;

			text.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 100);

			DOTween.Sequence()
				   .Append
				   (
				   		go.transform.DOBlendableMoveBy(new Vector3(0, 300, 0), 4f).SetEase(Ease.OutSine)
				   )
				   .Insert
				   (
				   		0.0f,
				   		text.DOFade(0, 4f).SetEase(Ease.OutSine)
				   )
				   .AppendCallback
				   (
				   		() => 
				   		{
				   			DestroyImmediate(go);
				   		}
				   );
		}
	}
}