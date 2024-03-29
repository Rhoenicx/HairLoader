using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.UI;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ReLogic.Content;
using HairLoader.UI;
using static Terraria.ModLoader.ModContent;
using Steamworks;
using log4net.Repository.Hierarchy;
using IL.Terraria.GameContent.UI.States;

namespace HairLoader
{    
    public class PlayerHairEntry
    {
        public Asset<Texture2D> hair { get; set; }      // Front hair texture file
        public Asset<Texture2D> hairAlt { get; set; }   // back hair texture file
        public float hairOffset { get; set; }           // X offset of the hairstyle
        public int index { get; set; }                  // Index ID of the vanilla hair number, modded = -1
        public int currency { get; set; }               // Determines the currency that will be used to buy this HairStyle, -1 = coins, >-1 = ItemID
        public int hairPrice { get; set; }              // The price of this Hairstyle in the amount of currency above
        public int colorPrice { get; set; }             // The price to change the color of this Hairstyle in the amount of currency above
        public bool CharacterCreator { get; set; }      // Whether this hairstyle is available at the character creator
        public bool UnlockCondition { get; set; }       // Whether this hairstyle must be unlocked to appear in the Stylist's hairwindow
        public bool HairWindowVisible { get; set; }
        public string UnlockHint { get; set; }          // Hint for the player how to unlock this hairstyle
    }

    class HairLoader : Mod
    {
        internal static HairLoader Instance;

        // Dictionary that stores all the PlayerHairEntries
        public static Dictionary<string, Dictionary<string, PlayerHairEntry>> HairTable = new Dictionary<string, Dictionary<string, PlayerHairEntry>>();
        public static Dictionary<string, string> ModDisplayNames = new Dictionary<string, string>();

        // Point to this Mod object Instance.
        public HairLoader()
        {
            Instance = this;
        }

        public override void Load()
        {
            IL.Terraria.DataStructures.PlayerDrawLayers.DrawPlayer_01_BackHair += HookBackHair;
            IL.Terraria.DataStructures.PlayerDrawLayers.DrawPlayer_21_Head += HookHead;
            IL.Terraria.GameContent.UI.States.UICharacterCreation.MakeHairsylesMenu += HookUICharacterCreation_MakeHairsylesMenu;

            // Code not ran on server
            if (!Terraria.Main.dedServ)
            {
                HairTable = new Dictionary<string, Dictionary<string, PlayerHairEntry>>();
                ModDisplayNames = new Dictionary<string, string>();

                // Load UI textures
                Main.instance.LoadItem(ItemID.PlatinumCoin);
                Main.instance.LoadItem(ItemID.GoldCoin);
                Main.instance.LoadItem(ItemID.SilverCoin);
                Main.instance.LoadItem(ItemID.CopperCoin);
                HairSlot.backgroundTexture = Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanel");
                HairSlot.highlightTexture = Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanelHighlight");
                HairSlot.hoverTexture = Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanelBorder");

                // Load all the Vanilla HairStyles 
                LoadVanillaHair();             
            }
            base.Load();
        }

        private void UICharacterCreation_MakeHairsylesMenu(ILContext il)
        {
            throw new NotImplementedException();
        }

        public override void PostSetupContent()
        {
            // Here is the example to add a new HairStyle
            if (!Main.dedServ)
            {
                ModLoader.TryGetMod("HairLoader", out Mod HairLoader);
                if (HairLoader != null)
                {
                    HairLoader.Call(
                    "RegisterHairStyle",                                                // The mod call name to add a new hairstyle
                    "HairLoader",                                                       // Enter the name of your mod's class
                    "Hair Loader",                                                      // The name of your mod that will be displayed in the Stylist's Hair UI
                    "Example 1",                                                        // The name of your hairstyle - each hairstyle needs an unique name. (May be displayed in the future so choose wisely!)
                    Assets.Request<Texture2D>("HairStyles/HairLoader/Example_1"),       // Load the hair texture as an asset (Remember to keep the texture the same height as vanilla hairstyles)
                    Assets.Request<Texture2D>("HairStyles/HairLoader/ExampleAlt_1"),    // Load the hair alt texture as an asset
                    0f,                                                                 // X offset of the hairstyle, use this when your texture's width is larger than default
                    2,                                                                  // Currency to buy this hairstyle, enter -1 for coins, otherwise enter the ItemID of the custom currency (2 = dirt block)
                    5,                                                                  // The price to buy this hairstyle
                    1,                                                                  // The price to re-color this hairstyle
                    false,                                                              // Available in the character creator menu?
                    true,                                                               // Has an custom unlock condition? Yes => add a mod call to your own mod, for the example see line 151
                    "Unlocked by defeating the Eye of Cthulhu");                        // Hint for players how to unlock this hairstyle, can be left out and/or can also be null to display nothing
                }
            }
        }

