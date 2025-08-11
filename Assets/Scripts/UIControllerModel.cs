using System;

public class UIControllerModel
{
    public UIControllerCollection[] EnumArray { get; private set; }

    private static readonly UIControllerCollection[] mrEnums =
        { UIControllerCollection.Progress, UIControllerCollection.Tool, UIControllerCollection.Log,
            UIControllerCollection.Setting, UIControllerCollection.Logout, UIControllerCollection.Quit };
    private static readonly UIControllerCollection[] vrEnums =
        { UIControllerCollection.Log, UIControllerCollection.Setting, UIControllerCollection.BackToMR,
            UIControllerCollection.Quit };

    public UIControllerModel(UIEnvironment environment)
    {
        UpdateEnumArray(environment);
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
}
