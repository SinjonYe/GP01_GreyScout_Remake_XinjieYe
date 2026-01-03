using UnityEngine;
using UnityEngine.AI;

public class HostageFollower : MonoBehaviour
{
    [Header("引用")]
    public NavMeshAgent agent;          // 人质的 NavMeshAgent
    public Transform followTarget;      // 跟随目标（玩家）

    [Header("跟随参数")]
    public float followDistance = 1.5f; // 离玩家多近
    public float followSideOffset = 0.6f; // 横向偏移（队伍排列）
    public float updateInterval = 0.15f;  // 目标更新频率

    [Header("队伍序号")]
    public int followIndex = 0; // 第几个跟随的人质（0,1,2...）

    [Header("Carry Mode")]
    public bool isCarried = false;

    // 玩家手的抓握点（HandGripPoint）
    public Transform carryPoint;

    // 人质自己的“被抓的手点”（HandGrabPoint）——你刚刚已经绑定了
    [Header("Hand Points")]
    public Transform handGrabPoint;   // 这个人质用来“抓前面”的手点（已有）
    public Transform handGripPoint;   // 这个人质“给后面抓”的手点（新增）

    public float carryRotateSpeed = 18f;

    /* 关键：缓存“手点到根节点”的相对偏移（防止抖动/缩放问题）
    private Vector3 carryPosOffset;
    private Quaternion carryRotOffset;*/

    // 手抓点在“人质根节点空间”的局部位置与局部旋转（用于反解根节点）
    private Vector3 handLocalPos;
    private Quaternion handLocalRot;

    private Collider[] cachedColliders;
    private float timer;

    [Header("Animator")]
    public Animator hostageAnimator;
    public string animCarryBool = "Carried";
    public string animSpeedFloat = "Speed";

    [Header("Player Root (for speed)")]
    public Transform playerRoot; // 用来读玩家速度（可选）
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

        // 关键：记录“手抓点在根节点空间”的局部姿态
        handLocalPos = transform.InverseTransformPoint(handGrabPoint.position);
        handLocalRot = Quaternion.Inverse(transform.rotation) * handGrabPoint.rotation;

        // Carry 时禁用 NavMeshAgent，避免抢位置抖动/报错
        if (agent != null)
        {
            // 只有在“已启用且在 NavMesh 上”时，才能调用 isStopped
            if (agent.enabled && agent.isOnNavMesh)
                agent.isStopped = true;

            agent.enabled = false;
        }


        // Carry 时禁用碰撞，避免把玩家顶飞
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

        // 恢复碰撞
        if (cachedColliders != null)
        {
            foreach (var c in cachedColliders)
            {
                if (c == null) continue;
                c.enabled = true;
            }
        }

        // 恢复 NavMeshAgent：先把人质“放回 NavMesh 上”再启用 agent
        if (agent != null)
        {
            // 1) 先尝试射线往下找地面（地面要有 Collider）
            Vector3 origin = transform.position + Vector3.up * 2.0f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit groundHit, 20f, ~0, QueryTriggerInteraction.Ignore))
            {
                transform.position = groundHit.point;
            }

            // 2) 再吸附到 NavMesh（把半径从 2 提高到 6~10 更稳）
            Vector3 pos = transform.position;
            if (NavMesh.SamplePosition(pos, out NavMeshHit navHit, 10.0f, NavMesh.AllAreas))
            {
                agent.enabled = true;
                agent.Warp(navHit.position);    // 用 Warp，避免 agent 与 transform 打架
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
            // 1) 只取 carryPoint 的水平朝向（避免手骨骼上下翻转带来的倾斜）
            Vector3 fwd = carryPoint.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;

            Quaternion carryYaw = Quaternion.LookRotation(fwd, Vector3.up);

            // 2) root 旋转：用水平化后的 carryYaw，而不是 carryPoint.rotation
            Quaternion targetRootRot = carryYaw * Quaternion.Inverse(handLocalRot);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRootRot, Time.deltaTime * carryRotateSpeed);

            // 3) 位置：先算出目标根节点位置
            Vector3 cp = carryPoint.position;

            // 锁定 Y：跟随玩家根节点高度（避免手部动画上下抖动带着人质飞）
            if (playerRoot != null) cp.y = playerRoot.position.y;

            Vector3 targetRootPos = cp - (transform.rotation * handLocalPos);

            // 4) 进一步保险：把目标点吸附到 NavMesh（只吸附高度/小范围，避免漂移）
            if (NavMesh.SamplePosition(targetRootPos, out NavMeshHit carryHit, 1.0f, NavMesh.AllAreas))
                targetRootPos = carryHit.position;

            transform.position = targetRootPos;

            // (可选) Animator Speed 逻辑你可保留
            return;
        }


        // ---------------- 跟随 ----------------
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
