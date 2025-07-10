using UnityEngine;

public class UIControllerModel
{
    public UICollection CurrentUI { get; private set; }

    public UIControllerModel(UICollection collection)
    {
        CurrentUI = collection;
    }

    public void UpdateCurrentUI(UICollection collection)
    {
        CurrentUI = collection;
    }
}
