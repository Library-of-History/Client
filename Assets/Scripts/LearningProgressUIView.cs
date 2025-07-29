using System;
using System.Collections.Generic;
using System.Text;
using Anaglyph.Menu;
using Radishmouse;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LearningProgressUIView : MonoBehaviour
{
    [SerializeField] private GameObject[] panels;
    [SerializeField] private GameObject[] buttonPrefabs;
    [SerializeField] private UILineRenderer lineRenderer;

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

    public void ShowButtons<T>(T[] array, LearningProgressUICollection collection, Action<GameObject> onClick)
    {
        foreach (var element in array)
        {
            // if (element.ToString() == "None")
            // {
            //     continue;
            // }
            
            var button = Instantiate(buttonPrefabMap[collection], panelMap[collection].transform, false);
            var btnCopy = button;
            btnCopy.GetComponent<Button>().onClick.RemoveAllListeners();

            if (collection == LearningProgressUICollection.Subject)
            {
                var navPage = panelMap[collection].GetComponentInParent<NavPage>(true);

                btnCopy.GetComponent<Button>().onClick.AddListener(delegate
                {
                    var goToPage =
                        panelMap[LearningProgressUICollection.Age].GetComponentInParent<NavPage>(true);
                    navPage.ParentView.GoToPage(goToPage);
                });
            }
            
            StringBuilder sb = new StringBuilder(element.ToString());
            sb.Append("Button");
            btnCopy.name = sb.ToString();
            
            sb.Clear();
            sb.Append(UIDisplayNameParser.GetDisplayNameGeneric(element));
            btnCopy.GetComponentInChildren<TextMeshProUGUI>().text = sb.ToString();

            if (collection == LearningProgressUICollection.Age)
            {
                buttons.Add(btnCopy);
            }
            
            btnCopy.GetComponent<Button>().onClick.AddListener(() => onClick(btnCopy));
        }

        if (collection == LearningProgressUICollection.Age)
        {
            lineRenderer.points = new Vector2[buttons.Count];
            
            var layout = panelMap[collection].GetComponent<HorizontalLayoutGroup>();
            var scrollRect = panelMap[collection].GetComponentInParent<ScrollRect>(true).gameObject.GetComponent<RectTransform>();
            var ageButtonRect = buttonPrefabMap[collection].GetComponent<RectTransform>();
            var next = new Vector2(0f, -1f * scrollRect.rect.height / 2);
                    
            for (int i = 0; i < buttons.Count; i++)
            {
                if (i == 0)
                {
                    next += new Vector2(layout.padding.left, 0f);
                }
                else
                {
                    next += new Vector2(ageButtonRect.rect.width / 2 + layout.spacing, 0f);
                }

                next += new Vector2(ageButtonRect.rect.width / 2, 0f);
                lineRenderer.points[i] = next;
            }
            
            lineRenderer.SetVerticesDirty();
        }
    }
}
