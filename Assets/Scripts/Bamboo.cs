using UnityEngine;

public class Bamboo : MonoBehaviour
{
    public enum BambooSize { Small, Medium, Large }

    [SerializeField] private BambooSize size = BambooSize.Small;

    private static readonly int[] pointValues = { 10, 25, 50 };
    private static readonly float[] clumsinessValues = { 0.2f, 0.45f, 0.7f };

    private PlayerMovement leftEndPlayer;
    private PlayerMovement rightEndPlayer;
    private DistanceJoint2D leftJoint;
    private DistanceJoint2D rightJoint;

    public int GetPointValue() => pointValues[(int)size];
    public float GetClumsinessModifier() => clumsinessValues[(int)size];

    public void SetGrabbedEnd(PlayerMovement player)
    {
        Debug.Log("SetGrabbedEnd: " + player.name);

        bool leftFree = leftEndPlayer == null;
        bool rightFree = rightEndPlayer == null;

        if (leftFree && rightFree)
        {
            // First grab
            leftEndPlayer = player;
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            player.SetAnchored(true);
            player.SetHeldBamboo(this);
        }
        else if (!leftFree && rightFree && leftEndPlayer != player)
        {
            // Second grab from the right end
            rightEndPlayer = player;
            leftEndPlayer.SetAnchored(false);
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            player.SetHeldBamboo(this);
            LinkPlayers();
        }
        else if (leftFree && !rightFree && rightEndPlayer != player)
        {
            // Second grab from the left end (first player grabbed right)
            leftEndPlayer = player;
            rightEndPlayer.SetAnchored(false);
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            player.SetHeldBamboo(this);
            LinkPlayers();
        }
    }

    public void ReleasePlayer(PlayerMovement player)
    {
        if (player == leftEndPlayer)
        {
            if (leftJoint != null) { Destroy(leftJoint); leftJoint = null; }
            ResetEndForPlayer(player);
            leftEndPlayer = null;
        }
        else if (player == rightEndPlayer)
        {
            if (rightJoint != null) { Destroy(rightJoint); rightJoint = null; }
            ResetEndForPlayer(player);
            rightEndPlayer = null;
        }
        else return;

        player.SetClumsiness(0f);
        player.SetHeldBamboo(null);

        // Re-anchor the remaining player and freeze bamboo
        PlayerMovement remaining = leftEndPlayer ?? rightEndPlayer;
        if (remaining != null)
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            remaining.SetAnchored(true);
        }
        else
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        }
    }

    private void ResetEndForPlayer(PlayerMovement player)
    {
        foreach (var end in GetComponentsInChildren<BambooEnd>())
        {
            if (end.HeldByPlayer == player)
            {
                end.ResetEnd();
                break;
            }
        }
    }

    private void LinkPlayers()
    {
        Rigidbody2D bambooRb = GetComponent<Rigidbody2D>();
        Rigidbody2D leftRb = leftEndPlayer.GetComponent<Rigidbody2D>();
        Rigidbody2D rightRb = rightEndPlayer.GetComponent<Rigidbody2D>();

        if (leftRb == null || rightRb == null || bambooRb == null)
        {
            Debug.LogError("Missing Rigidbody2D for bamboo link!");
            return;
        }

        leftJoint = gameObject.AddComponent<DistanceJoint2D>();
        leftJoint.connectedBody = leftRb;
        leftJoint.autoConfigureDistance = true;

        rightJoint = gameObject.AddComponent<DistanceJoint2D>();
        rightJoint.connectedBody = rightRb;
        rightJoint.autoConfigureDistance = true;

        float c = GetClumsinessModifier();
        leftEndPlayer.SetClumsiness(c);
        rightEndPlayer.SetClumsiness(c);

        Debug.Log("Players linked! Clumsiness: " + c);
    }

    public void UnlinkPlayers()
    {
        if (leftJoint != null) { Destroy(leftJoint); leftJoint = null; }
        if (rightJoint != null) { Destroy(rightJoint); rightJoint = null; }

        if (leftEndPlayer != null) { leftEndPlayer.SetClumsiness(0); leftEndPlayer.SetHeldBamboo(null); leftEndPlayer.SetAnchored(false); }
        if (rightEndPlayer != null) { rightEndPlayer.SetClumsiness(0); rightEndPlayer.SetHeldBamboo(null); rightEndPlayer.SetAnchored(false); }

        leftEndPlayer = null;
        rightEndPlayer = null;
    }
}
