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

            //prevent sibling from triggering the grab logic
            if (gameObject.name == "LeftEnd")
            {
                transform.parent.Find("RightEnd").GetComponent<BambooEnd>().isGrabbed = true;
            }
            else if (gameObject.name == "RightEnd")
            {
                transform.parent.Find("LeftEnd").GetComponent<BambooEnd>().isGrabbed = true;
            }

            // tell parent bamboo to which end was grabbed by which panda
            transform.parent.GetComponent<Bamboo>().SetGrabbedEnd(collision.GetComponent<PlayerMovement>()); //BREAK THIS UP
        }

    }
}


