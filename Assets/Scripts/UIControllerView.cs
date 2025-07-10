using System;
using UnityEngine;

public class UIControllerView : MonoBehaviour
{
    public event Action<UICollection> OnPlayerInput;

    private void OnClickLearningProgress()
    {
        OnPlayerInput?.Invoke(UICollection.LearningProgress);
    }
    
    private void OnClickLearningLog()
    {
        OnPlayerInput?.Invoke(UICollection.LearningLog);
    }
}
