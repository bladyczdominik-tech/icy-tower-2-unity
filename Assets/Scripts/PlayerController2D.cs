using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    private Rigidbody2D rb; private SpriteRenderer sprite; private bool grounded; private float horizontal; private float coyote; private int audioProfile;
    public void Initialize(Rigidbody2D body){rb=body;sprite=GetComponent<SpriteRenderer>();}
    public void SetAudioProfile(int profile){audioProfile=profile;}
    private void Update(){horizontal=Input.GetAxisRaw("Horizontal");if(Mathf.Abs(horizontal)>.01f)sprite.flipX=horizontal<0;coyote=grounded?.12f:Mathf.Max(0,coyote-Time.deltaTime);if((Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.W)||Input.GetKeyDown(KeyCode.UpArrow))&&coyote>0){rb.velocity=new Vector2(rb.velocity.x,14.5f);grounded=false;coyote=0;AudioFactory.PlayJump(audioProfile);}}
    private void FixedUpdate(){rb.velocity=new Vector2(horizontal*7.5f,rb.velocity.y);var c=GetComponent<BoxCollider2D>();if(c==null)return;var b=c.bounds;grounded=Physics2D.BoxCast(new Vector2(b.center.x,b.min.y+.05f),new Vector2(b.size.x*.7f,.1f),0,Vector2.down,.12f,LayerMask.GetMask("Default")).collider!=null;}
}
