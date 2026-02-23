using UnityEngine;

public class BambooEnd : MonoBehaviour
{
    private bool isGrabbed = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isGrabbed)
        {
            isGrabbed = true;

            // tell parent bamboo to which end was grabbed by which panda
            transform.parent.GetComponent<Bamboo>().SetGrabbedEnd(this);
        }

    }
}


