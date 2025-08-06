using System;
using UnityEngine;

public class SummonPreviewBook : MonoBehaviour
{
    private const float Offset = 0.2f;
    
    [SerializeField] private GameObject previewBook;
    private Vector3 initPos;

    private void Awake()
    {
        var presenter = SystemManager.Inst.SystemUI.GetComponentInChildren<LearningProgressUIPresenter>(true);
        var subject = presenter.GetModel().CurrentSubject;
        var age = presenter.GetModel().CurrentAge;
        string key = subject + "_" + age;
        
        var nameList = SystemManager.Inst.ScenesData.BookMap[key];

        initPos = new Vector3(0f, -0.135f, 0.658f);

        foreach (var item in nameList)
        {
            var book = Instantiate(previewBook, gameObject.transform, false);
            book.name = key + "_" + item;
            book.transform.localPosition = initPos;
            book.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            
            initPos += new Vector3(Offset, 0f, 0f);
        }
    }
}
