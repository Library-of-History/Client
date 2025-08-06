using System;
using Anaglyph.Menu;
using UnityEngine;
using UnityEngine.UI;

public class LearningProgressUIPresenter : MonoBehaviour
{
    private LearningProgressUIModel model;
    [SerializeField] private LearningProgressUIView view;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        model = new LearningProgressUIModel(LearningProgressUICollection.Subject);
        
        view.ShowButtons(model.SubjectArray, LearningProgressUICollection.Subject, OnButtonClick);
        view.ShowButtons(model.AgeArray, LearningProgressUICollection.Age, OnButtonClick);
        
        backButton.onClick.AddListener(delegate
        {
            model.SwitchCurrentState(LearningProgressUICollection.Subject);
        });
    }

    public LearningProgressUIModel GetModel()
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
                model.SwitchCurrentState(age);
            }
        }
    }
}
