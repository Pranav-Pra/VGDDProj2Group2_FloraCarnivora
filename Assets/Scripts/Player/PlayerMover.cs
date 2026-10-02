using System.Collections;
using UnityEngine;
using TMPro;

// Moves the plant smoothly to the center of a spot.
public class PlayerMover : MonoBehaviour
{

    #region Serialized Fields
    [Tooltip("The plant can only move once the score is greater than this.")]
    [SerializeField] private int requiredScore = 5;
    [SerializeField] private TMP_Text messageText;
    [Tooltip("time messaeeg stays on screen before clearing.")]
    [SerializeField] private float messageDuration = 2f;
    #endregion

    #region Private Fields
    private Coroutine clearMessageRoutine;
    #endregion

    #region Public Methods
    /// Moves the plant to the target's X/Z position, keeping its own height.
    public void MoveTo(Transform target)
    {
        Vector3 targetPosition = target.position;
        transform.position = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);

    }

    public bool TryUnlock()
    {
        if (!HasEnoughPoints())
        {
            ShowMessage($"Need a score of at least {requiredScore} to unlock.");
            return false;
        }
 
        ScoreCounter.Instance.SpendPoints(requiredScore);
        return true;
    }

    #endregion

    #region Unlock
    private bool HasEnoughPoints()
    {
        return ScoreCounter.Instance != null && ScoreCounter.Instance.Score >= requiredScore;
    }
 

    private void ShowMessage(string message)
    {
        messageText.text = message;
 
        // restart tiner
        if (clearMessageRoutine != null)
        {
            StopCoroutine(clearMessageRoutine);
        }
 
        clearMessageRoutine = StartCoroutine(ClearMessageAfterDelay());
    }
 
    private IEnumerator ClearMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
 
        messageText.text = string.Empty;
        clearMessageRoutine = null;
    }
    #endregion

}
