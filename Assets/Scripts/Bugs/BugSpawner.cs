using UnityEngine;
using System.Collections.Generic;

/// Spawns bugs at random spots on the floor in a ring around the plant,
/// skipping the minigame circles.
public class BugSpawner : MonoBehaviour
{
    #region Serialized Fields
    [Header("Bug")]
    [SerializeField] private Bug bugPrefab;
 
    [Header("Spawn Area")]
    [Tooltip("Center of the spawn area (drag the Plant here).")]
    [SerializeField] private Transform center;
    [Tooltip("Bugs won't spawn closer than this to the plant.")]
    [SerializeField] private float minRadius = 1f;
    [Tooltip("Bugs won't spawn farther than this from the plant.")]
    [SerializeField] private float maxRadius = 5.5f;
    [Tooltip("Y position of the floor's top surface.")]
    [SerializeField] private float floorHeight = 0.5f;
 
    [Header("Excluded Spots")]
    [Tooltip("Drag the 3 minigame circles here.")]
    [SerializeField] private Transform[] excludedSpots;
    [Tooltip("Radius of each circle (half its X scale).")]
    [SerializeField] private float spotRadius = 1.5f;
    [Tooltip("Extra gap so bugs don't overlap a circle's edge.")]
    [SerializeField] private float spotPadding = 0.3f;
 
    [Header("Timing")]
    [Tooltip("Seconds between spawns.")]
    [SerializeField] private float spawnInterval = 1.5f;
    [Tooltip("Most bugs allowed at once.")]
    [SerializeField] private int maxBugs = 8;
    #endregion

    #region Private Fields
    private const int MaxSpawnAttempts = 30;
 
    private readonly List<Bug> activeBugs = new List<Bug>();
    private float spawnTimer;
    #endregion
 
    #region Unity Methods
    private void Update()
    {
        spawnTimer += Time.deltaTime;
 
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnBug();
        }
    }
 
    private void OnDrawGizmosSelected()
    {
        // Shows the spawn area in the Scene view when this object is selected
        if (center == null)
        {
            return;
        }
 
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center.position, maxRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center.position, minRadius);
 
        if (excludedSpots == null)
        {
            return;
        }
 
        Gizmos.color = Color.red;
        foreach (Transform spot in excludedSpots)
        {
            if (spot != null)
            {
                Gizmos.DrawWireSphere(spot.position, spotRadius + spotPadding);
            }
        }
    }
    #endregion

    #region Spawning
    private void TrySpawnBug()
    {
        // remove busgs that are clicked
        activeBugs.RemoveAll(bug => bug == null);
 
        if (activeBugs.Count >= maxBugs)
        {
            return;
        }
 
        if (TryGetSpawnPoint(out Vector3 point))
        {
            Bug bug = Instantiate(bugPrefab, point, Quaternion.identity, transform);
            activeBugs.Add(bug);
        }
    }
 
    private bool TryGetSpawnPoint(out Vector3 point)
    {
        // pick random points until isn't on a circle
        for (int i = 0; i < MaxSpawnAttempts; i++)
        {
            Vector2 offset = RandomPointInRing();
            point = new Vector3(center.position.x + offset.x,
                                floorHeight,
                                center.position.z + offset.y);
 
            if (!IsOnSpot(point))
            {
                return true;
            }
        }
 
        point = Vector3.zero;
        return false;
    }
 
    private Vector2 RandomPointInRing()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
 
        // square root spreads bugs evenly instead of bunching near the center
        float radius = Mathf.Sqrt(Random.Range(minRadius * minRadius, maxRadius * maxRadius));
 
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }
 
    private bool IsOnSpot(Vector3 point)
    {
        float blockedRadius = spotRadius + spotPadding;
 
        foreach (Transform spot in excludedSpots)
        {
            Vector2 difference = new Vector2(point.x - spot.position.x,
                                             point.z - spot.position.z);
 
            if (difference.sqrMagnitude < blockedRadius * blockedRadius)
            {
                return true;
            }
        }
 
        return false;
    }
    #endregion
}
