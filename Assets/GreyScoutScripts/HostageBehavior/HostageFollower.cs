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

    private float timer;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        timer = 0;
    }

    private void Update()
    {
        if (agent == null || followTarget == null) return;

        timer += Time.deltaTime;
        if (timer < updateInterval) return;
        timer = 0f;

        // 计算队伍目标位置：在玩家身后排队
        Vector3 behind = -followTarget.forward * (followDistance + followIndex * 0.9f);

        // 让队伍稍微左右错开（避免完全重叠）
        float side = (followIndex % 2 == 0) ? 1f : -1f;
        Vector3 sideOffset = followTarget.right * side * followSideOffset * (1 + followIndex * 0.1f);

        Vector3 desiredPos = followTarget.position + behind + sideOffset;

        // 在 NavMesh 上找一个可走点（避免目标点不在 NavMesh）
        if (NavMesh.SamplePosition(desiredPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            agent.SetDestination(followTarget.position);
        }
    }
}
