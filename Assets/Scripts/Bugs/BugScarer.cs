using UnityEngine;

public class BugScarer : MonoBehaviour
{
    #region Serialized Fields
    [Tooltip("Bugs within this distance of the click scatter.")]
    [SerializeField] private float scareRadius = 1.5f;
    #endregion
 
    #region Unity Methods
    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }
 
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
 
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            foreach (Collider nearby in Physics.OverlapSphere(hit.point, scareRadius))
            {
                if (nearby.TryGetComponent(out Bug bug))
                {
                    bug.Scare(hit.point);
                }
            }
        }
    }
    #endregion
}
