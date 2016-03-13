using UnityEngine;
using System.Collections;
using DG.Tweening;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

namespace QtmCat
{
	/**
	 * 对话框状态对象
	 * */
	[Serializable]
	public class DialogState
	{	
		// 状态名
		public string       name;
		// 这个状态下对话框需要显示的子元素
		public GameObject[] array;
	}


	public class Dialog : MonoBehaviour 
	{
		// 动画类型
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

		// 对话框唯一标示
		public string dialogName;
		public int    width  = 0;
		public int    height = 0;

		[Space(10)]
		// 是否显示遮挡层
		public bool isModal = true;
		// 点击事件是否穿透
		public bool isTouchFallThrough;
		// 点击非窗口是否需要关闭对话框
		public bool isCloseOnFocusOutside;

		[Space(10)]
		// 是否需要模糊背景
		public bool isUseBlur = false;

		[Space(10)]
		// 默认打开动画类型
		public TransitionStyle openTransition = TransitionStyle.zoom;
		// 动画缓动类型
		public Ease            openEase       = Ease.OutBack;
		// 动画时间
		public float           openDuration   = 0.618f;
		// 对话框位置X偏移
		public int             openPosOffsetX = 0;
		// 对话框位置Y偏移
		public int             openPosOffsetY = 0;

		[Space(10)]
		// 关闭动画类型
		public TransitionStyle closeTransition = TransitionStyle.none;
		// 关闭缓动类型
		public Ease            closeEase       = Ease.OutBack;
		// 关闭时间
		public float           closeDuration   = 0.618f;

		[Space(10)]
		// 动画使用animator，需要挂载的打开 animation clip
		public AnimationClip             openAnimation       = null;
		// 动画使用animator，需要挂载的关闭 animation clip
		public AnimationClip             closeAnimation      = null;
		// 动画使用animator，需要挂载的控制器
		public RuntimeAnimatorController controllerAnimation = null;

		[Space(10)]
		// 如果是提示框，自动关闭的时间
		public float  autoCloseTime = 0f;
		// 挂载关闭对话框按钮
		public Button closeButton;
		// 对话框打开后，需要隐藏的对话框名称 dialogName
		public string hideDialogName;


		[Space(10)]
		// 添加对话框不同状态对应的展现
		public DialogState[] dialogStates;

		[HideInInspector]
		// 对话框容器
		public GameObject container;
		[HideInInspector]
		// 遮挡层
		public GameObject scrim;
		[HideInInspector]
		// 自定义绑定数据
		public object     userData;
		[HideInInspector]
		// 对话框某个状态的名字
		public string     tabName = "";


		[HideInInspector]
		// 对话框正在打开
		public bool isOpenning;
		[HideInInspector]
		// 对话框正在关闭
		public bool isClosing;

		[HideInInspector]
		// 模糊背景使用的填充模糊纹理数据对象
		public RawImage  rawImage;

		// 打开动画完成回调
		private Action<Dialog> onOpened;
		// 关闭动画完成回调
		private Action<Dialog> onClosed;
		// 关闭开始回调
		private Action<Dialog> onBeginClose;

		// 只播放打开关闭动画不执行回调
		private bool           isJustPlayAnimator = false; 

		/**
		 * 添加打开回调
		 * */
		public Dialog AddOpened(Action<Dialog> action)
		{
			this.onOpened += action;
			return this;
		}

		/**
		 * 添加关闭回调
		 * */
		public Dialog AddClosed(Action<Dialog> action)
		{
			this.onClosed += action;
			return this;
		}

		/**
		 * 添加开始关闭回调
		 * */
		public Dialog AddBeginClose(Action<Dialog> action)
		{
			this.onBeginClose += action;
			return this;
		}

		/**
		 * 执行打开回调
		 * */
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

		/**
		 *  执行关闭回调
		 * */
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

		/**
		 * 执行关闭开始回调
		 * */
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

		/**
		 * 根据状态名称，隐藏显示配置的GameObject
		 * */
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

		/**
		 * 动画关闭完成回调
		 * */
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

		/**
		 * 动画打开完成回调
		 * */
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

		/**
		 * 当前对话框是否激活
		 * */
		public bool IsActive
		{
			get
			{
				return this.container.activeSelf;
			}
		}
	}

}