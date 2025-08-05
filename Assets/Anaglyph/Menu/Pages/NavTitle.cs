using Anaglyph.Menu;
using UnityEngine;

namespace Anaglyph.MenuXR
{
    public class NavTitle : MonoBehaviour
    {
		private NavPage parentNavPage;
		[SerializeField] private GameObject backButton;
		[SerializeField] private GameObject nextButton;
		[SerializeField] private RectTransform titleRectTransform;

		private bool showBackButton;

		private void Awake()
		{
			parentNavPage = GetComponentInParent<NavPage>(true);
		}

		private void OnEnable()
		{
			parentNavPage = GetComponentInParent<NavPage>(true);

			if (parentNavPage != null)
			{
				showBackButton = parentNavPage.showBackButton && parentNavPage.ParentView?.History.Count > 1;
			}

			if (backButton != null)
			{
				backButton.SetActive(showBackButton);
			}
			
			if (nextButton != null)
			{
				nextButton.SetActive(!showBackButton);
			}
			
			//titleRectTransform.anchoredPosition = showBackButton ? new Vector2(80, 0) : Vector2.zero;
		}
	}
}
