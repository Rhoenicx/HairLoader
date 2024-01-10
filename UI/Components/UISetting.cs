using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI;

namespace HairLoader.UI.Components
{
    internal class UISetting : UIElement
    {
        // Instance of the Hairwindow this element is placed on
        private readonly HairWindow _hairWindow;

        private readonly Texture2D _tickOff = TextureAssets.InventoryTickOff.Value;
        private readonly Texture2D _tickOn = TextureAssets.InventoryTickOn.Value;

        public UISetting(HairWindow window)
        {
            // Assign the window
            _hairWindow = window;

            Width.Set(_tickOff.Width, 0f);
            Height.Set(_tickOff.Height, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            // Size of this element
            CalculatedStyle dimensions = GetInnerDimensions();

            if (_hairWindow.ShowLocked)
            {
                spriteBatch.Draw(_tickOn, dimensions.Center(), _tickOn.Bounds, Color.White, 0f, _tickOn.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }
            else
            {
                spriteBatch.Draw(_tickOff, dimensions.Center(), _tickOn.Bounds, Color.White, 0f, _tickOn.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }

            if (IsMouseHovering)
            {
                _hairWindow.HighlightText = Language.GetTextValue("Mods.HairLoader.HairWindowUI.LockedButton");
            }
        }
    }

}
