using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
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

        public static int CharacterCreatorHairCount = 0;

        // Point to this Mod object Instance.
        public HairLoader()
        {
            Instance = this;
        }

        public override void Load()
        {
            // Code not ran on server
            if (!Terraria.Main.dedServ)
            {
                HairTable = new Dictionary<string, Dictionary<string, PlayerHairEntry>>();
                ModDisplayNames = new Dictionary<string, string>();

                // Define a detour on MakeHairStylesMenu in the vanilla code. This detour does NOT call orig!
                On.Terraria.GameContent.UI.States.UICharacterCreation.MakeHairsylesMenu += UICharacterCreation_MakeHairsylesMenuON;
                On.Terraria.DataStructures.PlayerDrawLayers.DrawPlayer_01_BackHair += PlayerDrawLayers_DrawPlayer_01_BackHair;
                On.Terraria.DataStructures.PlayerDrawLayers.DrawPlayer_21_Head += PlayerDrawLayers_DrawPlayer_21_Head;

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

                    if (Main.netMode != NetmodeID.Server)
                    {
                        if (HairTable.ContainsKey(modClassName))
                        {
                            if (HairTable[modClassName].ContainsKey(hairEntryName))
                            {
                                if (HairTable[modClassName][hairEntryName].index != -1)
                                {
                                    player.hair = HairTable[modClassName][hairEntryName].index;
                                }
                            }
                        }
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


        //-----------------------------------------------------------------------------------------------------------------------------
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

            CalculateCharacterCreatorHairCount();
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

        //-----------------------------------------------------------------------------------------------------------------------------
        public static void CalculateCharacterCreatorHairCount()
        {
            foreach (KeyValuePair<string, Dictionary<string, PlayerHairEntry>> mod in HairTable)
            {
                foreach (KeyValuePair<string, PlayerHairEntry> hair in mod.Value)
                {
                    if (hair.Value.CharacterCreator)
                    {
                        CharacterCreatorHairCount++;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
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

                if (CharacterCreator == true)
                {
                    CharacterCreatorHairCount++;
                }
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

        //-----------------------------------------------------------------------------------------------------------------------------
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

        public static bool getModAndHairNames(ref string modClassName, ref string hairEntryName, int index)
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
        private void DrawHair(ref PlayerDrawSet drawinfo, bool altHair, Vector2 position, bool backHair = false)
        {
            string modClassName = drawinfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_modClassName;
            string hairEntryName = drawinfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_hairEntryName;

            bool valid = true;
            if (!HairTable.ContainsKey(modClassName))
            {
                valid = false;
            }
            else if (!HairTable[modClassName].ContainsKey(hairEntryName))
            {
                valid = false;
            }

            if (!valid)
            {
                if (getModAndHairNames(ref modClassName, ref hairEntryName, drawinfo.drawPlayer.hair))
                {
                    drawinfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_modClassName = modClassName;
                    drawinfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_hairEntryName = hairEntryName;
                }
                else
                {
                    Logger.Warn("HAIRLOADER: HAIRTABLE DOES NOT CONTAIN VANILLA HAIRSTYLE: " + modClassName + " - " + hairEntryName + " ! Report this to the developer!");
                    return;
                }
            }

            DrawData data = new DrawData(
                altHair ? HairTable[modClassName][hairEntryName].hairAlt.Value : HairTable[modClassName][hairEntryName].hair.Value,
                position + new Vector2(drawinfo.drawPlayer.direction == 1 ? HairTable[modClassName][hairEntryName].hairOffset : -HairTable[modClassName][hairEntryName].hairOffset, 0f),
                backHair ? new Rectangle?(drawinfo.hairBackFrame) : new Rectangle ?(drawinfo.hairFrontFrame),
                drawinfo.colorHair,
                drawinfo.drawPlayer.headRotation,
                drawinfo.headVect,
                1f,
                drawinfo.playerEffect,
                0
            );
            data.shader = drawinfo.hairDyePacked;
            drawinfo.DrawDataCache.Add(data);
        }

        //----------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------  HACKS  --------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------
        private void UICharacterCreation_MakeHairsylesMenuON(On.Terraria.GameContent.UI.States.UICharacterCreation.orig_MakeHairsylesMenu orig, Terraria.GameContent.UI.States.UICharacterCreation self, UIElement middleInnerPanel)
        {
            Main.Hairstyles.UpdateUnlocks();
            UIElement element = new UIElement()
            {
                Width = StyleDimension.FromPixelsAndPercent(-10f, 1f),
                Height = StyleDimension.FromPixelsAndPercent(0.0f, 1f),
                HAlign = 0.5f,
                VAlign = 0.5f,
                Top = StyleDimension.FromPixels(6f)
            };
            middleInnerPanel.Append(element);
            element.SetPadding(0.0f);
            UIList uiList1 = new UIList();
            uiList1.Width = StyleDimension.FromPixelsAndPercent(-18f, 1f);
            uiList1.Height = StyleDimension.FromPixelsAndPercent(-6f, 1f);
            UIList uiList2 = uiList1;
            uiList2.SetPadding(4f);
            element.Append((UIElement)uiList2);
            UIScrollbar uiScrollbar = new UIScrollbar();
            uiScrollbar.HAlign = 1f;
            uiScrollbar.Height = StyleDimension.FromPixelsAndPercent(-30f, 1f);
            uiScrollbar.Top = StyleDimension.FromPixels(10f);
            UIScrollbar scrollbar = uiScrollbar;
            scrollbar.SetView(100f, 1000f);
            uiList2.SetScrollbar(scrollbar);
            element.Append((UIElement)scrollbar);
            int count = Terraria.Main.Hairstyles.AvailableHairstyles.Count;
            UIElement uiElement = new UIElement()
            {
                Width = StyleDimension.FromPixelsAndPercent(0.0f, 1f),
                Height = StyleDimension.FromPixelsAndPercent((float)(48 * (CharacterCreatorHairCount / 10 + (CharacterCreatorHairCount % 10 != 0 ? 1 : 0))), 0.0f)
            };
            uiList2.Add(uiElement);
            uiElement.SetPadding(0.0f);
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

            // Set the private variable _hairstylesContainer
            FieldInfo fi = typeof(Terraria.GameContent.UI.States.UICharacterCreation).GetField("_hairstylesContainer", BindingFlags.NonPublic | BindingFlags.Instance);
            fi.SetValue(self, element);
        }

        private void PlayerDrawLayers_DrawPlayer_01_BackHair(On.Terraria.DataStructures.PlayerDrawLayers.orig_DrawPlayer_01_BackHair orig, ref PlayerDrawSet drawinfo)
        {
            if (drawinfo.hideHair || !drawinfo.backHairDraw)
                return;
            Vector2 position = Vector2.Add(Vector2.Add(Vector2.Add(new Vector2((float)(int)(drawinfo.Position.X - Main.screenPosition.X - (double)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (double)(drawinfo.drawPlayer.width / 2)), (float)(int)(drawinfo.Position.Y - Main.screenPosition.Y + (double)drawinfo.drawPlayer.height - (double)(float)drawinfo.drawPlayer.bodyFrame.Height + 4.0)), drawinfo.drawPlayer.headPosition), drawinfo.headVect), drawinfo.hairOffset);
            if (drawinfo.drawPlayer.head == -1 || drawinfo.fullHair || drawinfo.drawsBackHairWithoutHeadgear)
            {
                DrawHair(ref drawinfo, false, position, true);
            }
            else
            {
                if (!drawinfo.hatHair)
                    return;
                DrawHair(ref drawinfo, true, position, true);
            }
        }

        private void PlayerDrawLayers_DrawPlayer_21_Head(On.Terraria.DataStructures.PlayerDrawLayers.orig_DrawPlayer_21_Head orig, ref PlayerDrawSet drawinfo)
        {
            typeof(PlayerDrawLayers).GetMethod("DrawPlayer_21_Head_TheFace", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { drawinfo });
            bool flag1 = drawinfo.drawPlayer.head == 14 || drawinfo.drawPlayer.head == 56 || (drawinfo.drawPlayer.head == 114 || drawinfo.drawPlayer.head == 158) || drawinfo.drawPlayer.head == 69 || drawinfo.drawPlayer.head == 180;
            bool flag2 = drawinfo.drawPlayer.head == 28;
            bool flag3 = drawinfo.drawPlayer.head == 39 || drawinfo.drawPlayer.head == 38;
            Vector2 helmetOffset = drawinfo.helmetOffset;
            Vector2 value = new Vector2((float)(-(float)drawinfo.drawPlayer.bodyFrame.Width / 2 + drawinfo.drawPlayer.width / 2), (float)(drawinfo.drawPlayer.height - drawinfo.drawPlayer.bodyFrame.Height + 4));
            Vector2 position = new Vector2((float)Math.Floor(drawinfo.Position.X - Terraria.Main.screenPosition.X + value.X), (float)Math.Floor(drawinfo.Position.Y - Terraria.Main.screenPosition.Y + value.Y)) + drawinfo.drawPlayer.headPosition + drawinfo.headVect;
            if (((Enum)(object)drawinfo.playerEffect).HasFlag((Enum)(object)(SpriteEffects)2))
            {
                position.Y += (int)(drawinfo.drawPlayer.bodyFrame.Height - drawinfo.hairFrontFrame.Height);
            }
            if (drawinfo.fullHair)
            {
                DrawData item = new DrawData(TextureAssets.ArmorHead[drawinfo.drawPlayer.head].Value, helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, new Rectangle?(drawinfo.drawPlayer.bodyFrame), drawinfo.colorArmorHead, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect, 0);
                item.shader = drawinfo.cHead;
                drawinfo.DrawDataCache.Add(item);
                if (!drawinfo.drawPlayer.invis)
                {
                    DrawHair(ref drawinfo, false, position);
                }
                if (drawinfo.drawPlayer.faceFlower > 0)
                {
                    item = new DrawData(TextureAssets.AccFace[(int)drawinfo.drawPlayer.faceFlower].Value, helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, new Rectangle?(drawinfo.drawPlayer.bodyFrame), drawinfo.colorArmorHead, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect, 0);
                    item.shader = drawinfo.cFaceFlower;
                    drawinfo.DrawDataCache.Add(item);
                }
            }
            if (drawinfo.hatHair && !drawinfo.drawPlayer.invis)
            {
                DrawHair(ref drawinfo, true, position);
            }
            if (drawinfo.drawPlayer.head == 23)
            {
                DrawData item;
                if (!drawinfo.drawPlayer.invis)
                {
                    DrawHair(ref drawinfo, false, position);
                }
                if (drawinfo.drawPlayer.faceFlower > 0)
                {
                    item = new DrawData(TextureAssets.AccFace[(int)drawinfo.drawPlayer.faceFlower].Value, helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, new Rectangle?(drawinfo.drawPlayer.bodyFrame), drawinfo.colorArmorHead, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect, 0);
                    item.shader = drawinfo.cFaceFlower;
                    drawinfo.DrawDataCache.Add(item);
                }
                item = new DrawData(TextureAssets.ArmorHead[drawinfo.drawPlayer.head].Value, helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, new Rectangle?(drawinfo.drawPlayer.bodyFrame), drawinfo.colorArmorHead, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect, 0);
                item.shader = drawinfo.cHead;
                drawinfo.DrawDataCache.Add(item);
            }
            if (drawinfo.drawPlayer.head == 270)
            {
                Rectangle bodyFrame = drawinfo.drawPlayer.bodyFrame;
                bodyFrame.Width += 2;
                DrawData item = new DrawData(TextureAssets.ArmorHead[drawinfo.drawPlayer.head].Value, helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, new Rectangle?(bodyFrame), drawinfo.colorArmorHead, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect, 0);
                item.shader = drawinfo.cHead;
                drawinfo.DrawDataCache.Add(item);
                item = new DrawData(TextureAssets.GlowMask[drawinfo.headGlowMask].Value, helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, new Rectangle?(bodyFrame), drawinfo.headGlowColor, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect, 0);
                item.shader = drawinfo.cHead;
                drawinfo.DrawDataCache.Add(item);
            }
            else if (flag1)
            {
                Rectangle bodyFrame2 = drawinfo.drawPlayer.bodyFrame;
                Vector2 headVect = drawinfo.headVect;
                if (drawinfo.drawPlayer.gravDir == 1f)
                {
                    if (bodyFrame2.Y != 0)
                    {
                        bodyFrame2.Y -= 2;
                        headVect.Y += 2f;
                    }
                    bodyFrame2.Height -= 8;
                }
                else
                {
                    if (bodyFrame2.Y != 0)
                    {
                        bodyFrame2.Y -= 2;
                        headVect.Y -= 10f;
                    }
                    bodyFrame2.Height -= 8;
                }
                DrawData item = new DrawData(TextureAssets.ArmorHead[drawinfo.drawPlayer.head].Value, helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, new Rectangle?(bodyFrame2), drawinfo.colorArmorHead, drawinfo.drawPlayer.headRotation, headVect, 1f, drawinfo.playerEffect, 0);
                item.shader = drawinfo.cHead;
                drawinfo.DrawDataCache.Add(item);
            }
            else if (drawinfo.drawPlayer.head == 259)
            {
                int verticalFrames = 27;
                Texture2D value2 = TextureAssets.ArmorHead[drawinfo.drawPlayer.head].Value;
                Rectangle rectangle = value2.Frame(1, verticalFrames, 0, drawinfo.drawPlayer.rabbitOrderFrame.DisplayFrame, 0, 0);
                Vector2 origin = rectangle.Size() / 2f;
                int num2 = drawinfo.drawPlayer.babyBird.ToInt();

                Vector2 value3 = (Vector2)typeof(PlayerDrawLayers).GetMethod("DrawPlayer_21_Head_GetSpecialHatDrawPosition", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { drawinfo, helmetOffset, new Vector2((float)(1 + num2 * 2), (float)(-26 + drawinfo.drawPlayer.babyBird.ToInt() * -6))});
                int num23 = (int)typeof(PlayerDrawLayers).GetMethod("DrawPlayer_21_head_GetHatStacks", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { drawinfo, 4955 });
                float num3 = 0.05235988f;
                float num4 = num3 * drawinfo.drawPlayer.position.X % 6.2831855f;
                for (int num5 = num23 - 1; num5 >= 0; num5--)
                {
                    float x = Vector2.UnitY.RotatedBy((double)(num4 + num3 * (float)num5), default(Vector2)).X * ((float)num5 / 30f) * 2f - (float)(num5 * 2 * drawinfo.drawPlayer.direction);
                    DrawData item = new DrawData(value2, value3 + new Vector2(x, (float)(num5 * -14) * drawinfo.drawPlayer.gravDir), new Rectangle?(rectangle), drawinfo.colorArmorHead, drawinfo.drawPlayer.headRotation, origin, 1f, drawinfo.playerEffect, 0);
                    item.shader = drawinfo.cHead;
                    drawinfo.DrawDataCache.Add(item);
                }
                if (!drawinfo.drawPlayer.invis)
                {
                    DrawHair(ref drawinfo, false, position);
                }
                if (drawinfo.drawPlayer.faceFlower > 0)
                {
                    DrawData item = new DrawData(TextureAssets.AccFace[(int)drawinfo.drawPlayer.faceFlower].Value, helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, new Rectangle?(drawinfo.drawPlayer.bodyFrame), drawinfo.colorArmorHead, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect, 0);
                    item.shader = drawinfo.cFaceFlower;
                    drawinfo.DrawDataCache.Add(item);
                }
            }
            else if (drawinfo.drawPlayer.head > 0 && !flag2)
            {
                if (!drawinfo.drawPlayer.invis || !flag3)
                {
                    if (drawinfo.drawPlayer.head == 13)
                    {
                        int num6 = 0;
                        int num7 = 0;
                        if (drawinfo.drawPlayer.armor[num7] != null && drawinfo.drawPlayer.armor[num7].type == ItemID.EmptyBucket && drawinfo.drawPlayer.armor[num7].stack > 0)
                        {
                            num6 += drawinfo.drawPlayer.armor[num7].stack;
                        }
                        num7 = 10;
                        if (drawinfo.drawPlayer.armor[num7] != null && drawinfo.drawPlayer.armor[num7].type == ItemID.EmptyBucket && drawinfo.drawPlayer.armor[num7].stack > 0)
                        {
                            num6 += drawinfo.drawPlayer.armor[num7].stack;
                        }
                        float num8 = 0.05235988f;
                        float num9 = num8 * drawinfo.drawPlayer.position.X % 6.2831855f;
                        for (int i = 0; i < num6; i++)
                        {
                            float num10 = Vector2.UnitY.RotatedBy((double)(num9 + num8 * (float)i), default(Vector2)).X * ((float)i / 30f) * 2f;
                            DrawData item = new DrawData(TextureAssets.ArmorHead[drawinfo.drawPlayer.head].Value, helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))) + num10, (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f - (float)(4 * i)))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, new Rectangle?(drawinfo.drawPlayer.bodyFrame), drawinfo.colorArmorHead, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect, 0);
                            item.shader = drawinfo.cHead;
                            drawinfo.DrawDataCache.Add(item);
                        }
                    }
                    else if (drawinfo.drawPlayer.head == 265)
                    {
                        int verticalFrames2 = 6;
                        Texture2D value4 = TextureAssets.ArmorHead[drawinfo.drawPlayer.head].Value;
                        Rectangle rectangle2 = value4.Frame(1, verticalFrames2, 0, drawinfo.drawPlayer.rabbitOrderFrame.DisplayFrame, 0, 0);
                        Vector2 origin2 = rectangle2.Size() / 2f;
                        Vector2 value5 = (Vector2)typeof(PlayerDrawLayers).GetMethod("DrawPlayer_21_Head_GetSpecialHatDrawPosition", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { drawinfo, helmetOffset, new Vector2(0f, -9f)});
                        int num24 = (int)typeof(PlayerDrawLayers).GetMethod("DrawPlayer_21_head_GetHatStacks", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { drawinfo, 5004 });
                        float num11 = 0.05235988f;
                        float num12 = num11 * drawinfo.drawPlayer.position.X % 6.2831855f;
                        int num13 = num24 * 4 + 2;
                        int num14 = 0;
                        bool flag4 = (Main.GlobalTimeWrappedHourly + 180f) % 600f < 60f;
                        for (int num15 = num13 - 1; num15 >= 0; num15--)
                        {
                            int num16 = 0;
                            if (num15 == num13 - 1)
                            {
                                rectangle2.Y = 0;
                                num16 = 2;
                            }
                            else if (num15 == 0)
                            {
                                rectangle2.Y = rectangle2.Height * 5;
                            }
                            else
                            {
                                rectangle2.Y = rectangle2.Height * (num14++ % 4 + 1);
                            }
                            if (rectangle2.Y != rectangle2.Height * 3 || !flag4)
                            {
                                float x2 = Vector2.UnitY.RotatedBy((double)(num12 + num11 * (float)num15), default(Vector2)).X * ((float)num15 / 10f) * 4f - (float)num15 * 0.1f * (float)drawinfo.drawPlayer.direction;
                                DrawData item = new DrawData(value4, value5 + new Vector2(x2, (float)(num15 * -4 + num16) * drawinfo.drawPlayer.gravDir), new Rectangle?(rectangle2), drawinfo.colorArmorHead, drawinfo.drawPlayer.headRotation, origin2, 1f, drawinfo.playerEffect, 0);
                                item.shader = drawinfo.cHead;
                                drawinfo.DrawDataCache.Add(item);
                            }
                        }
                    }
                    else
                    {
                        Rectangle bodyFrame3 = drawinfo.drawPlayer.bodyFrame;
                        Vector2 headVect2 = drawinfo.headVect;
                        if (drawinfo.drawPlayer.gravDir == 1f)
                        {
                            bodyFrame3.Height -= 4;
                        }
                        else
                        {
                            headVect2.Y -= 4f;
                            bodyFrame3.Height -= 4;
                        }
                        DrawData item = new DrawData(TextureAssets.ArmorHead[drawinfo.drawPlayer.head].Value, helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, new Rectangle?(bodyFrame3), drawinfo.colorArmorHead, drawinfo.drawPlayer.headRotation, headVect2, 1f, drawinfo.playerEffect, 0);
                        item.shader = drawinfo.cHead;
                        drawinfo.DrawDataCache.Add(item);
                        if (drawinfo.headGlowMask != -1)
                        {
                            if (drawinfo.headGlowMask == 309)
                            {
                                int num17 = PlayerDrawLayers.DrawPlayer_Head_GetTVScreen(drawinfo.drawPlayer);
                                if (num17 != 0)
                                {
                                    int num18 = 0;
                                    num18 += drawinfo.drawPlayer.bodyFrame.Y / 56;
                                    if (num18 >= Main.OffsetsPlayerHeadgear.Length)
                                    {
                                        num18 = 0;
                                    }
                                    Vector2 value6 = Main.OffsetsPlayerHeadgear[num18];
                                    value6 *= (float)(-(float)drawinfo.playerEffect.HasFlag((Enum)(object)(SpriteEffects)2).ToDirectionInt());
                                    Texture2D value7 = TextureAssets.GlowMask[drawinfo.headGlowMask].Value;
                                    int frameY = drawinfo.drawPlayer.miscCounter % 20 / 5;
                                    if (num17 == 5)
                                    {
                                        frameY = 0;
                                        if (drawinfo.drawPlayer.eyeHelper.EyeFrameToShow > 0)
                                        {
                                            frameY = 2;
                                        }
                                    }
                                    Rectangle value8 = value7.Frame(6, 4, num17, frameY, 0, 0);
                                    item = new DrawData(value7, value6 + helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, new Rectangle?(value8), drawinfo.headGlowColor, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect, 0);
                                    item.shader = drawinfo.cHead;
                                    drawinfo.DrawDataCache.Add(item);
                                }
                            }
                            else if (drawinfo.headGlowMask == 273)
                            {
                                for (int j = 0; j < 2; j++)
                                {
                                    Vector2 position2 = new Vector2((float)Main.rand.Next(-10, 10) * 0.125f, (float)Main.rand.Next(-10, 10) * 0.125f) + helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect;
                                    item = new DrawData(TextureAssets.GlowMask[drawinfo.headGlowMask].Value, position2, new Rectangle?(bodyFrame3), drawinfo.headGlowColor, drawinfo.drawPlayer.headRotation, headVect2, 1f, drawinfo.playerEffect, 0);
                                    item.shader = drawinfo.cHead;
                                    drawinfo.DrawDataCache.Add(item);
                                }
                            }
                            else
                            {
                                item = new DrawData(TextureAssets.GlowMask[drawinfo.headGlowMask].Value, helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, new Rectangle?(bodyFrame3), drawinfo.headGlowColor, drawinfo.drawPlayer.headRotation, headVect2, 1f, drawinfo.playerEffect, 0);
                                item.shader = drawinfo.cHead;
                                drawinfo.DrawDataCache.Add(item);
                            }
                        }
                        if (drawinfo.drawPlayer.head == 211)
                        {
                            Color color = new Color(100, 100, 100, 0);
                            ulong seed = (ulong)((long)(drawinfo.drawPlayer.miscCounter / 4 + 100));
                            int num19 = 4;
                            for (int k = 0; k < num19; k++)
                            {
                                float x3 = (float)Utils.RandomInt(ref seed, -10, 11) * 0.2f;
                                float y = (float)Utils.RandomInt(ref seed, -14, 1) * 0.15f;
                                item = new DrawData(TextureAssets.GlowMask[241].Value, helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect + new Vector2(x3, y), new Rectangle?(drawinfo.drawPlayer.bodyFrame), color, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect, 0);
                                item.shader = drawinfo.cHead;
                                drawinfo.DrawDataCache.Add(item);
                            }
                        }
                    }
                }
            }
            else if (!drawinfo.drawPlayer.invis && (drawinfo.drawPlayer.face < 0 || !ArmorIDs.Face.Sets.PreventHairDraw[(int)drawinfo.drawPlayer.face]))
            {
                DrawHair(ref drawinfo, false, position);
                DrawData item;
                if (drawinfo.drawPlayer.faceFlower > 0)
                {
                    item = new DrawData(TextureAssets.AccFace[(int)drawinfo.drawPlayer.faceFlower].Value, helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, new Rectangle?(drawinfo.drawPlayer.bodyFrame), drawinfo.colorArmorHead, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect, 0);
                    item.shader = drawinfo.cFaceFlower;
                    drawinfo.DrawDataCache.Add(item);
                }
            }
            if (drawinfo.drawPlayer.beard > 0 && (drawinfo.drawPlayer.head < 0 || !ArmorIDs.Head.Sets.PreventBeardDraw[drawinfo.drawPlayer.head]))
            {
                Color color2 = drawinfo.colorArmorHead;
                if (ArmorIDs.Beard.Sets.UseHairColor[(int)drawinfo.drawPlayer.beard])
                {
                    color2 = drawinfo.colorHair;
                }
                DrawData item = new DrawData(TextureAssets.AccBeard[(int)drawinfo.drawPlayer.beard].Value, new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, new Rectangle?(drawinfo.drawPlayer.bodyFrame), color2, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect, 0);
                item.shader = drawinfo.cBeard;
                drawinfo.DrawDataCache.Add(item);
            }
            if (drawinfo.drawPlayer.head == 205)
            {
                DrawData item = new DrawData(TextureAssets.Extra[77].Value, new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, new Rectangle?(drawinfo.drawPlayer.bodyFrame), drawinfo.colorHead, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect, 0)
                {
                    shader = drawinfo.skinDyePacked
                };
                drawinfo.DrawDataCache.Add(item);
            }
            if (drawinfo.drawPlayer.head == 214 && !drawinfo.drawPlayer.invis)
            {
                Rectangle bodyFrame4 = drawinfo.drawPlayer.bodyFrame;
                bodyFrame4.Y = 0;
                float num20 = (float)drawinfo.drawPlayer.miscCounter / 300f;
                Color color3 = new Color(0, 0, 0, 0);
                float num21 = 0.8f;
                float num22 = 0.9f;
                if (num20 >= num21)
                {
                    color3 = Color.Lerp(Color.Transparent, new Color(200, 200, 200, 0), Utils.GetLerpValue(num21, num22, num20, true));
                }
                if (num20 >= num22)
                {
                    color3 = Color.Lerp(Color.Transparent, new Color(200, 200, 200, 0), Utils.GetLerpValue(1f, num22, num20, true));
                }
                color3 *= drawinfo.stealth * (1f - drawinfo.shadow);
                DrawData item = new DrawData(TextureAssets.Extra[90].Value, helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect - Main.OffsetsPlayerHeadgear[drawinfo.drawPlayer.bodyFrame.Y / drawinfo.drawPlayer.bodyFrame.Height], new Rectangle?(bodyFrame4), color3, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect, 0);
                drawinfo.DrawDataCache.Add(item);
            }
            if (drawinfo.drawPlayer.head == 137)
            {
                DrawData item = new DrawData(TextureAssets.JackHat.Value, helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, new Rectangle?(drawinfo.drawPlayer.bodyFrame), new Color(255, 255, 255, 255), drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect, 0);
                drawinfo.DrawDataCache.Add(item);
                for (int l = 0; l < 7; l++)
                {
                    Color color4 = new Color(110 - l * 10, 110 - l * 10, 110 - l * 10, 110 - l * 10);
                    Vector2 value9 = new Vector2((float)Main.rand.Next(-10, 11) * 0.2f, (float)Main.rand.Next(-10, 11) * 0.2f);
                    value9.X = drawinfo.drawPlayer.itemFlamePos[l].X;
                    value9.Y = drawinfo.drawPlayer.itemFlamePos[l].Y;
                    value9 *= 0.5f;
                    item = new DrawData(TextureAssets.JackHat.Value, helmetOffset + new Vector2((float)((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2))), (float)((int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f))) + drawinfo.drawPlayer.headPosition + drawinfo.headVect + value9, new Rectangle?(drawinfo.drawPlayer.bodyFrame), color4, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect, 0);
                    drawinfo.DrawDataCache.Add(item);
                }
            }
        }
    }

    public enum HairLoaderMessageType : byte
    {
        HairUpdate
    }
}