        public override void Unload()
        {
            // Code not ran on server
            if (Main.dedServ)
            {
                // Clear the HairTable
                HairTable = null;
                ModDisplayNames = null;
            }
               
            // Clear our mod instance
            Instance = null;

            base.Unload();
        }

        public override object Call(params object[] args)
        {
            int argsLength = args.Length;
            Array.Resize(ref args, 15);

            try {
                string messageType = args[0] as string;

                if (messageType == "RegisterHairStyle")
                {
                    RegisterCustomHair(
                        args[1] as string, //modClassName
                        args[2] as string, //modDisplayName
                        args[3] as string, //hairEntryName
                        args[4] as Asset<Texture2D>, //hair Texture
                        args[5] as Asset<Texture2D>, //hairAlt texture
                        Convert.ToSingle(args[6]), // X offset
                        Convert.ToInt32(args[7]), // currency
                        Convert.ToInt32(args[8]), // hair price
                        Convert.ToInt32(args[9]), // color price
                        args[10] as bool?, // Character Creation
                        args[11] as bool?, // Unlock Condition
                        args[12] as string // Unlock Hint
                    );

                    return "Success";
                }

                // This is the example mod call needed for custom unlock conditions
                else if (messageType == "HairLoaderUnlockCondition")
                {
                    // Create a switch with a case for every hairstyle added, enter the same name of the hairstyle you used to add them
                    switch (args[1] as string)
                    {
                        case "Example 1":
                            {
                                // Write your unlock condition here, return it as a bool
                                // This Exmaple Hairstyle is unlocked when the Eye of Cthulhu has been defeated
                                return NPC.downedBoss1;
                            }
                    }

                    // For safety add the return false here
                    return false;
                }

                else if (messageType == "ChangePlayerHairStyle")
                {
                    ChangePlayerHairStyle(
                        args[1] as string, //modClassName
                        args[2] as string, //hairEntryName
                        Convert.ToInt32(args[3]) //playerID
                    );

                    return "Success";
                }
                else
                {
                    Logger.Error("HairLoader: Unknown Message: {message}");
                }
            }
            catch (Exception e) {
				Logger.Error($"HairLoader: Call error {e.StackTrace} {e.Message}");
			}

			return "Failure";
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            HairLoaderMessageType msg = (HairLoaderMessageType)reader.ReadByte();

            switch (msg)
            {
                case HairLoaderMessageType.HairUpdate:
                {
                    int PlayerID = reader.ReadByte();
                    string modClassName = reader.ReadString();
                    string hairEntryName = reader.ReadString();

                    Player player = Main.player[PlayerID];

                    player.GetModPlayer<HairLoaderPlayer>().Hair_modClassName = modClassName;
                    player.GetModPlayer<HairLoaderPlayer>().Hair_hairEntryName = hairEntryName;

                    if (Main.netMode != NetmodeID.Server
                        && HairTable.ContainsKey(modClassName)
                        && HairTable[modClassName].ContainsKey(hairEntryName)
                        && HairTable[modClassName][hairEntryName].index != -1)
                    {
                        player.hair = HairTable[modClassName][hairEntryName].index;
                    }

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)HairLoaderMessageType.HairUpdate);
                        packet.Write((byte)PlayerID);
                        packet.Write(modClassName);
                        packet.Write(hairEntryName);
                        packet.Send(-1, whoAmI);
                    }

                    break;
                }

