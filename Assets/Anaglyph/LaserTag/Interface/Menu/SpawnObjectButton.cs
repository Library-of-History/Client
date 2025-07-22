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
			var spawner = isRight ? Spawner.Right : Spawner.Left;
			spawner.gameObject.SetActive(true);
			spawner.SetObjectToSpawn(objectToSpawn);
			Destroy(gameObject);
		}
	}
}