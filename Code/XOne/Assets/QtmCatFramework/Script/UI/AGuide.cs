using System;
using UnityEngine.UI;
using DG;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;


namespace QtmCatFramework
{
	public class Guide
	{ 
		public string       id; 
		public bool         guideTrigger; 
		public List<string> moduleID; 
		public string       description; 
		public string       spawnsFrom; 
		public string       skipCondition; 
	} 

	public static class AGuide
	{
		private static List<Guide> guideList     = new List<Guide>();
		private static Guide       activeGuide   = null;

		public static void Init()
		{

		}

		public static void Dialogue(string prefabName, string name, string content, float delayTime)
		{
			Dialog dialog = AUIManager.OpenDialog
				(
					prefabName, null, 
					(Dialog d) => 
					{
						if (delayTime > 0)
						{
							d.isCloseOnFocusOutside = false;
							d.AddOpened
							(
								(Dialog dd) =>
								{
									DOTween.Sequence()
										.AppendInterval(delayTime)
										.AppendCallback
										(
											() =>
											{
												if (dd != null)
												{
													AUIManager.CloseDialog(dd);
												}
											}
										).Play();
								}
							);
						}
						else 
						{
							d.isCloseOnFocusOutside = true;
						}
					}                 
				);


			dialog.AddClosed
			(
				(Dialog d) =>
				{
					ActiveGuideComplete();
				}
			);
		}

		public static void StartCheckGuideState()
		{
			ACoroutineManager.StartCoroutineTask(CheckGuideStateRoutine());
		}

		private static void ActiveGuideComplete()
		{
			if (GuideMask.instance == null)
			{
				AUIManager.instance.eventSystem.gameObject.SetActive(false);
			}

			DOTween.Sequence()
				.AppendInterval(0.5f)
				.AppendCallback
				(
					() =>
					{
						AUIManager.instance.eventSystem.gameObject.SetActive(true);
						activeGuide = null;
					}
				)
				.Play();
		}



		private static IEnumerator CheckGuideStateRoutine()
		{
			while (true)
			{
				while (activeGuide != null)
				{
					if (activeGuide.skipCondition != "")
					{
						ActiveGuideComplete();
					}

					yield return null;
				}

				for (int i = 0; i < guideList.Count; i++)
				{
					Guide guide = guideList[i];

					if (guide.guideTrigger)
					{
						activeGuide = guide;
						guideList.Remove(activeGuide);
						break;
					}

					yield return null;
				}

				yield return null;
			}
		}


		public static void LockScreenForDelay(float duration, float opactiy = 0.0f)
		{
			GuideMask mask = GuideMask.SetRectCenter
				(
					new Vector2(0.0f, 0.0f),
					new Vector2(0.0f, 0.0f),
					() =>
					{

					},
					false
				);

			DOTween.Sequence()
				.AppendInterval(duration)
				.AppendCallback
				(
					() =>
					{
						if (mask != null)
						{
							GameObject.DestroyImmediate(mask.gameObject);
						}
					}
				)
				.Play();
		}



		public static GameObject SetArrow(GameObject destination, int offset, int arrowDirection, string arrowPrefab = "arrow")
		{
			GameObject prefab                            = AResource.Load<GameObject>(arrowPrefab) as GameObject;
			GameObject arrow                             = GameObject.Instantiate<GameObject>(prefab) as GameObject;
			CanvasGroup canvasGroup                      = arrow.AddComponent<CanvasGroup>();
			canvasGroup.blocksRaycasts                   = false;
			canvasGroup.interactable                     = false;

			arrow.transform.SetParent(destination.transform);

//			switch (arrowDirection)
//			{
//				case AGameConst.ArrowDirection.Up:
//					{
//						arrow.transform.localEulerAngles = new Vector3(0, 0, 0);
//						arrow.transform.localPosition    = new Vector3(0, offset, 0);
//					} break;
//				case AGameConst.ArrowDirection.Right:
//					{
//						arrow.transform.localEulerAngles = new Vector3(0, 0, 270);
//						arrow.transform.localPosition    = new Vector3(offset, 0, 0);
//					} break;
//				case AGameConst.ArrowDirection.Down:
//					{
//						arrow.transform.localEulerAngles = new Vector3(0, 0, 180);
//						arrow.transform.localPosition    = new Vector3(0, -offset, 0);
//					} break;
//				case AGameConst.ArrowDirection.Left:
//					{
//						arrow.transform.localEulerAngles = new Vector3(0, 0, 90);
//						arrow.transform.localPosition    = new Vector3(-offset, 0, 0);
//					} break;
//			}

			return arrow;
		}
	}
}