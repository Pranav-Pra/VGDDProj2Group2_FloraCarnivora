using UnityEngine;


public class PlayerController : MonoBehaviour
{
     #region Serialized Fields
    [Header("Rotation")]
    [Tooltip("How fast the plant turns")]
    [SerializeField] private float rotationSpeed = 120f;
 
    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Tooltip("Fixed downward tilt of the view")]
    [SerializeField] private float cameraTilt = 10f;
    #endregion

    #region Unity Methods
    private void Start()
    {
        cameraTransform.localRotation = Quaternion.Euler(cameraTilt, 0f, 0f);
    }
 
    private void Update()
    {
        HandleRotation();
    }
    #endregion

    #region Rotation
    private void HandleRotation()
    {
        float input = Input.GetAxisRaw("Horizontal");
        transform.Rotate(Vector3.up, input * rotationSpeed * Time.deltaTime);
    }
    #endregion
}



