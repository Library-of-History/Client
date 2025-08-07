using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class CoreStone : MonoBehaviour
{
   [Header("돌 속성")] 
   public float durability = 6f; // 내구도
   public GameObject flakePrefab; // 떨어질 격지 프리팹
   public GameObject handAxePrefab; // 완성 주먹도끼 프리팹

   [Header("단계별 외형 설정")] 
   public Mesh[] stageMeshes;
   public Material[] stageMaterials;
   private MeshFilter meshFilter;
   private MeshRenderer meshRenderer;
   public Vector3[] stageScales;
   private int currentStage = 0;
   
   [Header("격지 생성 설정")] 
   public float minFlakingForce = 2f; // 격지가 떨어지는 최소 힘
   public float maxFlakeSize = 0.6f;   // 최대 격지 크기

   [Header("햅틱 설정")]
   public float hapticDurationBase = 0.3f;
   public float hapticDurationMax = 0.3f;
   
   [Header("완성 설정")]
   public Vector3 handAxeSpawnOffset = new Vector3(0f, 0.1f, 0f);
   public bool dropHandAxe = false;
   
   [Header("사운드 설정")]
   public AudioSource audioSource;
   public AudioClip[] impactSounds;
   public AudioClip[] flakingSounds;
   [Range(0f, 1f)] public float volumeBase = 0.5f;
   [Range(0f, 1f)] public float volumeMax = 1f;
   public float pitchVariation = 0.1f;
   
   private Rigidbody rb;
   private XRGrabInteractable grabInteractable;
   private void Start()
   {
      rb = GetComponent<Rigidbody>();
      rb.mass = 50f; // 질량
      rb.linearDamping = 5f; // 공기저항
      rb.angularDamping = 10f; // 회전저항
      rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
      
      grabInteractable = GetComponent<XRGrabInteractable>();
      if (!grabInteractable)
      {
         grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
      }
      
      //Audio
      if (audioSource == null)
      {
         audioSource = SystemManager.Inst.AudioManagerInst.GetSfxSource();
      }
      
      audioSource.playOnAwake = false;
      audioSource.spatialBlend = 0f;
      
      MeshCollider meshCollider = GetComponent<MeshCollider>();
      if (meshCollider)
      {
         meshCollider.convex = true;
      }
      
      meshFilter = GetComponent<MeshFilter>();
      meshRenderer = GetComponent<MeshRenderer>();

      if (meshFilter == null)
      {
         Debug.Log("MeshFilter 없음");
      }
   }

   private void OnCollisionEnter(Collision collision)
   {
      // 망치돌과의 충돌 확인
      if (collision.gameObject.CompareTag("HammerStone"))
      {
         HammerStone hammer = collision.gameObject.GetComponent<HammerStone>();
         if (hammer != null)
         {
            ProcessImpact(hammer, collision);  
         }
      }
   }

   private void ProcessImpact(HammerStone hammer, Collision collision)
   {
      float impactForce = hammer.GetImpactForce();
            
      Debug.Log($"충돌! 충격력: {impactForce}");
            
      //충격력에 따른 사운드 볼륨
      float normalizedForce = Mathf.InverseLerp(0f, 100f, impactForce);
      float volume = Mathf.Lerp(volumeBase, volumeMax, normalizedForce);
      
      // 햅틱 피드백 처리
      if (impactForce >= hammer.minHapticForce)
      {
           // 충격력에 따른 햅틱 강도 / 지속시간
           float hapticIntensity = Mathf.InverseLerp(20f, 100f, impactForce);
           float hapticDuration = Mathf.Lerp(hapticDurationBase, hapticDurationMax, hapticIntensity);
           
           // 망치돌 햅틱 전송
           hammer.SendHapticFeedback(hapticIntensity, hapticDuration);
           
           // 몸돌 햅틱 전송
           SendHapticToSelf(hapticIntensity * 0.5f, hapticDuration * 0.7f);
      }
      
      // 격지 생성 처리 (충분한 힘이 가해질 때만)
      if (impactForce >= minFlakingForce)
      {
         Debug.Log($"격지 생성!");
         ContactPoint contact = collision.contacts[0];
         CreateFlake(contact.point, contact.normal, impactForce);
         
         // 격지 생성 사운드
         PlayFlakingSound(volume);
         
         // 격지 생성 성공 햅틱
         hammer.SendHapticFeedback(0.9f, 0.15f);
         SendHapticToSelf(0.7f, 0.1f);
      }
      else
      {
         // 일반 충돌음
         PlayImpactSound(volume);
      }
   }

   private void PlayImpactSound(float volume)
   {
      if (audioSource != null && impactSounds != null && impactSounds.Length > 0)
      {
         AudioClip clip = impactSounds[Random.Range(0, impactSounds.Length)];
         
         audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
         
         audioSource.PlayOneShot(clip, volume);
      }
   }

   private void PlayFlakingSound(float volume)
   {
      if (audioSource != null && flakingSounds != null && flakingSounds.Length > 0)
      {
         AudioClip clip = flakingSounds[Random.Range(0, flakingSounds.Length)];
         
         audioSource.pitch = 1.1f + Random.Range(-pitchVariation, pitchVariation);
         
         audioSource.PlayOneShot(clip, volume);
      }
   }
   private void SendHapticToSelf(float intensity, float duration)
   {
      if (grabInteractable != null && grabInteractable.isSelected && grabInteractable.interactorsSelecting.Count > 0)
      {
         var interactor = grabInteractable.interactorsSelecting[0];
         HapticUtil.SendHaptic(interactor, intensity, duration);
      }
   }
   
   private void CreateFlake(Vector3 impactPoint, Vector3 impactNormal, float force)
   {
      // 격지 생성
      GameObject flake = Instantiate(flakePrefab, impactPoint, Quaternion.identity);

      // 격지 크기 설정 (힘에 비례)
      float normalizedForce = Mathf.InverseLerp(minFlakingForce, 100f, force);
      float flakeScale = Mathf.Lerp(0.2f, maxFlakeSize, normalizedForce);
      flake.transform.localScale = Vector3.one * flakeScale;

      // 격지에 물리 적용
      Rigidbody flakeRb = flake.GetComponent<Rigidbody>();
      if (flakeRb)
      {
         // 충돌 방향으로 튕겨나가도록 함
         Vector3 flakeDirection = impactNormal + Random.insideUnitSphere * 0.3f;
         flakeRb.linearVelocity = flakeDirection.normalized * force * 0.1f;
         flakeRb.angularVelocity = Random.insideUnitSphere * 10f;
      }
      
      // 내구도 감소
      float previousDurability = durability;
      durability -= force * 0.1f;
      Debug.Log($"격지 생성! 크기 : {flakeScale}, 남은 내구도: {durability}");
      
      // 단계별 외형 변화
      CheckStageTransition(previousDurability);
      
      // 완성
      if (durability <= 0)
      {
         CompleteHandAxe();
      }
   }

   private void CheckStageTransition(float previousDurability)
   {
      if (previousDurability > 4f && durability <= 4f)
      {
         TransitionToStage(1);
      }
      
      else if (previousDurability > 2f && durability <= 2f)
      {
         TransitionToStage(2);
      }
   }

   private void TransitionToStage(int stage)
   {
      currentStage = stage;
      Debug.Log($"CoreStone 단계 {stage}로 전환. (내구도: {durability})");

      if (meshFilter != null && stageMeshes != null && stage - 1 < stageMeshes.Length)
      {
         if (stageMeshes[stage - 1] != null)
         {
            meshFilter.mesh = stageMeshes[stage - 1];
            
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
               meshCollider.sharedMesh = stageMeshes[stage - 1];
            }
         }
      }

      if (meshRenderer != null && stageMaterials != null && stage - 1 < stageMaterials.Length)
      {
         if (stageMaterials[stage - 1] != null)
         {
            meshRenderer.material = stageMaterials[stage - 1];
         }
      }

      if (stageScales != null && stage - 1 < stageScales.Length)
      {  
         Debug.Log("sdisjisjfd");
         transform.localScale = stageScales[stage - 1];
      }
   }
   
   private void CompleteHandAxe()
   {
      Debug.Log("주먹도끼 완성");

      if (handAxePrefab == null) 
      {
         Debug.LogError("Hand Axe Prefab 없음");
         return;
      }

      IXRSelectInteractor savedInteractor = null;
      Transform savedAttachTransform = null;

      if (grabInteractable != null && grabInteractable.isSelected && grabInteractable.interactorsSelecting.Count > 0)
      {
         savedInteractor = grabInteractable.interactorsSelecting[0];
         savedAttachTransform = grabInteractable.GetAttachTransform(savedInteractor);
         
         Debug.Log($"저장된 인터렉터: {(savedInteractor as Component)?.name}");
      }
      
      Vector3 spawnPosition = savedAttachTransform != null ? savedAttachTransform.position : transform.position;
      Quaternion spawnRotation = savedAttachTransform != null ? savedAttachTransform.rotation : transform.rotation;
      GameObject handAxe = Instantiate(handAxePrefab, spawnPosition, spawnRotation);
      
     SetupHandAxe(handAxe);
      
      if (savedInteractor != null && !dropHandAxe)
      {
         XRGrabInteractable handAxeGrab = handAxe.GetComponent<XRGrabInteractable>();
         StartCoroutine(TransferGrab(savedInteractor, grabInteractable, handAxeGrab));
      }
      else
      {
         Rigidbody handAxeRb = handAxe.GetComponent<Rigidbody>();
         if (handAxeRb != null)
         {
            handAxeRb.linearVelocity = Vector3.up * 0.5f + Random.insideUnitSphere * 0.2f;
            handAxeRb.angularVelocity = Random.insideUnitSphere * 2f;
         }
      }

      PlayCompletionEffects(spawnPosition);
      
      Destroy(gameObject, 0.1f);
   }

   private IEnumerator TransferGrab(IXRSelectInteractor interactor, XRGrabInteractable oldGrabbable, XRGrabInteractable newGrabbable)
   {
      if (interactor == null || newGrabbable == null)
      {
         Debug.LogWarning("TransferGrab: interactor 또는 newGrabbable null임.");
         yield break;
      }
      
      // 기존 오브젝트 놓기
      if (oldGrabbable != null && oldGrabbable.interactionManager != null)
      {
         Debug.Log("기존 오브젝트 놓음");
         oldGrabbable.interactionManager.SelectExit(interactor, oldGrabbable);
      }
      
      yield return new WaitForFixedUpdate();

      // 새 오브젝트 잡기
      if (interactor != null && newGrabbable != null && newGrabbable.interactionManager != null)
      {
         Debug.Log("새 오브젝트 잡기 시도");
         newGrabbable.interactionManager.SelectEnter(interactor, newGrabbable);
      }
      
   }

   private void SetupHandAxe(GameObject handAxe)
   {
      Rigidbody handAxeRb = handAxe.GetComponent<Rigidbody>();
      if (handAxeRb == null)
      {
         handAxeRb = handAxe.GetComponent<Rigidbody>();
      }

      // Rigidbody 설정
      handAxeRb.mass = 0.5f;
      handAxeRb.linearDamping = 1f;
      handAxeRb.angularDamping = 1f;
      handAxeRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
      
      // XRGrabInteractable 설정
      XRGrabInteractable handAxeGrab = handAxe.GetComponent<XRGrabInteractable>();
      if (handAxeGrab == null)
      {
         handAxeGrab = handAxe.AddComponent<XRGrabInteractable>();
      }

      if (grabInteractable != null && grabInteractable.interactionManager != null)
      {
         handAxeGrab.interactionManager = grabInteractable.interactionManager;
      }
      
      // Movement Type 설정
      handAxeGrab.movementType = XRGrabInteractable.MovementType.VelocityTracking;
      handAxeGrab.trackPosition = true;
      handAxeGrab.trackRotation = true;
      handAxeGrab.smoothPosition = true;
      handAxeGrab.smoothRotation = true;
      handAxeGrab.smoothPositionAmount = 0.5f;
      handAxeGrab.smoothRotationAmount = 0.5f;
      handAxeGrab.tightenPosition = 0.5f;
      handAxeGrab.tightenRotation = 0.5f;

      handAxeGrab.throwOnDetach = true;
      handAxeGrab.throwSmoothingDuration = 0.25f;
      handAxeGrab.throwSmoothingCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
      handAxeGrab.throwVelocityScale = 1.5f;
      handAxeGrab.throwAngularVelocityScale = 1f;
      
      Collider handAxeCollider = handAxe.GetComponent<Collider>();
      if (handAxeCollider == null)
      {
         MeshFilter meshFilter = handAxe.GetComponent<MeshFilter>();
         if (meshFilter != null && meshFilter.sharedMesh != null)
         {
            MeshCollider meshCollider = handAxe.AddComponent<MeshCollider>();
            meshCollider.convex = true;
         }
         else
         {
            BoxCollider boxCollider = handAxe.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(0.1f, 0.15f, 0.05f);
         }
      }
      

   }
   
   private void PlayCompletionEffects(Vector3 position)
   {
      if (audioSource != null && flakingSounds.Length > 0)
      {
         AudioClip completionSound = flakingSounds[flakingSounds.Length - 1];
         audioSource.PlayOneShot(completionSound, volumeMax);
      }

      if (grabInteractable != null && grabInteractable.isSelected && grabInteractable.interactorsSelecting.Count > 0)
      {
         var interactor = grabInteractable.interactorsSelecting[0];
         HapticUtil.SendHaptic(interactor, 1f, 0.5f);
      }
   }
   
}
