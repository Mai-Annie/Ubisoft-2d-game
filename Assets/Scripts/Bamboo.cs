using UnityEngine;

public class Bamboo : MonoBehaviour
{
    private int grabbedEnd = 0; // 0 = none, 1 = left, 2 = right
    private PlayerMovement playerGrabbed;
    private DistanceJoint2D joint;
    public void SetGrabbedEnd(PlayerMovement player)
    {
        Debug.Log("SetGrabbedEnd called, grabbedEnd count: " + grabbedEnd);
        grabbedEnd += 1;

        if (grabbedEnd < 2)
        {
            Debug.Log("First end grabbed, anchoring player");
            // call method to anchor player movement
            player.SetAnchored(true);
            playerGrabbed = player;
        }
        else
        {
            Debug.Log("Second end grabbed, linking players");
            // both ends are grabbed, call method to unanchor the other player movement
            playerGrabbed.SetAnchored(false);

            //physically connect the bamboo to the player using a joint
            LinkPlayers();
        }
    }

    private void LinkPlayers()
    {
        joint = gameObject.AddComponent<DistanceJoint2D>();
        //add damper and elasticity to the joint for better feel

    }
    //Todo: unlink players
}
