using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UnityEvent playerConnected;

    private IEnumerator Start()
    { 
        bool connected = false;

        // Starts a guest session using the "LootLocker" SDK
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            // If the guest session startup is not succesful, then display an error message in the log.
            if (!response.success)
            {
                Debug.Log("Error on session startup");
                return;
            }
            // If the guest session startup is  succesful, then display a successful message in the log.
            Debug.Log("Successful Startup of session");

            // the player is now connected to the LootLocker Server and leaderboard for score tracking.
            connected = true;
        });    

        // waits until the player is connected. 
         yield return new WaitUntil(() => connected);

         // then invokes the unity event of the connected player.
        playerConnected.Invoke();   
            
    }
}
