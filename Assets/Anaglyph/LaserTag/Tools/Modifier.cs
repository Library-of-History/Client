using System;
using System.Collections;
using Anaglyph.Menu;
using Anaglyph.XRTemplate;
using com.meta.xr.depthapi.utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Cysharp.Threading.Tasks;
using UnityEngine.Playables;

namespace Anaglyph.Lasertag
{
	public class Modifier : MonoBehaviour
	{
		[SerializeField] private AudioClip[] tutorialLines;
		
		[SerializeField] private Transform cursor;
		[SerializeField] private LineRenderer lineRenderer;
		[SerializeField] private GameObject bookUI;
		
		private HandedHierarchy hand;
		private GameObject selectedObject;

		private void Awake()
		{
			hand = GetComponentInParent<HandedHierarchy>(true);

			lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.zero });
			lineRenderer.useWorldSpace = false;
		}

		private void Update()
		{
			lineRenderer.enabled = false;
			cursor.gameObject.SetActive(false);

			bool overUI = hand.RayInteractor.IsOverUIGameObject();
			bool overPortal = false;
			
			if (SystemManager.Inst.Portal != null)
			{
				var portalObjects = 
					SystemManager.Inst.Portal.GetComponentsInChildren<XRSimpleInteractable>();

				foreach (var obj in portalObjects)
				{
					overPortal |= hand.RayInteractor.IsHovering(obj);
				}
			}
			
			if(overUI || overPortal)
			{
				return;
			}

			lineRenderer.SetPosition(1, Vector3.forward);
			lineRenderer.enabled = true;

			Ray ray = new(transform.position, transform.forward);
			bool didHit = Physics.Raycast(ray, out RaycastHit hitInfo);
			selectedObject = hitInfo.collider?.gameObject;
			
			if (!didHit || selectedObject == null)
			{
				return;
			}

			cursor.gameObject.SetActive(true);

			cursor.position = hitInfo.point;
			lineRenderer.SetPosition(1, Vector3.forward * hitInfo.distance);
		}

		private void OnRightFire(InputAction.CallbackContext context)
		{
			if (context.performed && context.ReadValueAsButton())
			{
				if (selectedObject != null)
				{
					var book = selectedObject.GetComponentInParent<BookState>();
					var animHandler = book.gameObject.GetComponent<BookInteraction>();

					if (!animHandler.IsAnimating)
					{
						if (!book.IsOpened)
						{
							book.ChangeState(true);
							var director = animHandler.PlayBookOpenTimeLine();

							DisplayUI(director, book, animHandler);
						}
						else
						{
							book.ChangeState(false);
							animHandler.PlayBookCloseTimeLine();
							
							book.UI.SetActive(false);
						}
					}
				}
			}
		}

		private async UniTaskVoid DisplayUI(PlayableDirector director, BookState book, BookInteraction animHandler)
		{
			while (director.state == PlayState.Playing)
			{
				await UniTask.Yield();
			}
			
			if (book.UI == null)
			{
				var ui = Instantiate(bookUI, book.gameObject.transform, false);
				ui.transform.localPosition = new Vector3(0f, 0f, 0.5f);
				ui.transform.localRotation = Quaternion.Euler(-90f, -180f, 0f);
						
				var summary = ui.GetComponentInChildren<ScrollRect>();
				var summaryText = summary.GetComponentInChildren<TextMeshProUGUI>();
				summaryText.text = SystemManager.Inst.SceneDataInst.SummaryMap[book.gameObject.name];

				var record = ui.GetComponentInChildren<LoadingMedia>(true);
				record.SetBookName(book.gameObject.name);
						
				ui.GetComponentInChildren<Button>().onClick.AddListener(delegate
				{
					if (SystemManager.Inst.IsDocentProcessing)
					{
						return;
					}

					SystemManager.Inst.CurrentReadingBook = book.gameObject;
					book.UI.SetActive(false);
					
					animHandler.StartBookInteractionSequence(() =>
					{
						animHandler.IsAnimating = false;
						SystemManager.Inst.SystemUI.GetComponentInChildren<UIControllerPresenter>(true).EnvSwitch();
					
						if (SystemManager.Inst.SystemUI.activeSelf)
						{
							SystemManager.Inst.SystemUI.GetComponent<MenuPositioner>().ToggleVisible();
						}
					
						SystemManager.Inst.CurrentSceneName = book.gameObject.name;
						var asyncOp = SceneManager.LoadSceneAsync(book.gameObject.name, LoadSceneMode.Additive);
						FindEndingCutscene(asyncOp);
					
						var navPagesParent = ui.GetComponentInChildren<NavPagesParent>(true);
						var summaryPage = navPagesParent.GetComponentInChildren<NavPage>(true);
							
						navPagesParent.GoToPage(summaryPage);
						
						SystemManager.Inst.MRScene.SetActive(false);
						PassthroughManager.SetPassthrough(false);
					});
				});
						
				book.SetUI(ui);
			}
			else
			{
				book.UI.SetActive(true);
			}
		}

		private async UniTaskVoid FindEndingCutscene(AsyncOperation asyncOp)
		{
			while (!asyncOp.isDone)
			{
				await UniTask.Yield();
			}
			
			SystemManager.Inst.CurrentEndingCutscene = FindAnyObjectByType<PlayableDirector>();
			SystemManager.Inst.FadeUI.SetCamera();
			SystemManager.Inst.FadeUI.ResetFadeEffect(DocentProcess);
		}
		
		private void DocentProcess()
		{
			if ((SystemManager.Inst.TutorialState & 4) == 1)
			{
				return;
			}
        
			SystemManager.Inst.TutorialState |= 4;
			SystemManager.Inst.SummonDocent();

			var queue = SystemManager.Inst.Docent.GetComponent<StaticQuoteQueue>();

			foreach (var line in tutorialLines)
			{
				queue.EnqueueAudio(line);
			}
		}
	}
}