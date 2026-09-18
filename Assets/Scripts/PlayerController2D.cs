using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private bool isGrounded;
    private float moveSpeed = 7.5f;
    private float jumpForce = 14.5f;
    private float horizontal;

    public void Initialize(Rigidbody2D rigidbody)
    {
        rb = rigidbody;
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(horizontal) > 0.01f)
        {
            float dir = horizontal > 0 ? 1f : -1f;
            sprite.flipX = dir < 0f;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                isGrounded = false;
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 desired = new Vector2(horizontal * moveSpeed, rb.velocity.y);
        rb.velocity = desired;

        var col = GetComponent<BoxCollider2D>();
        if (col == null)
            return;

        var bounds = col.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + 0.05f);
        Vector2 size = new Vector2(bounds.size.x * 0.7f, 0.1f);

        isGrounded = Physics2D.BoxCast(origin, size, 0f, Vector2.down, 0.12f, LayerMask.GetMask("Default")).collider != null;
    }
}
