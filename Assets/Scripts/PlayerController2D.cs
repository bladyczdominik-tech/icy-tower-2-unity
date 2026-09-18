using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private bool grounded;
    private float horizontal;
    private float coyote;
    private int audioProfile;

    public void Initialize(Rigidbody2D body)
    {
        rb = body;
        sprite = GetComponent<SpriteRenderer>();
    }

    public void SetAudioProfile(int profile)
    {
        audioProfile = profile;
    }

    private void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(horizontal) > 0.01f && sprite != null)
            sprite.flipX = horizontal < 0f;

        coyote = grounded ? 0.12f : Mathf.Max(0f, coyote - Time.deltaTime);

        bool jumpPressed = Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.W)
            || Input.GetKeyDown(KeyCode.UpArrow);

        if (jumpPressed && coyote > 0f && rb != null)
        {
            rb.velocity = new Vector2(rb.velocity.x, 14.5f);
            grounded = false;
            coyote = 0f;
            AudioFactory.PlayJump(audioProfile);
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        rb.velocity = new Vector2(horizontal * 7.5f, rb.velocity.y);

        var collider = GetComponent<BoxCollider2D>();
        if (collider == null)
            return;

        Bounds bounds = collider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + 0.05f);
        Vector2 size = new Vector2(bounds.size.x * 0.7f, 0.1f);
        int platformMask = LayerMask.GetMask("Default");

        grounded = Physics2D.BoxCast(
            origin,
            size,
            0f,
            Vector2.down,
            0.12f,
            platformMask
        ).collider != null;
    }
}
