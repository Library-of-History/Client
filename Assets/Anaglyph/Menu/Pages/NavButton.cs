using UnityEngine;
using UnityEngine.UI;

namespace Anaglyph.Menu
{
    public class NavButton : MonoBehaviour
    {
		private NavPage navPage;
		public NavPage goToPage;

		private void OnEnable()
		{
			navPage = GetComponentInParent<NavPage>(true);

			if (navPage != null)
			{
				Debug.Log("Yes");
				Debug.Log(goToPage.name);
			}

			GetComponent<Button>().onClick.AddListener(delegate
			{
				navPage.ParentView.GoToPage(goToPage);
			});
		}
	}
}
