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
        private PrototypeInteractionTarget activeTarget;
        private float holdTime;

        public CarryableKeycard CarriedKeycard { get; private set; }
        public string CurrentPrompt { get; private set; }
        public float InteractionProgress01 { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            viewCamera = GetComponentInChildren<Camera>();
            holdPoint = new GameObject("KeycardHoldPoint").transform;
            holdPoint.SetParent(viewCamera.transform, false);
            holdPoint.localPosition = new Vector3(0.35f, -0.25f, 0.8f);
        }

        private void Start()
        {
            LockCursor(true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                LockCursor(false);
            }

            if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
            {
                LockCursor(true);
            }

            if (PrototypeState.Instance == null || PrototypeState.Instance.Outcome == PrototypeOutcome.None)
            {
                UpdateLook();
                UpdateMovement();
                UpdateInteraction();
                UpdateCarryAndPush();
            }
            else
            {
                ResetInteraction();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                PrototypeBootstrap.Rebuild();
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

        public void ReceivePush(Vector3 direction)
        {
            DropKeycard(direction.normalized * pushForce);
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

        private void UpdateMovement()
        {
            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1f);
            Vector3 velocity = transform.TransformDirection(input) * moveSpeed;

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += Physics.gravity.y * Time.deltaTime;
            velocity.y = verticalVelocity;
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
                otherPlayer.ReceivePush(viewCamera.transform.forward);
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
