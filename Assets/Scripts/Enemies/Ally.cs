using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ally : MonoBehaviour
{
    private Rigidbody2D m_Rigidbody2D;
    private SpriteRenderer m_SpriteRenderer; // Reference to SpriteRenderer
    private bool m_FacingRight = true;  
    public float life = 10;
    private bool facingRight = true;
    public float speed = 5f; 
    public bool isInvincible = false;
    private bool isHitted = false;
    [SerializeField] private float m_DashForce = 25f;
    private bool isDashing = false;
    public GameObject enemy;
    private float distToPlayer;
    private float distToPlayerY;
    public float meleeDist = 1.5f;
    public float rangeDist = 5f;
    private bool canAttack = true;
    private Transform attackCheck;
    public float dmgValue = 4;
    public GameObject throwableObject;
    private float randomDecision = 0;
    private bool doOnceDecision = true;
    private bool endDecision = false;
    private Animator anim;

    [Header("AI Settings")]
    [SerializeField] private float detectionRadius = 10f; 
    [SerializeField] private bool enableRangedAttack = true; 

    // ---------------------------------------------------------
    // NEW GLITCH VFX SETTINGS
    // ---------------------------------------------------------
    [Header("Glitch FX Settings")]
    [SerializeField] private Color glitchColor = new Color(1f, 0f, 0f, 0.6f); // Reddish tint
    [SerializeField] private float glitchDuration = 0.5f; // How long the glitch lasts
    [SerializeField] private float glitchFrequency = 0.05f; // How fast it flickers
    // ---------------------------------------------------------

    [Header("Hit Stop Settings")]
    [SerializeField] private float hitStopDuration = 0.1f; 
    private bool isHitStopping = false;
   
    void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_SpriteRenderer = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer
        attackCheck = transform.Find("AttackCheck").transform;
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (life <= 0)
        {
            StartCoroutine(DestroyEnemy());
        }
        else if (enemy != null) 
        {
            if (isDashing)
            {
                m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * m_DashForce, 0);
            }
            else if (!isHitted)
            {
                float actualDistance = Vector2.Distance(transform.position, enemy.transform.position);

                if (actualDistance > detectionRadius)
                {
                    m_Rigidbody2D.linearVelocity = new Vector2(0f, m_Rigidbody2D.linearVelocity.y);
                    anim.SetBool("IsWaiting", true);
                    return; 
                }

                distToPlayer = enemy.transform.position.x - transform.position.x;
                distToPlayerY = enemy.transform.position.y - transform.position.y;

                if (Mathf.Abs(distToPlayer) < 0.10f)
                {
                    GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0f, m_Rigidbody2D.linearVelocity.y);
                    anim.SetBool("IsWaiting", true);
                }
                else if (Mathf.Abs(distToPlayer) > 0.10f && Mathf.Abs(distToPlayer) < meleeDist && Mathf.Abs(distToPlayerY) < 2f)
                {
                    GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0f, m_Rigidbody2D.linearVelocity.y);
                    if ((distToPlayer > 0f && transform.localScale.x < 0f) || (distToPlayer < 0f && transform.localScale.x > 0f)) 
                        Flip();
                    if (canAttack)
                    {
                        MeleeAttack();
                    }
                }
                else if (Mathf.Abs(distToPlayer) > meleeDist && Mathf.Abs(distToPlayer) < rangeDist)
                {
                    anim.SetBool("IsWaiting", false);
                    m_Rigidbody2D.linearVelocity = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * speed, m_Rigidbody2D.linearVelocity.y);
                }
                else
                {
                    if (!endDecision)
                    {
                        if ((distToPlayer > 0f && transform.localScale.x < 0f) || (distToPlayer < 0f && transform.localScale.x > 0f)) 
                            Flip();

                        if (randomDecision < 0.4f)
                            Run();
                        else if (randomDecision >= 0.4f && randomDecision < 0.6f)
                            Jump();
                        else if (randomDecision >= 0.6f && randomDecision < 0.8f)
                            StartCoroutine(Dash());
                        else if (randomDecision >= 0.8f && randomDecision < 0.95f && enableRangedAttack)
                            RangeAttack();
                        else
                            Idle();
                    }
                    else
                    {
                        endDecision = false;
                    }
                }
            }
            else if (isHitted)
            {
                if ((distToPlayer > 0f && transform.localScale.x > 0f) || (distToPlayer < 0f && transform.localScale.x < 0f))
                {
                    Flip();
                    StartCoroutine(Dash());
                }
                else
                    StartCoroutine(Dash());
            }
        }
        else 
        {
            enemy = GameObject.Find("Player");
        }

        if (transform.localScale.x * m_Rigidbody2D.linearVelocity.x > 0 && !m_FacingRight && life > 0)
        {
            Flip();
        }
        else if (transform.localScale.x * m_Rigidbody2D.linearVelocity.x < 0 && m_FacingRight && life > 0)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void ApplyDamage(float damage)
    {
        if (!isInvincible)
        {
            float direction = damage / Mathf.Abs(damage);
            damage = Mathf.Abs(damage);
            anim.SetBool("Hit", true);
            life -= damage;
            transform.gameObject.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, 0);
            transform.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(direction * 300f, 100f)); 
            
            // Start the logic handling the Hit State
            StartCoroutine(HitTime());

            // Start the Visual Glitch Effect separately
            StartCoroutine(GlitchEffect());
        }
    }

    public void MeleeAttack()
    {
        transform.GetComponent<Animator>().SetBool("Attack", true);
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, 0.9f);
        
        bool hitSomething = false; 

        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Enemy" && collidersEnemies[i].gameObject != gameObject )
            {
                if (transform.localScale.x < 1)
                {
                    dmgValue = -dmgValue;
                }
                collidersEnemies[i].gameObject.SendMessage("ApplyDamage", dmgValue);
                hitSomething = true;
            }
            else if (collidersEnemies[i].gameObject.tag == "Player")
            {
                collidersEnemies[i].gameObject.GetComponent<CharacterController2D>().ApplyDamage(2f, transform.position);
                hitSomething = true;
            }
        }

        if (hitSomething && !isHitStopping)
        {
            StartCoroutine(HitStop());
        }

        StartCoroutine(WaitToAttack(0.5f));
    }

    public void RangeAttack()
    {
        if (doOnceDecision)
        {
            GameObject throwableProj = Instantiate(throwableObject, transform.position + new Vector3(transform.localScale.x * 0.5f, -0.2f), Quaternion.identity) as GameObject;
            throwableProj.GetComponent<ThrowableProjectile>().owner = gameObject;
            Vector2 direction = new Vector2(transform.localScale.x, 0f);
            throwableProj.GetComponent<ThrowableProjectile>().direction = direction;
            StartCoroutine(NextDecision(0.5f));
        }
    }

    // ... (Run, Jump, Idle, EndDecision, Dash, NextDecision, DestroyEnemy unchanged)
    public void Run()
    {
        anim.SetBool("IsWaiting", false);
        m_Rigidbody2D.linearVelocity = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * speed, m_Rigidbody2D.linearVelocity.y);
        if (doOnceDecision)
            StartCoroutine(NextDecision(0.5f));
    }
    public void Jump()
    {
        Vector3 targetVelocity = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * speed, m_Rigidbody2D.linearVelocity.y);
        Vector3 velocity = Vector3.zero;
        m_Rigidbody2D.linearVelocity = Vector3.SmoothDamp(m_Rigidbody2D.linearVelocity, targetVelocity, ref velocity, 0.05f);
        if (doOnceDecision)
        {
            anim.SetBool("IsWaiting", false);
            m_Rigidbody2D.AddForce(new Vector2(0f, 850f));
            StartCoroutine(NextDecision(1f));
        }
    }

    public void Idle()
    {
        m_Rigidbody2D.linearVelocity = new Vector2(0f, m_Rigidbody2D.linearVelocity.y);
        if (doOnceDecision)
        {
            anim.SetBool("IsWaiting", true);
            StartCoroutine(NextDecision(1f));
        }
    }

    public void EndDecision()
    {
        randomDecision = Random.Range(0.0f, 1.0f); 
        endDecision = true;
    }

    IEnumerator WaitToAttack(float time)
    {
        canAttack = false;
        yield return new WaitForSeconds(time);
        canAttack = true;
    }

    IEnumerator Dash()
    {
        anim.SetBool("IsDashing", true);
        isDashing = true;
        yield return new WaitForSeconds(0.1f);
        isDashing = false;
        EndDecision();
    }

    IEnumerator NextDecision(float time)
    {
        doOnceDecision = false;
        yield return new WaitForSeconds(time);
        EndDecision();
        doOnceDecision = true;
        anim.SetBool("IsWaiting", false);
    }

    IEnumerator DestroyEnemy()
    {
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(1f, 0.25f);
        capsule.offset = new Vector2(0f, -0.8f);
        capsule.direction = CapsuleDirection2D.Horizontal;
        transform.GetComponent<Animator>().SetBool("IsDead", true);
        yield return new WaitForSeconds(0.25f);
        m_Rigidbody2D.linearVelocity = new Vector2(0, m_Rigidbody2D.linearVelocity.y);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    IEnumerator HitTime()
    {
        isInvincible = true;
        isHitted = true;
        yield return new WaitForSeconds(0.1f);
        isHitted = false;
        isInvincible = false;
    }

    IEnumerator HitStop()
    {
        isHitStopping = true;
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = originalTimeScale;
        isHitStopping = false;
    }

    // ---------------------------------------------------------
    // NEW COROUTINE: GLITCH EFFECT
    // ---------------------------------------------------------
    IEnumerator GlitchEffect()
    {
        float timer = 0f;
        
        // Store original color to reset later
        Color originalColor = Color.white; 
        if(m_SpriteRenderer != null) originalColor = m_SpriteRenderer.color;

        while (timer < glitchDuration && m_SpriteRenderer != null)
        {
            // Randomly choose a state: 
            // 0 = Normal
            // 1 = Glitch Color (Red/White)
            // 2 = Invisible (Simulates glitchy rendering)
            int rnd = Random.Range(0, 3);

            if (rnd == 0)
            {
                m_SpriteRenderer.color = originalColor;
            }
            else if (rnd == 1)
            {
                m_SpriteRenderer.color = glitchColor;
            }
            else if (rnd == 2)
            {
                // Make it invisible or semi-transparent
                m_SpriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            }

            // Wait for a tiny fraction of a second
            yield return new WaitForSeconds(glitchFrequency);
            timer += glitchFrequency;
        }

        // Ensure we reset to normal visibility at the end
        if(m_SpriteRenderer != null) m_SpriteRenderer.color = originalColor;
    }
}