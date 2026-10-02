
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class VineController : MonoBehaviour
{
    #region Serialized Fields
 
    [Header("Images")]
    [Tooltip("idle vine.")]
    [SerializeField] private Sprite idleSprite;
    [Tooltip("vine smacking the ground.")]
    [SerializeField] private Sprite smackSprite;
 
    [Header("Smack")]
    [SerializeField] private KeyCode smackKey = KeyCode.Space;
    [Tooltip("Seconds the smack image stays on screen.")]
    [SerializeField] private float smackDuration = 0.25f;
    #endregion

    #region Private Fields
    private Image vineImage;
    private bool isSmacking;
    #endregion
 
    #region Unity Methods
    private void Awake()
    {
        vineImage = GetComponent<Image>();
        vineImage.sprite = idleSprite;
    }
 
    private void Update()
    {
        if (Input.GetKeyDown(smackKey) && !isSmacking)
        {
            StartCoroutine(Smack());
        }
    }
    #endregion

    #region Smack
    private IEnumerator Smack()
    {
        isSmacking = true;
        vineImage.sprite = smackSprite;
 
        yield return new WaitForSeconds(smackDuration);
 
        vineImage.sprite = idleSprite;
        isSmacking = false;
    }
    #endregion
}
