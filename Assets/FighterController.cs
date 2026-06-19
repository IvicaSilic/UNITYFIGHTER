using UnityEngine;
using TMPro;

public class FighterController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public bool isPlayer1;
    public bool inputLocked;

    [Header("Defense")]
    public bool isBlocking;

    [Header("Combo")]
    public TextMeshProUGUI comboText;
    public float comboWindow = 1.5f;

    private int comboCount = 0;
    private float comboTimer = 0f;

    [Header("Combat")]
    public int maxHealth = 100;
    public int currentHealth;
    public int attackDamage = 10;
    public float attackRange = 1.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip blockSound;
    public AudioClip koSound;



    private Animator animator;
    private Rigidbody2D rb;

    private bool isAttacking = false;
    private bool isStunned = false;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;

        if (comboText != null)
            comboText.text = "";
    }

    void Update()
    {
        if (isDead)
            return;

        if (isPlayer1)
            HandlePlayer1Input();
        else
            HandlePlayer2Input();

        UpdateCombo();
    }

    void HandlePlayer1Input()
    {
        if (inputLocked) return;
        if (isStunned || isDead) return;

        isBlocking = Input.GetKey(KeyCode.S);
        animator.SetBool("Block", isBlocking);

        if (isBlocking)
        {
            animator.SetFloat("Speed", 0);
            return;
        }

        float move = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            move = -1f;
            transform.localScale = new Vector3(-5, 5, 1);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            move = 1f;
            transform.localScale = new Vector3(5, 5, 1);
        }

        transform.Translate(Vector2.right * move * moveSpeed * Time.deltaTime);
        animator.SetFloat("Speed", Mathf.Abs(move));

        if (Input.GetKeyDown(KeyCode.F) && !isAttacking)
        {
            isAttacking = true;
            animator.SetTrigger("Attack");

            Invoke(nameof(PerformAttack), 0.4f);
            Invoke(nameof(ResetAttack), 0.8f);
        }
    }

    void HandlePlayer2Input()
    {
        if (inputLocked) return;
        if (isStunned || isDead) return;

        isBlocking = Input.GetKey(KeyCode.DownArrow);
        animator.SetBool("Block", isBlocking);

        if (isBlocking)
        {
            animator.SetFloat("Speed", 0);
            return;
        }

        float move = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            move = -1f;
            transform.localScale = new Vector3(-5, 5, 1);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            move = 1f;
            transform.localScale = new Vector3(5, 5, 1);
        }

        transform.Translate(Vector2.right * move * moveSpeed * Time.deltaTime);
        animator.SetFloat("Speed", Mathf.Abs(move));

        if (Input.GetKeyDown(KeyCode.RightControl) && !isAttacking)
        {
            isAttacking = true;
            animator.SetTrigger("Attack");

            Invoke(nameof(PerformAttack), 0.4f);
            Invoke(nameof(ResetAttack), 0.8f);
        }
    }

    void PerformAttack()
    {
        string enemyTag = isPlayer1 ? "Player2" : "Player1";

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag(enemyTag))
                continue;

            FighterController enemy = hit.GetComponent<FighterController>();

            if (enemy == null)
                continue;

            bool didHit = enemy.TakeDamage(attackDamage, transform.position);

            if (didHit)
            {
                comboCount++;
                comboTimer = comboWindow;

                if (comboText != null)
                    comboText.text = comboCount > 1 ? comboCount + " HIT COMBO!" : "";
            }

            break;
        }
    }

    public bool TakeDamage(int damage, Vector2 attackerPosition)
    {
        if (isDead)
            return false;

        if (isBlocking)
        {
            animator.SetTrigger("BlockHit");
            if (audioSource != null && blockSound != null)
                audioSource.PlayOneShot(blockSound);

            return false;
        }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return true;
        }

        if (audioSource != null && hitSound != null)
            audioSource.PlayOneShot(hitSound);

        Vector2 knockDir = (transform.position - (Vector3)attackerPosition).normalized;
        rb.AddForce(knockDir * 2f, ForceMode2D.Impulse);

        animator.SetTrigger("GetHit");

        isStunned = true;
        Invoke(nameof(ResetStun), 0.5f);

        return true;
    }

    void UpdateCombo()
    {
        if (comboCount <= 0)
            return;

        comboTimer -= Time.deltaTime;

        if (comboTimer <= 0)
        {
            comboCount = 0;

            if (comboText != null)
                comboText.text = "";
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");

        // --- PLAY KO SOUND HERE ---
        if (audioSource != null && koSound != null)
            audioSource.PlayOneShot(koSound);

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void ResetAttack()
    {
        isAttacking = false;
    }

    void ResetStun()
    {
        isStunned = false;
    }

    public void ResetFighter(Vector3 spawnPos)
    {
        transform.position = spawnPos;

        currentHealth = maxHealth;

        isDead = false;
        isStunned = false;
        isAttacking = false;
        isBlocking = false;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        animator.Rebind();
        animator.Update(0f);

        animator.Play("Idle");
    }
}