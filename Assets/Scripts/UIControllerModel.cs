using System;

public class UIControllerModel
{
    public UIControllerCollection CurrentUI { get; private set; }
    public UIControllerCollection[] EnumArray { get; private set; }

    private static readonly UIControllerCollection[] mrEnums =
        { UIControllerCollection.Progress, UIControllerCollection.Tool, UIControllerCollection.Log,
            UIControllerCollection.Setting, UIControllerCollection.Quit };
    private static readonly UIControllerCollection[] vrEnums =
        { UIControllerCollection.Log, UIControllerCollection.Setting, UIControllerCollection.BackToMR,
            UIControllerCollection.Quit };

    public UIControllerModel(UIEnvironment environment, UIControllerCollection current)
    {
        UpdateEnumArray(environment);
        SetCurrentUI(current);
    }

    public void UpdateEnumArray(UIEnvironment environment)
    {
        if (environment == UIEnvironment.MR)
        {
            EnumArray = mrEnums;
        }
        else
        {
            EnumArray = vrEnums;
        }
    }

    public void SetCurrentUI(UIControllerCollection current)
    {
        CurrentUI = current;
    }
}
