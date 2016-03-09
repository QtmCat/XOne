using UnityEngine;
using System.Collections;
using DG.Tweening;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

namespace QtmCat
{

	[Serializable]
	public class DialogState
	{
		public string name;
		public GameObject[] array;
	}


	public class Dialog : MonoBehaviour 
	{

		public enum TransitionStyle
		{
			zoom,
			slide_left,
			slide_right,
			slide_top,
			slide_bottom,
			animation,
			none,
		}

		public string dialogName;
		public int width = 0;
		public int height = 0;

		[Space(10)]
		public bool isModal = true;
		public bool isTouchFallThrough;
		public bool isCloseOnFocusOutside;

		[Space(10)]
		public bool isUseBlur = false;

		[Space(10)]
		public TransitionStyle openTransition = TransitionStyle.zoom;
		public Ease openEase = Ease.OutBack;
		public float openDuration = 0.618f;
		public int openPosOffsetX = 0;
		public int openPosOffsetY = 0;

		[Space(10)]
		public TransitionStyle closeTransition = TransitionStyle.none;
		public Ease closeEase = Ease.OutBack;
		public float closeDuration = 0.618f;

		[Space(10)]
		public AnimationClip openAnimation = null;
		public AnimationClip closeAnimation = null;
		public RuntimeAnimatorController controllerAnimation = null;

		[Space(10)]
		public float  autoCloseTime = 0f;
		public Button closeButton;
		// when opened which Dialog need to hide
		public string   hideDialogName;


		[Space(10)]
		public DialogState[] dialogStates;


		[HideInInspector]
		public GameObject container;
		[HideInInspector]
		public GameObject scrim;
		[HideInInspector]
		public object userData;
		[HideInInspector]
		public string tabName = "";


		[HideInInspector]
		public bool isOpenning;
		[HideInInspector]
		public bool isClosing;

		[HideInInspector]
		public RawImage  rawImage;


		private Action<Dialog> onOpened;
		private Action<Dialog> onClosed;
		private Action<Dialog> onBeginClose;


		private bool           isJustPlayAnimator = false; 


		public Dialog AddOpened(Action<Dialog> action)
		{
			this.onOpened += action;
			return this;
		}


		public Dialog AddClosed(Action<Dialog> action)
		{
			this.onClosed += action;
			return this;
		}

		public Dialog AddBeginClose(Action<Dialog> action)
		{
			this.onBeginClose += action;
			return this;
		}


		public Dialog OnOpened()
		{
			if (this.isJustPlayAnimator)
			{
				this.isJustPlayAnimator = false;
				return this;
			}

			if (this.onOpened != null)
			{
				this.onOpened(this);
			}

			return this;
		}

		public Dialog OnClosed()
		{
			if (this.isJustPlayAnimator)
			{
				this.isJustPlayAnimator = false;
				return this;
			}

			if (this.onClosed != null)
			{
				this.onClosed(this);
			}

			return this;
		}

		public Dialog OnBeginClose()
		{
			if (this.isJustPlayAnimator)
			{
				return this;
			}

			if (this.onBeginClose != null)
			{
				this.onBeginClose(this);
			}

			return this;
		}


		void Awake()
		{
			if (this.isUseBlur)
			{
				AUIManager.instance.uiCamera.GetComponent<BlurEffect>().enabled = true;
			}
		}


		void Start()
		{
			if (this.closeButton != null) 
			{
				this.closeButton.onClick.AddListener
				(
					() =>
					{
						AUIManager.CloseDialog(this);
					}
				);
			}

			switch (this.hideDialogName)
			{
			case "":
				break;

			case "HUD":
				{
					// open hide dialog when closed
					this.onBeginClose += (Dialog d) =>
					{
						if (this.hideDialogName == "")
						{
							return;
						}
					};

					// close hide dialog

				} break;

			default:
				{
					// open hide dialog when closed
					this.onBeginClose += (Dialog d) =>
					{
						AUIManager.SetDialogActive(this.hideDialogName, true);
					};

					this.onOpened += (Dialog d) =>
					{
						// close hide dialog
						AUIManager.SetDialogActive(this.hideDialogName, false);
					};
				}
				break;
			}

			ChangeViewState();
		}


		public void ChangeViewState()
		{
			DialogState temp = new DialogState();
			if (tabName != "default")
			{
				foreach (DialogState ds in dialogStates) 
				{
					if (ds.name == tabName) 
					{
						temp = ds;
					}
					else
					{
						foreach(GameObject go in ds.array)
						{
							go.SetActive(false);
						}
					}
				}
				if (temp.array != null)
				{
					foreach (GameObject go in temp.array)
					{
						go.SetActive(true);
					}
				}
			}
		}


		public void OnCloseComplete()
		{
			if (this.isJustPlayAnimator)
			{
				return;
			}

			if (this.isUseBlur)
			{
				RenderTexture.ReleaseTemporary((RenderTexture) this.rawImage.texture);
			}

			this.isClosing = false;

			if (this.onClosed != null)
			{
				this.onClosed(this);
			}

			Destroy(this.container);

			// UnityEngine.Resources.UnloadUnusedAssets();
		}

		public void OnOpenComplete()
		{
			if (this.openTransition == Dialog.TransitionStyle.animation)
			{
				Animator animator = this.GetComponent<Animator>();

				if (animator.enabled && this.closeTransition != Dialog.TransitionStyle.animation)
				{
					// close animation will cause tween not work so we need false
					animator.enabled = false;
				}

				animator.StopPlayback();
			}

			if (this.onOpened != null)
			{
				this.onOpened(this);
			}

			this.isOpenning = false;

			//			AUIManager.HUD.container.transform.SetAsLastSibling();

		}

		public bool IsActive
		{
			get
			{
				return this.container.activeSelf;
			}
		}
	}
}