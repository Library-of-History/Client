using TMPro;
using UnityEngine;

public class ResetInputField : MonoBehaviour
{
    [SerializeField] private TMP_InputField id;
    [SerializeField] private TMP_InputField password;

    public void OnClick()
    {
        id.text = "";
        password.text = "";
    }
}
