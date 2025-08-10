using System;
using System.Collections;
using UnityEngine;

public class SceneSwitchFadeEffect : MonoBehaviour
{
    public float FadeDuration = 2f;

    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;

    private Camera mainCam;
    private Vector3 offset = new Vector3(0f, 0f, 0.5f);

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        SetCamera();
    }

    private void Update()
    {
        SetPose();
    }

    public void SetCamera()
    {
        mainCam = Camera.main;
    }

    public void SetPose()
    {
        Vector3 flatForward = mainCam.transform.forward.normalized;

        Matrix4x4 pose = Matrix4x4.LookAt(mainCam.transform.position, mainCam.transform.position + flatForward, Vector3.up);

        transform.position = pose.MultiplyPoint(offset);

        Vector3 forward = (transform.position - mainCam.transform.position).normalized;

        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }
    
    public void FadeToWhite(Action onComplete = null)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToValue(0f, 1f, FadeDuration, onComplete));
    }

    public void ResetFadeEffect(Action onComplete = null)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToValue(1f, 0f, FadeDuration, onComplete));
    }
    
    private IEnumerator FadeToValue(float startValue, float targetValue, float duration, Action onComplete = null)
    {
        yield return StartCoroutine(FadeToValueCoroutine(startValue, targetValue, duration));
        onComplete?.Invoke();
        fadeCoroutine = null;
    }
    
    private IEnumerator FadeToValueCoroutine(float startValue, float targetValue, float duration)
    {
        float start = startValue;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            t = Mathf.SmoothStep(0f, 1f, t);

            canvasGroup.alpha = Mathf.Lerp(start, targetValue, t);
            yield return null;
        }

        canvasGroup.alpha = targetValue;
    }
}
