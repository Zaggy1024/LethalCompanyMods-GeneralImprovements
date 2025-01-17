# GeneralImprovements

Everything is mostly configurable and improves (IMO) several things about the game, with more to come.

Check the config settings or the [wiki!](https://thunderstore.io/c/lethal-company/p/ShaosilGaming/GeneralImprovements/wiki/) I default most of the improvements to OFF to maximize compatibility with other mods, but there's some good QoL features in there you might not want to miss.

### GENERAL IMPROVEMENTS:
* Places newly picked up items in the hotbar in left-right order. May be disabled in config.
* Rearranges hotbar items when dropping things. May be disabled in config.
* Always puts two-handed items in slot 1 (makes selling things a bit faster). May be disabled in config.
* Decreases the time required in between scrolling through inventory slots. Configurable.
* Decreases the time required in between placing items on the company counter.
* Removes the wait to begin typing at the terminal when activating it.
* Skips the "bootup" menu animation when launching the game. May be disabled in config.
* Allows all items to be picked up before the game starts.
* Changes the "Beam up" hover tip for inverse teleporters to say "Beam out" for clarity.
* Moves the ship clipboard manual to start pinned to the wall. This makes it easier to find, and moves it out of the way of the teleport button.
* Introduces a degrees config option that snap rotates placeable ship objects in build mode, along with configurable modifier keybinds.
* Adds an option to make the ship's map camera rotated so that it faces straight up, instead of at an angle.
* The ESC key will now cancel out of ship build mode instead of bringing up the menu (similar to the terminal).
* Adds a config setting to hide the clipboard and sticky note.
* Changes the inverse teleporter cooldown to be the same as the regular (10 seconds). Both cooldowns may be customized in the config.
* The little monitors above the map screen will now share the power state of the map screen. Behavior may be disabled in config.
* Increases the max items able to be saved on the ship from 45 to 999 (affects saving and loading game files).
* Adds an option to show a small reticle on the HUD UI so you can see where you are pointing.
* Adds an option to hide scan node subtext if it has no scrap value or description.
* Adds an option to scan tools.
* [Host Must Have Mod] Adds the ability to toggle the fancy lamp on and off while holding it.
* Locks the camera while at the terminal so it doesn't keep getting pulled back if you try to move it. Behavior may be disabled in config.

### NEW FEATURES:
* A total of 14 customizable, screen-integrated monitors for the ship (instead of the vanilla 8)! Choose which monitor to put what item, including ship cams and background color.
* Available monitors to customize are things like weather, sales, time, credits, door power, ship cams, total days, total quotas, scrap remaining, etc. Work in progress, may be enabled individually in the config.
* If specified in the config, the game will automatically select ONLINE or LAN upon launch.
* Using the up/down arrow keys at the terminal will navigate through the previous (n) commands. (n) may range from 0-100 in the config.
* Using the left/right arrow keys at the terminal while viewing the radar will quickly cycle through available targets.
* [Host Only] Adds an option to specify starting money per player, as well as define a minumum credit amount. Accepts ranges from -1 (disabled/default) to 1000.
* [Host Only] Adds an option to prevent tools from getting struck by lightning. This is a bit of a cheat in some opinions, so I recommend leaving it off. If the host/server player has this enabled, it will apply to all clients.
* Adds an option to display text under the 'Deadline' monitor with the current ship loot total.
* [Host Required] Adds an option to roll over surplus credits to the next quota. If clients do NOT have this enabled, there will be visual desyncs only.
* Adds an option for adding a little medical station above the ship's charging station that heals you back to full health when used.

### MINOR BUGFIXES:
* Stops all non-scrap objects from showing value (when scanned and sold) when they do not actually have any.
* Removes the random 'n' in the middle-left of the terminal monitor when switching through radar cams.
* Flips the rotation of fire entrances 180 degrees so you are facing inside the facility when entering.
* Dead bodies will now instantly show as collected when teleported back to the ship.
* Fixes the scan terminal command and end-of-round scrap sum to include all valuables outside the ship, as well as factor in the current scrap value multiplier (under the hood, not related to company).
* The initial monitor view now shows the correct player name when first starting a round.
* Fixes ship scrap not being marked as 'in the ship' for clients when joining. This fixes several things client side, including terminal scans, extra scrap collection pings, and more.
* [Host Only] Whoopie cushions and flasks will no longer be hit by lightning. If the host/server player has this enabled, it will apply to all clients.
* [Host Only] When a new client connects to your lobby, they should see the correct position, rotation, and current emote of each player.
* When loading a file, items in the ship will no longer fall through shelves, tables, etc. May be disabled.
* Adds an option to fix the personal scanners sensitivity, making it function more reliably, for example being able to ping the ship on Rend.
* Fixes the ship scan node showing up outside of the ship while flying in to a moon.

This pairs well with my other mod, [FlashlightFix.](https://thunderstore.io/c/lethal-company/p/ShaosilGaming/FlashlightFix/)

I will probably keep adding to this as I see minor things that could be improved or fixed.

### WARNING

Because this mod can shift inventory slots around, if you play with people who do NOT also have this mod installed, the slots they are aware about on your character may be different, and as a result, they may not see you holding what you actually are.