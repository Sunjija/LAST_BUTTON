using UnityEngine;

namespace LastButton.Prototype
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PrototypePlayer : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float lookSensitivity = 2f;
        [SerializeField] private float interactDistance = 3f;
        [SerializeField] private float pushForce = 7f;

        private CharacterController controller;
        private Camera viewCamera;
        private Transform holdPoint;
        private float verticalVelocity;
        private float pitch;
        private bool botControlled;
        private Vector3 botMoveInput;
        private Vector3 knockbackVelocity;
        private float pushImmuneUntil;
        private PrototypeInteractionTarget activeTarget;
        private float holdTime;

        public CarryableKeycard CarriedKeycard { get; private set; }
        public bool IsBot => botControlled;
        public string DisplayName { get; private set; } = "PLAYER";
        public string CurrentPrompt { get; private set; }
        public float InteractionProgress01 { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            viewCamera = GetComponentInChildren<Camera>();
            holdPoint = new GameObject("KeycardHoldPoint").transform;
            holdPoint.SetParent(viewCamera != null ? viewCamera.transform : transform, false);
            holdPoint.localPosition = viewCamera != null
                ? new Vector3(0.35f, -0.25f, 0.8f)
                : new Vector3(0.45f, 1.1f, 0.35f);
        }

        private void Start()
        {
            if (!botControlled)
            {
                LockCursor(true);
            }
        }

        private void Update()
        {
            if (!botControlled && Input.GetKeyDown(KeyCode.Escape))
            {
                LockCursor(false);
            }

            if (!botControlled && Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
            {
                LockCursor(true);
            }

            if (PrototypeState.Instance == null || PrototypeState.Instance.Outcome == PrototypeOutcome.None)
            {
                if (botControlled)
                {
                    UpdateMovement(botMoveInput);
                }
                else
                {
                    UpdateLook();
                    Vector3 localInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
                    UpdateMovement(transform.TransformDirection(localInput));
                    UpdateInteraction();
                    UpdateCarryAndPush();
                }
            }
            else
            {
                ResetInteraction();
            }

            if (!botControlled && Input.GetKeyDown(KeyCode.R))
            {
                PrototypeBootstrap.Rebuild();
            }
        }

        public void ConfigureBot(string displayName)
        {
            botControlled = true;
            DisplayName = displayName;
            CurrentPrompt = string.Empty;
        }

        public void SetBotMove(Vector3 worldDirection)
        {
            botMoveInput = Vector3.ClampMagnitude(worldDirection, 1f);
            Vector3 planar = new Vector3(botMoveInput.x, 0f, botMoveInput.z);
            if (planar.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(planar.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 7f);
            }
        }

        public void PushOther(PrototypePlayer target)
        {
            if (target == null || target == this)
            {
                return;
            }

            Vector3 direction = (target.transform.position - transform.position).normalized;
            if (target.ReceivePush(direction))
            {
                PrototypeState.Instance?.Announce($"{DisplayName}이(가) {target.DisplayName}을(를) 밀쳤습니다.");
            }
        }

        public bool TryCarry(CarryableKeycard keycard)
        {
            if (CarriedKeycard != null || keycard == null)
            {
                return false;
            }

            CarriedKeycard = keycard;
            keycard.AttachTo(holdPoint);
            PrototypeState.Instance.MarkKeycardTaken();
            return true;
        }

        public void DropKeycard(Vector3 impulse)
        {
            if (CarriedKeycard == null)
            {
                return;
            }

            CarryableKeycard dropped = CarriedKeycard;
            CarriedKeycard = null;
            dropped.Drop(transform.position + transform.forward + Vector3.up * 0.5f, impulse);
        }

        public bool ReceivePush(Vector3 direction)
        {
            if (Time.time < pushImmuneUntil)
            {
                return false;
            }

            pushImmuneUntil = Time.time + 1.75f;
            knockbackVelocity += direction.normalized * pushForce + Vector3.up * 2.5f;
            DropKeycard(direction.normalized * pushForce);
            return true;
        }

        private void UpdateLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            float yaw = Input.GetAxis("Mouse X") * lookSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
            pitch = Mathf.Clamp(pitch, -85f, 85f);
            transform.Rotate(Vector3.up * yaw);
            viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void UpdateMovement(Vector3 worldInput)
        {
            Vector3 input = Vector3.ClampMagnitude(worldInput, 1f);
            Vector3 velocity = input * moveSpeed;

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += Physics.gravity.y * Time.deltaTime;
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime * 3.5f);
            velocity += knockbackVelocity;
            velocity.y += verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }

        private void UpdateInteraction()
        {
            PrototypeInteractionTarget target = FindInteractionTarget();
            CurrentPrompt = target != null ? target.GetPrompt(this) : string.Empty;

            if (target == null || !target.CanInteract(this) || !Input.GetKey(KeyCode.E))
            {
                ResetInteraction(target);
                return;
            }

            if (activeTarget != target)
            {
                activeTarget = target;
                holdTime = 0f;
            }

            holdTime += Time.deltaTime;
            InteractionProgress01 = Mathf.Clamp01(holdTime / Mathf.Max(0.01f, target.HoldSeconds));

            if (holdTime >= target.HoldSeconds)
            {
                target.Complete(this);
                ResetInteraction();
            }
        }

        private void UpdateCarryAndPush()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                DropKeycard(transform.forward * 2f);
            }

            if (!Input.GetKeyDown(KeyCode.Q))
            {
                return;
            }

            Ray ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            if (!Physics.SphereCast(ray, 0.3f, out RaycastHit hit, 2.2f))
            {
                return;
            }

            PrototypePlayer otherPlayer = hit.collider.GetComponentInParent<PrototypePlayer>();
            if (otherPlayer != null && otherPlayer != this)
            {
                PushOther(otherPlayer);
            }

            Rigidbody body = hit.collider.attachedRigidbody;
            if (body != null && !body.isKinematic)
            {
                body.AddForce(viewCamera.transform.forward * pushForce, ForceMode.Impulse);
            }
        }

        private PrototypeInteractionTarget FindInteractionTarget()
        {
            Ray ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                return null;
            }

            return hit.collider.GetComponentInParent<PrototypeInteractionTarget>();
        }

        private void ResetInteraction(PrototypeInteractionTarget nextTarget = null)
        {
            activeTarget = nextTarget;
            holdTime = 0f;
            InteractionProgress01 = 0f;
        }

        private static void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
