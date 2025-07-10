using System;
using UnityEngine;

public class UIControllerPresenter : MonoBehaviour
{
    private UIControllerModel model;
    [SerializeField] private UIControllerView view;
    
    [SerializeField] private LearningProgressUIPresenter learningProgressPresenter;
    [SerializeField] private LearningLogUIPresenter learningLogPresenter;

    private void Awake()
    {
        model = new UIControllerModel(UICollection.LearningProgress);
        view.OnPlayerInput += GetPlayerInput;
    }

    private void OnEnable()
    {
        view.gameObject.SetActive(true);
    }
    
    private void OnDisable()
    {
        view.gameObject.SetActive(false);
    }

    private void GetPlayerInput(UICollection collection)
    {
        model.UpdateCurrentUI(collection);
    }
}
