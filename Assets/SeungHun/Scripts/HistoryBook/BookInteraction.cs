using System;
using Anaglyph.XRTemplate;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.XR.Interaction.Toolkit;
using Cysharp.Threading.Tasks;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BookInteraction : MonoBehaviour
{
    [Header("애니메이션 세팅")] 
    public float moveRotationDuration = 2.0f;
    public Ease movementEase = Ease.OutQuart;
    public Ease rotationEase = Ease.OutQuart;

    [Header("타겟 세팅")] 
    public Transform vrCamera;
    public Vector3 targetLocalPosition = new Vector3(0, 0, 1f);
    public Vector3 targetRotation = Vector3.zero;
    
    [Header("타임라인 세팅")]
    public PlayableDirector timelineDirector;
    public TimelineAsset bookOpenTimeLine;

    [Header("파티클 이펙트")] 
    public ParticleSystem[] bookOpenParticles;
    public bool autoFindParticles = true;
    
    [Header("포인트 라이트")]
    public Light bookOpenLight;
    public bool enableLightOnOpen = true;
    public float lightIntensityDuration = 6f;
    public float targetLightIntensity = 4f;
    public Ease lightIntensityEase = Ease.OutQuart;

    [Header("사운드 이펙트")] 
    public AudioSource bookOpenSFX;
    public bool playSFXOnOpen = true;
    
    [Header("씬 전환")] 
    public string nextSceneName = "VR";
    public bool useSceneTransition = true;

    [Header("프리팹 세팅")] 
    public bool isPrefabInstance = false;
    public bool autoFindComponents = true;
    
    private Vector3 originalPosition;   
    private Quaternion originalRotation;
    
    public bool IsAnimating = false;
    private bool isBookOpened = false;
    
    private Sequence animationSequence;
    private Action callBack;
    
    private void Start()
    {
        InitializeComponents();
        SetupInteractionEvents();
        InitializeLights();

        SystemManager.Inst.AudioManagerInst.OnSfxVolumeChanged += VolumeChange;
    }

    private void VolumeChange(float value)
    {
        bookOpenSFX.volume = value;
    }

    private void InitializeComponents()
    {
        if (autoFindComponents)
        {
            AutoFindRequiredComponents();
        }
    }

    private void AutoFindRequiredComponents()
    {
        if (vrCamera == null)
        {
            vrCamera = Camera.main?.transform;

            if (vrCamera == null)
            {
                var xrOrigin = MainXROrigin.Instance;
                if (xrOrigin != null)
                {
                    vrCamera = xrOrigin.Camera?.transform;
                }
            }

            if (vrCamera == null)
            {
                vrCamera = FindFirstObjectByType<Camera>()?.transform;
            }
        }

        if (timelineDirector == null)
        {
            timelineDirector = GetComponent<PlayableDirector>();
            if (timelineDirector == null)
            {
                timelineDirector = GetComponentInChildren<PlayableDirector>();
            }
        }

        if (autoFindParticles && (bookOpenParticles == null || bookOpenParticles.Length == 0))
        {
            bookOpenParticles = GetComponentsInChildren<ParticleSystem>();
        }
    }
    
    private void StoreOriginalPosition()
    {
        originalPosition = transform.position;
    }
    
    public void StoreOriginalRotation()
    {
        originalRotation = transform.rotation;
    }

    private void InitializeLights()
    {
        if (bookOpenLight != null)
        {
            bookOpenLight.enabled = false;            
        }
    }

    private void SetupInteractionEvents()
    {
        if (timelineDirector != null)
        {
            timelineDirector.stopped += OnTimeLineFinished;
        }
    }
    
    private bool CanStartAnimation()
    {
        return !IsAnimating && !isBookOpened;
    }

    public void StartBookInteractionSequence(Action action)
    {
        if (vrCamera == null)
        {
            return;
        }
        
        if (animationSequence != null)
        {
            animationSequence.Kill();
        }

        IsAnimating = true;
        animationSequence = DOTween.Sequence();
        callBack = action;
        StoreOriginalPosition();
        StoreOriginalRotation();
        
        Vector3 targetWorldPosition = vrCamera.TransformPoint(targetLocalPosition);
        Quaternion targetWorldRotation = vrCamera.rotation * Quaternion.Euler(targetRotation);  
        
        animationSequence.Append(transform.DOMove(targetWorldPosition, moveRotationDuration).SetEase(movementEase));
        animationSequence.Join(transform.DORotateQuaternion(targetWorldRotation, moveRotationDuration).SetEase(rotationEase));

        animationSequence.Play()
            .OnComplete(() =>
            {
                PlayBookOpenParticles();
                EnableBookOpenLight();
                PlayBookOpenSFX();
                SystemManager.Inst.FadeUI.FadeToWhite(callBack);
            });
    }
    
    public void FinishBookInteractionSequence(Action action)
    {
        if (vrCamera == null)
        {
            return;
        }
        
        if (animationSequence != null)
        {
            animationSequence.Kill();
        }

        IsAnimating = true;
        animationSequence = DOTween.Sequence();
        callBack = null;
        
        animationSequence.Append(transform.DOMove(originalPosition, moveRotationDuration).SetEase(movementEase));
        animationSequence.Join(transform.DORotateQuaternion(originalRotation, moveRotationDuration).SetEase(rotationEase));

        animationSequence.Play()
            .OnComplete(() =>
            {
                action.Invoke();
                bookOpenLight.enabled = false;
                IsAnimating = false;
            });
    }
    
    public PlayableDirector PlayBookOpenTimeLine()
    {
        if (timelineDirector == null)
        {
            OnTimeLineFinished(null);
            return null;
        }

        if (bookOpenTimeLine != null)
        {
            timelineDirector.playableAsset = bookOpenTimeLine;
        }

        IsAnimating = true;
        gameObject.transform.DOLocalRotate(new Vector3(0f, 90f, 0f), 1f, RotateMode.LocalAxisAdd);
        
        timelineDirector.Play();
        return timelineDirector;
    }
    
    public void PlayBookCloseTimeLine()
    {
        if (timelineDirector == null)
        {
            OnTimeLineFinished(null);
            return;
        }

        if (bookOpenTimeLine != null)
        {
            timelineDirector.playableAsset = bookOpenTimeLine;
        }

        IsAnimating = true;
        timelineDirector.time = timelineDirector.duration;
        BookClose();
    }

    private async UniTaskVoid BookClose()
    {
        while (timelineDirector.time > 0)
        {
            timelineDirector.time -= Time.deltaTime;
            timelineDirector.Evaluate();

            await UniTask.Yield();
        }
        
        gameObject.transform.DOLocalRotate(new Vector3(0f, -90f, 0f), 1f, RotateMode.LocalAxisAdd)
            .OnComplete(() =>
            {
                IsAnimating = false;
            });
    }

    private void PlayBookOpenParticles()
    {
        if (bookOpenParticles != null && bookOpenParticles.Length > 0)
        {
            foreach (var particle in bookOpenParticles)
            {
                if (particle != null)
                {
                    particle.Play();
                }
            }
        }
    }

    private void EnableBookOpenLight()
    {
        if (enableLightOnOpen && bookOpenLight != null)
        {
            bookOpenLight.enabled = true;
            bookOpenLight.intensity = 0.1f;

            bookOpenLight.DOIntensity(targetLightIntensity, lightIntensityDuration)
                .SetEase(lightIntensityEase);
        }
    }

    private void PlayBookOpenSFX()
    {
        if (playSFXOnOpen && bookOpenSFX != null)
        {
            bookOpenSFX.Play();
        }
    }
    
    private void OnTimeLineFinished(PlayableDirector director)
    {
        isBookOpened = true;
        IsAnimating = false;
    }
    
    private void OnDestroy()
    {
        if (animationSequence != null)
        {
            animationSequence.Kill();
        }

        transform.DOKill();

        if (bookOpenLight != null)
        {
            bookOpenLight.DOKill();
        }

        if (timelineDirector != null)
        {
            timelineDirector.stopped -= OnTimeLineFinished;
        }
    }

}
