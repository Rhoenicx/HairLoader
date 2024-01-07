using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.UI;
using HairLoader.UI;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;

namespace HairLoader
{
     class HairloaderSystem : ModSystem
     {
        // UI elements
        public HairWindow HairWindow;
        public UserInterface HairWindowInterface;

        internal static HairloaderSystem Instance;

        public HairloaderSystem()
        {
            Instance = this;
        }

        public override void Load()
        {
            // Code not ran on server
            if (!Main.dedServ)
            {
                // Load UI textures
                HairSlot.backgroundTexture = Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanel");
                HairSlot.highlightTexture = Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanelHighlight");
                HairSlot.hoverTexture = Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanelBorder");

                // Activate the new HairWindow UI element
                HairWindow = new HairWindow();
                HairWindow.Activate();

                HairWindowInterface = new UserInterface();
                HairWindowInterface.SetState(HairWindow);
            }
        }

        public override void Unload()
        {
            if (!Main.dedServ)
            {
                HairWindow = null;
                HairWindowInterface = null;
            }
            
            Instance = null;
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

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.hairWindow)
            {
                Main.hairWindow = !Main.hairWindow;

                if (Main.player[Main.myPlayer].talkNPC > -1 && Main.npc[Main.player[Main.myPlayer].talkNPC].type == NPCID.Stylist)
                {
                    Main.player[Main.myPlayer].SetTalkNPC(-1);
                }
            }

            if (HairWindowInterface != null && HairWindow.Visible)
            {
                HairWindowInterface.Update(gameTime);
            }
        }
    }
}
