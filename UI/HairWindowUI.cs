using System;
using System.Globalization;
using ReLogic.Graphics;
using ReLogic.OS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.ID;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.GameContent.UI;
using HairLoader.UI.Components;

namespace HairLoader.UI
{
    public class HairWindow : UIState
    {
        // Visibility of panel
        public bool Visible;

        // Highlights
        public string HighlightDisplayName
            = Language.GetTextValue("Mods.HairLoader.HairWindowUI.Categories.AllEntry");
        public string HighlightText = null;

        // New Selected Entry
        public int SelectedHairID;

        // Price of the new selected hair and/or color
        public int BuyPrice = 0;

        // Save old player hair & color
        public int OldHairID;
        public Color OldHairColor;

        // Saved Colors
        public float Color_Hue;
        public float Color_Saturation;
        public float Color_Luminosity;

        // Show Locked hairstyles
        public bool ShowLocked = false;

        // Animation
        private const int numAnimationFrames = 12;
        public int AnimationProgress;
        private int AnimationCounter;

        // Background Element
        private DragableUIPanel HairWindowPanel;
        public Vector2 HairWindowPosition => 
            new(HairWindowPanel.Left.Pixels + HairWindowPanel.Left.Precent * Main.screenWidth,
                HairWindowPanel.Top.Pixels + HairWindowPanel.Top.Precent * Main.screenHeight);

        // Mod List Element
        private UIPanel ModListPanel;
        private UIText ModListText;
        private UIList ModList;
        private UIScrollbar ModListScrollbar;

        // Hair List Element
        private UIPanel HairListPanel;
        private UIText HairListText;
        private UIGrid HairGrid;
        private UIScrollbar HairListScrollbar;

        // Color bars
        private UIText ColorPanelText;
        private UIPanel ColorPanel;
        private UIColorBar ColorHueBar;
        private UIColorBar ColorSaturationBar;
        private UIColorBar ColorLuminosityBar;

        // Copy/paste/randomize buttons
        private UIColoredImageButton CopyColorButton;
        private UIColoredImageButton PasteColorButton;
        private UIColoredImageButton RandomizeColorButton;
        private UIPanel HexCodePanel;
        private UIText HexCodeText;

        // Text
        private UIText BuyText;
        private UIText CancelText;

        // Price
        private UIPrice PriceElement;

        // Show locked hair button
        private UISetting ShowLockedButton;

