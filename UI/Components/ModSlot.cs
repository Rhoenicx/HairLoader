using HairLoader.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;

namespace HairLoader.UI.Components
{
    internal class ModSlot : UITextPanel<string>
    {
        // This element doesn't need any additional data, text is already set on create
        public ModSlot(string text) : base(text)
        {

        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            // If the text of this element is the current selected mod in the hairwindow
            if (Text == HairloaderSystem.Instance.HairWindow.HighlightDisplayName)
            {
                BackgroundColor = new Color(255, 199, 0);
            }
            else
            {
                BackgroundColor = new Color(33, 64, 141);
            }

            base.DrawSelf(spriteBatch);
        }
    }
}