                default:
                    Logger.Warn("Kourindou: Unknown NetMessage type: " + msg);
                    break;
            }
        }

        public void LoadVanillaHair()
        {
            Main.Hairstyles.UpdateUnlocks();

            for (int i = 0; i < Main.maxHairStyles; i++)
            {
                Main.instance.LoadHair(i);
                Main.Hairstyles.UpdateUnlocks();
                bool visible = Main.Hairstyles.AvailableHairstyles.Contains(i);

                if (!HairTable.ContainsKey("Vanilla"))
                {
                    HairTable.Add("Vanilla", new Dictionary<string, PlayerHairEntry>());
                }

                if (!HairTable["Vanilla"].ContainsKey("Vanilla " + (i + 1).ToString()))
                {
                    HairTable["Vanilla"].Add(
                        "Vanilla " + (i + 1).ToString(),
                        new PlayerHairEntry
                        {
                            hair = TextureAssets.PlayerHair[i],
                            hairAlt = TextureAssets.PlayerHairAlt[i],
                            hairOffset = Main.player[Main.myPlayer].GetHairDrawOffset(i, false).X,
                            index = i,
                            currency = -1,
                            hairPrice = i <= 51 ? 20000 : 200000,
                            colorPrice = 20000,
                            CharacterCreator = visible,
                            UnlockCondition = false,
                            HairWindowVisible = true,
                            UnlockHint = GetVanillaUnlockHint(i)
                        }
                    );
                }
                else
                {
                    HairTable["Vanilla"]["Vanilla " + (i + 1).ToString()] = new PlayerHairEntry { 
                        hair = TextureAssets.PlayerHair[i], 
                        hairAlt = TextureAssets.PlayerHairAlt[i], 
                        hairOffset = Main.player[Main.myPlayer].GetHairDrawOffset(i, false).X,
                        index = i, 
                        currency = -1, 
                        hairPrice = i <= 51 ? 20000 : 200000, 
                        colorPrice = 20000,
                        CharacterCreator = visible,
                        UnlockCondition = false,
                        HairWindowVisible = true,
                        UnlockHint = GetVanillaUnlockHint(i)
                    };
                }

                if (!ModDisplayNames.ContainsKey("Vanilla"))
                {
                    ModDisplayNames.Add("Vanilla", "Terraria");
                }
            }
        }

        public static string GetVanillaUnlockHint(int index)
        {
            switch (index)
            { 
                case > 0 and <= 50:
                    {
                        return "Available everywhere";
                    }
                case > 50 and <= 122 or 134 or 135 or 145 or 146 or 152 or 153 or 156 or 159 or 160 or 163 or 164:
                    {
                        return "Available at the stylist";
                    }
                case 162:
                    {
                        return "Unlocked by defeating Plantera";
                    }
                case > 122 and <= 132:
                    {
                        return "Unlocked by defeating the Martian invasion";
                    }
                case 133:
                    {
                        return "Unlocked by defeating the Martian invasion and the Moon Lord";
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        public static int GetCharacterCreatorHairCount()
        {
            int amount = 0;

            foreach (KeyValuePair<string, Dictionary<string, PlayerHairEntry>> mod in HairTable)
            {
                foreach (KeyValuePair<string, PlayerHairEntry> hair in mod.Value)
                {
                    if (hair.Value.CharacterCreator)
                    {
                        amount++;
                    }
                }
            }

            return amount;
        }

        public static void RegisterCustomHair(string modClassName, string modDisplayName, string hairEntryName, Asset<Texture2D> hair, Asset<Texture2D> hairAlt, float hairOffset, int currency, int hairPrice, int colorPrice, bool? CharacterCreator, bool? UnlockCondition , string UnlockHint = null)
        {
            if (modClassName == "Vanilla" || modClassName == "All")
            {
                modClassName += "_";
            }
            
            if (!HairTable.ContainsKey(modClassName))
            {
                HairTable.Add(modClassName, new Dictionary<string, PlayerHairEntry>());
            }

            if (!ModDisplayNames.ContainsKey(modClassName))
            {
                ModDisplayNames.Add(modClassName, modDisplayName);
            }

            if (!HairTable[modClassName].ContainsKey(hairEntryName))
            {
                HairTable[modClassName].Add(hairEntryName, new PlayerHairEntry
                {
                    hair = hair,
                    hairAlt = hairAlt,
                    hairOffset = hairOffset,
                    index = -1,
                    currency = currency,
                    hairPrice = hairPrice,
                    colorPrice = colorPrice,
                    CharacterCreator = CharacterCreator.HasValue ? CharacterCreator.Value : false,
                    UnlockCondition = UnlockCondition.HasValue ? UnlockCondition.Value : false,
                    HairWindowVisible = UnlockCondition.HasValue ? !UnlockCondition.Value : false,
                    UnlockHint = UnlockHint
                });
            }
            else
            {
                HairTable[modClassName][hairEntryName] = new PlayerHairEntry { 
                    hair = hair, 
                    hairAlt = hairAlt,
                    hairOffset = hairOffset,
                    index = -1, 
                    currency = currency, 
                    hairPrice = hairPrice, 
                    colorPrice = colorPrice,
                    CharacterCreator = CharacterCreator.HasValue ? CharacterCreator.Value : false,
                    UnlockCondition = UnlockCondition.HasValue ? UnlockCondition.Value : false,
                    HairWindowVisible = UnlockCondition.HasValue ? !UnlockCondition.Value : false,
                    UnlockHint = UnlockHint
                };
            }
        }

        public static void UpdateHairStyleVisibility(string modClassName, string hairEntryName, bool? newVisibility)
        {
            if (HairTable.ContainsKey(modClassName) && HairTable[modClassName].ContainsKey(hairEntryName))
            {
                HairTable[modClassName][hairEntryName].HairWindowVisible = newVisibility.HasValue ? newVisibility.Value : false;
            }
        }

        public void ChangePlayerHairStyle (string modClassName, string hairEntryName, int PlayerID)
        {
            if (!HairTable.ContainsKey(modClassName) || !HairTable[modClassName].ContainsKey(hairEntryName))
            {
                return;
            }

            Main.player[PlayerID].GetModPlayer<HairLoaderPlayer>().Hair_modClassName = modClassName;
            Main.player[PlayerID].GetModPlayer<HairLoaderPlayer>().Hair_hairEntryName = hairEntryName;

            if (HairTable[modClassName][hairEntryName].index > -1 && HairTable[modClassName][hairEntryName].index < Main.maxHairStyles)
            {
                Main.player[PlayerID].hair = HairTable[modClassName][hairEntryName].index;
            }

            if (PlayerID == Main.myPlayer)
            {
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = GetPacket();
                    packet.Write((byte)HairLoaderMessageType.HairUpdate);
                    packet.Write((byte)PlayerID);
                    packet.Write(modClassName);
                    packet.Write(hairEntryName);
                    packet.Send();
                }
            }
        }

        public static bool GetModAndHairNames(ref string modClassName, ref string hairEntryName, int index)
        {
            foreach (var mod in HairTable)
            {
                foreach (var hair in HairTable[mod.Key])
                {
                    if (HairTable[mod.Key][hair.Key].index == index)
                    {
                        modClassName = mod.Key;
                        hairEntryName = hair.Key;
                        return true;
                    }
                }
            }
            return false;
        }

        //----------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------  HAIR   --------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------
        
        public static DrawData EditHairTexture(Player player, DrawData drawData)
        {
            drawData.texture = HairTable[player.GetModPlayer<HairLoaderPlayer>().Hair_modClassName][player.GetModPlayer<HairLoaderPlayer>().Hair_hairEntryName].hair.Value;
            return drawData;
        }

        public static DrawData EditHairAltTexture(Player player, DrawData drawData)
        {
            drawData.texture = HairTable[player.GetModPlayer<HairLoaderPlayer>().Hair_modClassName][player.GetModPlayer<HairLoaderPlayer>().Hair_hairEntryName].hairAlt.Value;
            return drawData;
        }

        //----------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------  IL PATCHES  ------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------

        private void HookBackHair(ILContext il)
        {
            // Create a new cursor and label
            var c1 = new ILCursor(il);
            ILLabel label = null;

            // First edit
            if (c1.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                x => x.MatchLdfld<Player>("head"),
                x => x.MatchLdcI4(-1),
                x => x.MatchBeq(out label),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerDrawSet>("fullHair"),
                x => x.MatchBrtrue(out label),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerDrawSet>("drawsBackHairWithoutHeadgear"),
                x => x.MatchBrfalse(out label)))
            {
                c1.Index += 11;

                if (c1.TryGotoNext(
                    x => x.MatchLdloca(1),
                    x => x.MatchLdsfld(typeof(TextureAssets), "PlayerHair"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                    x => x.MatchLdfld<Player>("hair"),
                    x => x.MatchLdelemRef(),
                    x => x.MatchCallvirt("ReLogic.Content.Asset`1<Microsoft.Xna.Framework.Graphics.Texture2D>", "get_Value"),
                    x => x.MatchLdloc(0),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("hairBackFrame"),
                    x => x.MatchNewobj("System.Nullable`1<Microsoft.Xna.Framework.Rectangle>"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("colorHair"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                    x => x.MatchLdfld<Player>("headRotation"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("headVect"),
                    x => x.MatchLdcR4(1),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("playerEffect"),
                    x => x.MatchLdcI4(0),
                    x => x.MatchCall(out _)
                    ))
                {
                    c1.Index += 23;
                    c1.Emit(OpCodes.Ldarg_0);
                    c1.Emit(OpCodes.Ldfld, typeof(PlayerDrawSet).GetField("drawPlayer"));
                    c1.Emit(OpCodes.Ldloc_1);
                    c1.EmitDelegate(EditHairTexture);
                    c1.Emit(OpCodes.Stloc_1);
                    Logger.Debug("Successfully implemented IL Patch #1 in BackHair");
                }
                else { Logger.Debug("IL Patch #1 in BackHair failed at search #2"); }
            }
            else { Logger.Debug("IL Patch #1 in BackHair failed at search #1"); }

            // Second edit
            if (c1.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerDrawSet>("hatHair"),
                x => x.MatchBrfalse(out label)
                ))
            {
                c1.Index += 3;

                if (c1.TryGotoNext(
                    x => x.MatchLdloca(1),
                    x => x.MatchLdsfld(typeof(TextureAssets), "PlayerHairAlt"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                    x => x.MatchLdfld<Player>("hair"),
                    x => x.MatchLdelemRef(),
                    x => x.MatchCallvirt("ReLogic.Content.Asset`1<Microsoft.Xna.Framework.Graphics.Texture2D>", "get_Value"),
                    x => x.MatchLdloc(0),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("hairBackFrame"),
                    x => x.MatchNewobj("System.Nullable`1<Microsoft.Xna.Framework.Rectangle>"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("colorHair"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                    x => x.MatchLdfld<Player>("headRotation"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("headVect"),
                    x => x.MatchLdcR4(1),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("playerEffect"),
                    x => x.MatchLdcI4(0),
                    x => x.MatchCall(out _)
                ))
                {
                    c1.Index += 23;
                    c1.Emit(OpCodes.Ldarg_0);
                    c1.Emit(OpCodes.Ldfld, typeof(PlayerDrawSet).GetField("drawPlayer"));
                    c1.Emit(OpCodes.Ldloc_1);
                    c1.EmitDelegate(EditHairAltTexture);
                    c1.Emit(OpCodes.Stloc_1);
                    Logger.Debug("Successfully implemented IL Patch #2 in BackHair");
                }
                else { Logger.Debug("IL Patch #2 in BackHair failed at search #2"); }
            }
            else { Logger.Debug("IL Patch #2 in BackHair failed at search #1"); }
        }

        private void HookHead(ILContext il)
        {
            // Create a new cursor and label
            var c1 = new ILCursor(il);
            ILLabel label = null;

            // First edit
            if (c1.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerDrawSet>("fullHair"),
                x => x.MatchBrfalse(out label)
                ))
            {
                c1.Index += 3;

                if (c1.TryGotoNext(
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                    x => x.MatchLdfld<Player>("invis"),
                    x => x.MatchBrtrue(out label)
                    ))
                {
                    c1.Index += 4;

                    if (c1.TryGotoNext(
                        x => x.MatchLdloca(6),
                        x => x.MatchLdsfld(typeof(TextureAssets), "PlayerHair"),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                        x => x.MatchLdfld<Player>("hair"),
                        x => x.MatchLdelemRef(),
                        x => x.MatchCallvirt("ReLogic.Content.Asset`1<Microsoft.Xna.Framework.Graphics.Texture2D>", "get_Value"),
                        x => x.MatchLdloc(5),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<PlayerDrawSet>("hairFrontFrame"),
                        x => x.MatchNewobj("System.Nullable`1<Microsoft.Xna.Framework.Rectangle>"),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<PlayerDrawSet>("colorHair"),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                        x => x.MatchLdfld<Player>("headRotation"),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<PlayerDrawSet>("headVect"),
                        x => x.MatchLdcR4(1),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<PlayerDrawSet>("playerEffect"),
                        x => x.MatchLdcI4(0),
                        x => x.MatchCall(out _)
                        ))
                    {
                        c1.Index += 23;
                        c1.Emit(OpCodes.Ldarg_0);
                        c1.Emit(OpCodes.Ldfld, typeof(PlayerDrawSet).GetField("drawPlayer"));
                        c1.Emit(OpCodes.Ldloc, 6);                                 
                        c1.EmitDelegate(EditHairTexture);
                        c1.Emit(OpCodes.Stloc, 6);
                        Logger.Debug("Successfully implemented IL Patch #1 in Head");
                    }
                    else { Logger.Debug("IL Patch #1 in Head failed at search #3"); }
                }
                else { Logger.Debug("IL Patch #1 in Head failed at search #2"); }
            }
            else { Logger.Debug("IL Patch #1 in Head failed at search #1"); }

            //Second edit
            if (c1.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerDrawSet>("hatHair"),
                x => x.MatchBrfalse(out label),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                x => x.MatchLdfld<Player>("invis"),
                x => x.MatchBrtrue(out label)
                ))
            {
                c1.Index += 7;

                if (c1.TryGotoNext(
                        x => x.MatchLdloca(6),
                        x => x.MatchLdsfld(typeof(TextureAssets), "PlayerHairAlt"),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                        x => x.MatchLdfld<Player>("hair"),
                        x => x.MatchLdelemRef(),
                        x => x.MatchCallvirt("ReLogic.Content.Asset`1<Microsoft.Xna.Framework.Graphics.Texture2D>", "get_Value"),
                        x => x.MatchLdloc(5),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<PlayerDrawSet>("hairFrontFrame"),
                        x => x.MatchNewobj("System.Nullable`1<Microsoft.Xna.Framework.Rectangle>"),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<PlayerDrawSet>("colorHair"),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                        x => x.MatchLdfld<Player>("headRotation"),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<PlayerDrawSet>("headVect"),
                        x => x.MatchLdcR4(1),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<PlayerDrawSet>("playerEffect"),
                        x => x.MatchLdcI4(0),
                        x => x.MatchCall(out _)
                    ))
                {
                    c1.Index += 23;
                    c1.Emit(OpCodes.Ldarg_0);
                    c1.Emit(OpCodes.Ldfld, typeof(PlayerDrawSet).GetField("drawPlayer"));
                    c1.Emit(OpCodes.Ldloc, 6);                                 
                    c1.EmitDelegate(EditHairAltTexture);
                    c1.Emit(OpCodes.Stloc, 6);
                    Logger.Debug("Successfully implemented IL Patch #2 in Head");
                }
                else { Logger.Debug("IL Patch #2 in Head failed at search #2"); }
            }
            else { Logger.Debug("IL Patch #2 in Head failed at search #1"); }

            // Third edit
            if (c1.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                x => x.MatchLdfld<Player>("head"),
                x => x.MatchLdcI4(23),
                x => x.MatchBneUn(out label),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                x => x.MatchLdfld<Player>("invis"),
                x => x.MatchBrtrue(out label)
                ))
            {
                c1.Index += 9;

                if (c1.TryGotoNext(
                    x => x.MatchLdloca(6),
                    x => x.MatchLdsfld(typeof(TextureAssets), "PlayerHair"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                    x => x.MatchLdfld<Player>("hair"),
                    x => x.MatchLdelemRef(),
                    x => x.MatchCallvirt("ReLogic.Content.Asset`1<Microsoft.Xna.Framework.Graphics.Texture2D>", "get_Value"),
                    x => x.MatchLdloc(5),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("hairFrontFrame"),
                    x => x.MatchNewobj("System.Nullable`1<Microsoft.Xna.Framework.Rectangle>"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("colorHair"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                    x => x.MatchLdfld<Player>("headRotation"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("headVect"),
                    x => x.MatchLdcR4(1),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("playerEffect"),
                    x => x.MatchLdcI4(0),
                    x => x.MatchCall(out _)
                    ))
                {
                    c1.Index += 23;
                    c1.Emit(OpCodes.Ldarg_0);
                    c1.Emit(OpCodes.Ldfld, typeof(PlayerDrawSet).GetField("drawPlayer"));
                    c1.Emit(OpCodes.Ldloc, 6);
                    c1.EmitDelegate(EditHairTexture);
                    c1.Emit(OpCodes.Stloc, 6);
                    Logger.Debug("Successfully implemented IL Patch #3 in Head");
                }
                else { Logger.Debug("IL Patch #3 in Head failed at search #2"); }
            }
            else { Logger.Debug("IL Patch #3 in Head failed at search #1"); }

            // Fourth edit
            if (c1.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                x => x.MatchLdfld<Player>("invis"),
                x => x.MatchBrtrue(out label)
                ))
            {
                c1.Index += 4;

                if (c1.TryGotoNext(
                    x => x.MatchLdloca(6),
                    x => x.MatchLdsfld(typeof(TextureAssets), "PlayerHair"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                    x => x.MatchLdfld<Player>("hair"),
                    x => x.MatchLdelemRef(),
                    x => x.MatchCallvirt("ReLogic.Content.Asset`1<Microsoft.Xna.Framework.Graphics.Texture2D>", "get_Value"),
                    x => x.MatchLdloc(5),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("hairFrontFrame"),
                    x => x.MatchNewobj("System.Nullable`1<Microsoft.Xna.Framework.Rectangle>"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("colorHair"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                    x => x.MatchLdfld<Player>("headRotation"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("headVect"),
                    x => x.MatchLdcR4(1),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("playerEffect"),
                    x => x.MatchLdcI4(0),
                    x => x.MatchCall(out _)
                    ))
                {
                    c1.Index += 23;
                    c1.Emit(OpCodes.Ldarg_0);
                    c1.Emit(OpCodes.Ldfld, typeof(PlayerDrawSet).GetField("drawPlayer"));
                    c1.Emit(OpCodes.Ldloc, 6);                                
                    c1.EmitDelegate(EditHairTexture);
                    c1.Emit(OpCodes.Stloc, 6);
                    Logger.Debug("Successfully implemented IL Patch #4 in Head");
                }
                else { Logger.Debug("IL Patch #4 in Head failed at search #2"); }
            }
            else { Logger.Debug("IL Patch #4 in Head failed at search #1"); }

            // Fifth edit
            if (c1.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                x => x.MatchLdfld<Player>("invis"),
                x => x.MatchBrtrue(out label),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                x => x.MatchLdfld<Player>("face"),
                x => x.MatchLdcI4(0),
                x => x.MatchBlt(out label),
                x => x.MatchLdsfld(typeof(Terraria.ID.ArmorIDs.Face.Sets), "PreventHairDraw"),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                x => x.MatchLdfld<Player>("face"),
                x => x.MatchLdelemU1(),
                x => x.MatchBrtrue(out label)
                ))
            {
                c1.Index += 15;

                if (c1.TryGotoNext(
                    x => x.MatchLdloca(6),
                    x => x.MatchLdsfld(typeof(TextureAssets), "PlayerHair"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                    x => x.MatchLdfld<Player>("hair"),
                    x => x.MatchLdelemRef(),
                    x => x.MatchCallvirt("ReLogic.Content.Asset`1<Microsoft.Xna.Framework.Graphics.Texture2D>", "get_Value"),
                    x => x.MatchLdloc(5),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("hairFrontFrame"),
                    x => x.MatchNewobj("System.Nullable`1<Microsoft.Xna.Framework.Rectangle>"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("colorHair"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("drawPlayer"),
                    x => x.MatchLdfld<Player>("headRotation"),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("headVect"),
                    x => x.MatchLdcR4(1),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerDrawSet>("playerEffect"),
                    x => x.MatchLdcI4(0),
                    x => x.MatchCall(out _)
                    ))
                {
                    c1.Index += 23;
                    c1.Emit(OpCodes.Ldarg_0);
                    c1.Emit(OpCodes.Ldfld, typeof(PlayerDrawSet).GetField("drawPlayer"));
                    c1.Emit(OpCodes.Ldloc, 6);
                    c1.EmitDelegate(EditHairTexture);
                    c1.Emit(OpCodes.Stloc, 6);
                    Logger.Debug("Successfully implemented IL Patch #5 in Head");
                }
                else { Logger.Debug("IL Patch #5 in Head failed at search #2"); }
            }
            else { Logger.Debug("IL Patch #5 in Head failed at search #1"); }
        }

        private void HookUICharacterCreation_MakeHairsylesMenu(ILContext il)
        {
            // Create a new cursor and label
            var c1 = new ILCursor(il);
            ILLabel label = null;

            // First edit
            if (c1.TryGotoNext(
                x => x.MatchLdloc(1),
                x => x.MatchLdloc(4),
                x => x.MatchCallvirt("Terraria.GameContent.UI.Elements.UIList", "Add"),
                x => x.MatchLdloc(4),
                x => x.MatchLdcR4(0),
                x => x.MatchCallvirt("Terraria.UI.UIElement", "SetPadding")
                ))
            {
                c1.Emit(OpCodes.Ldloc, 4);
                c1.EmitDelegate<Func<UIElement, UIElement>>((uiElement) =>
                {
                    int amount = GetCharacterCreatorHairCount();
                    uiElement.Width = StyleDimension.FromPixelsAndPercent(0.0f, 1f);
                    uiElement.Height = StyleDimension.FromPixelsAndPercent((float)(48 * (amount / 10 + (amount % 10 != 0 ? 1 : 0))), 0.0f);
                    return uiElement;
                });
                c1.Emit(OpCodes.Stloc, 4);
                Logger.Debug("Successfully implemented IL Patch #1 in MakeHairstylesMenu");
            }
            else { Logger.Debug("IL Patch #1 in MakeHairstylesMenu failed at search #1"); }


            // Second edit
            if (c1.TryGotoNext(
                x => x.MatchLdcI4(0),
                x => x.MatchStloc(5),
                x => x.MatchBr(out label),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Terraria.GameContent.UI.States.UICharacterCreation>("_player"),
                x => x.MatchLdsfld(typeof(Main), "Hairstyles"),
                x => x.MatchLdfld<HairstyleUnlocksHelper>("AvailableHairstyles"),
                x => x.MatchLdloc(5),
                x => x.MatchCallvirt("System.Collections.Generic.List`1<System.Int32>", "get_Item"),
                x => x.MatchNewobj(typeof(UIHairStyleButton)),
                x => x.MatchDup(),
                x => x.MatchLdloc(5),
                x => x.MatchLdcI4(10),
                x => x.MatchRem(),
                x => x.MatchConvR4(),
                x => x.MatchLdcR4(46),
                x => x.MatchMul(),
                x => x.MatchLdcR4(6),
                x => x.MatchAdd(),
                x => x.MatchCall<StyleDimension>("FromPixels"),
                x => x.MatchStfld<UIElement>("Left"),
                x => x.MatchDup(),
                x => x.MatchLdloc(5),
                x => x.MatchLdcI4(10),
                x => x.MatchDiv(),
                x => x.MatchConvR4(),
                x => x.MatchLdcR4(48),
                x => x.MatchMul(),
                x => x.MatchLdcR4(1),
                x => x.MatchAdd(),
                x => x.MatchCall<StyleDimension>("FromPixels"),
                x => x.MatchStfld<UIElement>("Top"),
                x => x.MatchStloc(6),
                x => x.MatchLdloc(6),
                x => x.MatchLdstr("Middle"),
                x => x.MatchLdloc(5),
                x => x.MatchLdloca(7),
                x => x.MatchInitobj("System.Nullable`1<Microsoft.Xna.Framework.Vector2>"),
                x => x.MatchLdloc(7),
                x => x.MatchLdloca(7),
                x => x.MatchInitobj("System.Nullable`1<Microsoft.Xna.Framework.Vector2>"),
                x => x.MatchLdloc(7),
                x => x.MatchCallvirt<UIElement>("SetSnapPoint"),
                x => x.MatchLdloc(4),
                x => x.MatchLdloc(6),
                x => x.MatchCallvirt<UIElement>("Append"),
                x => x.MatchLdloc(5),
                x => x.MatchLdcI4(1),
                x => x.MatchAdd(),
                x => x.MatchStloc(5),
                x => x.MatchLdloc(5),
                x => x.MatchLdloc(3),
                x => x.MatchBlt(out label)
                ))
            {
                c1.RemoveRange(53);
                c1.Emit(OpCodes.Ldloc, 4);
                c1.EmitDelegate<Func<UIElement, UIElement>>((uiElement) =>
                {
                    int index = 0;
                    foreach (KeyValuePair<string, Dictionary<string, PlayerHairEntry>> mod in HairTable)
                    {
                        foreach (KeyValuePair<string, PlayerHairEntry> hair in mod.Value)
                        {
                            if (hair.Value.CharacterCreator)
                            {
                                UICustomHairStyleButton uiHairStyleButton1 = new UICustomHairStyleButton(Main.PendingPlayer, mod.Key, hair.Key);
                                uiHairStyleButton1.Left = StyleDimension.FromPixels((float)((double)(index % 10) * 46.0 + 6.0));
                                uiHairStyleButton1.Top = StyleDimension.FromPixels((float)((double)(index / 10) * 48.0 + 1.0));
                                UICustomHairStyleButton uiHairStyleButton2 = uiHairStyleButton1;
                                uiHairStyleButton2.SetSnapPoint("Middle", index);
                                uiElement.Append((UIElement)uiHairStyleButton2);
                                index++;
                            }
                        }
                    }
                    return uiElement;
                });
                c1.Emit(OpCodes.Stloc, 4);
                Logger.Debug("Successfully implemented IL Patch #2 in MakeHairstylesMenu");
            }
            else { Logger.Debug("IL Patch #2 in MakeHairstylesMenu failed at search #1"); }
        }
    }

    public enum HairLoaderMessageType : byte
    {
        HairUpdate
    }
}