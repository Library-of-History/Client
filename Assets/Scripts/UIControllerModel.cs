using System;

public class UIControllerModel
{
    public UICollection CurrentUI { get; private set; }
    public UICollection[] EnumArray { get; private set; }

    private static readonly UICollection[] mrEnums = { UICollection.LearningProgress, UICollection.LearningLog };
    private static readonly UICollection[] vrEnums = { UICollection.LearningProgress };

    public UIControllerModel(UICollection collection, UIEnvironment environment)
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
