﻿using GameNetcodeStuff;
using GeneralImprovements.Utilities;
using HarmonyLib;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GeneralImprovements.Patches
{
    internal static class StartOfRoundPatch
    {
        private static MethodInfo _updatePlayerPositionClientRpcMethod;
        private static MethodInfo UpdatePlayerPositionClientRpcMethod
        {
            get
            {
                // Lazy load and cache reflection info
                if (_updatePlayerPositionClientRpcMethod == null)
                {
                    _updatePlayerPositionClientRpcMethod = typeof(PlayerControllerB).GetMethod("UpdatePlayerPositionClientRpc", BindingFlags.Instance | BindingFlags.NonPublic);
                }

                return _updatePlayerPositionClientRpcMethod;
            }
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(Start))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.High)]
        private static void Start(StartOfRound __instance)
        {
            // Set all grabbable objects as in ship if the game hasn't started (it never should be unless someone is using a join mid-game mod or something)
            if (!__instance.IsHost && __instance.inShipPhase)
            {
                var allGrabbables = Object.FindObjectsOfType<GrabbableObject>();
                foreach (var grabbable in allGrabbables)
                {
                    grabbable.isInElevator = true;
                    grabbable.isInShipRoom = true;
                    grabbable.scrapPersistedThroughRounds = true;
                }
            }

            // Rotate ship camera if specified
            if (Plugin.ShipMapCamDueNorth.Value)
            {
                Plugin.MLS.LogInfo("Rotating ship map camera to face north");
                Vector3 curAngles = __instance.mapScreen.mapCamera.transform.eulerAngles;
                __instance.mapScreen.mapCamera.transform.rotation = Quaternion.Euler(curAngles.x, 90, curAngles.z);
            }

            // Create new monitors only if any changes are specified
            if (Plugin.UseBetterMonitors.Value || !Plugin.ShowBlueMonitorBackground.Value || Plugin.ShowBackgroundOnAllScreens.Value
                || Plugin.MonitorBackgroundColor.Value != Plugin.MonitorBackgroundColor.DefaultValue.ToString() || Plugin.MonitorTextColor.Value != Plugin.MonitorTextColor.DefaultValue.ToString()
                || Plugin.ShipInternalCamFPS.Value != (int)Plugin.ShipInternalCamFPS.DefaultValue || Plugin.ShipInternalCamSizeMultiplier.Value != (int)Plugin.ShipInternalCamSizeMultiplier.DefaultValue
                || Plugin.ShipExternalCamFPS.Value != (int)Plugin.ShipExternalCamFPS.DefaultValue || Plugin.ShipExternalCamSizeMultiplier.Value != (int)Plugin.ShipExternalCamSizeMultiplier.DefaultValue
                || Plugin.ShipMonitorAssignments.Any(m => m.Value != m.DefaultValue.ToString()))
            {
                MonitorsHelper.CreateExtraMonitors();

                MonitorsHelper.UpdateTotalDaysMonitors();
                MonitorsHelper.UpdateTotalQuotasMonitors();
                MonitorsHelper.UpdateDaysSinceDeathMonitors();
            }

            // Add medical charging station
            ItemHelper.CreateMedStation();

            // Fix max items allowed to be stored
            __instance.maxShipItemCapacity = 999;

            // Remove the intro sound effects if needed
            if (!Plugin.SpeakerPlaysIntroVoice.Value)
            {
                Plugin.MLS.LogInfo("Nulling out intro audio SFX");
                StartOfRound.Instance.shipIntroSpeechSFX = null;
            }
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(OnShipLandedMiscEvents))]
        [HarmonyPostfix]
        private static void OnShipLandedMiscEvents()
        {
            RoundManagerPatch.EnableAndAttachShipScanNode();
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(ShipLeave))]
        [HarmonyPrefix]
        private static void ShipLeave()
        {
            // Easter egg
            if (RoundManagerPatch.CurShipNode != null)
            {
                RoundManagerPatch.CurShipNode.subText = "BYE LOL";
            }
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(ShipHasLeft))]
        [HarmonyPrefix]
        private static void ShipHasLeft()
        {
            // Manually destroy the node here since it won't be after attaching it to the ship
            if (RoundManagerPatch.CurShipNode != null)
            {
                Object.Destroy(RoundManagerPatch.CurShipNode.gameObject);
            }
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(SetMapScreenInfoToCurrentLevel))]
        [HarmonyPostfix]
        private static void SetMapScreenInfoToCurrentLevel()
        {
            // Update weather monitor text
            MonitorsHelper.UpdateWeatherMonitors();
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(SwitchMapMonitorPurpose))]
        [HarmonyPostfix]
        private static void SwitchMapMonitorPurpose(StartOfRound __instance, bool displayInfo)
        {
            var map = __instance.mapScreen;

            if (!displayInfo && !string.IsNullOrWhiteSpace(map?.radarTargets[map.targetTransformIndex]?.name))
            {
                __instance.mapScreenPlayerName.text = $"MONITORING: {__instance.mapScreen.radarTargets[__instance.mapScreen.targetTransformIndex].name}";
            }
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(OnClientConnect))]
        [HarmonyPrefix]
        private static void OnClientConnect(StartOfRound __instance)
        {
            if (__instance.IsServer)
            {
                // Add to the terminal credits before this function gets called so it is relayed to the connecting client
                TerminalPatch.AdjustGroupCredits(true);

                // Send positional, rotational, and emotional (heh) data to all when new people connect
                foreach (var connectedPlayer in __instance.allPlayerScripts.Where(p => p.isPlayerControlled))
                {
                    UpdatePlayerPositionClientRpcMethod.Invoke(connectedPlayer, new object[] { connectedPlayer.thisPlayerBody.localPosition, connectedPlayer.isInElevator,
                        connectedPlayer.isInHangarShipRoom, connectedPlayer.isExhausted, connectedPlayer.thisController.isGrounded });

                    if (connectedPlayer.performingEmote)
                    {
                        connectedPlayer.StartPerformingEmoteClientRpc();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(OnClientDisconnect))]
        [HarmonyPostfix]
        private static void OnClientDisconnect()
        {
            TerminalPatch.AdjustGroupCredits(false);
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(SyncShipUnlockablesClientRpc))]
        [HarmonyPostfix]
        private static void SyncShipUnlockablesClientRpc()
        {
            MonitorsHelper.UpdateShipScrapMonitors();
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(ResetShip))]
        [HarmonyPostfix]
        private static void ResetShip(StartOfRound __instance)
        {
            TerminalPatch.SetStartingMoneyPerPlayer();
            if (ItemHelper.MedStation != null)
            {
                ItemHelper.MedStation.MaxLocalPlayerHealth = __instance.localPlayerController?.health ?? 100;
            }

            MonitorsHelper.UpdateScrapLeftMonitors();
            MonitorsHelper.UpdateTotalDaysMonitors();
            MonitorsHelper.UpdateTotalQuotasMonitors();
            MonitorsHelper.UpdateDaysSinceDeathMonitors(true);
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(ReviveDeadPlayers))]
        [HarmonyPrefix]
        private static void ReviveDeadPlayers(StartOfRound __instance)
        {
            bool playersDied = __instance.allPlayerScripts.Any(p => p.isPlayerDead);
            MonitorsHelper.UpdateDaysSinceDeathMonitors(playersDied);
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(AllPlayersHaveRevivedClientRpc))]
        [HarmonyPostfix]
        private static void AllPlayersHaveRevivedClientRpc()
        {
            MonitorsHelper.UpdateShipScrapMonitors();
            MonitorsHelper.UpdateScrapLeftMonitors();
            MonitorsHelper.UpdateTotalDaysMonitors();
            MonitorsHelper.UpdateTotalQuotasMonitors();
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(LoadShipGrabbableItems))]
        [HarmonyPostfix]
        private static void LoadShipGrabbableItems()
        {
            MonitorsHelper.UpdateShipScrapMonitors();
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(Update))]
        [HarmonyPostfix]
        private static void Update()
        {
            MonitorsHelper.AnimateSpecialMonitors();
            MonitorsHelper.UpdateCreditsMonitors();
            MonitorsHelper.UpdateDoorPowerMonitors();
        }
    }
}