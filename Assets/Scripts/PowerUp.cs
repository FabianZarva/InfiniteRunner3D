using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InfiniteRunner3D.Player
{
public class PowerUp : MonoBehaviour
{
    // in Unity Editor, attach this script to a prefab that you want to be a powerup. Choose its type, boost value and duration.
    [SerializeField] private PowerUpType powerUpType;
    [SerializeField] private float powerUpValue;
    [SerializeField] private float powerUpDuration = 5f;

// power up types you can choose from
    public enum PowerUpType
    {
        Speed,
        Jump,
        
    }

// when the player collides with the powerup object, the powerup activates(the attributes are given to the player) and the game object dissapears.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyPowerUp(other.gameObject);
            Destroy(gameObject);
        }
    }

    private void ApplyPowerUp(GameObject player)
    {
        // power up is also based on player's controller component. If the controller exists, there are 2 cases.
        PlayerController playerController = player.GetComponent<PlayerController>();

        if (playerController != null)
        {
            // if the powerup type is for speed. It increases the current player speed value for a short amount of time, then decreases the value by the same amount(see PlayerController.cs)
            switch (powerUpType)
            {
                case PowerUpType.Speed:
                    playerController.IncreaseSpeed(powerUpValue, powerUpDuration);
                    break;
            // if the powerup type is for jump. It increases the current player jump power value for a short amount of time, then decreases the value by the same amount(see PlayerController.cs)
                case PowerUpType.Jump:
                    playerController.IncreaseJump(powerUpValue, powerUpDuration);
                    break;
         
                default:
                    break;
            }
        }
    }
}
}
