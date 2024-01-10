using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.UI;
using Terraria;

namespace HairLoader.UI.Components
{
    internal class UIColorBar : UIElement
    {
        // Instance of the Hairwindow this element is placed on
        private readonly HairWindow _hairWindow;

        // Base size of the Color Bar element
        private Rectangle _rect = new(0, 0, 178, 16);

        // Width of the side to the start of the inner colors
        private const int _rectSideWidth = 5;

        // Grab the vanilla textures for color sliders... (These are already loaded in during mod.load())
        private readonly Texture2D _hueBarTex = TextureAssets.Hue.Value;
        private readonly Texture2D _colorBarTex = TextureAssets.ColorBar.Value;
        private readonly Texture2D _sliderHighlightTex = TextureAssets.ColorHighlight.Value;
        private readonly Texture2D _colorBlipTex = TextureAssets.ColorBlip.Value;
        private readonly Texture2D _colorSliderTex = TextureAssets.ColorSlider.Value;

        // User is currently dragging 
        private bool _dragging;

        // Type of this colorbar, 0 = Hue, 1 = Saturation, 2 = Luminosity
        private readonly int _type;

        // On create set this:
        public UIColorBar(HairWindow window, int type)
        {
            // Assign the window
            _hairWindow = window;

            // Given type of this colorbar element
            _type = type;

            // Size of the element
            Width.Set(_rect.Width, 0f);
            Height.Set(_rect.Height, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            // Size of this element
            CalculatedStyle dimensions = GetInnerDimensions();

            // If the element type is a saturation or luminosity bar
            if (_type > 0)
            {
                // Draw the normal color bar texture without inner texture
                spriteBatch.Draw(_colorBarTex, dimensions.Center(), _rect, Color.White, 0f, _colorBarTex.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }
            else
            {
                // Draw the specific hue bar texture for the color slider
                spriteBatch.Draw(_hueBarTex, dimensions.Center(), _hueBarTex.Bounds, Color.White, 0f, _hueBarTex.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }

            // If we're hovering over this bar with the mouse cursor
            if (IsMouseHovering)
            {
                // Draw the highlight texture
                spriteBatch.Draw(_sliderHighlightTex, dimensions.Center(), _sliderHighlightTex.Bounds, Main.OurFavoriteColor, 0f, _sliderHighlightTex.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }

            // If this is not the hue bar => draw the inner of this bar depending on the chosen color, saturation and luminosity
            if (_type > 0)
            {
                // The inner of this bar is made up from 168 segments (pixels)
                for (int i = 0; i < _rect.Width - _rectSideWidth * 2; i++)
                {
                    // Calculate the current hsl value of the bar segment
                    float pointX = i / (float)(_rect.Width - _rectSideWidth * 2);

                    // Get the color of the current segment in rgb
                    Color rgb = _type == 1 ? Main.hslToRgb(_hairWindow.Color_Hue, pointX, _hairWindow.Color_Luminosity) : Main.hslToRgb(_hairWindow.Color_Hue, _hairWindow.Color_Saturation, pointX);

                    // Draw the current segment with the calculated color
                    spriteBatch.Draw(_colorBlipTex, new Vector2(dimensions.X + _rectSideWidth + i, dimensions.Y + 4), rgb);
                }
            }

            float SliderPos = 0f;

            // Calculate the slider's position depending on the type of this color bar type
            switch (_type)
            {
                case 0: SliderPos = (dimensions.Width - 4) * _hairWindow.Color_Hue; break;
                case 1: SliderPos = (dimensions.Width - 4) * _hairWindow.Color_Saturation; break;
                case 2: SliderPos = (dimensions.Width - 4) * _hairWindow.Color_Luminosity; break;
            }

            // Draw the Slider on top of the color bar
            spriteBatch.Draw(_colorSliderTex, new Vector2(dimensions.X + SliderPos - _colorSliderTex.Width / 2, dimensions.Y - 4), _colorSliderTex.Bounds, Color.White);
        }

        public override void Update(GameTime gameTime)
        {
            CalculatedStyle dimensions = GetInnerDimensions();

            // Check whether the user has started dragging the slider and still has the mouse button held down
            if (_dragging)
            {
                // Calculate the point of the mouse cursor as a 0f to 1f float depending on the width of the color bar
                float pointX = (Main.MouseScreen.X - dimensions.Position().X) / dimensions.Width;

                // If the mouse cursor is outside on the left of the color bar
                if (Main.MouseScreen.X < dimensions.Position().X)
                {
                    // Limit point X to 0f
                    pointX = 0f;
                }

                // If the mouse cursor is outside on the right of the color bar
                if (Main.MouseScreen.X > dimensions.Position().X + dimensions.Width)
                {
                    // Limit point X to 1f
                    pointX = 1f;
                }

                // Depending on the type of the bar write the point x value to the color float of the hairwindow, also apply it to the player
                switch (_type)
                {
                    case 0:
                        _hairWindow.Color_Hue = pointX;
                        Main.player[Main.myPlayer].hairColor = Main.hslToRgb(_hairWindow.Color_Hue, _hairWindow.Color_Saturation, _hairWindow.Color_Luminosity);
                        break;

                    case 1:
                        _hairWindow.Color_Saturation = pointX;
                        Main.player[Main.myPlayer].hairColor = Main.hslToRgb(_hairWindow.Color_Hue, _hairWindow.Color_Saturation, _hairWindow.Color_Luminosity);
                        break;

                    case 2:
                        _hairWindow.Color_Luminosity = pointX;
                        Main.player[Main.myPlayer].hairColor = Main.hslToRgb(_hairWindow.Color_Hue, _hairWindow.Color_Saturation, _hairWindow.Color_Luminosity);
                        break;
                }
            }
        }

        // Rising adge on mouse down
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
            // user started 'dragging' 
            _dragging = true;
        }

        // Rising adge on mouse up
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            // user stopped 'dragging' 
            _dragging = false;
        }
    }

}
