using UnityEngine;
using StarterAssets;

[RequireComponent(typeof(CharacterController))]
public class PlayerCrouch : MonoBehaviour
{
    [Header("References")]
    public ThirdPersonController thirdPerson;   // Starter Assets 的移动脚本
    public Animator animator;                   // PlayerArmature 上的 Animator
    public Transform cameraRoot;                // 你的 PlayerCameraRoot（不是 MainCamera）

    [Header("Input")]
    public KeyCode crouchKey = KeyCode.C;
    public bool toggleMode = true;              // true=按一下切换；false=按住下蹲

    [Header("CharacterController Height")]
    public float standHeight = 1.8f;
    public float crouchHeight = 1.1f;
    public float standCenterY = 0.93f;          // 你当前截图里 CharacterController Center Y=0.93
    public float crouchCenterY = 0.55f;

    [Header("Movement Speeds")]
    public float standMoveSpeed = 2.0f;         // 你 ThirdPersonController 的 Move Speed
    public float standSprintSpeed = 5.335f;     // 你 ThirdPersonController 的 Sprint Speed
    public float crouchMoveSpeed = 1.2f;        // 下蹲移动更慢
    public float crouchSprintSpeed = 1.8f;      // 下蹲时通常不允许冲刺，可设很低

    [Header("Camera Root Offset")]
    public float standCamLocalY = 0f;           // 以当前 PlayerCameraRoot 的 localPosition.y 为基准
    public float crouchCamLocalY = -0.35f;      // 下蹲时相机降低多少（负值=往下）

    [Header("Ceiling Check")]
    public LayerMask obstacleMask = ~0;         // 默认检测所有层（你也可改成 Ground/Default）
    public float headCheckRadius = 0.2f;
    public float headCheckExtra = 0.05f;

    public bool IsCrouching { get; private set; }

    private CharacterController cc;
    private Vector3 camRootStartLocalPos;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();

        if (thirdPerson == null) thirdPerson = GetComponent<ThirdPersonController>();
        if (animator == null) animator = GetComponent<Animator>();
        if (cameraRoot != null) camRootStartLocalPos = cameraRoot.localPosition;

        // 用当前值初始化站立参数（避免你后面改过 Inspector 忘了同步）
        standHeight = cc.height;
        standCenterY = cc.center.y;

        if (cameraRoot != null)
            standCamLocalY = camRootStartLocalPos.y;
    }

    private void Update()
    {
        bool wantToggle = Input.GetKeyDown(crouchKey);
        bool wantHold = Input.GetKey(crouchKey);

        if (toggleMode)
        {
            if (wantToggle)
            {
                if (!IsCrouching) EnterCrouch();
                else TryExitCrouch();
            }
        }
        else
        {
            if (wantHold) EnterCrouch();
            else TryExitCrouch();
        }
    }

    private void EnterCrouch()
    {
        if (IsCrouching) return;

        IsCrouching = true;

        // 1) 调整 CharacterController
        cc.height = crouchHeight;
        cc.center = new Vector3(cc.center.x, crouchCenterY, cc.center.z);

        // 2) 调整移动速度（Starter Assets）
        if (thirdPerson != null)
        {
            thirdPerson.MoveSpeed = crouchMoveSpeed;
            thirdPerson.SprintSpeed = crouchSprintSpeed;
        }

        // 3) 调整相机根节点高度（不改 Cinemachine，不破坏第三人称）
        if (cameraRoot != null)
        {
            var p = camRootStartLocalPos;
            p.y = standCamLocalY + crouchCamLocalY;
            cameraRoot.localPosition = p;
        }

        // 4) Animator 参数
        if (animator != null)
            animator.SetBool("Crouch", true);
    }

    private void TryExitCrouch()
    {
        if (!IsCrouching) return;

        // 头顶有障碍，不允许站起来（防止穿模）
        if (HasCeiling())
            return;

        ExitCrouch();
    }

    private void ExitCrouch()
    {
        IsCrouching = false;

        cc.height = standHeight;
        cc.center = new Vector3(cc.center.x, standCenterY, cc.center.z);

        if (thirdPerson != null)
        {
            thirdPerson.MoveSpeed = standMoveSpeed;
            thirdPerson.SprintSpeed = standSprintSpeed;
        }

        if (cameraRoot != null)
        {
            var p = camRootStartLocalPos;
            p.y = standCamLocalY;
            cameraRoot.localPosition = p;
        }

        if (animator != null)
            animator.SetBool("Crouch", false);
    }

    private bool HasCeiling()
    {
        // 从角色中心往上检测一个小球，判断是否有顶
        Vector3 origin = transform.position + Vector3.up * (cc.center.y + (cc.height * 0.5f));
        float checkDist = (standHeight - crouchHeight) + headCheckExtra;
        return Physics.SphereCast(origin, headCheckRadius, Vector3.up, out _, checkDist, obstacleMask, QueryTriggerInteraction.Ignore);
    }
}
