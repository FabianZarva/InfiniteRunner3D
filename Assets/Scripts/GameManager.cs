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
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (!response.success)
            {
                Debug.Log("Error on session startup");
                return;
            }
            Debug.Log("Successful Startup of session");
            connected = true;
        });    
         yield return new WaitUntil(() => connected);
        playerConnected.Invoke();   
            
    }
}
