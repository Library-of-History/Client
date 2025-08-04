using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIControllerView : MonoBehaviour
{
    [SerializeField] private Button[] buttons;

    private Dictionary<UIControllerCollection, Button> buttonMap;

    private void Awake()
    {
        buttonMap = new Dictionary<UIControllerCollection, Button>();
        
        foreach (var button in buttons)
        {
            if (Enum.TryParse(button.gameObject.name, out UIControllerCollection collection))
            {
                buttonMap[collection] = button;
            }
        }
    }

    public void SetActiveButton(UIControllerCollection[] collections)
    {
        foreach (var pair in buttonMap)
        {
            bool check = false;
            
            foreach (var item in collections)
            {
                if (item == pair.Key)
                {
                    check = true;
                    break;
                }
            }

            if (check)
            {
                pair.Value.gameObject.SetActive(true);
            }
            else
            {
                pair.Value.gameObject.SetActive(false);
            }
        }
    }

    public void SetActiveCurrentUI(UIControllerCollection collection)
    {
        buttonMap[collection].onClick.Invoke();
    }
}
