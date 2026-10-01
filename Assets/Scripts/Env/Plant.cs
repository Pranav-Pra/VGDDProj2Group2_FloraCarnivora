using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Plant : MonoBehaviour
{
    #region Serialized Fields
    [Tooltip("hide the plant when the player is closer than this (standing on its spot).")]
    [SerializeField] private float hideDistance = 1f;
    [Tooltip("The PlayerMover on the player. Filled in automatically for plants created by arrows.")]
    [SerializeField] private PlayerMover playerMover;
    [Tooltip("The spot this plant stands on. Filled in automatically for plants created by arrows.")]
    [SerializeField] private Transform spot;

    [Header("Hover")]
    [Tooltip("How much bigger the arrow gets when the mouse is over it.")]
    [SerializeField] private float hoverScale = 1.2f;
    #endregion
 
    #region Private Fields
    private Transform cameraTransform;
    private Renderer plantRenderer;
    private Collider plantCollider;
    private Vector3 startScale;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        plantRenderer = GetComponent<Renderer>();
        plantCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        startScale = transform.localScale;
        cameraTransform = Camera.main.transform;
    }
 
    private void LateUpdate()
    {
        FaceCamera();
        UpdateVisibility();
    }

    private void OnMouseEnter()
    {
        Debug.Log($"{name} hovered");
        transform.localScale = startScale * hoverScale;
    }
 
    private void OnMouseExit()
    {
        transform.localScale = startScale;
    }
 
    private void OnMouseDown()
    {
        playerMover.MoveTo(spot);
    }
    #endregion

    #region Setup
    public void Initialize(PlayerMover mover, Transform targetSpot)
    {
        playerMover = mover;
        spot = targetSpot;
    }
    #endregion

    #region Billboarding
    private void FaceCamera()
    {
        // Face toward the camera's position
        Vector3 awayFromCamera = transform.position - cameraTransform.position;
        awayFromCamera.y = 0f; // stay upright

 
        if (awayFromCamera.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(awayFromCamera);
        }
    }
 
    private void UpdateVisibility()
    {
        Vector3 offset = cameraTransform.position - transform.position;
        offset.y = 0f;
 
        bool isVisible = offset.magnitude > hideDistance;
 
        plantRenderer.enabled = isVisible;
        plantCollider.enabled = isVisible;
    }
    #endregion
}
