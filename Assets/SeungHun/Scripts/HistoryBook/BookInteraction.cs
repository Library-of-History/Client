using UnityEngine;
using DG.Tweening;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.XR.Interaction.Toolkit;
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
    private bool isAnimating = false;
    private bool isBookOpened = false;
    
    private Sequence animationSequence;
    private XRSimpleInteractable simpleInteractable;
    
    private void Start()
    {
        InitializeComponents();
        StoreOriginalTransform();
        SetupInteractionEvents();
        InitializeLights();
    }

    private void InitializeComponents()
    {
        if (autoFindComponents)
        {
            AutoFindRequiredComponents();
        }

        if (simpleInteractable == null)
        {
            simpleInteractable = GetComponent<XRSimpleInteractable>();
        }
        
        RegisterWithInteractionManager();
    }

    private void AutoFindRequiredComponents()
    {
        if (vrCamera == null)
        {
            vrCamera = Camera.main?.transform;

            if (vrCamera == null)
            {
                var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
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
    
    private void RegisterWithInteractionManager()
    {
        if (simpleInteractable != null)
        {
            var interactionManager = FindFirstObjectByType<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();
            if (interactionManager != null)
            {
                interactionManager.RegisterInteractable((IXRInteractable)simpleInteractable);
            }
        }
    }
    
    private void StoreOriginalTransform()
    {
        originalPosition = transform.position;
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
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.AddListener(OnBookTouched);
        }

        if (timelineDirector != null)
        {
            timelineDirector.stopped += OnTimeLineFinished;
        }
    }
    
    public void OnBookTouched(SelectEnterEventArgs args)
    {
        if (CanStartAnimation())
        {
            StartBookInteractionSequence();   
        }
    }
    
    private bool CanStartAnimation()
    {
        return !isAnimating && !isBookOpened;
    }

    private void StartBookInteractionSequence()
    {
        if (vrCamera == null)
        {
            return;
        }
        
        isAnimating = true;
        
        if (animationSequence != null)
        {
            animationSequence.Kill();
        }
        
        animationSequence = DOTween.Sequence();
        
        Vector3 targetWorldPosition = vrCamera.TransformPoint(targetLocalPosition);
        Quaternion targetWorldRotation = vrCamera.rotation * Quaternion.Euler(targetRotation);  
        
        animationSequence.Append(transform.DOMove(targetWorldPosition, moveRotationDuration).SetEase(movementEase));
        animationSequence.Join(transform.DORotateQuaternion(targetWorldRotation, moveRotationDuration).SetEase(rotationEase));

        animationSequence.AppendCallback(PlayBookOpenTimeLine);

        animationSequence.Play();
    }
    
    private void PlayBookOpenTimeLine()
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
        
        PlayBookOpenParticles();
        EnableBookOpenLight();
        PlayBookOpenSFX();
        
        timelineDirector.Play();
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
        isAnimating = false;
        
        TriggerSceneTransition();
    }

    private void TriggerSceneTransition()
    {
        if (AutoExposureFade.Instance != null)
        {
            if (useSceneTransition && !string.IsNullOrEmpty(nextSceneName))
            {
                AutoExposureFade.Instance.TransitionToScene(nextSceneName);
            }
        }
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
        
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.RemoveListener(OnBookTouched);
        }

        if (timelineDirector != null)
        {
            timelineDirector.stopped -= OnTimeLineFinished;
        }
    }

}
