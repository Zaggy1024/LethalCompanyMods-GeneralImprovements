﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GeneralImprovements.Patches;
using HarmonyLib;

namespace GeneralImprovements
{
    [BepInPlugin(Metadata.GUID, Metadata.PLUGIN_NAME, Metadata.VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource MLS { get; private set; }

        private const string GeneralSection = "General";
        public static ConfigEntry<bool> PickupInOrder { get; private set; }
        public static ConfigEntry<bool> RearrangeOnDrop { get; private set; }
        public static ConfigEntry<bool> TwoHandedInSlotOne { get; private set; }
        public static ConfigEntry<float> ScrollDelay { get; private set; }

        private void Awake()
        {
            MLS = Logger;

            PickupInOrder = Config.Bind(GeneralSection, nameof(PickupInOrder), true, "When picking up items, will always put them in left - right order.");
            RearrangeOnDrop = Config.Bind(GeneralSection, nameof(RearrangeOnDrop), true, "When dropping items, will rearrange other inventory items to ensure slots are filled left - right.");
            TwoHandedInSlotOne = Config.Bind(GeneralSection, nameof(TwoHandedInSlotOne), true, $"When picking up a two handed item, it will always place it in slot 1 and shift things to the right if needed. Makes selling quicker when paired with {nameof(RearrangeOnDrop)}.");
            ScrollDelay = Config.Bind(GeneralSection, nameof(ScrollDelay), 0.1f, $"The minimum time you must wait to scroll to another item in your inventory. Ignores values outside of 0.05 - 0.3. Vanilla: 0.3.");
            MLS.LogInfo("Configuration Initialized.");

            Harmony.CreateAndPatchAll(typeof(PlayerControllerBPatch));
            MLS.LogInfo("PlayerControllerB patched.");

            Harmony.CreateAndPatchAll(typeof(GrabbableObjectPatch));
            MLS.LogInfo("GrabbableObjects patched.");

            Harmony.CreateAndPatchAll(typeof(DepositItemsDeskPatch));
            MLS.LogInfo("DepositItemsDesk patched.");

            Harmony.CreateAndPatchAll(typeof(TerminalPatch));
            MLS.LogInfo("Terminal patched.");

            MLS.LogInfo($"{Metadata.PLUGIN_NAME} v{Metadata.VERSION} fully loaded.");
        }
    }
}