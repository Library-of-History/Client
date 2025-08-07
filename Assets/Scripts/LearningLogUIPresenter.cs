using System;
using Anaglyph.Menu;
using UnityEngine;
using UnityEngine.UI;

public class LearningLogUIPresenter : MonoBehaviour
{
    private LearningLogUIModel model;
    [SerializeField] private LearningLogUIView view;
    [SerializeField] private Button[] backButtons;

    private void Awake()
    {
        model = new LearningLogUIModel(LearningProgressUICollection.Subject);
        
        view.ShowButtons(model.SubjectArray, LearningProgressUICollection.Subject, OnButtonClick);
        view.ShowButtons(model.AgeArray, LearningProgressUICollection.Age, OnButtonClick);

        foreach (var button in backButtons)
        {
            button.onClick.AddListener(delegate
            {
                var pageName = button.GetComponentInParent<NavPage>(true).gameObject.name;

                if (Enum.TryParse(pageName, out LearningProgressUICollection collection))
                {
                    switch (collection)
                    {
                        case LearningProgressUICollection.Age:
                            model.SwitchCurrentState(LearningProgressUICollection.Subject);
                            break;
                        
                        case LearningProgressUICollection.Period:
                            model.SwitchCurrentState(LearningProgressUICollection.Age);
                            break;
                        
                        default:
                            break;
                    }
                }
            });
        }
    }

    private void OnEnable()
    {
        model.CalcPercent();
        view.UpdateEntireLog(model.CurrentEntirePercent);

        if (model.CurrentUI == LearningProgressUICollection.Period)
        {
            view.ShowPartialLogs(model.CurrentSubject, model.CurrentAge, model);
        }
    }

    private void OnDisable()
    {
        if (model.CurrentUI == LearningProgressUICollection.Period)
        {
            view.HidePartialLogs();
        }
    }

    public LearningLogUIModel GetModel()
    {
        return model;
    }

    private void OnButtonClick(GameObject btn)
    {
        if (model.CurrentUI == LearningProgressUICollection.Subject)
        {
            if (Enum.TryParse<Subject>(btn.name.Substring(0, btn.name.Length - 6), out var subject))
            {
                model.SwitchCurrentState(subject);
                model.SwitchCurrentState(LearningProgressUICollection.Age);
            }
        }
        else if (model.CurrentUI == LearningProgressUICollection.Age)
        {
            if (Enum.TryParse<Age>(btn.name.Substring(0, btn.name.Length - 6), out var age))
            {
                view.HidePartialLogs();

                model.SwitchCurrentState(age);
                model.SwitchCurrentState(LearningProgressUICollection.Period);
                
                view.ShowPartialLogs(model.CurrentSubject, model.CurrentAge, model);
            }
        }
    }
}
