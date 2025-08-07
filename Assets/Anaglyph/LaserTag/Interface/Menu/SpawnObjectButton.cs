using Anaglyph.Menu;
using Anaglyph.XRTemplate;
using UnityEngine;

namespace Anaglyph.Lasertag.UI
{
	public class SpawnObjectButton : MonoBehaviour
	{
		public GameObject objectToSpawn;

		public void OnClick(bool isRight)
		{
			if (SystemManager.Inst.MRSelectedObject != null)
			{
				return;
			}
			
			SystemManager.Inst.CurrentSelectedBookName = gameObject.name;
			var spawner = isRight ? Spawner.Right : Spawner.Left;
			spawner.gameObject.SetActive(true);
			spawner.SetObjectToSpawn(objectToSpawn);
			
			Destroy(SystemManager.Inst.Portal);
			SystemManager.Inst.Portal = null;
		}
	}
}