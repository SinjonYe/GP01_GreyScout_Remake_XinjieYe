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
    public Transform carryPoint;     // 玩家身上的挂点
    public float carryRotateSpeed = 12f;


    private float timer;

    // 新增：缓存所有碰撞体/刚体
    private Collider[] cachedColliders;
    private Rigidbody cachedRb;

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        cachedColliders = GetComponentsInChildren<Collider>(true);
        cachedRb = GetComponent<Rigidbody>();
    }


    public void SetCarried(Transform point)
    {
        isCarried = true;
        carryPoint = point;

        // 1) 关导航（你已做）
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false; // 关键：避免 NavMesh 抢位置导致抖动
        }

        // 2) 关所有 Collider，避免把玩家顶飞
        if (cachedColliders != null)
        {
            foreach (var col in cachedColliders)
            {
                if (col != null) col.enabled = false;
            }
        }

        // 3) 如有人质 Rigidbody，设为运动学，避免物理力
        if (cachedRb != null)
        {
            cachedRb.isKinematic = true;
            cachedRb.velocity = Vector3.zero;
            cachedRb.angularVelocity = Vector3.zero;
        }

        // 4) 可选：直接挂到 carryPoint（更稳，不容易抖）
        transform.SetParent(carryPoint, worldPositionStays: false);
        transform.localPosition = new Vector3(0.0f, 0.0f, -0.4f); // 让人质站在手后方一点
        transform.localRotation = Quaternion.identity;

    }

    public void ReleaseCarried()
    {
        isCarried = false;

        // 1) 解除父子关系
        transform.SetParent(null, worldPositionStays: true);

        // 2) 恢复 Collider
        if (cachedColliders != null)
        {
            foreach (var col in cachedColliders)
            {
                if (col != null) col.enabled = true;
            }
        }

        // 3) 恢复 Rigidbody
        if (cachedRb != null)
        {
            cachedRb.isKinematic = false;
        }

        // 4) 恢复 NavMeshAgent
        carryPoint = null;
        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }
    }


    private void Update()
    {
        // Carry 状态下不再走队伍跟随逻辑（因为我们已经 parent 了）
        if (isCarried) return;

        if (agent == null || followTarget == null) return;

        timer += Time.deltaTime;
        if (timer < updateInterval) return;
        timer = 0f;

        Vector3 behind = -followTarget.forward * (followDistance + followIndex * 0.9f);
        float side = (followIndex % 2 == 0) ? 1f : -1f;
        Vector3 sideOffset = followTarget.right * side * followSideOffset * (1 + followIndex * 0.1f);
        Vector3 desiredPos = followTarget.position + behind + sideOffset;

        if (NavMesh.SamplePosition(desiredPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            agent.SetDestination(followTarget.position);
    }
}
