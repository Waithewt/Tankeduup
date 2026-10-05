using UnityEngine;

public class AIShell : MonoBehaviour
{
    public GameObject explosion;

    public float explosionRadius = 10f;
    public float explosionMaxDamage = 50f;

    Rigidbody body;

    private void OnCollisionEnter(Collision collision)
    {
        // Dano em área da explosão
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            explosionRadius
        );

        foreach (Collider collider in colliders)
        {
            TankHealth health =
                collider.GetComponentInParent<TankHealth>();

            if (health != null)
            {
                float distance =
                    Vector3.Distance(
                        transform.position,
                        collider.transform.position
                    );

                float areaDamage =
                    Mathf.Lerp(
                        explosionMaxDamage,
                        0f,
                        distance / explosionRadius
                    );

                health.TakeDamage(areaDamage);
            }
        }

        // Explosão visual
        GameObject exp =
            Instantiate(
                explosion,
                transform.position,
                Quaternion.identity
            );

        Destroy(exp, 0.5f);

        Destroy(gameObject);
    }

    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    void Update()
    {
        transform.forward = body.linearVelocity;
    }
}