using System;

public class UIControllerModel
{
    public UIControllerCollection CurrentUI { get; private set; }
    public UIControllerCollection[] EnumArray { get; private set; }

    private static readonly UIControllerCollection[] mrEnums =
        { UIControllerCollection.LearningProgress, UIControllerCollection.ObjectModify };
    private static readonly UIControllerCollection[] vrEnums =
        { UIControllerCollection.LearningProgress };

    public UIControllerModel(UIControllerCollection collection, UIEnvironment environment)
    {
        CurrentUI = collection;
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

    public void SwitchUpCurrentUI()
    {
        int idx = Array.IndexOf(EnumArray, CurrentUI);
        idx = (idx - 1 + EnumArray.Length) % EnumArray.Length;
        CurrentUI = EnumArray[idx];
    }

    public void SwitchDownCurrentUI()
    {
        int idx = Array.IndexOf(EnumArray, CurrentUI);
        idx = (idx + 1) % EnumArray.Length;
        CurrentUI = EnumArray[idx];
    }
}
