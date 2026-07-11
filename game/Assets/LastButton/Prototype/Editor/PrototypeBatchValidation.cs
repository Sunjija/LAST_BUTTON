using System;
using UnityEditor;
using UnityEngine;

namespace LastButton.Prototype.Editor
{
    public static class PrototypeBatchValidation
    {
        public static void Run()
        {
            try
            {
                ValidateCommonEscape();
                ValidateSoloEscape();
                ValidateWorldBootstrap();
                Debug.Log("LAST_BUTTON_VALIDATION_PASSED");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError("LAST_BUTTON_VALIDATION_FAILED");
                EditorApplication.Exit(1);
            }
        }

        private static void ValidateCommonEscape()
        {
            GameObject root = new GameObject("Common Escape Validation");
            PrototypeState state = root.AddComponent<PrototypeState>();

            Require(state.AddRepair(0.8f), "Common repair should be accepted.");
            Require(Mathf.Approximately(state.RepairProgress, 1f), "Common repair should reach 100%.");
            Require(state.CompleteCommonEscape(), "Common escape should complete at 100% repair.");
            Require(state.Outcome == PrototypeOutcome.CommonEscape, "Outcome should be CommonEscape.");

            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void ValidateSoloEscape()
        {
            GameObject root = new GameObject("Solo Escape Validation");
            PrototypeState state = root.AddComponent<PrototypeState>();

            state.MarkKeycardTaken();
            Require(state.ChargePod(), "Pod charging should consume available shared power.");
            Require(Mathf.Approximately(state.RepairProgress, 0.1f), "Pod charge should consume 10% repair.");
            Require(state.CompleteSoloEscape(), "Solo escape should complete after pod charge.");
            Require(state.Outcome == PrototypeOutcome.SoloEscape, "Outcome should be SoloEscape.");

            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void ValidateWorldBootstrap()
        {
            PrototypeBootstrap.Rebuild();

            GameObject root = GameObject.Find("LAST_BUTTON_PROTOTYPE_ROOT");
            Require(root != null, "Prototype world root should be created.");
            Require(UnityEngine.Object.FindObjectsByType<PrototypePlayer>().Length == 1,
                "Prototype world should contain one player.");
            Require(UnityEngine.Object.FindObjectsByType<RepairConsole>().Length == 3,
                "Prototype world should contain three repair consoles.");
            Require(UnityEngine.Object.FindObjectsByType<CarryableKeycard>().Length == 1,
                "Prototype world should contain one physical keycard.");
            Require(UnityEngine.Object.FindObjectsByType<EscapePodCharger>().Length == 1,
                "Prototype world should contain one pod charger.");
            Require(UnityEngine.Object.FindObjectsByType<LastButtonTarget>().Length == 1,
                "Prototype world should contain one last button.");

            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
