using UnityEngine;

public class BookState : MonoBehaviour
{
    public bool IsOpened { get; private set; } = false;

    public void ChangeState(bool state)
    {
        IsOpened = state;
    }
}
