using UnityEngine;

public class TankHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public GameObject explosion;

    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        Debug.Log(
            gameObject.name +
            " recebeu " +
            damage +
            " de dano. Vida: " +
            currentHealth
        );

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(
            gameObject.name +
            " foi destruído!"
        );

        if (explosion != null)
        {
            GameObject exp = Instantiate(
                explosion,
                transform.position,
                Quaternion.identity
            );

            Destroy(exp, 0.5f);
        }

        Destroy(gameObject);
    }
}