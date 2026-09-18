using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private bool isGrounded;
    private float moveSpeed = 7.5f;
    private float jumpForce = 14.5f;
    private float horizontal;
    private float coyoteTime;

    public void Initialize(Rigidbody2D rigidbody)
    {
        rb = rigidbody;
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(horizontal) > 0.01f)
            sprite.flipX = horizontal < 0f;

        coyoteTime = isGrounded ? 0.12f : Mathf.Max(0f, coyoteTime - Time.deltaTime);
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
        if (jumpPressed && coyoteTime > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
            coyoteTime = 0f;
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);
        var col = GetComponent<BoxCollider2D>();
        if (col == null) return;
        var bounds = col.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + 0.05f);
        Vector2 size = new Vector2(bounds.size.x * 0.7f, 0.1f);
        isGrounded = Physics2D.BoxCast(origin, size, 0f, Vector2.down, 0.12f, LayerMask.GetMask("Default")).collider != null;
    }
}
