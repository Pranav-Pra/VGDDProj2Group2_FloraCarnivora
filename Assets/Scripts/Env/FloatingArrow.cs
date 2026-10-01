using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class FloatingArrow : MonoBehaviour
{
     #region Serialized Fields
    [Header("Bobbing")]
    [Tooltip("How far the arrow moves up and down.")]
    [SerializeField] private float bobHeight = 0.15f;
    [Tooltip("How fast the arrow bobs.")]
    [SerializeField] private float bobSpeed = 2f;
 
    [Header("Spinning")]
    [Tooltip("How fast the arrow spins, in degrees per second. Negative spins the other way.")]
    [SerializeField] private float spinSpeed = 90f;
 
    [Header("Sprite")]
    [Tooltip("Extra spin so the arrow points down. Use 180 for an image that points up.")]
    [SerializeField] private float spriteRotation = 0f;
 
    [Header("Hover")]
    [Tooltip("How much bigger the arrow gets when the mouse is over it.")]
    [SerializeField] private float hoverScale = 1.2f;
 
    [Header("Click")]
    [Tooltip("What happens when the arrow is clicked. Hook things up here later.")]
    [SerializeField] private UnityEvent onClicked;
    #endregion

     #region Private Fields
    private Vector3 startPosition;
    private Vector3 startScale;
    private float bobPhase;
    private float spinAngle;
    #endregion
 
    #region Unity Methods
    private void Start()
    {
        startPosition = transform.position;
        startScale = transform.localScale;
        bobPhase = Random.Range(0f, Mathf.PI * 2f); // so the arrows don't bob in sync
    }
 
    private void Update()
    {
        Bob();
        Spin();
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
        Debug.Log($"{name} clicked");
        onClicked.Invoke();
    }
    #endregion

    #region Movement
    private void Bob()
    {
        float offset = Mathf.Sin(Time.time * bobSpeed + bobPhase) * bobHeight;
        transform.position = startPosition + Vector3.up * offset;
    }
 
    private void Spin()
    {
        spinAngle = (spinAngle + spinSpeed * Time.deltaTime) % 360f;
 
        // Spin around the vertical axis, keeping the arrow pointed down
        transform.rotation = Quaternion.Euler(0f, spinAngle, 0f) * Quaternion.Euler(0f, 0f, spriteRotation);
    }
    #endregion
}
