using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.GameContent.UI;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI.Chat;
using Terraria.UI;
using Terraria;
using Microsoft.Xna.Framework;

namespace HairLoader.UI.Components
{
    internal class UIPrice : UIElement
    {
        // Instance of the Hairwindow this element is placed on
        private readonly HairWindow _hairWindow;

        public UIPrice(HairWindow window)
        {
            _hairWindow = window;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            // Return if the selected hair does not exist in the hairtable or when there are no changes on the player
            if (!HairLoader.HairTable.ContainsKey(_hairWindow.SelectedHairID)
                || _hairWindow.SelectedHairID == _hairWindow.OldHairID && Main.player[Main.myPlayer].hairColor == _hairWindow.OldHairColor)
            {
                return;
            }

            Vector2 windowPosition = _hairWindow.HairWindowPosition;

            // Draw the savings element
            DrawCurrency(
                spriteBatch,
                new Vector2(windowPosition.X + 632f, windowPosition.Y + 160f),
                Lang.inter[66].Value,
                HairLoader.HairTable[_hairWindow.SelectedHairID].HasCustomPrice
                    ? HairLoader.HairTable[_hairWindow.SelectedHairID].CustomCurrencyID
                    : -1,
                true);

            // Draw the price element
            DrawCurrency(
                spriteBatch,
                new Vector2(windowPosition.X + 632f, windowPosition.Y + 190f),
                Language.GetTextValue("Mods.HairLoader.HairWindowUI.Cost"),
                HairLoader.HairTable[_hairWindow.SelectedHairID].HasCustomPrice
                    ? HairLoader.HairTable[_hairWindow.SelectedHairID].CustomCurrencyID
                    : -1,
                false);
        }

