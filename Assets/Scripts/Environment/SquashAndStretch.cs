using UnityEngine;
using System.Collections;

public class SquashAndStretch : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Amount to scale X and Y during squash (e.g., Wider X, Shorter Y)")]
    public Vector2 squashScale = new Vector2(1.3f, 0.7f);
    
    [Tooltip("Amount to scale X and Y during stretch (e.g., Thinner X, Taller Y)")]
    public Vector2 stretchScale = new Vector2(0.7f, 1.3f);
    
    [Tooltip("How fast the animation plays")]
    public float speed = 5.0f;

    private Vector3 originalScale;
    private bool isAnimating = false;

    void Start()
    {
        // Remember the object's starting size
        originalScale = transform.localScale;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object colliding is the Player
        // Make sure your Player object has the tag "Player"
        if (other.CompareTag("Player") && !isAnimating)
        {
            StartCoroutine(SquashStretchRoutine());
        }
    }

    IEnumerator SquashStretchRoutine()
    {
        isAnimating = true;

        // Phase 1: Squash
        yield return StartCoroutine(AnimateScale(squashScale));

        // Phase 2: Stretch
        yield return StartCoroutine(AnimateScale(stretchScale));

        // Phase 3: Return to Normal
        yield return StartCoroutine(AnimateScale(originalScale));

        isAnimating = false;
    }

    // A helper function to smooth the transition between scales
    IEnumerator AnimateScale(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            // Lerp calculates the point between start and target based on 't'
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null; // Wait for next frame
        }

        // Ensure we end up exactly at the target
        transform.localScale = targetScale;
    }
}