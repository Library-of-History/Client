using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class AutoExposureFade : MonoBehaviour
{
    [Header("페이드 설정")] 
    [SerializeField] private float defaultExposure = 0f;
    [SerializeField] private float whiteExposure = 10f;
    [SerializeField] private Volume volume;

    [Header("씬 전환")] 
    [SerializeField] private float fadeOutDuration = 3.0f;
    [SerializeField] private float fadeInDuration = 3.0f;
    [SerializeField] private float holdWWhiteDuration = 0.2f;
    
    private ColorAdjustments colorAdjustments;
    private Coroutine fadeCoroutine;

    public static AutoExposureFade Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeColorAdjustments();   
    }

    private void InitializeColorAdjustments()
    {
        if (volume != null && volume.profile != null)
        {
            if (!volume.profile.TryGet(out colorAdjustments))
            {
                colorAdjustments = volume.profile.Add<ColorAdjustments>();
            }
        }
        else
        {
            Debug.LogError("Volume 또는 Volume Profile이 할당되지 않음");
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.overrideState = true;
            colorAdjustments.postExposure.value = defaultExposure;
        }
    }
    
    public void FadeToWhite(float duration, Action onComplete = null)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToValue(whiteExposure, duration, onComplete));
    }

    public void ResetExposure(float duration, Action onComplete = null)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToValue(defaultExposure, duration, onComplete));
    }

    public void TransitionToScene(string sceneName)
    {
        if (fadeCoroutine != null) 
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(SceneTransitionSequence(sceneName));
    }
    
    private IEnumerator SceneTransitionSequence(string sceneName)
    {
        yield return StartCoroutine(FadeToValueCoroutine(whiteExposure, fadeOutDuration));

        yield return new WaitForSeconds(holdWWhiteDuration);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        yield return asyncLoad;

        yield return new WaitForEndOfFrame();
        RefreshVolumeReference();

        yield return StartCoroutine(FadeToValueCoroutine(defaultExposure, fadeInDuration));

        fadeCoroutine = null;
    }

    private void RefreshVolumeReference()
    {
        if (volume == null)
        {
            volume = FindObjectOfType<Volume>();
        }
        
        InitializeColorAdjustments();
    }

    private IEnumerator FadeToValue(float targetValue, float duration, Action onComplete = null)
    {
        yield return StartCoroutine(FadeToValueCoroutine(targetValue, duration));
        onComplete?.Invoke();
        fadeCoroutine = null;
    }
    
    private IEnumerator FadeToValueCoroutine(float targetValue, float duration)
    {
        if (colorAdjustments == null)
            yield break;

        float startValue = colorAdjustments.postExposure.value;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            t = Mathf.SmoothStep(0f, 1f, t);

            colorAdjustments.postExposure.value = Mathf.Lerp(startValue, targetValue, t);
            yield return null;
        }

        colorAdjustments.postExposure.value = targetValue;
    }
}
