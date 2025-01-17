﻿using GeneralImprovements.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GeneralImprovements.Patches
{
    internal static class GrabbableObjectsPatch
    {
        private static HashSet<GrabbableObject> _itemsToKeepInPlace = new HashSet<GrabbableObject>();

        [HarmonyPatch(typeof(GrabbableObject), "Start")]
        [HarmonyPrefix]
        private static void Start_Pre(GrabbableObject __instance)
        {
            if (__instance is ClipboardItem || (__instance is PhysicsProp && __instance.itemProperties.itemName == "Sticky note"))
            {
                // If this is the clipboard or sticky note, and we want to hide them, do so
                if (Plugin.HideClipboardAndStickyNote.Value)
                {
                    __instance.gameObject.SetActive(false);
                }
                // Otherwise, pin the clipboard to the wall when loading in
                else if (__instance is ClipboardItem)
                {
                    __instance.transform.SetLocalPositionAndRotation(new Vector3(2, 2.25f, -9.125f), Quaternion.Euler(0, -90, 90));
                }

                // Fix this being set elsewhere
                __instance.scrapPersistedThroughRounds = false;
            }

            // Ensure no non-scrap items have scrap value. This will update its value and description
            if (!__instance.itemProperties.isScrap)
            {
                if (__instance.GetComponentInChildren<ScanNodeProperties>() is ScanNodeProperties scanNode)
                {
                    // If the previous description had something other than "Value...", restore it afterwards
                    string oldDesc = scanNode.subText;
                    __instance.SetScrapValue(0);

                    if (oldDesc != null && !oldDesc.ToLower().StartsWith("value"))
                    {
                        scanNode.subText = oldDesc;
                    }
                }
                else
                {
                    __instance.scrapValue = 0;
                }
            }

            // Allow all items to be grabbed before game start
            if (!__instance.itemProperties.canBeGrabbedBeforeGameStart)
            {
                __instance.itemProperties.canBeGrabbedBeforeGameStart = true;
            }

            // Fix conductivity of certain objects
            if (__instance.itemProperties != null)
            {
                var nonConductiveItems = new string[] { "Flask", "Whoopie Cushion" };
                var tools = new string[] { "Jetpack", "Key", "Radar-booster", "Shovel", "Stop sign", "TZP-Inhalant", "Yield sign", "Zap gun" };

                if (nonConductiveItems.Any(n => __instance.itemProperties.itemName.Equals(n, StringComparison.OrdinalIgnoreCase))
                    || (Plugin.ToolsDoNotAttractLightning.Value && tools.Any(t => __instance.itemProperties.itemName.Equals(t, StringComparison.OrdinalIgnoreCase))))
                {
                    Plugin.MLS.LogInfo($"Item {__instance.itemProperties.itemName} being set to NON conductive.");
                    __instance.itemProperties.isConductiveMetal = false;
                }
            }

            // Prevent ship items from falling through objects when they spawn (prefix)
            if (Plugin.FixItemsFallingThrough.Value && __instance.isInShipRoom && __instance.isInElevator && __instance.scrapPersistedThroughRounds)
            {
                Plugin.MLS.LogDebug($"KEEPING {__instance.name} IN PLACE");
                _itemsToKeepInPlace.Add(__instance);
            }

            // Fix any min and max values being reversed
            if (__instance.itemProperties.minValue > __instance.itemProperties.maxValue)
            {
                int oldMin = __instance.itemProperties.minValue;
                __instance.itemProperties.minValue = __instance.itemProperties.maxValue;
                __instance.itemProperties.maxValue = oldMin;
            }

            // Add scan nodes to tools if requested
            if (Plugin.ScannableToolVals.Any(t => __instance.GetType() == t) && __instance.GetComponentInChildren<ScanNodeProperties>() == null)
            {
                var scanNodeObj = new GameObject("ScanNode", typeof(ScanNodeProperties), typeof(BoxCollider));
                scanNodeObj.layer = LayerMask.NameToLayer("ScanNode");
                scanNodeObj.transform.parent = __instance.transform;
                scanNodeObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                var newScanNode = scanNodeObj.GetComponent<ScanNodeProperties>();
                newScanNode.nodeType = 0;
                newScanNode.minRange = 1;
                newScanNode.maxRange = 13;
                newScanNode.headerText = __instance.itemProperties.itemName;
            }
        }

        [HarmonyPatch(typeof(GrabbableObject), "Start")]
        [HarmonyPostfix]
        private static void Start_Post(GrabbableObject __instance)
        {
            // Prevent ship items from falling through objects when they spawn (postfix)
            if (_itemsToKeepInPlace.Contains(__instance))
            {
                __instance.fallTime = 1;
                __instance.reachedFloorTarget = false;
                __instance.targetFloorPosition = __instance.transform.localPosition;
                _itemsToKeepInPlace.Remove(__instance);
            }
        }

        [HarmonyPatch(typeof(GrabbableObject), nameof(OnHitGround))]
        [HarmonyPostfix]
        private static void OnHitGround(GrabbableObject __instance)
        {
            if (__instance.isInShipRoom)
            {
                MonitorsHelper.UpdateShipScrapMonitors();
                MonitorsHelper.UpdateScrapLeftMonitors();
            }
        }

        public static KeyValuePair<int, int> GetOutsideScrap(bool approximate)
        {
            var fixedRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 91); // Why 91? Shrug. It's the offset in vanilla code and I kept it.
            var valuables = UnityEngine.Object.FindObjectsOfType<GrabbableObject>().Where(o => !o.isInShipRoom && !o.isInElevator && o.itemProperties.minValue > 0).ToList();

            float multiplier = RoundManager.Instance.scrapValueMultiplier;
            int sum = approximate ? (int)Math.Round(valuables.Sum(i => fixedRandom.Next(i.itemProperties.minValue, i.itemProperties.maxValue) * multiplier))
                : valuables.Sum(i => i.scrapValue);

            return new KeyValuePair<int, int>(valuables.Count, sum);
        }
    }
}