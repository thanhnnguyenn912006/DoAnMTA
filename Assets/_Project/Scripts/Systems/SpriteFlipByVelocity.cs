using UnityEngine;

namespace SurvivorGame.Systems
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class SpriteFlipByVelocity : MonoBehaviour
    {
        [Tooltip("Tích nếu hình gốc của sprite đang nhìn sang PHẢI")]
        [SerializeField] private bool spriteFacesRight = true;
        [SerializeField, Min(0f)] private float deadZone = 0.05f;

        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            float vx = _rb.linearVelocity.x;
            if (Mathf.Abs(vx) < deadZone) return;

            bool movingRight = vx > 0f;
            _spriteRenderer.flipX = spriteFacesRight ? !movingRight : movingRight;
        }
    }
}