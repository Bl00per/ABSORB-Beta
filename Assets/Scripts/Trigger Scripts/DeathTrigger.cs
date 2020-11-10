using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    private PlayerHandler _player;

    // Start is called before the first frame update
    void Start()
    {
        _player = FindObjectOfType<PlayerHandler>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _player.SetIsAlive(false);
            _player.SetCurrentHealth(0);
        }
    }
}
