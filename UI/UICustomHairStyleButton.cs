using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.GameContent;

namespace HairLoader.UI
{
    public class UICustomHairStyleButton : UIImageButton
    {
        private readonly Player _player;
        public readonly string modName;
        public readonly string hairName;
        private readonly Asset<Texture2D> _selectedBorderTexture;
        private readonly Asset<Texture2D> _hoveredBorderTexture;
        private bool _hovered;
        private bool _soundedHover;

        public UICustomHairStyleButton(Player player, string modName, string hairName)
          : base((Asset<Texture2D>)Main.Assets.Request<Texture2D>("Images/UI/CharCreation/CategoryPanel"))
        {
            this._player = player;
            this.modName = modName;
            this.hairName = hairName;
            this.Width = StyleDimension.FromPixels(44f);
            this.Height = StyleDimension.FromPixels(44f);
            this._selectedBorderTexture = (Asset<Texture2D>)Main.Assets.Request<Texture2D>("Images/UI/CharCreation/CategoryPanelHighlight");
            this._hoveredBorderTexture = (Asset<Texture2D>)Main.Assets.Request<Texture2D>("Images/UI/CharCreation/CategoryPanelBorder");
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (this._hovered)
            {
                if (!this._soundedHover)
                    SoundEngine.PlaySound(SoundID.MenuTick);
                this._soundedHover = true;
            }
            else
            {
                this._soundedHover = false;
            }

            base.DrawSelf(spriteBatch);

            if (this._player.GetModPlayer<HairLoaderPlayer>().Hair_modName == this.modName && this._player.GetModPlayer<HairLoaderPlayer>().Hair_hairName == this.hairName)
            {
                spriteBatch.Draw(this._selectedBorderTexture.Value, Vector2.Subtract(this.GetDimensions().Center(), Vector2.Divide(this._selectedBorderTexture.Size(), 2f)), Color.White);
            }
            if (this._hovered)
            {
                spriteBatch.Draw(this._hoveredBorderTexture.Value, Vector2.Subtract(this.GetDimensions().Center(), Vector2.Divide(this._hoveredBorderTexture.Size(), 2f)), Color.White);
            }

            int offsetX = 4;
            int offsetY = -38;

            spriteBatch.Draw(TextureAssets.Players[0, 0].Value, Vector2.Subtract(this.GetDimensions().Center(), Vector2.Divide(this._selectedBorderTexture.Size(), 2f)), new Rectangle?(new Rectangle(0, 0, TextureAssets.PlayerHair[Main.player[Main.myPlayer].hair].Width(), 56)), this._player.skinColor, 0.0f, new Vector2(offsetX, this.GetDimensions().Height + offsetY) * 0.5f, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Players[0, 1].Value, Vector2.Subtract(this.GetDimensions().Center(), Vector2.Divide(this._selectedBorderTexture.Size(), 2f)), new Rectangle?(new Rectangle(0, 0, TextureAssets.PlayerHair[Main.player[Main.myPlayer].hair].Width(), 56)), new Color(255, 255, 255, 255), 0.0f, new Vector2(offsetX, this.GetDimensions().Height + offsetY) * 0.5f, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Players[0, 2].Value, Vector2.Subtract(this.GetDimensions().Center(), Vector2.Divide(this._selectedBorderTexture.Size(), 2f)), new Rectangle?(new Rectangle(0, 0, TextureAssets.PlayerHair[Main.player[Main.myPlayer].hair].Width(), 56)), this._player.eyeColor, 0.0f, new Vector2(offsetX, this.GetDimensions().Height + offsetY) * 0.5f, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(HairLoader.HairTable[modName][hairName].hair.Value, Vector2.Subtract(this.GetDimensions().Center(), Vector2.Divide(this._selectedBorderTexture.Size(), 2f)) + new Vector2(HairLoader.HairTable[modName][hairName].hairOffset, 0f), new Rectangle?(new Rectangle(0, 0, HairLoader.HairTable[modName][hairName].hair.Width(), 56)), this._player.hairColor, 0.0f, new Vector2(offsetX, this.GetDimensions().Height + offsetY) * 0.5f, 1f, SpriteEffects.None, 0f);
        }

        public override void MouseDown(UIMouseEvent evt)
        {
            if (HairLoader.HairTable[modName][hairName].index >= 0)
            {
                this._player.hair = HairLoader.HairTable[modName][hairName].index;
            }

            this._player.GetModPlayer<HairLoaderPlayer>().Hair_modName = this.modName;
            this._player.GetModPlayer<HairLoaderPlayer>().Hair_hairName = this.hairName;
            SoundEngine.PlaySound(SoundID.MenuTick);
            base.MouseDown(evt);
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            this._hovered = true;
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            base.MouseOut(evt);
            this._hovered = false;
        }
    }
}
