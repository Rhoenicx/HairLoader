using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace HairLoader.UI
{
	class DragableUIPanel : UIPanel
	{
		private Vector2 _offset;
		private bool _dragging;

		public override void MouseDown(UIMouseEvent evt)
		{
			base.MouseDown(evt);
			DragStart(evt);
		}

		public override void MouseUp(UIMouseEvent evt)
		{
			base.MouseUp(evt);
			DragEnd(evt);
		}

		private void DragStart(UIMouseEvent evt)
		{
			float offsetX = Left.Pixels + Left.Precent * Main.screenWidth;
			float offsetY = Top.Pixels + Top.Precent * Main.screenHeight;
			_offset = new Vector2(evt.MousePosition.X - offsetX, evt.MousePosition.Y - offsetY);
			_dragging = true;
		}

		private void DragEnd(UIMouseEvent evt)
		{
			Vector2 end = evt.MousePosition;
			_dragging = false;

			Left.Set(end.X - _offset.X, 0f);
			Top.Set(end.Y - _offset.Y, 0f);

			Recalculate();
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (ContainsPoint(Main.MouseScreen))
			{
				Main.LocalPlayer.mouseInterface = true;
			}

			if (_dragging)
			{
				Left.Set(Main.mouseX - _offset.X, 0f);
				Top.Set(Main.mouseY - _offset.Y, 0f);
				Recalculate();
			}

			// Check whether DragableUIPanel is outside of UIState (i.e. screen)
			var parentSpace = Parent.GetDimensions().ToRectangle();
			if (!GetDimensions().ToRectangle().Intersects(parentSpace))
			{
				Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
				Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
				Recalculate();
			}
		}
	}
}
