using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using Terraria.GameContent;

namespace HairLoader
{    
    public class PlayerHairEntry
    {
        public Asset<Texture2D> hair { get; set; }      // Front hair texture file
        public Asset<Texture2D> hairAlt { get; set; }   // back hair texture file
        public int index { get; set; }                  // Index ID of the vanilla hair number, modded = -1
        public int currency { get; set; }               // Determines the currency that will be used to buy this HairStyle, -1 = coins, >-1 = ItemID
        public int hairPrice { get; set; }              // The price of this Hairstyle in the amount of currency above
        public int colorPrice { get; set; }             // The price to change the color of this Hairstyle in the amount of currency above
        public bool visibility { get; set; }            // If this hairstyle needs to be visible in the Hair Menu, Mods can change this using ChangePlayerHairEntryVisibility() mod call
        public float hairOffset { get; set; }           // X offset of the hairstyle
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
        public static VanillaTextureSlots[] VanillaTextureSlot = new VanillaTextureSlots[Main.maxHairStyles];

        // Point to this Mod object Instance.
        public HairLoader()
        {
            Instance = this;
        }

        public override void Load()
        {
            // Code not ran on server
            if (!Main.dedServ)
            {
                // Detect if the HairTable has been cleared by an Unload => create a new empty HairTable.
                if (HairTable == null)
                {
                    HairTable = new Dictionary<string, Dictionary<string, PlayerHairEntry>>();
                }

                // Load mics textures
                Main.instance.LoadItem(ItemID.PlatinumCoin);
                Main.instance.LoadItem(ItemID.GoldCoin);
                Main.instance.LoadItem(ItemID.SilverCoin);
                Main.instance.LoadItem(ItemID.CopperCoin);

                // Load all the Vanilla HairStyles 
                LoadVanillaHair();
                
                // Register 1 example HairStyle
                RegisterCustomHair("HairLoader", "Example_1", Assets.Request<Texture2D>("HairStyles/HairLoader/Example_1"), Assets.Request<Texture2D>("HairStyles/HairLoader/ExampleAlt_1"), 2, 5, 1, true, 0f);
                
            }

            base.Load();
        }

        public override void Unload()
        {
            // Code not ran on server
            if (!Main.dedServ)
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
                        Convert.ToInt32(args[5]), // currency
                        Convert.ToInt32(args[6]), // hair price
                        Convert.ToInt32(args[7]), // color price
                        args[8] as bool?, // visibility
                        Convert.ToSingle(args[9]) // X offset
                    );
                    

                    return "Success";
                }
                else if (messageType == "ChangeHairStyleVisibilityInUI")
                {
                    ChangePlayerHairEntryVisibility(
                        args[1] as string, //modName
                        args[2] as string, //hairName
                        args[3] as bool? //visibility
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

                    Player player = Main.player[PlayerID];

                    player.GetModPlayer<HairLoaderPlayer>().Hair_modName = modName;
                    player.GetModPlayer<HairLoaderPlayer>().Hair_hairName = hairName;

                    if (Main.netMode != NetmodeID.Server)
                    {
                        if (HairLoader.HairTable.ContainsKey(modName))
                        {
                            if (HairLoader.HairTable[modName].ContainsKey(hairName))
                            {
                                if (HairLoader.HairTable[modName][hairName].index != -1)
                                {
                                    player.hair = HairLoader.HairTable[modName][hairName].index;
                                }
                            }
                        }
                    }

                    if (Main.netMode == NetmodeID.Server)
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
            for (int i = 0; i < Main.maxHairStyles; i++)
            {
                Main.instance.LoadHair(i);

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
                            index = i,
                            currency = -1,
                            hairPrice = i <= 51 ? 10000 : 50000,
                            colorPrice = 10000,
                            visibility = true,
                            hairOffset = Main.player[Main.myPlayer].GetHairDrawOffset(i, false).X
                        }
                    ) ;
                }
                else
                {
                    HairTable["Vanilla"]["Vanilla " + (i + 1).ToString()] = new PlayerHairEntry { 
                        hair = TextureAssets.PlayerHair[i], 
                        hairAlt = TextureAssets.PlayerHairAlt[i], 
                        index = i, 
                        currency = -1, 
                        hairPrice = i <= 51 ? 10000 : 50000, 
                        colorPrice = 10000, 
                        visibility = true,
                        hairOffset = Main.player[Main.myPlayer].GetHairDrawOffset(i, false).X
                    };
                }

                VanillaTextureSlot[i] = new VanillaTextureSlots { modName = "Vanilla", hairName = "Vanilla " + (i + 1).ToString() };
            }
        }

//-----------------------------------------------------------------------------------------------------------------------------
        public static void RegisterCustomHair(string modName, string hairName, Asset<Texture2D> hair, Asset<Texture2D> hairAlt, int currency, int hairPrice, int colorPrice, bool? visibility, float hairOffset)
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
                    index = -1, 
                    currency = currency, 
                    hairPrice = hairPrice, 
                    colorPrice = colorPrice, 
                    visibility = visibility.HasValue ? visibility.Value : false,
                    hairOffset = hairOffset
                });
            }
            else
            {
                HairTable[modName][hairName] = new PlayerHairEntry { 
                    hair = hair, 
                    hairAlt = hairAlt, 
                    index = -1, 
                    currency = currency, 
                    hairPrice = hairPrice, 
                    colorPrice = colorPrice, 
                    visibility = visibility.HasValue ? visibility.Value : false,
                    hairOffset = hairOffset
                };
            }
        }

//-----------------------------------------------------------------------------------------------------------------------------
        public void ChangePlayerHairEntryVisibility (string modName, string hairName, bool? visibility)
        {
            if (!HairTable.ContainsKey(modName))
            {
                return;
            }
            
            if (HairTable[modName].ContainsKey(hairName))
            {
                return;
            }

            HairTable[modName][hairName].visibility = visibility.HasValue ? visibility.Value : false ;
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

            Main.player[PlayerID].GetModPlayer<HairLoaderPlayer>().Hair_modName = modName;
            Main.player[PlayerID].GetModPlayer<HairLoaderPlayer>().Hair_hairName = hairName;

            if (HairTable[modName][hairName].index > -1 && HairTable[modName][hairName].index < Main.maxHairStyles)
            {
                Main.player[PlayerID].hair = HairTable[modName][hairName].index;
            }

            if (PlayerID == Main.myPlayer)
            {
                if (Main.netMode == NetmodeID.MultiplayerClient)
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
    }
    public enum HairLoaderMessageType : byte
    {
        HairUpdate
    }
}
