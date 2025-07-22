using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIControllerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] texts;

    private Dictionary<UIControllerCollection, TextMeshProUGUI> textMap;

    private void Awake()
    {
        textMap = new Dictionary<UIControllerCollection, TextMeshProUGUI>();
        
        foreach (var text in texts)
        {
            if (Enum.TryParse(text.gameObject.name.Substring(0, text.gameObject.name.Length - 4),
                    out UIControllerCollection collection))
            {
                textMap[collection] = text;
            }
        }
        
        SetTextName();
    }

    public void SetTextName()
    {
        foreach (var pair in textMap)
        {
            pair.Value.text = UIDisplayNameParser.GetDisplayNameGeneric(pair.Key);
        }
    }

    public void SetActiveText(UIControllerCollection[] collections)
    {
        foreach (var pair in textMap)
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

    public void HighLightText(UIControllerCollection collection)
    {
        textMap[collection].color = Color.yellow;
    }
    
    public void UnHighLightText(UIControllerCollection collection)
    {
        textMap[collection].color = Color.white;
    }
}
