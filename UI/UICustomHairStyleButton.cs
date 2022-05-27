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
        public readonly string modClassName;
        public readonly string hairEntryName;
        private readonly Asset<Texture2D> _selectedBorderTexture;
        private readonly Asset<Texture2D> _hoveredBorderTexture;
        private bool _hovered;
        private bool _soundedHover;

        public UICustomHairStyleButton(Player player, string modClassName, string hairEntryName)
          : base((Asset<Texture2D>)Main.Assets.Request<Texture2D>("Images/UI/CharCreation/CategoryPanel"))
        {
            this._player = player;
            this.modClassName = modClassName;
            this.hairEntryName = hairEntryName;
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

            if (this._player.GetModPlayer<HairLoaderPlayer>().Hair_modClassName == this.modClassName && this._player.GetModPlayer<HairLoaderPlayer>().Hair_hairEntryName == this.hairEntryName)
            {
                spriteBatch.Draw(this._selectedBorderTexture.Value, Vector2.Subtract(this.GetDimensions().Center(), Vector2.Divide(this._selectedBorderTexture.Size(), 2f)), Color.White);
            }
            if (this._hovered)
            {
                spriteBatch.Draw(this._hoveredBorderTexture.Value, Vector2.Subtract(this.GetDimensions().Center(), Vector2.Divide(this._hoveredBorderTexture.Size(), 2f)), Color.White);
            }

            string ClassNameBackup = this._player.GetModPlayer<HairLoaderPlayer>().Hair_modClassName;
            string HairNameBackup = this._player.GetModPlayer<HairLoaderPlayer>().Hair_hairEntryName;

            this._player.GetModPlayer<HairLoaderPlayer>().Hair_modClassName = this.modClassName;
            this._player.GetModPlayer<HairLoaderPlayer>().Hair_hairEntryName = this.hairEntryName;

            Main.PlayerRenderer.DrawPlayerHead(Main.Camera, this._player, Vector2.Add(this.GetDimensions().Center(), new Vector2(-5f, -5f)), borderColor: new Color());

            this._player.GetModPlayer<HairLoaderPlayer>().Hair_modClassName = ClassNameBackup;
            this._player.GetModPlayer<HairLoaderPlayer>().Hair_hairEntryName = HairNameBackup;
        }

        public override void MouseDown(UIMouseEvent evt)
        {
            // Apply vanilla hair
            if (HairLoader.HairTable[modClassName][hairEntryName].index >= 0)
            {
                this._player.hair = HairLoader.HairTable[modClassName][hairEntryName].index;
            }

            // Apply custom hair
            this._player.GetModPlayer<HairLoaderPlayer>().Hair_modClassName = this.modClassName;
            this._player.GetModPlayer<HairLoaderPlayer>().Hair_hairEntryName = this.hairEntryName;
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
