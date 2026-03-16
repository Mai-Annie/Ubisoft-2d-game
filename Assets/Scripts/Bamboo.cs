using UnityEngine;

public class Bamboo : MonoBehaviour
{
    // Store references to each player holding an end, instead of just a counter
    private PlayerMovement leftEndPlayer = null;
    private PlayerMovement rightEndPlayer = null;

    private DistanceJoint2D leftJoint;
    private DistanceJoint2D rightJoint;

    public void SetGrabbedEnd(PlayerMovement player)
    {
        Debug.Log("SetGrabbedEnd called by: " + player.name);

        // Assign the player to whichever end is still free
        if (leftEndPlayer == null)
        {
            Debug.Log("First end grabbed, anchoring player");
            leftEndPlayer = player;

            // Anchor the first player until the second one grabs the other end
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            player.SetAnchored(true);
        }
        else
        {
            Debug.Log("Second end grabbed, linking players");
            rightEndPlayer = player;

            // Both ends are now grabbed, release the first player's anchor
            leftEndPlayer.SetAnchored(false);
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;

            // Physically link both players to the bamboo using joints
            LinkPlayers();
        }
    }

    private void LinkPlayers()
    {
        Rigidbody2D bambooRb = GetComponent<Rigidbody2D>();

        // Get each player's Rigidbody2D so the joint can connect them
        Rigidbody2D leftRb = leftEndPlayer.GetComponent<Rigidbody2D>();
        Rigidbody2D rightRb = rightEndPlayer.GetComponent<Rigidbody2D>();

        if (leftRb == null || rightRb == null || bambooRb == null)
        {
            Debug.LogError("Missing Rigidbody2D on one of the objects!");
            return;
        }

        // Create a joint on the bamboo connecting it to the left player
        leftJoint = gameObject.AddComponent<DistanceJoint2D>();
        leftJoint.connectedBody = leftRb;
        leftJoint.autoConfigureDistance = true;

        // Create a joint on the bamboo connecting it to the right player
        rightJoint = gameObject.AddComponent<DistanceJoint2D>();
        rightJoint.connectedBody = rightRb;
        rightJoint.autoConfigureDistance = true;

        Debug.Log("Players linked to bamboo!");
    }

    public void UnlinkPlayers()
    {
        // Remove the joints to disconnect the players from the bamboo
        if (leftJoint != null) Destroy(leftJoint);
        if (rightJoint != null) Destroy(rightJoint);

        // Clear the stored player references so the bamboo can be grabbed again
        leftEndPlayer = null;
        rightEndPlayer = null;

        Debug.Log("Players unlinked from bamboo.");
    }
}