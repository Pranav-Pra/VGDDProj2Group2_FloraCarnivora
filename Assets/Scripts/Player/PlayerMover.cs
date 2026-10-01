using System.Collections;
using UnityEngine;

// Moves the plant smoothly to the center of a spot.

public class PlayerMover : MonoBehaviour
{
    #region Public Methods
    /// Moves the plant to the target's X/Z position, keeping its own height.
    public void MoveTo(Transform target)
    {
        Vector3 targetPosition = target.position;
        transform.position = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
    }
 
    #endregion

}
