using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using HairLoader.UI;
using Terraria.UI;

namespace HairLoader
{
    public class PlayerHairTexture
    {
        public Texture2D hair { get; set; }     // Front hair texture file
        public Texture2D hairAlt { get; set; }  // back hair texture file
    }
    
    public class PlayerHairEntry
    {
        public int index { get; set; }          // Index ID where to find this style in the HairStyles Dictionary
        public int currency { get; set; }       // Determines the currency that will be used to buy this HairStyle, -1 = coins, >-1 = ItemID
        public int price { get; set; }          // The price of this Hairstyle in the amount of currency above
        public bool visibility { get; set; }    // If this hairstyle needs to be visible in the Hair Menu, Mods can change this using ChangePlayerHairEntryVisibility() mod call
    }

    class HairLoader : Mod
    {
        internal static HairLoader Instance;

        // Stores all the hairstyles based on ID
        public static Dictionary<int, PlayerHairTexture> HairStyles = new Dictionary<int, PlayerHairTexture>();

        // stores which mod added which texture
        public static Dictionary<string, Dictionary<string, PlayerHairEntry>> HairTable= new Dictionary<string, Dictionary<string, PlayerHairEntry>>();

        // UI elemtens
        public HairWindow HairWindow;
        public UserInterface HairWindowInterface;

        public HairLoader()
        {
            Instance = this;
        }

        public override void Load()
        {
            if (!Main.dedServ)
            {

                HairWindow = new HairWindow();
                HairWindow.Activate();

                HairWindowInterface = new UserInterface();
                HairWindowInterface.SetState(HairWindow);


                if (HairStyles == null || HairTable == null)
                {
                    HairStyles = new Dictionary<int, PlayerHairTexture>();
                    HairTable = new Dictionary<string, Dictionary<string, PlayerHairEntry>>();
                }

                LoadVanillaHair();
                RegisterCustomHair("HairLoader", "Example1", GetTexture("HairStyles/HairLoader/Example_1"), GetTexture("HairStyles/HairLoader/ExampleAlt_1"), -1, 10000, true);
            }

            base.Load();
        }

        public override void Unload()
        {
            if (!Main.dedServ)
            {
                HairStyles = null;
                HairTable = null;

                for (int i = 0; i < Main.maxHairTotal; i++)
                {
                    Main.hairLoaded[i] = false;
                }
            }

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

        //-----------------------------------------------------------------------------------------------------------------------------
        public void LoadVanillaHair()
        {
            for (int i = 0; i < Main.maxHairTotal; i++)
            {
                if(!HairTable.ContainsKey("Vanilla"))
                {
                    HairTable.Add("Vanilla", new Dictionary<string, PlayerHairEntry>());
                }

                if (!HairTable["Vanilla"].ContainsKey(i.ToString()))
                {
                    HairTable["Vanilla"].Add("Vanilla " + (i + 1).ToString(), new PlayerHairEntry { index = i, currency = -1, price = i <= 51 ? 10000 : 50000, visibility = true });
                }
                else
                {
                    HairTable["Vanilla"]["Vanilla " + (i + 1).ToString()] = new PlayerHairEntry { index = i, currency = -1, price = i <= 51 ? 10000 : 50000, visibility = true };
                }

                if (!HairStyles.ContainsKey(i))
                {
                    HairStyles.Add(i, new PlayerHairTexture { hair = GetTexture("HairStyles/Vanilla/Player_Hair_" + (i + 1).ToString()), hairAlt = GetTexture("HairStyles/Vanilla/Player_HairAlt_" + (i + 1).ToString()) });
                }
                else
                {
                    HairStyles[i] = new PlayerHairTexture { hair = GetTexture("HairStyles/Vanilla/Player_Hair_" + (i + 1).ToString()), hairAlt = GetTexture("HairStyles/Vanilla/Player_HairAlt_" + (i + 1).ToString()) };
                }
            }
        }

//-----------------------------------------------------------------------------------------------------------------------------
        public void RegisterCustomHair(string modName, string hairName, Texture2D hair, Texture2D hairAlt, int currency, int price, bool visibility)
        {
            int index = 0;

            if (HairStyles.Count > 0)
            {
                index = HairStyles.Keys.Max() >= Main.maxHairTotal ? HairStyles.Keys.Max() + 1 : Main.maxHairTotal;
            }
            else
            {
                index = Main.maxHairTotal;
            }

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
                HairTable[modName].Add(hairName, new PlayerHairEntry { index = index, currency = currency, price = price, visibility = visibility });
                
                if (!HairStyles.ContainsKey(index))
                {
                    HairStyles.Add(index, new PlayerHairTexture { hair = hair, hairAlt = hairAlt });
                }
            }
            else
            {
                HairTable[modName][hairName] = new PlayerHairEntry { index = index, currency = currency, price = price, visibility = visibility };
                HairStyles[HairTable[modName][hairName].index] = new PlayerHairTexture { hair = hair, hairAlt = hairAlt };
            }
        }

//-----------------------------------------------------------------------------------------------------------------------------
        public void ChangePlayerHairEntryVisibility (string modName, string hairName, bool visibility)
        {
            if (!HairTable.ContainsKey(modName))
            {
                return;
            }
            
            if (!HairTable[modName].ContainsKey(hairName))
            {
                return;
            }
            
            HairTable[modName][hairName].visibility = visibility;
        }

//-----------------------------------------------------------------------------------------------------------------------------
        public void ChangePlayerHairStyle (string modName, string hairName, int PlayerID, bool preview)
        {
            if (!HairTable.ContainsKey(modName))
            {
                return;
            }

            if (!HairTable[modName].ContainsKey(hairName))
            {
                return;
            }

            Main.player[PlayerID].GetModPlayer<HairLoaderPlayer>().HairStyleID = HairTable[modName][hairName].index;

            if (HairTable[modName][hairName].index < Main.maxHairTotal)
            {
                Main.player[PlayerID].hair = HairTable[modName][hairName].index;
            }

            if (Main.player[PlayerID].whoAmI == Main.myPlayer)
            {
                if (!preview)
                {
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
        
                    }
                }
            }
        }
        
        public void ChangePlayerHairStyleByID (int HairStyleID, int PlayerID, bool preview)
        {
            if (!HairStyles.ContainsKey(HairStyleID))
            {
                return;
            }
            
            Main.player[PlayerID].GetModPlayer<HairLoaderPlayer>().HairStyleID = HairStyleID;

            if (HairStyleID < Main.maxHairTotal)
            {
                Main.player[PlayerID].hair = HairStyleID;
            }

            if (Main.player[PlayerID].whoAmI == Main.myPlayer)
            {
                if (!preview)
                {
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
        
                    }
                }
            }
        }
    }
}    
