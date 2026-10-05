using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void UpdateAnimation(Vector2 movement)
    {
        if (movement == Vector2.zero)
        {
            animator.speed = 0;
            return;
        }

        animator.speed = 1;

        float x = movement.x;
        float y = movement.y;

        if (y > 0.5f)
        {
            animator.Play("Player_Up");
            spriteRenderer.flipX = x < 0;
        }
        else if (y < -0.5f)
        {
            animator.Play("Player_Down");
            spriteRenderer.flipX = x < 0;
        }
        else
        {
            animator.Play("Player_Left");
            spriteRenderer.flipX = x > 0;
        }
    }
}