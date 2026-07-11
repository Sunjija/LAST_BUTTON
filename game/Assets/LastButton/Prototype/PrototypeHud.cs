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

            GUI.Box(new Rect(20f, 20f, 360f, 155f), string.Empty);
            GUI.Label(new Rect(35f, 30f, 330f, 32f), "LAST BUTTON — CORE PROTOTYPE", titleStyle);
            GUI.Label(new Rect(35f, 66f, 330f, 95f),
                $"공동 복구율: {Mathf.RoundToInt(state.RepairProgress * 100f)}%\n" +
                $"키카드: {(player.CarriedKeycard != null ? "소지 중" : state.KeycardWasTaken ? "분실/바닥" : "보안함") }\n" +
                $"개인 포드: {(state.PodCharged ? "충전 완료" : "미충전")}\n" +
                "WASD 이동 / E 상호작용 / G 내려놓기 / Q 밀치기 / R 초기화",
                bodyStyle);

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
                string result = state.Outcome == PrototypeOutcome.SoloEscape
                    ? "개인 탈출 성공\n보상을 혼자 독식함"
                    : "공동 탈출 성공\n모두가 보상을 나눠 가짐";
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
