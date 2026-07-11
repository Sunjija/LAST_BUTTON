using UnityEngine;

namespace LastButton.Prototype
{
    [RequireComponent(typeof(PrototypePlayer))]
    public sealed class PrototypeBot : MonoBehaviour
    {
        private PrototypePlayer player;
        private bool opportunist;
        private int preferredConsole;
        private PrototypeInteractionTarget activeTarget;
        private float actionProgress;
        private float pushReadyAt;
        private float stunnedUntil;

        public string Activity { get; private set; } = "대기 중";
        public bool IsOpportunist => opportunist;
        public bool IsStunned => Time.time < stunnedUntil;

        public void Configure(string displayName, bool canBetray, int consoleIndex)
        {
            player = GetComponent<PrototypePlayer>();
            player.ConfigureBot(displayName);
            opportunist = canBetray;
            preferredConsole = consoleIndex;
        }

        private void Update()
        {
            PrototypeState state = PrototypeState.Instance;
            if (player == null || state == null || state.Outcome != PrototypeOutcome.None)
            {
                player?.SetBotMove(Vector3.zero);
                return;
            }

            if (IsStunned)
            {
                Activity = $"보안 교란됨 ({Mathf.CeilToInt(stunnedUntil - Time.time)}초)";
                player.SetBotMove(Vector3.zero);
                ResetAction(null);
                return;
            }

            PrototypePlayer carrier = FindKeycardCarrier();

            if (player.CarriedKeycard != null)
            {
                if (opportunist && state.PodCharged)
                {
                    TryUseSabotageNearSecurity();
                }

                PrototypeInteractionTarget destination = state.PodCharged
                    ? FindOne<LastButtonTarget>()
                    : FindOne<EscapePodCharger>();
                Activity = state.PodCharged ? "마지막 버튼으로 도주 중" : "개인 포드 충전 중";
                MoveAndInteract(destination);
                return;
            }

            if (opportunist && carrier != null && carrier != player)
            {
                Activity = $"{carrier.DisplayName}의 키카드를 노리는 중";
                ChaseAndPush(carrier);
                return;
            }

            bool chargeAlertMature = state.PodCharged && Time.time - state.PodChargedAt >= 4.5f;
            bool keycardMissingTooLong = Time.time - state.KeycardTakenAt >= 22f;
            bool loyalistsAreSuspicious = carrier != null
                && (chargeAlertMature || keycardMissingTooLong);
            bool assignedSecurity = preferredConsole == 0;
            if (!opportunist && assignedSecurity && loyalistsAreSuspicious)
            {
                Activity = $"{carrier.DisplayName} 추격 중";
                ChaseAndPush(carrier);
                return;
            }

            CarryableKeycard keycard = FindOne<CarryableKeycard>();
            if (opportunist && state.RepairProgress >= 0.4f && keycard != null && !keycard.IsCarried)
            {
                Activity = "키카드 탈취 시도 중";
                MoveAndInteract(keycard);
                return;
            }

            if (!opportunist && state.RepairProgress >= 1f)
            {
                Activity = "공동 탈출 구역으로 이동 중";
                MoveAndInteract(FindOne<CommonExitTarget>());
                return;
            }

            RepairConsole console = FindPreferredConsole();
            Activity = opportunist ? "수리하며 기회를 보는 중" : "함선 수리 중";
            MoveAndInteract(console);
        }

        private void ChaseAndPush(PrototypePlayer target)
        {
            if (target == null)
            {
                player.SetBotMove(Vector3.zero);
                return;
            }

            Vector3 offset = target.transform.position - transform.position;
            offset.y = 0f;
            if (offset.magnitude > 1.8f)
            {
                player.SetBotMove(GetSteeredDirection(offset.normalized, target.transform));
                ResetAction(null);
                return;
            }

            player.SetBotMove(Vector3.zero);
            if (Time.time >= pushReadyAt)
            {
                player.PushOther(target);
                pushReadyAt = Time.time + 4f;
            }
        }

        public void Stun(float seconds)
        {
            stunnedUntil = Mathf.Max(stunnedUntil, Time.time + seconds);
            player?.SetBotMove(Vector3.zero);
        }

        private void TryUseSabotageNearSecurity()
        {
            if (player.SabotageCharges <= 0)
            {
                return;
            }

            PrototypeBot[] bots = Object.FindObjectsByType<PrototypeBot>();
            foreach (PrototypeBot bot in bots)
            {
                if (!bot.opportunist && Vector3.Distance(transform.position, bot.transform.position) <= 7.5f)
                {
                    player.UseSabotage();
                    return;
                }
            }
        }

        private void MoveAndInteract(PrototypeInteractionTarget target)
        {
            if (target == null)
            {
                player.SetBotMove(Vector3.zero);
                ResetAction(null);
                return;
            }

            Vector3 offset = target.transform.position - transform.position;
            offset.y = 0f;
            if (offset.magnitude > 1.65f)
            {
                player.SetBotMove(GetSteeredDirection(offset.normalized, target.transform));
                ResetAction(target);
                return;
            }

            player.SetBotMove(Vector3.zero);
            if (!target.CanInteract(player))
            {
                ResetAction(target);
                return;
            }

            if (activeTarget != target)
            {
                activeTarget = target;
                actionProgress = 0f;
            }

            actionProgress += Time.deltaTime;
            if (actionProgress >= target.HoldSeconds)
            {
                target.Complete(player);
                actionProgress = 0f;
            }
        }

        private RepairConsole FindPreferredConsole()
        {
            RepairConsole[] consoles = Object.FindObjectsByType<RepairConsole>();
            if (consoles.Length == 0)
            {
                return null;
            }

            return consoles[Mathf.Abs(preferredConsole) % consoles.Length];
        }

        private static PrototypePlayer FindKeycardCarrier()
        {
            PrototypePlayer[] players = Object.FindObjectsByType<PrototypePlayer>();
            foreach (PrototypePlayer candidate in players)
            {
                if (candidate.CarriedKeycard != null)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static T FindOne<T>() where T : Object
        {
            T[] matches = Object.FindObjectsByType<T>();
            return matches.Length > 0 ? matches[0] : null;
        }

        private Vector3 GetSteeredDirection(Vector3 desiredDirection, Transform target)
        {
            Vector3 origin = transform.position + Vector3.up * 0.8f + desiredDirection * 0.6f;
            if (!Physics.SphereCast(origin, 0.3f, desiredDirection, out RaycastHit hit, 1.25f))
            {
                return desiredDirection;
            }

            if (hit.transform == target || hit.transform.IsChildOf(target))
            {
                return desiredDirection;
            }

            PrototypePlayer hitPlayer = hit.collider.GetComponentInParent<PrototypePlayer>();
            if (hitPlayer == player)
            {
                return desiredDirection;
            }

            float turn = preferredConsole % 2 == 0 ? 55f : -55f;
            return Quaternion.Euler(0f, turn, 0f) * desiredDirection;
        }

        private void ResetAction(PrototypeInteractionTarget nextTarget)
        {
            activeTarget = nextTarget;
            actionProgress = 0f;
        }
    }
}
