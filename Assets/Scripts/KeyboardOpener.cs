using UnityEngine;

public class KeyboardOpener : MonoBehaviour
{
    private TouchScreenKeyboard keyboard;

    public void OpenKeyboard(string initialText = "")
    {
        // 키보드 호출 (TouchScreenKeyboardType, autocorrect 등 옵션 지정 가능)
        keyboard = TouchScreenKeyboard.Open(initialText, TouchScreenKeyboardType.Default);
    }

    void Update()
    {
        if (keyboard != null)
        {
            // 키보드가 닫혔는지 등의 상태 체크 가능
            if (keyboard.status == TouchScreenKeyboard.Status.Done || keyboard.status == TouchScreenKeyboard.Status.Canceled)
            {
                // 키보드 닫힘 시 처리 (예: 입력값 저장)
                Debug.Log("Keyboard input: " + keyboard.text);
                keyboard = null;
            }
        }
    }
}