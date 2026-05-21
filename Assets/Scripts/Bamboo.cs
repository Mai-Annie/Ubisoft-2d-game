using UnityEngine;

public class Bamboo : MonoBehaviour
{
    public enum BambooSize { Small, Medium, Large }

    [SerializeField] private BambooSize size = BambooSize.Small;

    private static readonly int[] PointValues = { 10, 25, 50 };
    private static readonly float[] ClumsinessValues = { 0.2f, 0.45f, 0.7f };

    private PlayerMovement leftEndPlayer;
    private PlayerMovement rightEndPlayer;
    private DistanceJoint2D leftJoint;
    private DistanceJoint2D rightJoint;
    private Rigidbody2D rb;
    private Collider2D solidCollider;
    private BambooEnd[] ends;

    public int PointValue => PointValues[(int)size];
    public float ClumsinessModifier => ClumsinessValues[(int)size];

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ends = GetComponentsInChildren<BambooEnd>();
        foreach (var col in GetComponents<Collider2D>())
        {
            if (!col.isTrigger) { solidCollider = col; break; }
        }
    }

    public void OnEndGrabbed(PlayerMovement player)
    {
        bool leftFree = leftEndPlayer == null;
        bool rightFree = rightEndPlayer == null;

        if (leftFree && rightFree)
        {
            leftEndPlayer = player;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            player.SetAnchored(true);
            player.SetHeldBamboo(this);
        }
        else if (!leftFree && rightFree && leftEndPlayer != player)
        {
            rightEndPlayer = player;
            leftEndPlayer.SetAnchored(false);
            rb.constraints = RigidbodyConstraints2D.None;
            player.SetHeldBamboo(this);
            LinkPlayers();
        }
        else if (leftFree && !rightFree && rightEndPlayer != player)
        {
            leftEndPlayer = player;
            rightEndPlayer.SetAnchored(false);
            rb.constraints = RigidbodyConstraints2D.None;
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

        PlayerMovement remaining = leftEndPlayer ?? rightEndPlayer;
        if (remaining != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            remaining.SetAnchored(true);
        }
        else
        {
            if (solidCollider != null) solidCollider.enabled = true;
            rb.constraints = RigidbodyConstraints2D.None;
        }
    }

    private void ResetEndForPlayer(PlayerMovement player)
    {
        foreach (var end in ends)
        {
            if (end.HeldByPlayer == player) { end.ResetEnd(); break; }
        }
    }

    private void LinkPlayers()
    {
        Rigidbody2D leftRb = leftEndPlayer.GetComponent<Rigidbody2D>();
        Rigidbody2D rightRb = rightEndPlayer.GetComponent<Rigidbody2D>();

        if (leftRb == null || rightRb == null)
        {
            Debug.LogError("Player missing Rigidbody2D — cannot link!", this);
            return;
        }

        leftJoint = gameObject.AddComponent<DistanceJoint2D>();
        leftJoint.connectedBody = leftRb;
        leftJoint.autoConfigureDistance = true;

        rightJoint = gameObject.AddComponent<DistanceJoint2D>();
        rightJoint.connectedBody = rightRb;
        rightJoint.autoConfigureDistance = true;

        if (solidCollider != null) solidCollider.enabled = false;

        leftEndPlayer.SetClumsiness(ClumsinessModifier);
        rightEndPlayer.SetClumsiness(ClumsinessModifier);
    }

    public void UnlinkPlayers()
    {
        if (leftJoint != null) { Destroy(leftJoint); leftJoint = null; }
        if (rightJoint != null) { Destroy(rightJoint); rightJoint = null; }

        if (leftEndPlayer != null) { leftEndPlayer.SetClumsiness(0f); leftEndPlayer.SetHeldBamboo(null); leftEndPlayer.SetAnchored(false); }
        if (rightEndPlayer != null) { rightEndPlayer.SetClumsiness(0f); rightEndPlayer.SetHeldBamboo(null); rightEndPlayer.SetAnchored(false); }

        leftEndPlayer = null;
        rightEndPlayer = null;
    }
}
