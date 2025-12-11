using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public float dmgValue = 4;
    public GameObject throwableObject;
    public Transform attackCheck;
    private Rigidbody2D m_Rigidbody2D;
    public Animator animator;
    public bool canAttack = true;
    public bool isTimeToCheck = false;

    public GameObject cam;

    // ---------------------------------------------------------
    // NEW SERIALIZED FIELDS
    // ---------------------------------------------------------
    [Header("Hit Stop Settings")]
    [Tooltip("Drag specific GameObjects from the Hierarchy here.")]
    public List<GameObject> enemiesThatTriggerPause; 
    public float pauseDuration = 1.0f; // Duration of the freeze
    private bool isHitStopActive = false;
    // ---------------------------------------------------------

    // ---------------------------------------------------------
    // NEW GLITCH VFX VARIABLES
    // ---------------------------------------------------------
    [Header("Player Hit Glitch Settings")]
    [SerializeField] private Color glitchColor = new Color(1f, 0f, 0f, 0.5f); // Red transparent
    [SerializeField] private float glitchDuration = 0.2f;
    [SerializeField] private float glitchFrequency = 0.05f;
    private SpriteRenderer spriteRenderer;
    // ---------------------------------------------------------

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        // Get the SpriteRenderer so we can change colors
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && canAttack)
        {
            canAttack = false;
            animator.SetBool("IsAttacking", true);
            StartCoroutine(AttackCooldown());
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            GameObject throwableWeapon = Instantiate(throwableObject, transform.position + new Vector3(transform.localScale.x * 0.5f, -0.2f), Quaternion.identity) as GameObject;
            Vector2 direction = new Vector2(transform.localScale.x, 0);
            throwableWeapon.GetComponent<ThrowableWeapon>().direction = direction;
            throwableWeapon.name = "ThrowableWeapon";
        }
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(0.25f);
        canAttack = true;
    }

    public void DoDashDamage()
    {
        dmgValue = Mathf.Abs(dmgValue);
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, 0.9f);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Enemy")
            {
                if (collidersEnemies[i].transform.position.x - transform.position.x < 0)
                {
                    dmgValue = -dmgValue;
                }
                collidersEnemies[i].gameObject.SendMessage("ApplyDamage", dmgValue);
                cam.GetComponent<CameraFollow>().ShakeCamera();

                // ---------------------------------------------------------
                // NEW CHECK: Is this specific enemy in the "Pause List"?
                // ---------------------------------------------------------
                if (enemiesThatTriggerPause.Contains(collidersEnemies[i].gameObject))
                {
                    if (!isHitStopActive)
                    {
                        StartCoroutine(TriggerHitStop());
                    }
                }
                // ---------------------------------------------------------
            }
        }
    }

    // ---------------------------------------------------------
    // NEW METHOD: RECEIVE DAMAGE (Triggers the Glitch)
    // ---------------------------------------------------------
    // This function is called when an Enemy attacks THIS player
    public void ApplyDamage(float damage)
    {
        // Add your health reduction logic here if needed (e.g., currentHealth -= damage)
        
        // Trigger visual effect
        if (spriteRenderer != null) 
        {
            StartCoroutine(GlitchEffect());
        }
    }

    // ---------------------------------------------------------
    // NEW COROUTINE: PLAYER GLITCH EFFECT
    // ---------------------------------------------------------
    IEnumerator GlitchEffect()
    {
        Color originalColor = spriteRenderer.color;
        float timer = 0f;

        while (timer < glitchDuration)
        {
            // 0 = Normal, 1 = Glitch Color, 2 = Invisible
            int rnd = Random.Range(0, 3);

            if (rnd == 0)
                spriteRenderer.color = originalColor;
            else if (rnd == 1)
                spriteRenderer.color = glitchColor;
            else if (rnd == 2)
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

            yield return new WaitForSeconds(glitchFrequency);
            timer += glitchFrequency;
        }

        // Reset to normal
        spriteRenderer.color = originalColor;
    }

    // ---------------------------------------------------------
    // NEW COROUTINE (From your previous code)
    // ---------------------------------------------------------
    IEnumerator TriggerHitStop()
    {
        isHitStopActive = true;
        
        // Save original time scale
        float originalTimeScale = Time.timeScale;
        
        // Pause the game
        Time.timeScale = 0f;

        // Wait using Realtime (since game time is 0)
        yield return new WaitForSecondsRealtime(pauseDuration);

        // Resume game
        Time.timeScale = originalTimeScale;
        
        isHitStopActive = false;
    }
}