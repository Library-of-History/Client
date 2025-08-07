using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingMedia : MonoBehaviour
{
    [SerializeField] private GameObject imagePref;
    
    private string bookName = String.Empty;
    private List<GameObject> images;
    private string saveDir;

    private void OnEnable()
    {
        images = new List<GameObject>();
        saveDir = Path.Combine(Application.persistentDataPath, bookName);
            
        if (Directory.Exists(saveDir))
        {
            var files = Directory.GetFiles(saveDir);

            foreach (var file in files)
            {
                var image = Instantiate(imagePref, gameObject.transform, false);
                byte[] fileData = File.ReadAllBytes(file);

                Texture2D tex = new Texture2D(2, 2);
                if (tex.LoadImage(fileData)) // PNG 데이터를 텍스처로 변환
                {
                    // 텍스처 성공적으로 로드됨

                    // 만약 Sprite로 만들고 싶다면
                    Sprite sprite = Sprite.Create(tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f));
            
                    // sprite를 UI 이미지 등에 할당 가능
                    image.GetComponent<Image>().sprite = sprite;
                    images.Add(image);
            
                    Debug.Log($"Loaded image {file} size: {tex.width}x{tex.height}");
                }
                else
                {
                    Debug.LogWarning($"Failed to load image: {file}");
                }
            }
        }
    }

    private void OnDisable()
    {
        foreach (var image in images)
        {
            Destroy(image);
        }
        
        images.Clear();
    }

    public void SetBookName(string bookName)
    {
        this.bookName = bookName;
    }
}
