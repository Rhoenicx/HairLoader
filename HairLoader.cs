using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using Terraria.GameContent;
using HairLoader.UI;
using On.Terraria;

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
    }

    public class VanillaTextureSlots
    { 
        public string modName { get; set; }
        public string hairName { get; set; }
    }

    class HairLoader : Mod
    {
        internal static HairLoader Instance;

        // Dictionary that stores all the PlayerHairEntries
        public static Dictionary<string, Dictionary<string, PlayerHairEntry>> HairTable = new Dictionary<string, Dictionary<string, PlayerHairEntry>>();

        // Array to keep track if custom textures are loaded
        public static VanillaTextureSlots[] VanillaTextureSlot = new VanillaTextureSlots[Terraria.Main.maxHairStyles];

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
                VanillaTextureSlot = new VanillaTextureSlots[Terraria.Main.maxHairStyles];

                // Define a detour on MakeHairStylesMenu in the vanilla code. This detour does NOT call orig!
                On.Terraria.GameContent.UI.States.UICharacterCreation.MakeHairsylesMenu += UICharacterCreation_MakeHairsylesMenu;

                // Load mics textures
                Terraria.Main.instance.LoadItem(ItemID.PlatinumCoin);
                Terraria.Main.instance.LoadItem(ItemID.GoldCoin);
                Terraria.Main.instance.LoadItem(ItemID.SilverCoin);
                Terraria.Main.instance.LoadItem(ItemID.CopperCoin);

                // Load all the Vanilla HairStyles 
                LoadVanillaHair();
                
                // Register 1 example HairStyle
                RegisterCustomHair(
                    "HairLoader",
                    "Example_1",
                    Assets.Request<Texture2D>("HairStyles/HairLoader/Example_1"),
                    Assets.Request<Texture2D>("HairStyles/HairLoader/ExampleAlt_1"),
                    0f,
                    2,
                    5,
                    1,
                    true,
                    false);
                
            }

            base.Load();
        }

        public override void Unload()
        {
            // Code not ran on server
            if (!Terraria.Main.dedServ)
            {
                if (HairTable.ContainsKey("Vanilla"))
                {
                    foreach (KeyValuePair<string, PlayerHairEntry> entry in HairTable["Vanilla"])
                    {
                        TextureAssets.PlayerHair[entry.Value.index] = entry.Value.hair;
                        TextureAssets.PlayerHairAlt[entry.Value.index] = entry.Value.hairAlt;
                    }
                }

                // Clear the HairTable
                HairTable = null;
                VanillaTextureSlot = null;
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
                        args[1] as string, //modName
                        args[2] as string, //hairName
                        args[3] as Asset<Texture2D>, //hair Texture
                        args[4] as Asset<Texture2D>, //hairAlt texture
                        Convert.ToSingle(args[5]), // X offset
                        Convert.ToInt32(args[6]), // currency
                        Convert.ToInt32(args[7]), // hair price
                        Convert.ToInt32(args[8]), // color price
                        args[9] as bool?, // Character Creation
                        args[10] as bool? // Unlock Condition
                    );
                    

                    return "Success";
                }

                else if (messageType == "ChangePlayerHairStyle")
                {
                    ChangePlayerHairStyle(
                        args[1] as string, //modName
                        args[2] as string, //hairName
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
                    string modName = reader.ReadString();
                    string hairName = reader.ReadString();

                    Terraria.Player player = Terraria.Main.player[PlayerID];

                    player.GetModPlayer<HairLoaderPlayer>().Hair_modName = modName;
                    player.GetModPlayer<HairLoaderPlayer>().Hair_hairName = hairName;

                    if (Terraria.Main.netMode != NetmodeID.Server)
                    {
                        if (HairTable.ContainsKey(modName))
                        {
                            if (HairTable[modName].ContainsKey(hairName))
                            {
                                if (HairTable[modName][hairName].index != -1)
                                {
                                    player.hair = HairTable[modName][hairName].index;
                                }
                            }
                        }
                    }

                    if (Terraria.Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)HairLoaderMessageType.HairUpdate);
                        packet.Write((byte)PlayerID);
                        packet.Write(modName);
                        packet.Write(hairName);
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
            Terraria.Main.Hairstyles.UpdateUnlocks();

            for (int i = 0; i < Terraria.Main.maxHairStyles; i++)
            {
                Terraria.Main.instance.LoadHair(i);
                bool visible = Terraria.Main.Hairstyles.AvailableHairstyles.Contains(i);

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
                            hairOffset = Terraria.Main.player[Terraria.Main.myPlayer].GetHairDrawOffset(i, false).X,
                            index = i,
                            currency = -1,
                            hairPrice = i <= 51 ? 10000 : 50000,
                            colorPrice = 10000,
                            CharacterCreator = visible,
                            UnlockCondition = false
                        }
                    ) ;
                }
                else
                {
                    HairTable["Vanilla"]["Vanilla " + (i + 1).ToString()] = new PlayerHairEntry { 
                        hair = TextureAssets.PlayerHair[i], 
                        hairAlt = TextureAssets.PlayerHairAlt[i], 
                        hairOffset = Terraria.Main.player[Terraria.Main.myPlayer].GetHairDrawOffset(i, false).X,
                        index = i, 
                        currency = -1, 
                        hairPrice = i <= 51 ? 10000 : 50000, 
                        colorPrice = 10000,
                        CharacterCreator = visible,
                        UnlockCondition = false
                    };
                }

                VanillaTextureSlot[i] = new VanillaTextureSlots { modName = "Vanilla", hairName = "Vanilla " + (i + 1).ToString() };
            }

            CalculateCharacterCreatorHairCount();
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
        public static void RegisterCustomHair(string modName, string hairName, Asset<Texture2D> hair, Asset<Texture2D> hairAlt, float hairOffset, int currency, int hairPrice, int colorPrice, bool? CharacterCreator, bool? UnlockCondition )
        {
            if (modName == "Vanilla" || modName == "All")
            {
                modName += "_";
            }
            
            if (!HairTable.ContainsKey(modName))
            {
                HairTable.Add(modName, new Dictionary<string, PlayerHairEntry>());
            }

            if (!HairTable[modName].ContainsKey(hairName))
            {
                HairTable[modName].Add(hairName, new PlayerHairEntry { 
                    hair = hair, 
                    hairAlt = hairAlt,
                    hairOffset = hairOffset,
                    index = -1, 
                    currency = currency, 
                    hairPrice = hairPrice, 
                    colorPrice = colorPrice,
                    CharacterCreator = CharacterCreator.HasValue ? CharacterCreator.Value : false,
                    UnlockCondition = UnlockCondition.HasValue ? UnlockCondition.Value : false
                });

                if (CharacterCreator == true)
                {
                    CharacterCreatorHairCount++;
                }
            }
            else
            {
                HairTable[modName][hairName] = new PlayerHairEntry { 
                    hair = hair, 
                    hairAlt = hairAlt,
                    hairOffset = hairOffset,
                    index = -1, 
                    currency = currency, 
                    hairPrice = hairPrice, 
                    colorPrice = colorPrice,
                    CharacterCreator = CharacterCreator.HasValue ? CharacterCreator.Value : false,
                    UnlockCondition = UnlockCondition.HasValue ? UnlockCondition.Value : false
                };
            }
        }

