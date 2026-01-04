using UnityEngine;
using UnityEngine.AI;

public class HostageFollower : MonoBehaviour
{
    [Header("Refer")]
    public NavMeshAgent agent;          // Hostage's NavMeshAgent
    public Transform followTarget;      // Follow target (Player)

    [Header("Follow parameters")]
    public float followDistance = 1.5f; // Follow distance to player
    public float followSideOffset = 0.6f; // Horizontal offset (team formation)
    public float updateInterval = 0.15f;  // Target update frequency

    [Header("Team serial number")]
    public int followIndex = 0; // Follower order index (0,1,2...)

    [Header("Carry Mode")]
    public bool isCarried = false;

    // Player's hand grip point (HandGripPoint)
    public Transform carryPoint;

    // Hostage's grabbed hand point (HandGrabPoint)
    [Header("Hand Points")]
    public Transform handGrabPoint;   // Hostage's hand point for grabbing front teammate
    public Transform handGripPoint;   // Hostage's hand point for being grabbed by back teammate

    public float carryRotateSpeed = 18f;

    // Local position & rotation of grip point in hostage root space (for root inverse solve)
    private Vector3 handLocalPos;
    private Quaternion handLocalRot;

    private Collider[] cachedColliders;
    private float timer;

    [Header("Animator")]
    public Animator hostageAnimator;
    public string animCarryBool = "Carried";
    public string animSpeedFloat = "Speed";

    [Header("Player Root (for speed)")]
    public Transform playerRoot; //  For reading player movement speed
    private Vector3 lastPlayerPos;


    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Awake()
    {
        cachedColliders = GetComponentsInChildren<Collider>(true);
    }

    private void OnEnable()
    {
        timer = 0;
        if (playerRoot != null) lastPlayerPos = playerRoot.position;

    }

    public void SetCarried(Transform playerHandGripPoint)
    {
        isCarried = true;
        carryPoint = playerHandGripPoint;

        if (handGrabPoint == null)
        {
            Debug.LogError("Hostage handGrabPoint is NULL (bind it on prefab).");
            return;
        }

        // Key：Record local pose of grip point in root transform space
        handLocalPos = transform.InverseTransformPoint(handGrabPoint.position);
        handLocalRot = Quaternion.Inverse(transform.rotation) * handGrabPoint.rotation;

        // Disable NavMeshAgent when carried, avoid jitter/error from position conflict
        if (agent != null)
        {
            // Only call isStopped when agent is enabled and on NavMesh
            if (agent.enabled && agent.isOnNavMesh)
                agent.isStopped = true;

            agent.enabled = false;
        }


        // Disable collider when carried, prevent pushing player away
        if (cachedColliders != null)
        {
            foreach (var c in cachedColliders)
            {
                if (c == null) continue;
                c.enabled = false;
            }
        }

        if (hostageAnimator != null)
            hostageAnimator.SetBool(animCarryBool, true);
    }

    public void ReleaseCarried()
    {
        isCarried = false;
        carryPoint = null;

        // Restore collider
        if (cachedColliders != null)
        {
            foreach (var c in cachedColliders)
            {
                if (c == null) continue;
                c.enabled = true;
            }
        }

        // Restore NavMeshAgent: warp hostage back to NavMesh first then enable agent
        if (agent != null)
        {
            // 1) Raycast down to find ground (ground requires Collider)
            Vector3 origin = transform.position + Vector3.up * 2.0f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit groundHit, 20f, ~0, QueryTriggerInteraction.Ignore))
            {
                transform.position = groundHit.point;
            }

            // 2) Snap position to NavMesh
            Vector3 pos = transform.position;
            if (NavMesh.SamplePosition(pos, out NavMeshHit navHit, 10.0f, NavMesh.AllAreas))
            {
                agent.enabled = true;
                agent.Warp(navHit.position);    // Use Warp to prevent conflict between agent and transform position
                agent.isStopped = false;
            }
            else
            {
                agent.enabled = false;
                Debug.LogWarning($"[{name}] ReleaseCarried: no NavMesh nearby, keep agent disabled.");
            }
        }



        if (hostageAnimator != null)
            hostageAnimator.SetBool(animCarryBool, false);

    }

    private void Update()
    {
        // ---------------- Carry：手牵手对齐 ----------------
        if (isCarried && carryPoint != null)
        {
            // 1) Only take horizontal facing of carryPoint (avoid tilt from hand bone flip)
            Vector3 fwd = carryPoint.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;

            Quaternion carryYaw = Quaternion.LookRotation(fwd, Vector3.up);

            // 2) Root rotation : use flattened carryYaw instead of raw rotation
            Quaternion targetRootRot = carryYaw * Quaternion.Inverse(handLocalRot);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRootRot, Time.deltaTime * carryRotateSpeed);

            // 3) Position : calculate target root position first
            Vector3 cp = carryPoint.position;

            // Lock Y axis : follow player root height (prevent hostage lift from hand animation jitter)
            if (playerRoot != null) cp.y = playerRoot.position.y;

            Vector3 targetRootPos = cp - (transform.rotation * handLocalPos);

            // 4) Double insurance : snap target position to NavMesh (height/small range only, avoid drift)
            if (NavMesh.SamplePosition(targetRootPos, out NavMeshHit carryHit, 1.0f, NavMesh.AllAreas))
                targetRootPos = carryHit.position;

            transform.position = targetRootPos;

            // Animator Speed logic is retained
            return;
        }


        // ---------------- Follow State ----------------
        if (agent == null || followTarget == null) return;
        if (!agent.enabled) return;

        timer += Time.deltaTime;
        if (timer < updateInterval) return;
        timer = 0f;

        Vector3 behind = -followTarget.forward * (followDistance + followIndex * 0.9f);
        float side = (followIndex % 2 == 0) ? 1f : -1f;
        Vector3 sideOffset = followTarget.right * side * followSideOffset * (1 + followIndex * 0.1f);

        Vector3 desiredPos = followTarget.position + behind + sideOffset;

        if (NavMesh.SamplePosition(desiredPos, out NavMeshHit followhit, 4f, NavMesh.AllAreas))
            agent.SetDestination(followhit.position);
        else
            agent.SetDestination(followTarget.position);
    }
}
