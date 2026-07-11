using UnityEngine;

namespace LastButton.Prototype
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        private PrototypePlayer player;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle centerStyle;

        public void SetPlayer(PrototypePlayer prototypePlayer)
        {
            player = prototypePlayer;
        }

        private void OnGUI()
        {
            EnsureStyles();
            PrototypeState state = PrototypeState.Instance;
            if (state == null || player == null)
            {
                return;
            }

            GUI.Box(new Rect(20f, 20f, 410f, 245f), string.Empty);
            GUI.Label(new Rect(35f, 30f, 330f, 32f), "LAST BUTTON — CORE PROTOTYPE", titleStyle);
            int minutes = Mathf.FloorToInt(state.TimeRemaining / 60f);
            int seconds = Mathf.FloorToInt(state.TimeRemaining % 60f);
            GUI.Label(new Rect(35f, 66f, 380f, 185f),
                $"남은 시간: {minutes:00}:{seconds:00}\n" +
                $"공동 복구율: {Mathf.RoundToInt(state.RepairProgress * 100f)}%\n" +
                $"공동 탈출 예상 보상: {state.CommonReward:N0}\n" +
                $"개인 탈출 예상 보상: {state.SoloReward:N0}\n" +
                $"키카드: {(player.CarriedKeycard != null ? "소지 중" : state.KeycardWasTaken ? "분실/바닥" : "보안함") }\n" +
                $"개인 포드: {(state.PodCharged ? "충전 완료" : "미충전")}\n" +
                $"질주 에너지: {Mathf.RoundToInt(player.Sprint01 * 100f)}% / 보안 교란기: {player.SabotageCharges}회\n" +
                "WASD 이동 / Shift 질주 / E 상호작용 / F 교란기 / G 내려놓기 / Q 몸통박치기 / R 초기화",
                bodyStyle);

            PrototypeBot[] bots = Object.FindObjectsByType<PrototypeBot>();
            GUI.Box(new Rect(Screen.width - 390f, 20f, 370f, 45f + bots.Length * 34f), string.Empty);
            GUI.Label(new Rect(Screen.width - 375f, 30f, 340f, 28f), "CREW ACTIVITY", titleStyle);
            for (int i = 0; i < bots.Length; i++)
            {
                string marker = bots[i].IsOpportunist ? "?" : "✓";
                PrototypePlayer botPlayer = bots[i].GetComponent<PrototypePlayer>();
                GUI.Label(new Rect(Screen.width - 375f, 62f + i * 32f, 340f, 28f),
                    $"[{marker}] {botPlayer.DisplayName}: {bots[i].Activity}", bodyStyle);
            }

            GUI.Box(new Rect(20f, Screen.height - 82f, Screen.width - 40f, 60f), string.Empty);
            GUI.Label(new Rect(35f, Screen.height - 70f, Screen.width - 70f, 42f), state.LastAnnouncement, bodyStyle);

            GUI.Label(new Rect(Screen.width * 0.5f - 10f, Screen.height * 0.5f - 16f, 20f, 32f), "+", centerStyle);

            if (!string.IsNullOrWhiteSpace(player.CurrentPrompt))
            {
                GUI.Label(new Rect(Screen.width * 0.5f - 240f, Screen.height * 0.65f, 480f, 35f), player.CurrentPrompt, centerStyle);
            }

            if (player.InteractionProgress01 > 0f)
            {
                float width = 320f;
                Rect background = new Rect(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.7f, width, 18f);
                GUI.Box(background, string.Empty);
                GUI.Box(new Rect(background.x + 2f, background.y + 2f, (width - 4f) * player.InteractionProgress01, 14f), string.Empty);
            }

            if (state.Outcome != PrototypeOutcome.None)
            {
                string result = state.Outcome switch
                {
                    PrototypeOutcome.SoloEscape => "개인 탈출 성공\n보상을 혼자 독식함",
                    PrototypeOutcome.CommonEscape => "공동 탈출 성공\n모두가 보상을 나눠 가짐",
                    _ => "함선 복구 실패\n아무도 탈출하지 못함"
                };
                GUI.Box(new Rect(Screen.width * 0.5f - 250f, Screen.height * 0.5f - 90f, 500f, 180f), string.Empty);
                GUI.Label(new Rect(Screen.width * 0.5f - 230f, Screen.height * 0.5f - 55f, 460f, 120f), result + "\n\nR을 눌러 재시작", centerStyle);
            }
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.white },
                wordWrap = true
            };
            centerStyle = new GUIStyle(bodyStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };
        }
    }
}