        public override void OnInitialize()
        {
            // Background of the HairWindow
            HairWindowPanel = new DragableUIPanel(this);
            HairWindowPanel.SetPadding(10);
            HairWindowPanel.Left.Set(-426f, 0.5f);
            HairWindowPanel.Top.Set(0f, 0.55f);
            HairWindowPanel.Width.Set(852f, 0f);
            HairWindowPanel.Height.Set(300f, 0f);
            HairWindowPanel.BackgroundColor = new Color(73, 94, 171);

            // MODS
            ModListText = new UIText("Categories");
            ModListText.Top.Set(-4f, 0f);
            ModListText.Left.Pixels = 0f;
            HairWindowPanel.Append(ModListText);

            ModListPanel = new UIPanel();
            ModListPanel.Top.Set(20f, 0f);
            ModListPanel.Left.Set(0f, 0f);
            ModListPanel.Width.Set(212f, 0f);
            ModListPanel.Height.Set(-20f, 1f);
            ModListPanel.SetPadding(10);
            ModListPanel.BackgroundColor = new Color(53, 74, 151);

            ModList = new UIList();
            ModList.Width.Set(-16f, 1f);
            ModList.Height.Set(0f, 1f);
            ModList.ListPadding = 0f;
            ModListPanel.Append(ModList);

            ModListScrollbar = new UIScrollbar();
            ModListScrollbar.SetView(100f, 1000f);
            ModListScrollbar.Top.Pixels = 4f;
            ModListScrollbar.Left.Pixels = 4f;
            ModListScrollbar.Height.Set(-8f, 1f);
            ModListScrollbar.HAlign = 1f;
            ModListPanel.Append(ModListScrollbar);
            ModList.SetScrollbar(ModListScrollbar);

            HairWindowPanel.Append(ModListPanel);

            // Hair
            HairListText = new UIText("Hair Style");
            HairListText.Top.Set(-4f, 0f);
            HairListText.Left.Pixels = 220f;
            HairWindowPanel.Append(HairListText);

            HairListPanel = new UIPanel();
            HairListPanel.Top.Set(20f, 0f);
            HairListPanel.Left.Set(220f, 0f);
            HairListPanel.Width.Set(392f, 0f);
            HairListPanel.Height.Set(-20f, 1f);
            HairListPanel.SetPadding(10);
            HairListPanel.BackgroundColor = new Color(53, 74, 151);

            HairGrid = new UIGrid(7);
            HairGrid.Width.Set(-16f, 1f);
            HairGrid.Height.Set(0f, 1f);
            HairGrid.ListPadding = 6f;
            HairListPanel.Append(HairGrid);

            HairListScrollbar = new UIScrollbar();
            HairListScrollbar.SetView(100f, 1000f);
            HairListScrollbar.Top.Pixels = 4f;
            HairListScrollbar.Left.Pixels = 4f;
            HairListScrollbar.Height.Set(-8f, 1f);
            HairListScrollbar.HAlign = 1f;
            HairListPanel.Append(HairListScrollbar);
            HairGrid.SetScrollbar(HairListScrollbar);

            HairWindowPanel.Append(HairListPanel);

            // Color panel text
            ColorPanelText = new UIText("Color");
            ColorPanelText.Top.Set(-4f, 0f);
            ColorPanelText.Left.Pixels = 620f;
            HairWindowPanel.Append(ColorPanelText);

            // Color panel
            ColorPanel = new UIPanel();
            ColorPanel.Top.Set(20f, 0f);
            ColorPanel.Left.Set(620f, 0f);
            ColorPanel.Width.Set(212f, 0f);
            ColorPanel.Height.Set(118f, 0f);
            ColorPanel.SetPadding(10);
            ColorPanel.BackgroundColor = new Color(53, 74, 151);
            HairWindowPanel.Append(ColorPanel);

            // Hue Slider
            ColorHueBar = new UIColorBar(this, 0);
            ColorHueBar.Top.Set(34f, 0f);
            ColorHueBar.Left.Set(636f, 0f);
            HairWindowPanel.Append(ColorHueBar);

            // Saturation Slider
            ColorSaturationBar = new UIColorBar(this, 1);
            ColorSaturationBar.Top.Set(70f, 0f);
            ColorSaturationBar.Left.Set(636f, 0f);
            HairWindowPanel.Append(ColorSaturationBar);

            // Luminosity Slider
            ColorLuminosityBar = new UIColorBar(this, 2);
            ColorLuminosityBar.Top.Set(106f, 0f);
            ColorLuminosityBar.Left.Set(636f, 0f);
            HairWindowPanel.Append(ColorLuminosityBar);

            // Copy Color Button
            CopyColorButton = new UIColoredImageButton((Asset<Texture2D>)Main.Assets.Request<Texture2D>("Images/UI/CharCreation/Copy"), true);
            CopyColorButton.Top.Set(146f, 0f);
            CopyColorButton.Left.Set(620f, 0f);
            CopyColorButton.OnLeftMouseDown += (a, b) =>
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                ((IClipboard)Platform.Get<IClipboard>()).Value = HexCodeText.Text;
            };
            HairWindowPanel.Append(CopyColorButton);

            // Paste Color Button
            PasteColorButton = new UIColoredImageButton((Asset<Texture2D>)Main.Assets.Request<Texture2D>("Images/UI/CharCreation/Paste"), true);
            PasteColorButton.Top.Set(146f, 0f);
            PasteColorButton.Left.Set(660f, 0f);
            PasteColorButton.OnLeftMouseDown += (a, b) =>
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                if (!GetHexColor(((IClipboard)Platform.Get<IClipboard>()).Value, out Color rgb))
                    return;
                Vector3 hsl = Main.rgbToHsl(rgb);
                Color_Hue = hsl.X;
                Color_Saturation = hsl.Y;
                Color_Luminosity = hsl.Z;
                Main.player[Main.myPlayer].hairColor = rgb;
            };
            HairWindowPanel.Append(PasteColorButton);

