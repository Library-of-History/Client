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

			GetComponent<Button>().onClick.AddListener(delegate
			{
				navPage.ParentView.GoToPage(goToPage);
			});
		}
	}
}
