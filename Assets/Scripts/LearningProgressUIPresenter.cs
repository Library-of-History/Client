using System;
using UnityEngine;

public class LearningProgressUIPresenter : MonoBehaviour
{
    private LearningProgressUIModel model;
    [SerializeField] private LearningProgressUIView view;

    private void Awake()
    {
        model = new LearningProgressUIModel(LearningProgressUICollection.Subject);
        view.ShowButtons(model.SubjectArray, LearningProgressUICollection.Subject, OnButtonClick);
        gameObject.SetActive(false);
    }

    private void OnButtonClick(GameObject btn)
    {
        if (model.CurrentUI == LearningProgressUICollection.Subject)
        {
            if (Enum.TryParse<Subject>(btn.name.Substring(0, btn.name.Length - 6), out var subject))
            {
                view.HideButtons();
                model.SwitchCurrentState(subject);
                model.SwitchCurrentState(LearningProgressUICollection.Age);
                
                view.SwitchCurrentUI(model.CurrentUI);
                view.ShowButtons(model.AgeArray, model.CurrentUI, OnButtonClick);
            }
        }
        else if (model.CurrentUI == LearningProgressUICollection.Age)
        {
            if (Enum.TryParse<Age>(btn.name.Substring(0, btn.name.Length - 6), out var age))
            {
                view.HideButtons();
                model.SwitchCurrentState(age);
            
                view.SwitchCurrentUI(model.CurrentUI);
                gameObject.SetActive(false);
            }
        }
    }
}
