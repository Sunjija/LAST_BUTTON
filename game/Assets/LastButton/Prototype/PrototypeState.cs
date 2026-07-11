using System;
using UnityEngine;

namespace LastButton.Prototype
{
    public enum PrototypeOutcome
    {
        None,
        CommonEscape,
        SoloEscape
    }

    public sealed class PrototypeState : MonoBehaviour
    {
        public static PrototypeState Instance { get; private set; }

        public float RepairProgress { get; private set; } = 0.2f;
        public bool PodCharged { get; private set; }
        public bool KeycardWasTaken { get; private set; }
        public PrototypeOutcome Outcome { get; private set; }
        public string LastAnnouncement { get; private set; } = "함선 복구를 시작하십시오.";

        public event Action Changed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool AddRepair(float amount)
        {
            if (Outcome != PrototypeOutcome.None || RepairProgress >= 1f)
            {
                return false;
            }

            RepairProgress = Mathf.Clamp01(RepairProgress + amount);
            Announce(RepairProgress >= 1f
                ? "공동 탈출 구역이 개방되었습니다."
                : $"공동 복구율 {Mathf.RoundToInt(RepairProgress * 100f)}%");
            return true;
        }

        public void MarkKeycardTaken()
        {
            if (KeycardWasTaken)
            {
                return;
            }

            KeycardWasTaken = true;
            Announce("간부용 키카드가 보안함에서 제거되었습니다.");
        }

        public bool ChargePod()
        {
            if (Outcome != PrototypeOutcome.None || PodCharged || RepairProgress < 0.1f)
            {
                return false;
            }

            RepairProgress = Mathf.Clamp01(RepairProgress - 0.1f);
            PodCharged = true;
            Announce("비인가 전력 사용 감지. 개인 포드 충전 완료.");
            return true;
        }

        public bool CompleteSoloEscape()
        {
            if (Outcome != PrototypeOutcome.None || !PodCharged)
            {
                return false;
            }

            Outcome = PrototypeOutcome.SoloEscape;
            Announce("개인 포드 발사. 한 명이 보상을 독식했습니다.");
            return true;
        }

        public bool CompleteCommonEscape()
        {
            if (Outcome != PrototypeOutcome.None || RepairProgress < 1f)
            {
                return false;
            }

            Outcome = PrototypeOutcome.CommonEscape;
            Announce("공동 구조 성공. 모든 생존자가 보상을 획득합니다.");
            return true;
        }

        public void Announce(string message)
        {
            LastAnnouncement = message;
            Changed?.Invoke();
        }
    }
}
