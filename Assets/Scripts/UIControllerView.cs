using System;
using System.Collections.Generic;
using Anaglyph.Lasertag;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIControllerView : MonoBehaviour
{
    [SerializeField] private Button[] buttons;

    private Dictionary<UIControllerCollection, Button> buttonMap;
    private UIControllerCollection currentSelectedButton;

    public void Init()
    {
        buttonMap = new Dictionary<UIControllerCollection, Button>();
        
        foreach (var button in buttons)
        {
            if (Enum.TryParse(button.gameObject.name, out UIControllerCollection collection))
            {
                buttonMap[collection] = button;
                
                button.onClick.AddListener(delegate
                {
                    SetCurrentSelectedButton(collection);

                    foreach (var pair in buttonMap)
                    {
                        if (pair.Key == currentSelectedButton)
                        {
                            pair.Value.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.yellow;
                        }
                        else
                        {
                            pair.Value.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;

                        }
                    }
                });
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

    public void SetActiveCurrentUI()
    {
        buttonMap[currentSelectedButton].onClick.Invoke();
    }

    public void SetCurrentSelectedButton(UIControllerCollection collection)
    {
        currentSelectedButton = collection;
    }
}
