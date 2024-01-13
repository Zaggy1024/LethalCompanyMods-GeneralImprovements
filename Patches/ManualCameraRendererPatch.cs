﻿using HarmonyLib;

namespace GeneralImprovements.Patches
{
    internal static class ManualCameraRendererPatch
    {
        [HarmonyPatch(typeof(ManualCameraRenderer), nameof(updateMapTarget), MethodType.Enumerator)]
        [HarmonyPostfix]
        private static void updateMapTarget(int ___setRadarTargetIndex, bool ___calledFromRPC, bool __result)
        {
            // Wait until the enumerator is complete (result == false)
            Plugin.MLS.LogInfo($"updateMapTarget {__result}");
            var instance = StartOfRound.Instance.mapScreen;
            bool inTerminal = GameNetworkManager.Instance.localPlayerController.inTerminalMenu;
            bool curNodeIsSwitchCam = TerminalPatch.Instance.currentNode?.name == "SwitchedCam";
            bool validTarget = instance.radarTargets != null && instance.radarTargets[___setRadarTargetIndex] != null;

            if (!__result && !___calledFromRPC && inTerminal && curNodeIsSwitchCam && validTarget)
            {
                Plugin.MLS.LogInfo("Updating terminal node text to player name");
                string targetName = instance.radarTargets[___setRadarTargetIndex].name;
                TerminalPatch.Instance.screenText.text += $"Switched radar to {targetName}.\n\n";
                TerminalPatch.Instance.currentText = TerminalPatch.Instance.screenText.text;
                TerminalPatch.Instance.textAdded = 0;
                TerminalPatch.Instance.screenText.ActivateInputField();
                TerminalPatch.Instance.screenText.Select();
                TerminalPatch.Instance.scrollBarVertical.value = 0;
            }
        }

        [HarmonyPatch(typeof(ManualCameraRenderer), nameof(SwitchScreenOn))]
        [HarmonyPostfix]
        private static void SwitchScreenOn(bool on)
        {
            if (Plugin.SyncLittleScreensPower.Value)
            {
                StartOfRound.Instance.profitQuotaMonitorBGImage.gameObject.SetActive(on);
                StartOfRound.Instance.profitQuotaMonitorText.gameObject.SetActive(on);
                StartOfRound.Instance.deadlineMonitorText.gameObject.SetActive(on);
                StartOfRound.Instance.deadlineMonitorBGImage.gameObject.SetActive(on);
                SceneHelper.ToggleExtraMonitorPower(on);
            }
        }
    }
}