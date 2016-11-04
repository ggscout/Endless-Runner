using UnityEngine;
using System.Collections.Generic;   // not just System.Collections

/// <summary>
/// Maintains a set of platforms and places then as they're needed.
///
/// IMPORTANT: this must be placed in the player character,
/// not in its own object.
/// </summary>
[RequireComponent(typeof(Runner))]
public class PlatformManager : MonoBehaviour {
    #region Parameters editable in Unity
    /// <summary>
    /// Prefab to use for creating new platforms
    /// </summary>
    public GameObject PlatformPrefab;

    /// <summary>
    /// Spawn a new platform when the last platform is this far ahead of the player.
    /// </summary>
    public float SpawnAtDistance = 50f;

    // Platforms are created with random width in the range [MinPlatformWidth, MaxPlatformWidth]
    public float MinPlatformWidth = 20f;
    public float MaxPlatformWidth = 30f;

    // Platforms are spawned with a horizontal separation in the range [MinXSpacing, MaxXSpacing]
    public float MinXSpacing = 35f;
    public float MaxXSpacing = 45f;

    // Platforms are spawned with a vertical spacing in the range [MinYSpacing, MaxYSpacing]
    public float MinYSpacing = -8f;
    public float MaxYSpacing = 8f;

    /// <summary>
    /// Platforms will be recycled when the player is this distance past them.
    /// </summary>
    public float RecycleDistance = 50f;
    #endregion

    #region Internal state of the platform pool
    /// <summary>
    /// Location at which the next platform will be spawned
    /// </summary>
    private Vector3 nextSpawnPoint = new Vector3 (0f, -6.5f, 0f);

    /// <summary>
    /// The platforms that are currently on screen.
    /// </summary>
    private readonly Queue<GameObject> platformsInUse = new Queue<GameObject> ();
    /// <summary>
    /// A pool of platforms that have been instantiated in the past but that aren't
    /// currently needed.  We save these, rather than destroying them so that we can
    /// avoid destorying and recreating platforms, which is somewhat expensive.
    /// </summary>
    private readonly Queue<GameObject> unusedPlatforms = new Queue<GameObject> ();
    #endregion

    /// <summary>
    /// True when the player hasn't lost yet, i.e. when the game isn't over.
    /// </summary>
    private bool playerNotDead = true;

    /// <summary>
    /// Initialize
    /// </summary>
    internal void Start(){
        FindObjectOfType<Runner>().FellIntoTheVoid += OnGameOver;
        SpawnNewPlatform();
    }

    /// <summary>
    /// Returns the platform that's been on screen for the longest time,
    /// aka the rearmost platform that's on screen.
    /// </summary>
    private GameObject OldestPlatformInUse
    {
        get { return platformsInUse.Peek(); }
    }

    /// <summary>
    /// Remove an unused platform from the pool, place it, and activate it.
    /// </summary>
    private void SpawnNewPlatform() {
        GameObject platform;
        if (unusedPlatforms.Count > 0) {
            platform = unusedPlatforms.Dequeue();
            platform.SetActive(true);
        } else {
            platform = Instantiate(PlatformPrefab);
        }

        platformsInUse.Enqueue(platform);
        MovePlatformToSpawnPoint(platform);
    }

    /// <summary>
    /// Called when the game is over.
    /// </summary>
    private void OnGameOver(){
        // Clear playerNotDead
        playerNotDead = false;
    }

    /// <summary>
	/// Update platforms if the player is not dead
	/// </summary>
	internal void Update() {
		if (playerNotDead) {
			if (transform.position.x - OldestPlatformInUse.transform.position.x > RecycleDistance) {
				RecycleOldestPlatform ();
			}
			if (nextSpawnPoint.x - transform.position.x < SpawnAtDistance) {
				SpawnNewPlatform ();
			}
		}
	}

	/// <summary>
	/// remove the oldest platform and add it to the pool
	/// deactivate it
	/// </summary>
	private void RecycleOldestPlatform () {
		GameObject oldestPlatform = platformsInUse.Dequeue();
		unusedPlatforms.Enqueue (oldestPlatform);
		oldestPlatform.SetActive (false);
	}

	/// <summary>
	/// Moves the platform to spawn point.
	/// </summary>
	/// <param name="platform">Platform</param>
	private void MovePlatformToSpawnPoint (GameObject platform) {
		platform.transform.position = nextSpawnPoint;
		float width = Random.Range (MinPlatformWidth, MaxPlatformWidth);
		platform.transform.localScale = new Vector3 (width, 1, 1);
		nextSpawnPoint.x += Random.Range (MinXSpacing, MaxXSpacing);
		nextSpawnPoint.y += Random.Range (MinYSpacing, MaxYSpacing);
	}

}
