using UnityEngine;

public class BambooEnd : MonoBehaviour
{
    public bool isGrabbed = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Something entered the trigger");

        if (collision.CompareTag("Player") && !isGrabbed)
        {
            Debug.Log("Player detected on: " + gameObject.name);
            isGrabbed = true;

            // Get the PlayerMovement from the colliding player
            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            if (player == null)
            {
                Debug.LogError("PlayerMovement not found on colliding object!");
                return;
            }

            // Get the Bamboo script from the parent and report the grab
            Bamboo bamboo = transform.parent.GetComponent<Bamboo>();
            if (bamboo == null)
            {
                Debug.LogError("Bamboo script not found on parent object!");
                return;
            }
            
            bamboo.SetGrabbedEnd(player);
        }
    }
}

