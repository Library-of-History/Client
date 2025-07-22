using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LearningProgressUIView : MonoBehaviour
{
    [SerializeField] private GameObject[] panels;
    [SerializeField] private GameObject[] buttonPrefabs;

    private Dictionary<LearningProgressUICollection, GameObject> panelMap;
    private Dictionary<LearningProgressUICollection, GameObject> buttonPrefabMap;
    private List<GameObject> buttons;

    private void Awake()
    {
        panelMap = new Dictionary<LearningProgressUICollection, GameObject>();
        buttonPrefabMap = new Dictionary<LearningProgressUICollection, GameObject>();
        buttons = new List<GameObject>();

        foreach (var panel in panels)
        {
            if (Enum.TryParse(panel.name.Substring(
                    0, panel.name.Length - 5), out LearningProgressUICollection collection))
            {
                panelMap[collection] = panel;
            }
        }
        
        foreach (var button in buttonPrefabs)
        {
            if (Enum.TryParse(button.name.Substring(
                    0, button.name.Length - 6), out LearningProgressUICollection collection))
            {
                buttonPrefabMap[collection] = button;
            }
        }
    }
    
    public void SwitchCurrentUI(LearningProgressUICollection collection)
    {
        foreach (var pair in panelMap)
        {
            pair.Value.SetActive(pair.Key == collection);
        }
    }

    public void ShowButtons<T>(T[] array, LearningProgressUICollection collection, Action<GameObject> onClick)
    {
        foreach (var element in array)
        {
            if (element.ToString() == "None")
            {
                continue;
            }
            
            var button = Instantiate(buttonPrefabMap[collection], panelMap[collection].transform, false);
            StringBuilder sb = new StringBuilder(element.ToString());
            sb.Append("Button");
            button.name = sb.ToString();
            
            sb.Clear();
            sb.Append(UIDisplayNameParser.GetDisplayNameGeneric(element));
            button.GetComponentInChildren<TextMeshProUGUI>().text = sb.ToString();
            buttons.Add(button);
            
            var btnCopy = button;
            btnCopy.GetComponent<Button>().onClick.RemoveAllListeners();
            btnCopy.GetComponent<Button>().onClick.AddListener(() => onClick(btnCopy));
            
            Debug.Log(button.name);
        }
    }

    public void HideButtons()
    {
        foreach (var button in buttons)
        {
            Destroy(button);
        }
        
        buttons.Clear();
    }
}
