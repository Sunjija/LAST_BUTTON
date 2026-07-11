using UnityEngine;

namespace LastButton.Prototype
{
    public static class PrototypeBootstrap
    {
        private const string RootName = "LAST_BUTTON_PROTOTYPE_ROOT";

        public static Transform WorldRoot { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (GameObject.Find(RootName) == null)
            {
                BuildWorld();
            }
        }

        public static void Rebuild()
        {
            GameObject existing = GameObject.Find(RootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            BuildWorld();
        }

        private static void BuildWorld()
        {
            Application.targetFrameRate = 120;
#if UNITY_EDITOR
            Time.timeScale = 1f;
#else
            Time.timeScale = Application.isBatchMode ? 8f : 1f;
#endif

            GameObject root = new GameObject(RootName);
            WorldRoot = root.transform;
            PrototypeState state = root.AddComponent<PrototypeState>();

            CreateLighting(root.transform);
            CreateRoom(root.transform);
            PrototypePlayer player = CreatePlayer(root.transform);
            CreateGameplayObjects(root.transform);
            CreateBot(root.transform, "MISO", new Vector3(-3f, 1f, -15f), new Color(0.95f, 0.35f, 0.5f), false, 0);
            CreateBot(root.transform, "BOLT", new Vector3(3f, 1f, -15f), new Color(0.3f, 0.8f, 1f), false, 1);
            CreateBot(root.transform, "RAT", new Vector3(0f, 1f, -12f), new Color(1f, 0.65f, 0.1f), true, 2);

            PrototypeHud hud = root.AddComponent<PrototypeHud>();
            hud.SetPlayer(player);
            state.Announce("수리 콘솔을 작동해 공동 복구율을 올리십시오.");
            Debug.Log("LAST_BUTTON_WORLD_READY");
        }

        private static void CreateLighting(Transform parent)
        {
            GameObject lightObject = new GameObject("Directional Light");
            lightObject.transform.SetParent(parent);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            RenderSettings.ambientLight = new Color(0.22f, 0.24f, 0.3f);
        }

        private static void CreateRoom(Transform parent)
        {
            CreateBlock("Floor", new Vector3(0f, -0.5f, 0f), new Vector3(44f, 1f, 44f), new Color(0.14f, 0.16f, 0.2f), parent);
            CreateBlock("Wall North", new Vector3(0f, 2f, 22f), new Vector3(44f, 5f, 0.5f), new Color(0.08f, 0.1f, 0.13f), parent);
            CreateBlock("Wall South", new Vector3(0f, 2f, -22f), new Vector3(44f, 5f, 0.5f), new Color(0.08f, 0.1f, 0.13f), parent);
            CreateBlock("Wall East", new Vector3(22f, 2f, 0f), new Vector3(0.5f, 5f, 44f), new Color(0.08f, 0.1f, 0.13f), parent);
            CreateBlock("Wall West", new Vector3(-22f, 2f, 0f), new Vector3(0.5f, 5f, 44f), new Color(0.08f, 0.1f, 0.13f), parent);

            CreateZone("ENGINEERING", new Vector3(-16f, 0.03f, -12f), new Color(0.05f, 0.25f, 0.3f), parent);
            CreateZone("LIFE SUPPORT", new Vector3(-16f, 0.03f, 10f), new Color(0.05f, 0.3f, 0.18f), parent);
            CreateZone("REACTOR", new Vector3(0f, 0.03f, 8f), new Color(0.28f, 0.12f, 0.04f), parent);
            CreateZone("EXECUTIVE", new Vector3(16f, 0.03f, -12f), new Color(0.3f, 0.22f, 0.04f), parent);
            CreateZone("ESCAPE POD", new Vector3(16f, 0.03f, 9f), new Color(0.32f, 0.04f, 0.05f), parent);

            CreateBlock("Escape Cover West", new Vector3(12f, 1.5f, 9f), new Vector3(1f, 3f, 6f), new Color(0.11f, 0.13f, 0.17f), parent);
            CreateBlock("Escape Cover East", new Vector3(20f, 1.5f, 12f), new Vector3(1f, 3f, 6f), new Color(0.11f, 0.13f, 0.17f), parent);
            CreateBlock("Escape Cover Center", new Vector3(15f, 1.5f, 13f), new Vector3(4f, 3f, 1f), new Color(0.11f, 0.13f, 0.17f), parent);
        }

        private static PrototypePlayer CreatePlayer(Transform parent)
        {
            GameObject playerObject = new GameObject("Prototype Player");
            playerObject.transform.SetParent(parent);
            playerObject.transform.position = new Vector3(0f, 1.1f, -18f);

            CharacterController controller = playerObject.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            GameObject cameraObject = new GameObject("Player Camera");
            cameraObject.transform.SetParent(playerObject.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 75f;
            cameraObject.AddComponent<AudioListener>();

            return playerObject.AddComponent<PrototypePlayer>();
        }

        private static void CreateGameplayObjects(Transform parent)
        {
            Vector3[] consolePositions =
            {
                new Vector3(-16f, 0.75f, -12f),
                new Vector3(-16f, 0.75f, 10f),
                new Vector3(0f, 0.75f, 8f)
            };

            for (int i = 0; i < consolePositions.Length; i++)
            {
                GameObject console = CreateBlock($"Repair Console {i + 1}", consolePositions[i], new Vector3(1.5f, 1.5f, 0.8f), new Color(0.1f, 0.75f, 0.9f), parent);
                console.AddComponent<RepairConsole>();
                CreateLabel(console.transform, "REPAIR", Vector3.up * 1.1f);
            }

            GameObject keycard = CreateBlock("Executive Keycard", new Vector3(16f, 0.45f, -12f), new Vector3(0.9f, 0.12f, 0.55f), new Color(1f, 0.8f, 0.1f), parent);
            keycard.AddComponent<Rigidbody>();
            keycard.AddComponent<CarryableKeycard>();
            CreateLabel(keycard.transform, "KEYCARD", Vector3.up * 0.65f);

            GameObject charger = CreateBlock("Escape Pod Charger", new Vector3(16f, 0.8f, 4f), new Vector3(2f, 1.6f, 1f), new Color(0.2f, 0.4f, 1f), parent);
            charger.AddComponent<EscapePodCharger>();
            CreateLabel(charger.transform, "POD CHARGER", Vector3.up * 1.15f);

            GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            button.name = "LAST BUTTON";
            button.transform.SetParent(parent);
            button.transform.position = new Vector3(16f, 0.6f, 16f);
            button.transform.localScale = new Vector3(1.2f, 0.25f, 1.2f);
            button.GetComponent<Renderer>().material = CreateMaterial(new Color(1f, 0.05f, 0.05f));
            button.AddComponent<LastButtonTarget>();
            CreateLabel(button.transform, "LAST BUTTON", Vector3.up * 1.25f);

            GameObject commonExit = CreateBlock("Common Exit", new Vector3(0f, 0.1f, 20f), new Vector3(5f, 0.2f, 2f), new Color(0.2f, 1f, 0.35f), parent);
            commonExit.AddComponent<CommonExitTarget>();
            CreateLabel(commonExit.transform, "COMMON EXIT", Vector3.up * 0.75f);

            for (int i = 0; i < 24; i++)
            {
                float x = -10f + (i % 6) * 4f;
                float z = -8f + (i / 6) * 5f;
                GameObject crate = CreateBlock($"Push Crate {i + 1}", new Vector3(x, 0.4f, z), Vector3.one * 0.8f, new Color(0.55f, 0.3f, 0.12f), parent);
                Rigidbody body = crate.AddComponent<Rigidbody>();
                body.mass = 2f;
            }
        }

        private static void CreateBot(Transform parent, string displayName, Vector3 position, Color color, bool opportunist, int consoleIndex)
        {
            GameObject botObject = new GameObject("Bot " + displayName);
            botObject.transform.SetParent(parent);
            botObject.transform.position = position;

            CharacterController controller = botObject.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.45f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(botObject.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.8f, 0.9f, 0.8f);
            body.GetComponent<Renderer>().material = CreateMaterial(color);
            Object.DestroyImmediate(body.GetComponent<Collider>());

            PrototypePlayer player = botObject.AddComponent<PrototypePlayer>();
            PrototypeBot bot = botObject.AddComponent<PrototypeBot>();
            bot.Configure(displayName, opportunist, consoleIndex);
            CreateLabel(botObject.transform, opportunist ? displayName + " ?" : displayName, Vector3.up * 2.25f);
        }

        private static void CreateZone(string label, Vector3 position, Color color, Transform parent)
        {
            GameObject zone = CreateBlock(label + " Zone", position, new Vector3(8f, 0.05f, 8f), color, parent);
            CreateLabel(zone.transform, label, Vector3.up * 0.35f);
        }

        private static GameObject CreateBlock(string name, Vector3 position, Vector3 scale, Color color, Transform parent)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent);
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().material = CreateMaterial(color);
            return block;
        }

        private static Material CreateMaterial(Color color)
        {
            Shader shader = Resources.Load<Shader>("PrototypeColor");
            if (shader == null)
            {
                throw new System.InvalidOperationException("PrototypeColor shader is missing from Resources.");
            }

            Material material = new Material(shader);
            material.color = color;
            return material;
        }

        private static void CreateLabel(Transform parent, string text, Vector3 localPosition)
        {
            GameObject labelObject = new GameObject(text + " Label");
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = localPosition;
            labelObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.12f;
            label.fontSize = 42;
            label.color = Color.white;
        }
    }
}
