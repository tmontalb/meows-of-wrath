using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 12f;
    public float lifetime = 0.1f;

    Vector2 dir;
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void Init(Vector2 direction)
    {
        dir = direction.normalized;

        // Flip sprite based on horizontal direction
        if (sr != null && Mathf.Abs(dir.x) > 0.01f)
        {
            sr.flipX = dir.x < 0;
        }
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
    }
}