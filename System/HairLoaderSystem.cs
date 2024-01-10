using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.UI;
using HairLoader.UI;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;
using HairLoader.UI.Components;

namespace HairLoader.System;
class HairloaderSystem : ModSystem
{
    // UI elements
    public HairWindow HairWindow;
    public UserInterface HairWindowInterface;

    internal static HairloaderSystem Instance;

    public HairloaderSystem()
    {
        // Create instance
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

            // Setup the UserInterface of the HairWindow
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
        // Run the Update hook for our HairWindow interface,
        // only when the window is currently visible
        if (HairWindowInterface != null && HairWindow.Visible)
        {
            HairWindowInterface.Update(gameTime);
        }
    }
}

public class HairLoaderPlayer : ModPlayer
{
    public override void OnEnterWorld()
    {
        // Reset the position of the window when entering a world
        HairloaderSystem.Instance.HairWindow.ResetPosition();
    }
}
