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
        model = new UIControllerModel(UIEnvironment.MR, UIControllerCollection.Progress);
    }

    private void OnEnable()
    {
        view.SetActiveButton(model.EnumArray);
        view.SetActiveCurrentUI(model.CurrentUI);
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
        model.SetCurrentUI(model.EnumArray[0]);
    }
}
