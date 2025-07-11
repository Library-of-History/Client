using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIControllerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] texts;

    private Dictionary<UICollection, TextMeshProUGUI> textMap;

    private void Awake()
    {
        textMap = new Dictionary<UICollection, TextMeshProUGUI>();
        
        foreach (var text in texts)
        {
            if (Enum.TryParse(text.gameObject.name.Substring(0, text.gameObject.name.Length - 4),
                    out UICollection collection))
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
            pair.Value.text = UIDisplayNameParser.GetDisplayName(pair.Key);
        }
    }

    public void SetActiveText(UICollection[] collections)
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

    public void HighLightText(UICollection collection)
    {
        textMap[collection].color = Color.yellow;
    }
    
    public void UnHighLightText(UICollection collection)
    {
        textMap[collection].color = Color.white;
    }
}
