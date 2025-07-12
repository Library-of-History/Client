using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
// Interactables namespace is good. Interactors namespace might not be explicitly needed if only using XRBaseInteractor.
// using UnityEngine.XR.Interaction.Toolkit.Interactors; // This using might not be strictly necessary, but doesn't hurt.

public class StringInteraction : XRBaseInteractable
{
    [SerializeField] public Transform stringStartPoint;
    [SerializeField] public Transform stringEndPoint;

    // Use IXRInteractor instead of XRBaseInteractor for better interface-based coding
    private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor _stringInteractor = null; 
    
    // Member variables are fine as they are.
    private Vector3 _pullPosition;
    private Vector3 _pullDirection;
    private Vector3 _targetDirection;

    public float PullAmount { get; private set; } = 0.0f;

    // Use world positions for these properties if they are meant to be accessed as such,
    // or clarify local vs. world based on usage. Current usage in CalculatePull implies world.
    // If you need local position, stringStartPoint.localPosition is correct.
    // For consistency and typical usage in physics/vector math, world positions are often preferred.
    // Let's assume you intend for these to be world positions based on CalculatePull.
    public Vector3 stringStartWorldPosition { get => stringStartPoint.position; }
    public Vector3 stringEndWorldPosition { get => stringEndPoint.position; }

    protected override void Awake()
    {
        base.Awake(); // Ensure base.Awake() is called first. Good as is.
    }

    // XR Interaction Toolkit often uses IXRSelectInteractor now for selection.
    // Using args.interactorObject is good. Casting to IXRInteractor is safe.
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        // Use args.interactorObject which is IXRSelectInteractor.
        // We cast to IXRInteractor to maintain the field type.
        this._stringInteractor = args.interactorObject; 
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        this._stringInteractor = null;
        this.PullAmount = 0f;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
        
        // This is a common and correct pattern for updating interactables in Unity 6.
        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic && isSelected)
        {
            // Ensure stringInteractor is not null before accessing its transform.
            if (this._stringInteractor != null) 
            {
                this._pullPosition = this._stringInteractor.transform.position;
                this.PullAmount = CalculatePull(this._pullPosition);
                //Debug.Log("<<<<< Pull amount is "+ PullAmount+" >>>>>");
            }
        }           
    }

    private float CalculatePull(Vector3 currentPullWorldPosition) // Renamed for clarity
    {
        // Use stringStartPoint.position (world position) for calculations.
        // The previous code was already doing this, which is correct for world-space calculations.
        this._pullDirection = currentPullWorldPosition - stringStartPoint.position;
        this._targetDirection = stringEndPoint.position - stringStartPoint.position;
        float maxLength = _targetDirection.magnitude;
        
        // Handle division by zero for maxLength if points are at the same location.
        if (maxLength < Mathf.Epsilon) // Use Mathf.Epsilon for float comparisons near zero.
        {
            return 0f;
        }

        _targetDirection.Normalize();

        float pullValue = Vector3.Dot(_pullDirection, _targetDirection) / maxLength;
        return Mathf.Clamp(pullValue, 0, 1);
    }
}