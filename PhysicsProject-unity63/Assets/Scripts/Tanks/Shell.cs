using UnityEngine;

public class Shell : MonoBehaviour
{
    public GameObject explosion;

    public float explosionRadius = 10f;
    public float explosionMaxDamage = 50f;

    float speed = 0.0f;
    float mass = 1.0f;
    float force = 30.0f;
    float drag = 1.0f;
    float acceleration;

    float ySpeed = 0.0f;
    float gravity = -9.8f;
    float gravityAcceleration = 0.0f;

    void OnCollisionEnter(Collision col)
    {
        Debug.Log(
            "Shell colidiu com: " +
            col.gameObject.name +
            " | Tag: " +
            col.gameObject.tag
        );

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

    private void Start()
    {
        Debug.Log("SHELL FOI CRIADO: " + gameObject.name);

        acceleration = force / mass;

        speed += acceleration;

        gravityAcceleration =
            gravity / mass;
    }

    void Update()
    {
        speed *=
            (1 - Time.deltaTime * drag);

        ySpeed +=
            gravityAcceleration *
            Time.deltaTime *
            0.01f;

        transform.Translate(
            0,
            ySpeed,
            speed * Time.deltaTime
        );
    }
}