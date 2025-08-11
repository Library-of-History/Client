using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIControllerPresenter : MonoBehaviour
{
    private UIControllerModel model;
    [SerializeField] private UIControllerView view;
    
    private void Awake()
    {
        model = new UIControllerModel(UIEnvironment.MR);
        view.Init();
    }

    private void OnEnable()
    {
        view.SetActiveButton(model.EnumArray);
        view.SetActiveCurrentUI();
    }

    public void SetInitState()
    {
        view.SetCurrentSelectedButton(UIControllerCollection.Progress);
        view.SetActiveCurrentUI();
    }

    public void EnvSwitch()
    {
        if (SystemManager.Inst.CurrentEnv == UIEnvironment.MR)
        {
            SystemManager.Inst.CurrentEnv = UIEnvironment.VR;
        }
        else
        {
            SystemManager.Inst.CurrentEnv = UIEnvironment.MR;
        }
        
        model.UpdateEnumArray(SystemManager.Inst.CurrentEnv);
        view.SetCurrentSelectedButton(model.EnumArray[0]);
    }
}
