using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using HairLoader.UI;

namespace HairLoader
{    
    public class PlayerHairEntry
    {
        public Texture2D hair { get; set; }     // Front hair texture file
        public Texture2D hairAlt { get; set; }  // back hair texture file
        public int index { get; set; }          // Index ID of the vanilla hair number, modded = -1
        public int currency { get; set; }       // Determines the currency that will be used to buy this HairStyle, -1 = coins, >-1 = ItemID
        public int hairPrice { get; set; }      // The price of this Hairstyle in the amount of currency above
        public int colorPrice { get; set; }     // The price to change the color of this Hairstyle in the amount of currency above
        public bool visibility { get; set; }    // If this hairstyle needs to be visible in the Hair Menu, Mods can change this using ChangePlayerHairEntryVisibility() mod call
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
        public static VanillaTextureSlots[] VanillaTextureSlot = new VanillaTextureSlots[Main.maxHairTotal];

        // UI elements
        public HairWindow HairWindow;
        public UserInterface HairWindowInterface;

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
                // Activate the new HairWindow UI element
                HairWindow = new HairWindow();
                HairWindow.Activate();

                HairWindowInterface = new UserInterface();
                HairWindowInterface.SetState(HairWindow);

                // Detect if the HairTable has been cleared by an Unload => create a new empty HairTable.
                if (HairTable == null)
                {
                    HairTable = new Dictionary<string, Dictionary<string, PlayerHairEntry>>();
                }

                // Load all the Vanilla HairStyles into the HairTable
                LoadVanillaHair();
                
                // Register 1 example HairStyle
                RegisterCustomHair("HairLoader", "Example_1", GetTexture("HairStyles/HairLoader/Example_1"), GetTexture("HairStyles/HairLoader/ExampleAlt_1"), 2, 5, 1, true);
                
            }

            base.Load();
        }

        public override void Unload()
        {
            // Code not ran on server
            if (!Main.dedServ)
            {
                // Clear the HairTable
                HairTable = null;

                // Tell Terraria that all Hair Textures are 'not loaded anymore' to load the vanilla ones.
                // HairLoader's textures will get overwritten by the vanilla ones, to prevent lingering
                // HairLoader textures in playerhairtexture and playerhairalttexture array.
                for (int i = 0; i < Main.maxHairTotal; i++)
                {
                    Main.hairLoaded[i] = false;
                }
            }
               
            // Clear our mod instance
            Instance = null;

            base.Unload();
        }

        public override void UpdateUI (GameTime gameTime)
        {
            if (Main.hairWindow)
            {
                Main.hairWindow = !Main.hairWindow;

                if (Main.player[Main.myPlayer].talkNPC > -1 && Main.npc[Main.player[Main.myPlayer].talkNPC].type == NPCID.Stylist)
                {
                    Main.player[Main.myPlayer].talkNPC = -1;
                }
            }

            if (HairWindowInterface != null && HairWindow.Visible)
            {
                HairWindowInterface.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int vanillaHairWindowIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Hair Window"));
            if (vanillaHairWindowIndex != -1)
            {
                layers.Insert(vanillaHairWindowIndex, new LegacyGameInterfaceLayer(
                    "HairLoader: Hair Window",
                    delegate
                    {
                        if (HairWindow.Visible)
                        {
                            HairWindowInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }


            base.ModifyInterfaceLayers(layers);
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
                        args[3] as Texture2D, //hair Texture
                        args[4] as Texture2D, //hairAlt texture
                        Convert.ToInt32(args[5]), // currency
                        Convert.ToInt32(args[6]), // hair price
                        Convert.ToInt32(args[7]), // color price
                        args[8] as bool? // visibility
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
            for (int i = 0; i < Main.maxHairTotal; i++)
            {
                if(!HairTable.ContainsKey("Vanilla"))
                {
                    HairTable.Add("Vanilla", new Dictionary<string, PlayerHairEntry>());
                }

                if (!HairTable["Vanilla"].ContainsKey("Vanilla " + (i + 1).ToString()))
                {
                    HairTable["Vanilla"].Add(
                        "Vanilla " + (i + 1).ToString(),
                        new PlayerHairEntry { 
                            hair = GetTexture("HairStyles/Vanilla/Player_Hair_" + (i + 1).ToString()), 
                            hairAlt = GetTexture("HairStyles/Vanilla/Player_HairAlt_" + (i + 1).ToString()), 
                            index = i, 
                            currency = -1, 
                            hairPrice = i <= 51 ? 10000 : 50000, 
                            colorPrice = 10000, 
                            visibility = true
                        }
                    );
                }
                else
                {
                    HairTable["Vanilla"]["Vanilla " + (i + 1).ToString()] = new PlayerHairEntry { 
                        hair = GetTexture("HairStyles/Vanilla/Player_Hair_" + (i + 1).ToString()), 
                        hairAlt = GetTexture("HairStyles/Vanilla/Player_HairAlt_" + (i + 1).ToString()), 
                        index = i, 
                        currency = -1, 
                        hairPrice = i <= 51 ? 10000 : 50000, 
                        colorPrice = 10000, 
                        visibility = true 
                    };
                }

                VanillaTextureSlot[i] = new VanillaTextureSlots { modName = "Vanilla", hairName = "Vanilla " + (i + 1).ToString() };
            }
        }

//-----------------------------------------------------------------------------------------------------------------------------
        public void RegisterCustomHair(string modName, string hairName, Texture2D hair, Texture2D hairAlt, int currency, int hairPrice, int colorPrice, bool? visibility)
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
                    visibility = visibility.HasValue ? visibility.Value : false 
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
                    visibility = visibility.HasValue ? visibility.Value : false 
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

            if (HairTable[modName][hairName].index > -1 && HairTable[modName][hairName].index < Main.maxHairTotal)
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

        public bool getModAndHairNames(ref string modName, ref string hairName, int index)
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
