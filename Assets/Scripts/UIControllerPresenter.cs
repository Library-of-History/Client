using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIControllerPresenter : MonoBehaviour
{
    private UIControllerModel model;
    [SerializeField] private UIControllerView view;
    
    [SerializeField] private MonoBehaviour[] presenters;
    private Dictionary<UICollection, MonoBehaviour> presenterMap;
    
    private bool isApplicationUIOpened = false;
    private bool didMoveAxis = false;

    private void Awake()
    {
        model = new UIControllerModel(UICollection.LearningProgress, UIEnvironment.MR);
        presenterMap = new Dictionary<UICollection, MonoBehaviour>();

        foreach (var presenter in presenters)
        {
            if (Enum.TryParse<UICollection>(presenter.gameObject.name.Substring(
                    0, presenter.gameObject.name.Length - 2), out var collection))
            {
                presenterMap[collection] = presenter;
            }
        }
    }

    private void OnEnable()
    {
        view.HighLightText(model.CurrentUI);
    }

    private void OnLeftAxis(InputAction.CallbackContext context)
    {
        if (!view.gameObject.activeSelf)
        {
            return;
        }
        
        if (context.performed)
        {
            if (!didMoveAxis)
            {
                if (context.ReadValue<Vector2>().y > 0f)
                {
                    didMoveAxis = true;
                    view.UnHighLightText(model.CurrentUI);
                    model.SwitchUpCurrentUI();
                    view.HighLightText(model.CurrentUI);
                }
                else if (context.ReadValue<Vector2>().y < 0f)
                {
                    didMoveAxis = true;
                    view.UnHighLightText(model.CurrentUI);
                    model.SwitchDownCurrentUI();
                    view.HighLightText(model.CurrentUI);
                }
            }
        }
        else if (context.canceled)
        {
            didMoveAxis = false;
        }
    }

    private void OnLeftAxisClick(InputAction.CallbackContext context)
    {
        if (context.performed && context.ReadValueAsButton())
        {
            if (view.gameObject.activeSelf)
            {
                view.gameObject.SetActive(false);
            }
            else
            {
                view.gameObject.SetActive(true);
                view.HighLightText(model.CurrentUI);
                view.SetActiveText(model.EnumArray);
            }
        }
    }

    private void OnRightAxisClick(InputAction.CallbackContext context)
    {
        if (context.performed && context.ReadValueAsButton())
        {
            if (isApplicationUIOpened)
            {
                foreach (var pair in presenterMap)
                {
                    pair.Value.gameObject.SetActive(false);
                }
                
                isApplicationUIOpened = false;
            }
            else
            {
                foreach (var pair in presenterMap)
                {
                    pair.Value.gameObject.SetActive(pair.Key == model.CurrentUI);
                }
                
                isApplicationUIOpened = true;
            }
        }
    }
}