        private void DrawCurrency(SpriteBatch spriteBatch, Vector2 position, string text, int currency, bool savings = false)
        {
            // Buyprice is too low
            if (_hairWindow.BuyPrice <= 0)
            {
                return;
            }

            // Get the player instance 
            Player player = Main.player[Main.myPlayer];

            // Check which currency we need to display
            if (currency == -1)
            {
                // the amount to display
                long count;

                // Draw Savings icons
                if (savings)
                {
                    long num1 = Utils.CoinsCount(out _, player.bank.item);
                    long num2 = Utils.CoinsCount(out _, player.bank2.item);
                    long num3 = Utils.CoinsCount(out _, player.bank3.item);
                    long num4 = Utils.CoinsCount(out _, player.bank4.item);
                    count = Utils.CoinsCombineStacks(out _, num1, num2, num3, num4);
                    DrawSavingsIcon(spriteBatch, position, count, num1, num2, num3, num4);
                }
                else
                {
                    count = _hairWindow.BuyPrice;
                }

                // Draw Text
                DynamicSpriteFont value = FontAssets.MouseText.Value;
                Vector2 vector = value.MeasureString(text);
                Color baseColor = Color.Black * (Color.White.A / 255f);
                Vector2 origin = new Vector2(0f, 0f) * vector;
                Vector2 baseScale = new(1f);
                TextSnippet[] snippets = ChatManager.ParseMessage(text, Color.White).ToArray();
                ChatManager.ConvertNormalSnippets(snippets);
                ChatManager.DrawColorCodedStringShadow(spriteBatch, value, snippets, new Vector2(position.X, position.Y + 40f), baseColor, 0f, origin, baseScale, -1f, 1.5f);
                ChatManager.DrawColorCodedString(spriteBatch, value, snippets, new Vector2(position.X, position.Y + 40f), Color.White, 0f, origin, baseScale, out var _, -1f);

                // Draw Coins
                int[] coinsArray = Utils.CoinsSplit(count);

                for (int index = 0; index < 4; index++)
                {
                    Main.instance.LoadItem(74 - index);
                    Vector2 currencyPosition = new(position.X + 24f * index + 120f, position.Y + 50f);
                    spriteBatch.Draw(TextureAssets.Item[74 - index].Value, currencyPosition, new Rectangle?(), Color.White, 0.0f, TextureAssets.Item[74 - index].Value.Size() * 0.5f, 1f, SpriteEffects.None, 0.0f);
                    Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, coinsArray[3 - index].ToString(), currencyPosition.X - 11f, currencyPosition.Y, Color.White, Color.Black, new Vector2(0.3f), 0.75f);
                }
            }
            else if (CustomCurrencyManager.TryGetCurrencySystem(currency, out CustomCurrencySystem system))
            {
                // Reflection magic
                Type type = system.GetType();
                FieldInfo field = type.GetField("_valuePerUnit", BindingFlags.Instance | BindingFlags.NonPublic);
                Dictionary<int, int> valuePerUnit = field.GetValue(system) as Dictionary<int, int>;

                // the amount to display
                long count;

                // Draw Savings
                if (savings)
                {
                    long num1 = system.CountCurrency(out _, player.bank.item);
                    long num2 = system.CountCurrency(out _, player.bank2.item);
                    long num3 = system.CountCurrency(out _, player.bank3.item);
                    long num4 = system.CountCurrency(out _, player.bank4.item);
                    count = system.CombineStacks(out _, num1, num2, num3, num4);
                    DrawSavingsIcon(spriteBatch, position, count, num1, num2, num3, num4);
                }
                else
                {
                    count = _hairWindow.BuyPrice;
                }

                // Draw Text
                DynamicSpriteFont value = FontAssets.MouseText.Value;
                Vector2 vector = value.MeasureString(text);
                Color baseColor = Color.Black * (Color.White.A / 255f);
                Vector2 origin = new Vector2(0f, 0f) * vector;
                Vector2 baseScale = new(1f);
                TextSnippet[] snippets = ChatManager.ParseMessage(text, Color.White).ToArray();
                ChatManager.ConvertNormalSnippets(snippets);
                ChatManager.DrawColorCodedStringShadow(spriteBatch, value, snippets, new Vector2(position.X, position.Y + 40f), baseColor, 0f, origin, baseScale, -1f, 1.5f);
                ChatManager.DrawColorCodedString(spriteBatch, value, snippets, new Vector2(position.X, position.Y + 40f), Color.White, 0f, origin, baseScale, out var _, -1f);

                // Draw Coins
                int i = valuePerUnit.Keys.ElementAt(0);
                Main.instance.LoadItem(i);
                Texture2D texture = TextureAssets.Item[i].Value;

                Vector2 currencyPosition = new(position.X + 120f, position.Y + 50f);
                spriteBatch.Draw(texture, currencyPosition, new Rectangle?(), Color.White, 0.0f, texture.Size() * 0.5f, 0.8f, SpriteEffects.None, 0.0f);
                Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, count.ToString(), currencyPosition.X - 11f, currencyPosition.Y, Color.White, Color.Black, new Vector2(0.3f), 0.75f);
            }
        }

        private static void DrawSavingsIcon(SpriteBatch spriteBatch, Vector2 position, long count, long num1, long num2, long num3, long num4)
        {
            if (count > 0L)
            {
                Main.GetItemDrawFrame(4076, out Texture2D itemTexture1, out Rectangle itemFrame1);
                Main.GetItemDrawFrame(3813, out Texture2D itemTexture2, out Rectangle itemFrame2);
                Main.GetItemDrawFrame(346, out Texture2D itemTexture3, out Rectangle itemFrame3);
                Main.GetItemDrawFrame(87, out Texture2D itemTexture4, out Rectangle itemFrame4);

                if (num4 > 0L)
                    spriteBatch.Draw(itemTexture1, Utils.CenteredRectangle(new Vector2(position.X + 92f, position.Y + 45f), itemFrame1.Size() * 0.65f), new Rectangle?(), Color.White);
                if (num3 > 0L)
                    spriteBatch.Draw(itemTexture2, Utils.CenteredRectangle(new Vector2(position.X + 92f, position.Y + 45f), itemFrame2.Size() * 0.65f), new Rectangle?(), Color.White);
                if (num2 > 0L)
                    spriteBatch.Draw(itemTexture3, Utils.CenteredRectangle(new Vector2(position.X + 80f, position.Y + 50f), itemFrame3.Size() * 0.65f), new Rectangle?(), Color.White);
                if (num1 > 0L)
                    spriteBatch.Draw(itemTexture4, Utils.CenteredRectangle(new Vector2(position.X + 70f, position.Y + 60f), itemFrame4.Size() * 0.65f), new Rectangle?(), Color.White);
            }
        }
    }
}
