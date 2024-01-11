using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI;
using Terraria;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;

namespace HairLoader.UI.Components
{
    internal class HairSlot : UIElement
    {
        // Instance of the Hairwindow this element is placed on
        private readonly HairWindow _hairWindow;

        // Slot displays locked hairstyle
        private readonly bool _locked;

        // Hair ID this slot represents
        private readonly int _hairID;

        // Scale
        private readonly float _scale = 1f;

        // Offset of the hair in the slot
        private const int offsetX = 36;
        private const int offsetY = -8;

        // Vanilla textures for the slot
        public static Asset<Texture2D> backgroundTexture;
        public static Asset<Texture2D> highlightTexture;
        public static Asset<Texture2D> hoverTexture;

        public HairSlot(HairWindow window, int hairID, bool _locked)
        {
            // Assign the window
            _hairWindow = window;

            // On create write the hair and mod names to the internal variables
            _hairID = hairID;
            this._locked = _locked;

            // Set the width and height
            Width.Set(backgroundTexture.Width() * _scale, 0f);
            Height.Set(backgroundTexture.Height() * _scale, 0f);
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = GetInnerDimensions();

            // Draw the background texture
            backgroundTexture ??= Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanel");
            spriteBatch.Draw(backgroundTexture.Value, Vector2.Subtract(GetDimensions().Center(), Vector2.Divide(backgroundTexture.Size(), 2f)), null, _locked ? Color.Gray : Color.White, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);

            // If the hairstyle in this slot is the one the player is currently wearing
            highlightTexture ??= Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanelHighlight");
            if (Main.player[Main.myPlayer].hair == _hairID)
            {
                // draw the highlight texture
                spriteBatch.Draw(highlightTexture.Value, Vector2.Subtract(GetDimensions().Center(), Vector2.Divide(highlightTexture.Size(), 2f)), null, _locked ? Color.Gray : Color.White, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);
            }

            // If the mouse is hovering over this slot
            if (IsMouseHovering)
            {
                // Draw the hover texture
                hoverTexture ??= Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanelBorder");
                spriteBatch.Draw(hoverTexture.Value, Vector2.Subtract(GetDimensions().Center(), Vector2.Divide(hoverTexture.Size(), 2f)), null, Color.White, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);

                // Set the highlight text of the hairwindow to the name of the hairstyle in this slot
                if (HairLoader.HairTable.ContainsKey(_hairID) && HairLoader.HairTable[_hairID] != null)
                {
                    if (HairLoader.HairTable[_hairID].HasCustomHairName)
                    {
                        _hairWindow.HighlightText = HairLoader.HairTable[_hairID].CustomHairNameIsLocalized
                            ? Language.GetTextValue(HairLoader.HairTable[_hairID].CustomHairName)
                            : HairLoader.HairTable[_hairID].CustomHairName;
                    }
                    else
                    {
                        _hairWindow.HighlightText = HairLoader.HairTable[_hairID].HairNameIsLocalized
                            ? Language.GetTextValue(HairLoader.HairTable[_hairID].HairName)
                            : HairLoader.HairTable[_hairID].HairName;
                    }

                    if (HairLoader.HairTable[_hairID].HasUnlockHint)
                    {
                        if (HairLoader.HairTable[_hairID].HasCustomUnlockHint)
                        {
                            _hairWindow.HighlightText += "\r\n" + (HairLoader.HairTable[_hairID].CustomUnlockHintIsLocalized
                                    ? Language.GetTextValue(HairLoader.HairTable[_hairID].CustomUnlockHint)
                                    : HairLoader.HairTable[_hairID].CustomUnlockHint);
                        }
                        else
                        {
                            _hairWindow.HighlightText += "\r\n" + (HairLoader.HairTable[_hairID].UnlockHintIsLocalized
                                ? Language.GetTextValue(HairLoader.HairTable[_hairID].UnlockHint)
                                : HairLoader.HairTable[_hairID].UnlockHint);
                        }
                    }
                }
                else
                {
                    _hairWindow.HighlightText = "";
                }
            }

            // Draw the vanilla player textures in the slot => Head, eyeWhite and eye
            spriteBatch.Draw(TextureAssets.Players[0, 0].Value, dimensions.Center(), new Rectangle?(new Rectangle(0, 0, TextureAssets.PlayerHair[Main.player[Main.myPlayer].hair].Width(), 56)), _locked ? Color.Gray : Main.player[Main.myPlayer].skinColor, 0.0f, new Vector2(offsetX, dimensions.Height + offsetY) * 0.5f, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Players[0, 1].Value, dimensions.Center(), new Rectangle?(new Rectangle(0, 0, TextureAssets.PlayerHair[Main.player[Main.myPlayer].hair].Width(), 56)), new Color(255, 255, 255, 255), 0.0f, new Vector2(offsetX, dimensions.Height + offsetY) * 0.5f, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Players[0, 2].Value, dimensions.Center(), new Rectangle?(new Rectangle(0, 0, TextureAssets.PlayerHair[Main.player[Main.myPlayer].hair].Width(), 56)), _locked ? Color.Gray : Main.player[Main.myPlayer].eyeColor, 0.0f, new Vector2(offsetX, dimensions.Height + offsetY) * 0.5f, 1f, SpriteEffects.None, 0f);

            // Load the Hair textures
            Main.instance.LoadHair(_hairID);

            // Draw the full hairstyle
            Vector2 offset = Main.player[Main.myPlayer].GetHairDrawOffset(_hairID, false) + new Vector2(2f, _hairWindow.AnimationProgress is 1 or 2 or 3 or 8 or 9 or 10 ? 2f : 0f);
            spriteBatch.Draw(TextureAssets.PlayerHair[_hairID].Value, dimensions.Center() + offset, new Rectangle(0, 56 * _hairWindow.AnimationProgress, TextureAssets.PlayerHair[_hairID].Width(), 38 - (int)offset.Y), _locked ? Color.Gray : Main.player[Main.myPlayer].hairColor, 0.0f, new Vector2(TextureAssets.PlayerHair[_hairID].Width(), dimensions.Height + offsetY) * 0.5f, 1f, SpriteEffects.None, 0f);
        }
    }

}
