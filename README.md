HairLoader mod for Terraria 1.4


This mod creates the possibility for other mods to add custom hairstyles to the game using Mod Calls.


Current features:
- Replaces the Stylist's NPC hairwindow with a custom hairwindow UI.
- Hairwindow is sorted based on mod name.
- Custom currency support by specifying the item's ID.
- Option to specify the price of the hairstyle and re-color.
- Option to add the Hairstyle to the character creation window.
- Option to add custom unlock conditions (requires additional Mod Calls on the Mod that added the hairstyle)
- Utilizes a custom way to save the selected hair, based on mod (class) name and hair name.
- Reverts the players last selected vanilla hair if the custom hairstyle is not present (for example disabled the mod).
- HairOffset value for the X offset, this way you can move the hair along the X-axis (useful for hairstyles that exceed the vanilla hair texture width)


Limitations for other mods: 
Any mod that modifies the following methods or has an On.Terraria call on: 

    On.Terraria.GameContent.UI.States.UICharacterCreation.MakeHairsylesMenu
    On.Terraria.DataStructures.PlayerDrawLayers.DrawPlayer_01_BackHair
    On.Terraria.DataStructures.PlayerDrawLayers.DrawPlayer_21_Head

These limitations are present because I use a detour to run my custom code. For obvious reasons I do not call origin / return.
(Maybe in the future I will replace this with custom playerlayers, this is not going to affect any mods)
