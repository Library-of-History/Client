using UnityEngine;

public class BookState : MonoBehaviour
{
    public bool IsOpened { get; private set; } = false;
    public GameObject UI { get; private set; }

    public void ChangeState(bool state)
    {
        IsOpened = state;
    }

    public void SetUI(GameObject ui)
    {
        UI = ui;
    }
}
