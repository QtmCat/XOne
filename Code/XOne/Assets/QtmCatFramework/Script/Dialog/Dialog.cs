using UnityEngine;
using System.Collections;
using DG.Tweening;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

namespace QtmCatFramework
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
		public int width  = 0;
		public int height = 0;

		[Space(10)]
		public bool isModal = true;
		public bool isTouchFallThrough;
		public bool isCloseOnFocusOutside;

		[Space(10)]
		public bool isUseBlur   = false;

		[Space(10)]
		public bool  isFadeIn   = false;
		public float fadeInTime = 0.5f; 

		[Space(10)]
		public bool  isFadeOut   = false;
		public float fadeOutTime = 0.5f;

		[Space(10)]
		public bool  isParticleBack = false;


		[Space(10)]
		public TransitionStyle openTransition = TransitionStyle.zoom;
		public Ease            openEase       = Ease.OutBack;
		public float           openDuration   = 0.618f;
		public int             openPosOffsetX = 0;
		public int             openPosOffsetY = 0;

		[Space(10)]
		public TransitionStyle closeTransition = TransitionStyle.none;
		public Ease            closeEase       = Ease.OutBack;
		public float           closeDuration   = 0.618f;

		[Space(10)]
		public AnimationClip             openAnimation       = null;
		public AnimationClip             closeAnimation      = null;
		public RuntimeAnimatorController controllerAnimation = null;

		[Space(10)]
		public float  autoCloseTime = 0f;
		public Button closeButton;
		// when opened which Dialog need to hide
		public string hideDialogName;


		[Space(10)]
		public DialogState[] dialogStates;


		[HideInInspector]
		public GameObject container;
		[HideInInspector]
		public GameObject scrim;
		[HideInInspector]
		public object     userData;
		[HideInInspector]
		public string     tabName = "";
		[HideInInspector]
		public GameObject blur;


		[HideInInspector]
		public bool     isOpenning;
		[HideInInspector]
		public bool     isClosing;
		[HideInInspector]
		public RawImage rawImage;


		private Action<Dialog> OnOpened;
		private Action<Dialog> OnClosed;
		private Action<Dialog> OnBeginClose;


		private bool           isJustPlayAnimator = false; 
		private static int     openCount          = 1;


		public Dialog AddOpened(Action<Dialog> OnOpened)
		{
			this.OnOpened += OnOpened;
			return this;
		}


		public Dialog AddClosed(Action<Dialog> OnClosed)
		{
			this.OnClosed += OnClosed;
			return this;
		}

		public Dialog AddBeginClose(Action<Dialog> OnBeginClose)
		{
			this.OnBeginClose += OnBeginClose;
			return this;
		}

		public Dialog FireOpened()
		{
			if (this.isJustPlayAnimator)
			{
				this.isJustPlayAnimator = false;
				return this;
			}

			if (this.OnOpened != null)
			{
				this.OnOpened(this);
			}

			return this;
		}

		public Dialog FireClosed()
		{
			if (this.isJustPlayAnimator)
			{
				this.isJustPlayAnimator = false;
				return this;
			}

			if (this.OnClosed != null)
			{
				this.OnClosed(this);
			}

			return this;
		}

		public Dialog FireBeginClose()
		{
			if (this.isJustPlayAnimator)
			{
				return this;
			}

			if (this.OnBeginClose != null)
			{
				this.OnBeginClose(this);
			}

			return this;
		}


		void Awake()
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

			if (this.isUseBlur)
			{
				AUIManager.instance.uiCamera.GetComponent<BlurEffect>().enabled = true;
			}
		}


		void Start()
		{
			UIDepth uiDepth = null;

			if (this.container != null)
			{
				uiDepth = this.gameObject.GetComponent<UIDepth>();

				if (uiDepth == null)
				{
					uiDepth       = this.container.AddComponent<UIDepth>();
					uiDepth.order = ++openCount * 4;
				}
				else
				{
					this.container.AddComponent<UIDepth>().order = uiDepth.order;
				}

				if (this.isUseBlur)
				{
					this.blur.AddComponent<UIDepth>().order  = uiDepth.order - 3;
				}

				if (this.isModal)
				{
					this.scrim.AddComponent<UIDepth>().order = uiDepth.order - 2;
				}
			}
			else
			{
				ADebug.Assert(false, "Dialog not use OpenDialog to show, what the fuck are you thinking ?!");
			}

			foreach (ParticleSystem ps in this.gameObject.GetComponentsInChildren<ParticleSystem>(true))
			{
				if (this.isParticleBack)
				{
					ps.GetComponent<Renderer>().sortingOrder = uiDepth.order - 1;
				}
				else
				{
					ps.GetComponent<Renderer>().sortingOrder = uiDepth.order;
				}
			}


			switch (this.hideDialogName)
			{
				case "":
					break;

				case "PRE":
				{
					List<Dialog> list       = AUIManager.GetOpenedOrderedDialog();
					Dialog       hideDialog = list[list.Count - 2];

					// open hide dialog when closed
					this.OnBeginClose += (Dialog d) =>
					{
						AUIManager.SetDialogActive(hideDialog, true);
					};

					this.OnOpened += (Dialog d) =>
					{
						// close hide dialog
						AUIManager.SetDialogActive(hideDialog, false);
					};

				} break;

				case "HUD":
				{
					// open hide dialog when closed
					this.OnBeginClose += (Dialog d) =>
					{
						if (this.hideDialogName == "")
						{
							return;
						}

						Dialog dialog             = AUIManager.hudDialog;
						dialog.isJustPlayAnimator = true;
						dialog.GetComponent<Animator>().Play(dialog.openAnimation.name);
					};

					// close hide dialog
					Dialog dd             = AUIManager.hudDialog;
					dd.isJustPlayAnimator = true;
					dd.GetComponent<Animator>().Play(dd.closeAnimation.name);

				} break;

				default:
				{
					// open hide dialog when closed
					this.OnBeginClose += (Dialog d) =>
					{
						AUIManager.SetDialogActive(this.hideDialogName, true);
					};

					this.OnOpened += (Dialog d) =>
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

		public void SetDialogState(string stateName)
		{
			this.tabName = stateName;
			this.ChangeViewState();
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

			if (this.OnClosed != null)
			{
				this.OnClosed(this);
			}

			Destroy(this.container);

			if (
				AUIManager.hudDialog != null   && 
				AUIManager.hudDialog.container.transform.GetSiblingIndex() == AUIManager.instance.uiCamera.transform.childCount - 2
			   )
			{
				openCount = 1;
			}

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

			if (this.OnOpened != null)
			{
				this.OnOpened(this);
			}

			this.isOpenning = false;

			// AUIManager.HUD.container.buttonGroupTransform.SetAsLastSibling();

		}

		public bool isActive
		{
			get
			{
				return this.container.activeSelf;
			}
		}
	}
}