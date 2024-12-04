using UnityEngine;

public class Orb : MonoBehaviour
{
    GameObject Player;
    private Rigidbody2D _rigidbody;

    private void Start()
    {
        Player = GameObject.FindWithTag("Player");

        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.AddRelativeForce(new Vector2(0, 1), ForceMode2D.Impulse);
        _rigidbody.isKinematic = false;

        Destroy(gameObject, 15);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name.Contains("Player"))
        {
            var playerComponent = Player.GetComponent<PlayerController>();
            if (playerComponent != null && playerComponent.LeechLevel > 0)
            {
                playerComponent.Health += 2;
                playerComponent.Health = Mathf.Min(playerComponent.Health, playerComponent.MaxHealth);
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _rigidbody.isKinematic = true;
    }
}
