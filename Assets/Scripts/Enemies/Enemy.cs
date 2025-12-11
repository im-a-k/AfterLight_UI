using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

    public float life = 10;
    private bool isPlat;
    private bool isObstacle;
    private Transform fallCheck;
    private Transform wallCheck;
    public LayerMask turnLayerMask;
    private Rigidbody2D rb;

    private bool facingRight = true;
    
    public float speed = 5f;

    public bool isInvincible = false;
    private bool isHitted = false;

    // ---------------------------------------------------------
    // NEW GLITCH VARIABLES
    // ---------------------------------------------------------
    private SpriteRenderer spriteRenderer; 
    [Header("Glitch Effect Settings")]
    [SerializeField] private Color glitchColor = Color.red; // Color during glitch
    [SerializeField] private float glitchDuration = 0.2f;   // Total time of glitch
    [SerializeField] private float glitchFrequency = 0.05f; // Speed of flickering
    // ---------------------------------------------------------

    void Awake () {
        fallCheck = transform.Find("FallCheck");
        wallCheck = transform.Find("WallCheck");
        rb = GetComponent<Rigidbody2D>();
        
        // Initialize SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    // Update is called once per frame
    void FixedUpdate () {

        if (life <= 0) {
            transform.GetComponent<Animator>().SetBool("IsDead", true);
            StartCoroutine(DestroyEnemy());
        }

        isPlat = Physics2D.OverlapCircle(fallCheck.position, .2f, 1 << LayerMask.NameToLayer("Default"));
        isObstacle = Physics2D.OverlapCircle(wallCheck.position, .2f, turnLayerMask);

        if (!isHitted && life > 0 && Mathf.Abs(rb.linearVelocity.y) < 0.5f)
        {
            if (isPlat && !isObstacle && !isHitted)
            {
                if (facingRight)
                {
                    rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
                }
                else
                {
                    rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
                }
            }
            else
            {
                Flip();
            }
        }
    }

    void Flip (){
        // Switch the way the player is labelled as facing.
        facingRight = !facingRight;
        
        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void ApplyDamage(float damage) {
        if (!isInvincible) 
        {
            float direction = damage / Mathf.Abs(damage);
            damage = Mathf.Abs(damage);
            transform.GetComponent<Animator>().SetBool("Hit", true);
            life -= damage;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(direction * 500f, 100f));
            
            StartCoroutine(HitTime());

            // TRIGGER THE GLITCH EFFECT HERE
            if(spriteRenderer != null) StartCoroutine(GlitchEffect());
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && life > 0)
        {
            collision.gameObject.GetComponent<CharacterController2D>().ApplyDamage(2f, transform.position);
        }
    }

    IEnumerator HitTime()
    {
        isHitted = true;
        isInvincible = true;
        yield return new WaitForSeconds(0.1f);
        isHitted = false;
        isInvincible = false;
    }

    IEnumerator DestroyEnemy()
    {
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(1f, 0.25f);
        capsule.offset = new Vector2(0f, -0.8f);
        capsule.direction = CapsuleDirection2D.Horizontal;
        yield return new WaitForSeconds(0.25f);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }

    // ---------------------------------------------------------
    // NEW COROUTINE: GLITCH EFFECT
    // ---------------------------------------------------------
    IEnumerator GlitchEffect()
    {
        Color originalColor = spriteRenderer.color;
        float timer = 0f;

        while (timer < glitchDuration)
        {
            // Randomly choose a render state
            // 0 = Normal, 1 = Glitch Color, 2 = Transparent
            int state = Random.Range(0, 3);

            switch (state)
            {
                case 0:
                    spriteRenderer.color = originalColor;
                    break;
                case 1:
                    spriteRenderer.color = glitchColor;
                    break;
                case 2:
                    // Set alpha to 0 for a "missing" frame effect
                    spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
                    break;
            }

            yield return new WaitForSeconds(glitchFrequency);
            timer += glitchFrequency;
        }

        // Ensure we reset to normal at the end
        spriteRenderer.color = originalColor;
    }
}