//-----------------------------------------------------------------------------------------------------------------------------
        public void ChangePlayerHairStyle (string modName, string hairName, int PlayerID)
        {
            if (!HairTable.ContainsKey(modName))
            {
                return;
            }

            if (!HairTable[modName].ContainsKey(hairName))
            {
                return;
            }

            Terraria.Main.player[PlayerID].GetModPlayer<HairLoaderPlayer>().Hair_modName = modName;
            Terraria.Main.player[PlayerID].GetModPlayer<HairLoaderPlayer>().Hair_hairName = hairName;

            if (HairTable[modName][hairName].index > -1 && HairTable[modName][hairName].index < Terraria.Main.maxHairStyles)
            {
                Terraria.Main.player[PlayerID].hair = HairTable[modName][hairName].index;
            }

            if (PlayerID == Terraria.Main.myPlayer)
            {
                if (Terraria.Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = GetPacket();
                    packet.Write((byte)HairLoaderMessageType.HairUpdate);
                    packet.Write((byte)PlayerID);
                    packet.Write(modName);
                    packet.Write(hairName);
                    packet.Send();
                }
            }
        }

        public static bool getModAndHairNames(ref string modName, ref string hairName, int index)
        {
            foreach (var mod in HairLoader.HairTable)
            {
                foreach (var hair in HairLoader.HairTable[mod.Key])
                {
                    if (HairTable[mod.Key][hair.Key].index == index)
                    {
                        modName = mod.Key;
                        hairName = hair.Key;
                        return true;
                    }
                }
            }
            return false;
        }

        //----------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------  HACKS  --------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------
        private void UICharacterCreation_MakeHairsylesMenu(On.Terraria.GameContent.UI.States.UICharacterCreation.orig_MakeHairsylesMenu orig, Terraria.GameContent.UI.States.UICharacterCreation self, UIElement middleInnerPanel)
        {
            Terraria.Main.Hairstyles.UpdateUnlocks();
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
                        UICustomHairStyleButton uiHairStyleButton1 = new UICustomHairStyleButton(Terraria.Main.PendingPlayer, mod.Key, hair.Key);
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
    }
    public enum HairLoaderMessageType : byte
    {
        HairUpdate
    }




}
