using UnityEngine;
using StarterAssets;

[RequireComponent(typeof(CharacterController))]
public class PlayerCrouch : MonoBehaviour
{
    [Header("References")]
    public ThirdPersonController thirdPerson;   // Starter Assets move script
    public Animator animator;                   // Animator on PlayerArmature
    public Transform cameraRoot;                // Custom PlayerCameraRoot (Not MainCamera)

    [Header("Input")]
    public KeyCode crouchKey = KeyCode.C;
    public bool toggleMode = true;              // true = Toggle by press; false = Crouch by hold

    [Header("CharacterController Height")]
    public float standHeight = 1.8f;
    public float crouchHeight = 1.1f;
    public float standCenterY = 0.93f;
    public float crouchCenterY = 0.55f;

    [Header("Movement Speeds")]
    public float standMoveSpeed = 2.0f;         // ThirdPersonController Move Speed
    public float standSprintSpeed = 5.335f;     // ThirdPersonController Sprint Speed
    public float crouchMoveSpeed = 1.2f;        // Slower speed when crouching
    public float crouchSprintSpeed = 1.8f;      // Sprint forbidden while crouching, set low value

    [Header("Camera Root Offset")]
    public float standCamLocalY = 0f;           // Base on PlayerCameraRoot localPosition.y
    public float crouchCamLocalY = -0.35f;      // Camera down offset when crouching (Negative = Down)

    [Header("Ceiling Check")]
    public LayerMask obstacleMask = ~0;         // Default detect all layers
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

        // Init stand param with current value/ 用当前值初始化站立参数
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

        // 1) Adjust CharacterController
        cc.height = crouchHeight;
        cc.center = new Vector3(cc.center.x, crouchCenterY, cc.center.z);

        // 2) Adjust move speed (Starter Assets)
        if (thirdPerson != null)
        {
            thirdPerson.MoveSpeed = crouchMoveSpeed;
            thirdPerson.SprintSpeed = crouchSprintSpeed;
        }

        // 3) Adjust camera root height (No Cinemachine modify, no break third person)
        if (cameraRoot != null)
        {
            var p = camRootStartLocalPos;
            p.y = standCamLocalY + crouchCamLocalY;
            cameraRoot.localPosition = p;
        }

        // 4) Animator param
        if (animator != null)
            animator.SetBool("Crouch", true);
    }

    private void TryExitCrouch()
    {
        if (!IsCrouching) return;

        // Can't stand up if head blocked (Anti-clipping)
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
        // Sphere cast up from character center, check head obstacle
        Vector3 origin = transform.position + Vector3.up * (cc.center.y + (cc.height * 0.5f));
        float checkDist = (standHeight - crouchHeight) + headCheckExtra;
        return Physics.SphereCast(origin, headCheckRadius, Vector3.up, out _, checkDist, obstacleMask, QueryTriggerInteraction.Ignore);
    }
}
