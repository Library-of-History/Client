using UnityEngine;
using UnityEngine.UI;

public class SetControllerSprite : MonoBehaviour
{
    [SerializeField] private Image controllerImage;
    [SerializeField] private Sprite mrSprite;
    [SerializeField] private Sprite vrSprite;
    
    public void OnClickMR()
    {
        controllerImage.sprite = mrSprite;
    }

    public void OnClickVR()
    {
        controllerImage.sprite = vrSprite;
    }
}