            // Randomize Color Button
            RandomizeColorButton = new UIColoredImageButton((Asset<Texture2D>)Main.Assets.Request<Texture2D>("Images/UI/CharCreation/Randomize"), true);
            RandomizeColorButton.Top.Set(146f, 0f);
            RandomizeColorButton.Left.Set(700f, 0f);
            RandomizeColorButton.OnLeftMouseDown += (a, b) =>
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                Color rgb = new((int)Main.rand.Next(0, 256), (int)Main.rand.Next(0, 256), (int)Main.rand.Next(0, 256));
                Vector3 hsl = Main.rgbToHsl(rgb);
                Color_Hue = hsl.X;
                Color_Saturation = hsl.Y;
                Color_Luminosity = hsl.Z;
                Main.player[Main.myPlayer].hairColor = rgb;
            };
            HairWindowPanel.Append(RandomizeColorButton);

            // HEX Code panel
            HexCodePanel = new UIPanel();
            HexCodePanel.Top.Set(146f, 0f);
            HexCodePanel.Left.Set(740f, 0f);
            HexCodePanel.Width.Set(92f, 0f);
            HexCodePanel.Height.Set(32f, 0f);
            HairWindowPanel.Append(HexCodePanel);

            // HEX Code text
            HexCodeText = new UIText("#FFFFFF");
            HexCodeText.Top.Set(154f, 0f);
            HexCodeText.Left.Set(748f, 0f);
            HexCodeText.Width.Set(76f, 0f);
            HexCodeText.Height.Set(24f, 0f);
            HairWindowPanel.Append(HexCodeText);

            // Price
            PriceElement = new UIPrice(this);
            PriceElement.Top.Set(208f, 0f);
            PriceElement.Left.Set(646f, 0f);
            PriceElement.Width.Set(178f, 0f);
            PriceElement.Height.Set(84f, 0f);
            HairWindowPanel.Append(PriceElement);

            // Buy Text
            BuyText = new UIText("Buy", 1.25f, false);
            BuyText.Top.Set(256f, 0f);
            BuyText.Left.Set(630f, 0f);
            BuyText.OnMouseOver += (a, b) =>
            {
                if (Main.player[Main.myPlayer].hair != OldHairID || Main.player[Main.myPlayer].hairColor != OldHairColor)
                {
                    BuyText.TextColor = new Color(255, 199, 0);
                    BuyText.SetText("Buy", 1.5f, false);
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
            };
            BuyText.OnMouseOut += (a, b) =>
            {
                BuyText.TextColor = new Color(235, 235, 235);
                BuyText.SetText("Buy", 1.25f, false);
            };
            BuyText.OnLeftClick += (a, b) => { if (CanBuyHair()) BuyHairWindow(); };
            HairWindowPanel.Append(BuyText);

            // Cancel Text
            CancelText = new UIText("Cancel", 1.25f, false);
            CancelText.Top.Set(256f, 0f);
            CancelText.Left.Set(750f, 0f);
            CancelText.OnMouseOver += (a, b) =>
            {
                CancelText.TextColor = new Color(255, 199, 0);
                CancelText.SetText("Cancel", 1.5f, false);
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
            CancelText.OnMouseOut += (a, b) =>
            {
                CancelText.TextColor = new Color(235, 235, 235);
                CancelText.SetText("Cancel", 1.25f, false);
            };
            CancelText.OnLeftClick += (a, b) => { CloseHairWindow(); };
            HairWindowPanel.Append(CancelText);

            // Show locked button
            ShowLockedButton = new UISetting(this);
            ShowLockedButton.Top.Set(0f, 0f);
            ShowLockedButton.Left.Set(588f, 0f);
            ShowLockedButton.OnLeftMouseDown += (a, b) =>
            {
                ShowLocked = !ShowLocked;
                UpdateModList();
                ModListContainsSelected();
                UpdateHairGrid(HighlightDisplayName == "All", HighlightDisplayName);
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
            HairWindowPanel.Append(ShowLockedButton);

            Append(HairWindowPanel);
        }

        public void ResetPosition()
        {
            HairWindowPanel.Left.Set(-426f, 0.5f);
            HairWindowPanel.Top.Set(0f, 0.55f);
            HairWindowPanel.Recalculate();
        }

        public override void Update(GameTime gameTime)
        {
            if (Main.npcChatText != "" || Main.playerInventory || (Main.player[Main.myPlayer].chest != -1 || Main.npcShop != 0) || (Main.player[Main.myPlayer].talkNPC == -1 || Main.InGuideCraftMenu))
            {
                CloseHairWindow();
            }

            // Update animations
            if (AnimationCounter++ % 120 < 60)
            {
                int speed = 60 / numAnimationFrames;
                AnimationProgress = (AnimationCounter % 120) / speed;
            }
            else
            {
                AnimationProgress = 0;
            }

            // Calculate the buyprice
            BuyPrice = CalculatePrice();

            base.Update(gameTime);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (HairWindowPanel.ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            UpdateHexText(Main.hslToRgb(new Vector3(Color_Hue, Color_Saturation, Color_Luminosity)));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Reset the highlight text
            HighlightText = null;

            // Draw the Hairwindow UI => here the highlight text will probably be set to something else than null...
            // Reason why this is placed on top: We want to have the highlight text on the mouse cursor to be drawn on top of all the other elements.
            // so thats why we first draw the UI and then the text (properly layered)
            base.Draw(spriteBatch);

            // Mouse cursor is hovering over the Copy color button
            if (CopyColorButton.IsMouseHovering)
                HighlightText = Language.GetTextValue("UI.CopyColorToClipboard");

            // Mouse cursor is hovering over the Paste color button
            if (PasteColorButton.IsMouseHovering)
                HighlightText = Language.GetTextValue("UI.PasteColorFromClipboard");

            // Mouse cursor is hovering over the Randomize color button
            if (RandomizeColorButton.IsMouseHovering)
                HighlightText = Language.GetTextValue("UI.RandomizeColor");

            // The hightlight text is not null => means the mouse is currently hovering on top of an element that wants to display text
            if (HighlightText != null)
            {
                // Draw the mouse text
                float x = (float)FontAssets.MouseText.Value.MeasureString(HighlightText).X;
                Vector2 vector2 = new Vector2((float)Main.mouseX, (float)Main.mouseY) + new Vector2(16f);
                if (vector2.Y > (double)(Main.screenHeight - 30))
                    vector2.Y = (float)(double)(Main.screenHeight - 30);
                if (vector2.X > (double)Main.screenWidth - (double)x)
                    vector2.X = (float)(double)(Main.screenWidth - 460);
                Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, HighlightText, (float)vector2.X, (float)vector2.Y, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, Vector2.Zero);
            }
        }

        public void OpenHairWindow()
        {
            // Save the hair and color of the player
            OldHairID = Main.player[Main.myPlayer].hair;
            OldHairColor = Main.player[Main.myPlayer].hairColor;

            // Redundancy check if the hair exists
            if (!HairLoader.HairTable.ContainsKey(OldHairID))
            {
                // Somehow the hairentry could not be found in the hairtable...
                return;
            }

            // Write the hair to the selected hair variable
            SelectedHairID = OldHairID;

            // Convert the hairColor of the player from rgb to hsl
            Vector3 hsl = Main.rgbToHsl(OldHairColor);

            // Save the hairColor internally
            Color_Hue = hsl.X;
            Color_Saturation = hsl.Y;
            Color_Luminosity = hsl.Z;

            // Update the Hexcode text
            UpdateHexText(OldHairColor);

            // Close the player's inventory
            Main.playerInventory = false;

            // Reset the chatText
            Main.npcChatText = "";

            // Update the unlocked hairstyles in the entire hairtable
            UpdateUnlocks();

            // Update the modlist of the UI
            UpdateModList();

            // Force update the hairGrid of the UI with the latest values (or default)
            UpdateHairGrid(HighlightDisplayName == "All" || !ModListContainsSelected(), HighlightDisplayName);

            // Make the UI visible
            Visible = true;

            // Play opening sound
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }

        private void CloseHairWindow()
        {
            if (!Visible)
            {
                return;
            }

            Main.player[Main.myPlayer].hair = OldHairID;
            Main.player[Main.myPlayer].hairColor = OldHairColor;

            Visible = false;

            if (Main.player[Main.myPlayer].talkNPC > -1)
            {
                Main.player[Main.myPlayer].SetTalkNPC(-1);
            }

            SoundEngine.PlaySound(SoundID.MenuClose);
        }

        private void BuyHairWindow()
        {
            Main.player[Main.myPlayer].hair = SelectedHairID;
            Main.player[Main.myPlayer].hairColor = Main.hslToRgb(Color_Hue, Color_Saturation, Color_Luminosity);

            Visible = false;

            if (Main.player[Main.myPlayer].talkNPC > -1)
            {
                Main.player[Main.myPlayer].SetTalkNPC(-1);
            }

            SoundEngine.PlaySound(SoundID.Coins);

            NetMessage.SendData(MessageID.SyncPlayer, number: Main.myPlayer);
        }

        private bool CanBuyHair()
        {
            if ((Main.player[Main.myPlayer].hair == OldHairID && Main.player[Main.myPlayer].hairColor == OldHairColor)
                || (HairLoader.HairTable[SelectedHairID].HasCustomPrice && !CustomCurrencyManager.TryGetCurrencySystem(HairLoader.HairTable[SelectedHairID].CustomCurrencyID, out _)))
            {
                return false;
            }

            if (Main.player[Main.myPlayer].BuyItem(BuyPrice, HairLoader.HairTable[SelectedHairID].HasCustomPrice ? HairLoader.HairTable[SelectedHairID].CustomCurrencyID : -1))
            {
                return true;
            }

            return false;
        }

        private int CalculatePrice()
        {
            int price = 0;

            // Check if the hairstyle has been changed
            if (SelectedHairID != OldHairID)
            {
                // Increase price with hair buy price
                price += HairLoader.HairTable[SelectedHairID].HasCustomPrice
                    ? HairLoader.HairTable[SelectedHairID].CustomHairPrice
                    : HairLoader.HairTable[SelectedHairID].HairPrice;
            }

            // Check if the color has been changed
            if (Main.player[Main.myPlayer].hairColor != OldHairColor)
            {
                // Increase price with hair re-color price
                price += HairLoader.HairTable[SelectedHairID].HasCustomPrice
                    ? HairLoader.HairTable[SelectedHairID].CustomColorPrice
                    : HairLoader.HairTable[SelectedHairID].ColorPrice;
            }

            // Vanilla price adjustment from NPC happiness
            if ((!HairLoader.HairTable[SelectedHairID].HasCustomPrice && HairLoader.HairTable[SelectedHairID].UsePriceAdjustment)
                || (HairLoader.HairTable[SelectedHairID].HasCustomPrice && HairLoader.HairTable[SelectedHairID].UseCustomPriceAdjustment))
            {
                price = (int)((float)price * (float)Main.player[Main.myPlayer].currentShoppingSettings.PriceAdjustment);
            }

            return price;
        }

        private static void UpdateUnlocks()
        {
            // Trick the vanilla UpdateUnlock code
            Main.hairWindow = true;
            Main.Hairstyles.UpdateUnlocks();
            Main.hairWindow = false;
        }

        private void UpdateModList()
        {
            ModList.Clear();

            ModSlot modListEntryAll = new(Language.GetTextValue("Mods.HairLoader.HairWindowUI.Categories.AllEntry"));
            modListEntryAll.OnLeftClick += (a, b) =>
            {
                HighlightDisplayName = Language.GetTextValue("Mods.HairLoader.HairWindowUI.Categories.AllEntry");
                UpdateHairGrid(true, HighlightDisplayName);
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
            ModList.Add(modListEntryAll);

            HashSet<string> ModNames = new();

            foreach (HairEntry entry in HairLoader.HairTable.Values)
            {
                string modName = "";

                if (entry.HasCustomModName)
                {
                    modName = entry.CustomModNameIsLocalized ? Language.GetTextValue(entry.CustomModName) : entry.CustomModName;
                }
                else
                {
                    modName = entry.ModNameIsLocalized ? Language.GetTextValue(entry.ModName) : entry.ModName;
                }

                if (!ModNames.Contains(modName) 
                    && modName != Language.GetTextValue("Mods.HairLoader.HairWindowUI.Categories.AllEntry") 
                    && modName != "")
                {
                    ModNames.Add(modName);
                }
            }

            foreach (string modName in ModNames)
            {
                // Create a new mod slot with the displayname of this modclass
                ModSlot modListEntry = new(modName);
                
                // When the slot is clicked
                modListEntry.OnLeftClick += (a, b) =>
                {
                    // Write the displayname to the highlightDisplayName
                    HighlightDisplayName = modName;
                
                    // Update the Hairgrid with hairs from this mod
                    UpdateHairGrid(false, modName); // No 'all' button 
                
                    // Play tick sound
                    SoundEngine.PlaySound(SoundID.MenuTick);
                };
                
                ModList.Add(modListEntry);
            }
        }

        // This Function updates the hairgrid
        private void UpdateHairGrid(bool AllButton, string highlightDisplayName)
        {
            // First clear the entire grid from elements so we can rebuild it
            HairGrid.Clear();

            // Scan all the mods and hairstyles
            foreach (KeyValuePair<int, HairEntry> entry in HairLoader.HairTable)
            {
                string modName = "";

                if (entry.Value.HasCustomModName)
                {
                    modName = entry.Value.CustomModNameIsLocalized ? Language.GetTextValue(entry.Value.CustomModName) : entry.Value.CustomModName;
                }
                else
                {
                    modName = entry.Value.ModNameIsLocalized ? Language.GetTextValue(entry.Value.ModName) : entry.Value.ModName;
                }

                // Allbutton has been pressed or mod is the highlighted/selected mod
                if (AllButton || modName == highlightDisplayName)
                {
                    bool available = Main.Hairstyles.AvailableHairstyles.Contains(entry.Key);

                    // Check if this hairstyle should be displayed
                    if (!available && !ShowLocked)
                    {
                        continue;
                    }

                    // Add the hairstyle slot
                    HairSlot slot = new(this, entry.Key, !available);

                    // When this hairslot is clicked set the following:
                    slot.OnLeftClick += (a, b) =>
                    {
                        if (!available)
                        {
                            return;
                        }

                        // Apply the hairstyle of this slot to the player
                        Main.player[Main.myPlayer].hair = entry.Key;

                        // Save the selected hair and mod 
                        SelectedHairID = entry.Key;

                        // Play tick sound
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    };

                    // Add the UI element to the items of the grid
                    HairGrid._items.Add(slot);

                    // Append the slot to the innerList
                    HairGrid._innerList.Append(slot);
                }
            }

            // Update the order of the slots => default 
            HairGrid.UpdateOrder();

            // Recalculate the list so for the items to get placed on the right position inside the grid
            HairGrid._innerList.Recalculate();
        }

        // Function to update the text in the hex code text element
        private void UpdateHexText(Color pendingColor) => HexCodeText.SetText(GetHexText(pendingColor));

        // Function to convert the given color into a string
        private static string GetHexText(Color pendingColor) => "#" + pendingColor.Hex3().ToUpper();

        // Function used to determine if the given string is a hexcode
        private static bool GetHexColor(string hexString, out Color rgb)
        {
            // Check if the string starts with a #
            if (hexString.StartsWith("#"))
            {
                // Remove the first element of the string (the #)
                hexString = hexString[1..];
            }

            // Check if the given string has length 6 or lower and try to parse it as an hexnumber
            if (hexString.Length <= 6 && uint.TryParse(hexString, NumberStyles.HexNumber, (IFormatProvider)CultureInfo.CurrentCulture, out uint result))
            {
                // shift the bytes of the characters around to grab the individual numbers
                uint num1 = result & (uint)byte.MaxValue;
                uint num2 = result >> 8 & (uint)byte.MaxValue;
                uint num3 = result >> 16 & (uint)byte.MaxValue;

                // create the new color and return true
                rgb = new Color((int)num3, (int)num2, (int)num1);
                return true;
            }

            // string is no hexcode
            rgb = new Color(0, 0, 0);
            return false;
        }

        private bool ModListContainsSelected()
        {
            foreach (UIElement element in ModList._items)
            {
                if (element is ModSlot slot)
                {
                    if (slot.Text == HighlightDisplayName)
                    {
                        return true;
                    }
                }
            }

            HighlightDisplayName = Language.GetTextValue("Mods.HairLoader.HairWindowUI.Categories.AllEntry");
            return false;
        }

        public bool PreventDragging() => ModListPanel.ContainsPoint(Main.MouseScreen)
            || HairListPanel.ContainsPoint(Main.MouseScreen)
            || ColorPanel.ContainsPoint(Main.MouseScreen)
            || CopyColorButton.ContainsPoint(Main.MouseScreen)
            || PasteColorButton.ContainsPoint(Main.MouseScreen)
            || RandomizeColorButton.ContainsPoint(Main.MouseScreen)
            || BuyText.ContainsPoint(Main.MouseScreen)
            || CancelText.ContainsPoint(Main.MouseScreen)
            || ShowLockedButton.ContainsPoint(Main.MouseScreen);
    }
}