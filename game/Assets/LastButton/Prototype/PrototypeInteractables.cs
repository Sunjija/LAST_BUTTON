using UnityEngine;

namespace LastButton.Prototype
{
    public abstract class PrototypeInteractionTarget : MonoBehaviour
    {
        public abstract float HoldSeconds { get; }
        public abstract bool CanInteract(PrototypePlayer actor);
        public abstract string GetPrompt(PrototypePlayer actor);
        public abstract void Complete(PrototypePlayer actor);
    }

    public sealed class RepairConsole : PrototypeInteractionTarget
    {
        [SerializeField] private float repairAmount = 0.15f;
        [SerializeField] private float cooldown = 2f;
        private float readyAt;

        public override float HoldSeconds => 2f;

        public override bool CanInteract(PrototypePlayer actor)
        {
            return PrototypeState.Instance != null
                && PrototypeState.Instance.Outcome == PrototypeOutcome.None
                && PrototypeState.Instance.RepairProgress < 1f
                && Time.time >= readyAt;
        }

        public override string GetPrompt(PrototypePlayer actor)
        {
            if (PrototypeState.Instance != null && PrototypeState.Instance.RepairProgress >= 1f)
            {
                return "복구 완료";
            }

            return Time.time < readyAt ? "콘솔 재부팅 중" : "[E] 길게 눌러 수리";
        }

        public override void Complete(PrototypePlayer actor)
        {
            float appliedAmount = actor.IsBot ? repairAmount * 0.2f : repairAmount;
            if (PrototypeState.Instance.AddRepair(appliedAmount))
            {
                readyAt = Time.time + cooldown;
            }
        }
    }

    [RequireComponent(typeof(Rigidbody))]
    public sealed class CarryableKeycard : PrototypeInteractionTarget
    {
        private Rigidbody body;
        private Collider keycardCollider;
        private bool carried;
        private float pickupReadyAt;

        public override float HoldSeconds => 0.25f;
        public bool IsCarried => carried;
        public bool SabotageAvailable { get; private set; } = true;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            keycardCollider = GetComponent<Collider>();
        }

        public override bool CanInteract(PrototypePlayer actor)
        {
            return !carried
                && actor.CarriedKeycard == null
                && Time.time >= pickupReadyAt
                && PrototypeState.Instance != null
                && PrototypeState.Instance.Outcome == PrototypeOutcome.None
                && PrototypeState.Instance.RepairProgress >= 0.4f;
        }

        public override string GetPrompt(PrototypePlayer actor)
        {
            if (PrototypeState.Instance != null && PrototypeState.Instance.RepairProgress < 0.4f)
            {
                return "복구율 40%에서 키카드 잠금 해제";
            }

            return carried ? string.Empty : "[E] 간부용 키카드 탈취";
        }

        public override void Complete(PrototypePlayer actor)
        {
            actor.TryCarry(this);
        }

        public void AttachTo(Transform holdPoint)
        {
            carried = true;
            body.isKinematic = true;
            keycardCollider.enabled = false;
            transform.SetParent(holdPoint, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        public void Drop(Vector3 position, Vector3 impulse)
        {
            transform.SetParent(PrototypeBootstrap.WorldRoot, true);
            transform.position = position;
            carried = false;
            pickupReadyAt = Time.time + 2.5f;
            keycardCollider.enabled = true;
            body.isKinematic = false;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.AddForce(impulse, ForceMode.Impulse);
        }

        public bool TryConsumeSabotage()
        {
            if (!SabotageAvailable)
            {
                return false;
            }

            SabotageAvailable = false;
            return true;
        }
    }

    public sealed class EscapePodCharger : PrototypeInteractionTarget
    {
        public override float HoldSeconds => 8f;

        public override bool CanInteract(PrototypePlayer actor)
        {
            return PrototypeState.Instance != null
                && PrototypeState.Instance.Outcome == PrototypeOutcome.None
                && !PrototypeState.Instance.PodCharged
                && PrototypeState.Instance.RepairProgress >= 0.1f
                && actor.CarriedKeycard != null;
        }

        public override string GetPrompt(PrototypePlayer actor)
        {
            if (PrototypeState.Instance != null && PrototypeState.Instance.PodCharged)
            {
                return "포드 충전 완료";
            }

            return actor.CarriedKeycard == null
                ? "간부용 키카드 필요"
                : "[E] 길게 눌러 공동 전력 전용";
        }

        public override void Complete(PrototypePlayer actor)
        {
            PrototypeState.Instance.ChargePod();
        }
    }

    public sealed class LastButtonTarget : PrototypeInteractionTarget
    {
        public override float HoldSeconds => 5f;

        public override bool CanInteract(PrototypePlayer actor)
        {
            return PrototypeState.Instance != null
                && PrototypeState.Instance.Outcome == PrototypeOutcome.None
                && PrototypeState.Instance.PodCharged
                && actor.CarriedKeycard != null;
        }

        public override string GetPrompt(PrototypePlayer actor)
        {
            if (PrototypeState.Instance == null || !PrototypeState.Instance.PodCharged)
            {
                return "개인 포드 충전 필요";
            }

            return actor.CarriedKeycard == null
                ? "간부용 키카드 필요"
                : "[E] 5초간 눌러 혼자 탈출";
        }

        public override void Complete(PrototypePlayer actor)
        {
            PrototypeState.Instance.CompleteSoloEscape();
        }
    }

    public sealed class CommonExitTarget : PrototypeInteractionTarget
    {
        public override float HoldSeconds => 3f;

        public override bool CanInteract(PrototypePlayer actor)
        {
            return PrototypeState.Instance != null
                && PrototypeState.Instance.Outcome == PrototypeOutcome.None
                && PrototypeState.Instance.RepairProgress >= 1f;
        }

        public override string GetPrompt(PrototypePlayer actor)
        {
            return PrototypeState.Instance != null && PrototypeState.Instance.RepairProgress >= 1f
                ? "[E] 공동 탈출 실행"
                : "함선 복구 필요";
        }

        public override void Complete(PrototypePlayer actor)
        {
            PrototypeState.Instance.CompleteCommonEscape();
        }
    }
}
