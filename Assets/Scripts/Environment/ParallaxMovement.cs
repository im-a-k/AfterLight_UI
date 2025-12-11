using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Tooltip("Speed at which the background moves toward the left (-x axis).")]
    [SerializeField] private float moveSpeed = 5f; 
    
    private float backgroundImageWidth;
    private Vector3 startPosition;

    void Start()
    {
        // Store the starting position to create an anchor point
        startPosition = transform.position;

        // Get the real world width of the sprite (accounts for object scaling)
        backgroundImageWidth = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        // 1. Calculate movement amount (Speed * Time)
        float distanceToMove = moveSpeed * Time.deltaTime;

        // 2. Move the object to the left (-X axis)
        transform.Translate(Vector3.left * distanceToMove);

        // 3. Check if object has moved past its width relative to the start position
        if (transform.position.x < startPosition.x - backgroundImageWidth)
        {
            ResetPosition();
        }
    }

    void ResetPosition()
    {
        // 4. Loop back by adding the width to the current position
        // This maintains the offset for a perfect seamless transition
        transform.position += new Vector3(backgroundImageWidth, 0, 0);
    }
}