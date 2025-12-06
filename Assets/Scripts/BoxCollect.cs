using UnityEngine;

/// <summary>
/// Handles the logic for collectible items (Light Boxes).
/// Detects when the player touches the object, updates the score, and destroys itself.
/// </summary>
public class BoxCollect : MonoBehaviour
{
    /// <summary>
    /// Triggered when another Collider enters this object's trigger zone.
    /// Ensure the BoxCollider on this object has "Is Trigger" checked.
    /// </summary>
    /// <param name="other">The collider interacting with this box.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object colliding is the Player
        if (other.CompareTag("Player"))
        {
            // Safety check: Ensure GameManager exists before calling it
            if (GameManager.Instance != null)
            {
                // Notify GameManager to increase score
                GameManager.Instance.CollectBox();
            }
            else
            {
                Debug.LogWarning("GameManager is missing! Box collected but not counted.");
            }

            // Remove this box from the scene
            Destroy(gameObject);
        }
    }
}