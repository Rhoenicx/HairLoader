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
using Terraria.ModLoader;
using Terraria.Localization;
using ReLogic.Content;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using Terraria.GameContent.UI;

namespace HairLoader.UI
{
    public class HairWindow : UIState
    {
        // Visibility of panel
        public static bool Visible;

        // Highlights
        public static string HighlightDisplayName = "All";
        public static string HighlightText = null;

        // New Selected Entry
        public static int SelectedHairID;

        // Save old player hair & color
        public static int OldHairID;
        public static Color OldHairColor;

        // Saved Colors
        public static float Color_Hue;
        public static float Color_Saturation;
        public static float Color_Luminosity;

        // Show Locked hairstyles
        public static bool ShowLocked = false;

        // Background Element
        private UIPanel HairWindowPanel;

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
            HairWindowPanel = new UIPanel();
            HairWindowPanel.SetPadding(10);
            HairWindowPanel.Left.Set(-426f, 0.5f);
            HairWindowPanel.Top.Set(0f, 0.6f);
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
            ColorHueBar = new UIColorBar(0);
            ColorHueBar.Top.Set(34f, 0f);
            ColorHueBar.Left.Set(636f, 0f);
            HairWindowPanel.Append(ColorHueBar);

            // Saturation Slider
            ColorSaturationBar = new UIColorBar(1);
            ColorSaturationBar.Top.Set(70f, 0f);
            ColorSaturationBar.Left.Set(636f, 0f);
            HairWindowPanel.Append(ColorSaturationBar);

            // Luminosity Slider
            ColorLuminosityBar = new UIColorBar(2);
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
                Color rgb;
                if (!this.GetHexColor(((IClipboard)Platform.Get<IClipboard>()).Value, out rgb))
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
                Color rgb = new Color((int)Main.rand.Next(0, 256), (int)Main.rand.Next(0, 256), (int)Main.rand.Next(0, 256));
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
            PriceElement = new UIPrice();
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
            ShowLockedButton = new UISetting();
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

        public override void Update(GameTime gameTime)
        {
            if (Main.npcChatText != "" || Main.playerInventory || (Main.player[Main.myPlayer].chest != -1 || Main.npcShop != 0) || (Main.player[Main.myPlayer].talkNPC == -1 || Main.InGuideCraftMenu))
            {
                CloseHairWindow();
            }

            base.Update(gameTime);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = HairWindowPanel.GetInnerDimensions();

            if (HairWindowPanel.ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            UpdateHexText(Main.hslToRgb(new Vector3(HairWindow.Color_Hue, HairWindow.Color_Saturation, HairWindow.Color_Luminosity)));
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

        internal void OpenHairWindow(NPC npc = null)
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

        internal void CloseHairWindow()
        {
            if (!Visible)
            {
                return;
            }

            Main.player[Main.myPlayer].hair = OldHairID;
            Main.player[Main.myPlayer].hairColor = OldHairColor;

            Visible = false;

            if (Main.player[Main.myPlayer].talkNPC > -1 && Main.npc[Main.player[Main.myPlayer].talkNPC].type == NPCID.Stylist)
            {
                Main.player[Main.myPlayer].SetTalkNPC(-1);
            }

            SoundEngine.PlaySound(SoundID.MenuClose);
        }

        internal void BuyHairWindow()
        {
            Main.player[Main.myPlayer].hair = SelectedHairID;
            Main.player[Main.myPlayer].hairColor = Main.hslToRgb(Color_Hue, Color_Saturation, Color_Luminosity);

            Visible = false;

            if (Main.player[Main.myPlayer].talkNPC > -1 && Main.npc[Main.player[Main.myPlayer].talkNPC].type == NPCID.Stylist)
            {
                Main.player[Main.myPlayer].SetTalkNPC(-1);
            }

            SoundEngine.PlaySound(SoundID.Coins);
        }

        internal static bool CanBuyHair()
        {
            if (Main.player[Main.myPlayer].hair != OldHairID || Main.player[Main.myPlayer].hairColor != OldHairColor)
            {
                int price = 0;

                if (Main.player[Main.myPlayer].hair != OldHairID)
                {
                    price += HairLoader.HairTable[SelectedHairID].OverridePrice ? HairLoader.HairTable[SelectedHairID].HairPrice : SelectedHairID <= 51 ? 20000 : 200000;
                }

                if (Main.player[Main.myPlayer].hairColor != OldHairColor)
                {
                    price += HairLoader.HairTable[SelectedHairID].OverridePrice ? HairLoader.HairTable[SelectedHairID].ColorPrice : 20000;
                }

                price = (int)((float)price * (float)Main.player[Main.myPlayer].currentShoppingSettings.PriceAdjustment);

                if (Main.player[Main.myPlayer].BuyItem(price, HairLoader.HairTable[SelectedHairID].OverridePrice ? HairLoader.HairTable[SelectedHairID].SpecialCurrencyID : -1))
                {
                    return true;
                }
            }

            return false;
        }

        internal void UpdateUnlocks()
        {
            // Trick the vanilla UpdateUnlock code
            Main.hairWindow = true;
            Main.Hairstyles.UpdateUnlocks();
            Main.hairWindow = false;
        }

        internal void UpdateModList()
        {
            ModList.Clear();

            ModSlot modListEntryAll = new ModSlot("All", Language.GetTextValue("Mods.HairLoader.HairWindowUI.Categories.AllEntry"));
            modListEntryAll.OnLeftClick += (a, b) =>
            {
                HighlightDisplayName = "All";
                UpdateHairGrid(true, HighlightDisplayName);
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
            ModList.Add(modListEntryAll);

            Dictionary<string, string> ModNames = new();

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

                if (!ModNames.ContainsKey(modName) && modName != "All" && modName != "")
                {
                    ModNames.TryAdd(entry.ModName, modName);
                }
            }

            foreach (KeyValuePair<string, string> entry in ModNames)
            {
                // Create a new mod slot with the displayname of this modclass
                ModSlot modListEntry = new ModSlot(entry.Key, entry.Value);
                
                // When the slot is clicked
                modListEntry.OnLeftClick += (a, b) =>
                {
                    // Write the displayname to the highlightDisplayName
                    HighlightDisplayName = entry.Key;
                
                    // Update the Hairgrid with hairs from this mod
                    UpdateHairGrid(false, entry.Key); // No 'all' button 
                
                    // Play tick sound
                    SoundEngine.PlaySound(SoundID.MenuTick);
                };
                
                ModList.Add(modListEntry);
            }
        }

        // This Function updates the hairgrid
        internal void UpdateHairGrid(bool AllButton, string highlightDisplayName)
        {
            // First clear the entire grid from elements so we can rebuild it
            HairGrid.Clear();

            // Scan all the mods and hairstyles
            foreach (KeyValuePair<int, HairEntry> entry in HairLoader.HairTable)
            {
                // Allbutton has been pressed or mod is the highlighted/selected mod
                if (AllButton || entry.Value.ModName == highlightDisplayName)
                {
                    bool available = Main.Hairstyles.AvailableHairstyles.Contains(entry.Key);

                    // Check if this hairstyle should be displayed
                    if (!available && !ShowLocked)
                    {
                        continue;
                    }

                    // Add the hairstyle slot
                    HairSlot slot = new HairSlot(entry.Key, !available);

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
        public void UpdateHexText(Color pendingColor) => HexCodeText.SetText(GetHexText(pendingColor));

        // Function to convert the given color into a string
        private static string GetHexText(Color pendingColor) => "#" + pendingColor.Hex3().ToUpper();

        // Function used to determine if the given string is a hexcode
        private bool GetHexColor(string hexString, out Color rgb)
        {
            // Check if the string starts with a #
            if (hexString.StartsWith("#"))
            {
                // Remove the first element of the string (the #)
                hexString = hexString.Substring(1);
            }

            uint result;

            // Check if the given string has length 6 or lower and try to parse it as an hexnumber
            if (hexString.Length <= 6 && uint.TryParse(hexString, NumberStyles.HexNumber, (IFormatProvider)CultureInfo.CurrentCulture, out result))
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
                    if (slot.InternalName == HighlightDisplayName)
                    {
                        return true;
                    }
                }
            }

            HighlightDisplayName = "All";
            return false;
        }
    }

    // This is a Modslot element inside the list of mods
    internal class ModSlot : UITextPanel<string>
    {
        public string InternalName = "";

        // This element doesn't need any additional data, text is already set on create
        public ModSlot(string internalName, string text) : base(text)
        {
            InternalName = internalName;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            // If the text of this element is the current selected mod in the hairwindow
            if (InternalName == HairWindow.HighlightDisplayName)
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

    // This is a hairslot element inside the grid of the hairwindow
    internal class HairSlot : UIElement
    {
        private bool locked;

        // Scale
        private float scale = 1f;

        // Offset of the hair in the slot
        private const int offsetX = 36;
        private const int offsetY = -8;

        // Hair ID this slot represents
        private int _hairID;

        // Vanilla textures for the slot
        public static Asset<Texture2D> backgroundTexture = Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanel");
        public static Asset<Texture2D> highlightTexture = Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanelHighlight");
        public static Asset<Texture2D> hoverTexture = Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanelBorder");

        public HairSlot(int hairID, bool _locked)
        {
            // On create write the hair and mod names to the internal variables
            this._hairID = hairID;

            this.locked = _locked;

            // Set the width and height
            this.Width.Set(backgroundTexture.Width() * scale, 0f);
            this.Height.Set(backgroundTexture.Height() * scale, 0f);
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = base.GetInnerDimensions();

            // Draw the background texture
            spriteBatch.Draw(backgroundTexture.Value, Vector2.Subtract(this.GetDimensions().Center(), Vector2.Divide(backgroundTexture.Size(), 2f)), null, locked ? Color.Gray : Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            // If the hairstyle in this slot is the one the player is currently wearing
            if (Main.player[Main.myPlayer].hair == _hairID)
            {
                // draw the highlight texture
                spriteBatch.Draw(highlightTexture.Value, Vector2.Subtract(this.GetDimensions().Center(), Vector2.Divide(highlightTexture.Size(), 2f)), null, locked ? Color.Gray : Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }

            // If the mouse is hovering over this slot
            if (IsMouseHovering)
            {
                // Draw the hover texture
                spriteBatch.Draw(hoverTexture.Value, Vector2.Subtract(this.GetDimensions().Center(), Vector2.Divide(hoverTexture.Size(), 2f)), null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

                // Set the highlight text of the hairwindow to the name of the hairstyle in this slot
                if (HairLoader.HairTable.ContainsKey(_hairID) && HairLoader.HairTable[_hairID] != null)
                {
                    if (HairLoader.HairTable[_hairID].HasCustomHairName)
                    {
                        HairWindow.HighlightText = HairLoader.HairTable[_hairID].CustomHairNameIsLocalized
                            ? Language.GetTextValue(HairLoader.HairTable[_hairID].CustomHairName)
                            : HairLoader.HairTable[_hairID].CustomHairName;
                    }
                    else
                    {
                        HairWindow.HighlightText = HairLoader.HairTable[_hairID].HairNameIsLocalized
                            ? Language.GetTextValue(HairLoader.HairTable[_hairID].HairName)
                            : HairLoader.HairTable[_hairID].HairName;
                    }

                    if (HairLoader.HairTable[_hairID].HasUnlockHint)
                    {
                        HairWindow.HighlightText += "\r\n" + (HairLoader.HairTable[_hairID].UnlockHintIsLocalized 
                            ? Language.GetTextValue(HairLoader.HairTable[_hairID].UnlockHint) 
                            : HairLoader.HairTable[_hairID].UnlockHint);
                    }
                }
                else
                {
                    HairWindow.HighlightText = "";
                }
            }

            // Draw the vanilla player textures in the slot => Head, eyeWhite and eye
            spriteBatch.Draw(TextureAssets.Players[0, 0].Value, dimensions.Center(), new Rectangle?(new Rectangle(0, 0, TextureAssets.PlayerHair[Main.player[Main.myPlayer].hair].Width(), 56)), locked ? Color.Gray : Main.player[Main.myPlayer].skinColor, 0.0f, new Vector2(offsetX, dimensions.Height + offsetY) * 0.5f, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Players[0, 1].Value, dimensions.Center(), new Rectangle?(new Rectangle(0, 0, TextureAssets.PlayerHair[Main.player[Main.myPlayer].hair].Width(), 56)), new Color(255, 255, 255, 255), 0.0f, new Vector2(offsetX, dimensions.Height + offsetY) * 0.5f, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Players[0, 2].Value, dimensions.Center(), new Rectangle?(new Rectangle(0, 0, TextureAssets.PlayerHair[Main.player[Main.myPlayer].hair].Width(), 56)), locked ? Color.Gray : Main.player[Main.myPlayer].eyeColor, 0.0f, new Vector2(offsetX, dimensions.Height + offsetY) * 0.5f, 1f, SpriteEffects.None, 0f);

            // Load the Hair textures
            Main.instance.LoadHair(_hairID);

            // Draw the full hairstyle
            Vector2 offset = Main.player[Main.myPlayer].GetHairDrawOffset(_hairID, false);
            spriteBatch.Draw(TextureAssets.PlayerHair[_hairID].Value, dimensions.Center() + offset, new Rectangle(0, 0, TextureAssets.PlayerHair[_hairID].Width(), 38), locked ? Color.Gray : Main.player[Main.myPlayer].hairColor, 0.0f, new Vector2(TextureAssets.PlayerHair[_hairID].Width(), dimensions.Height + offsetY) * 0.5f, 1f, SpriteEffects.None, 0f);
        }
    }

    internal class UISetting : UIElement
    {
        Texture2D TickOff = TextureAssets.InventoryTickOff.Value;
        Texture2D TickOn = TextureAssets.InventoryTickOn.Value;

        public UISetting()
        {
            this.Width.Set(TickOff.Width, 0f);
            this.Height.Set(TickOff.Height, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            // Size of this element
            CalculatedStyle dimensions = base.GetInnerDimensions();

            if (HairWindow.ShowLocked)
            {
                spriteBatch.Draw(TickOn, dimensions.Center(), TickOn.Bounds, Color.White, 0f, TickOn.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }
            else
            {
                spriteBatch.Draw(TickOff, dimensions.Center(), TickOn.Bounds, Color.White, 0f, TickOn.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }

            if (IsMouseHovering)
            {
                HairWindow.HighlightText = Language.GetTextValue("Mods.HairLoader.HairWindowUI.LockedButton");
            }
        }
    }

    internal class UIColorBar : UIElement
    {
        // Base size of the Color Bar element
        Rectangle rect = new Rectangle(0, 0, 178, 16);

        // Width of the side to the start of the inner colors
        int rectSideWidth = 5;

        // Grab the vanilla textures for color sliders... (These are already loaded in during mod.load())
        Texture2D HueBarTex = TextureAssets.Hue.Value;
        Texture2D ColorBarTex = TextureAssets.ColorBar.Value;
        Texture2D SliderHighlightTex = TextureAssets.ColorHighlight.Value;
        Texture2D ColorBlipTex = TextureAssets.ColorBlip.Value;
        Texture2D ColorSliderTex = TextureAssets.ColorSlider.Value;

        // User is currently dragging 
        public bool dragging;

        // Type of this colorbar, 0 = Hue, 1 = Saturation, 2 = Luminosity
        public int type;

        // On create set this:
        public UIColorBar(int _type)
        {
            // Size of the element
            this.Width.Set(rect.Width, 0f);
            this.Height.Set(rect.Height, 0f);

            // Given type of this colorbar element
            type = _type;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            // Size of this element
            CalculatedStyle dimensions = base.GetInnerDimensions();

            // If the element type is a saturation or luminosity bar
            if (type > 0)
            {
                // Draw the normal color bar texture without inner texture
                spriteBatch.Draw(ColorBarTex, dimensions.Center(), rect, Color.White, 0f, ColorBarTex.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }
            else
            {
                // Draw the specific hue bar texture for the color slider
                spriteBatch.Draw(HueBarTex, dimensions.Center(), HueBarTex.Bounds, Color.White, 0f, HueBarTex.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }

            // If we're hovering over this bar with the mouse cursor
            if (IsMouseHovering)
            {
                // Draw the highlight texture
                spriteBatch.Draw(SliderHighlightTex, dimensions.Center(), SliderHighlightTex.Bounds, Main.OurFavoriteColor, 0f, SliderHighlightTex.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }

            // If this is not the hue bar => draw the inner of this bar depending on the chosen color, saturation and luminosity
            if (type > 0)
            {
                // The inner of this bar is made up from 168 segments (pixels)
                for (int i = 0; i < rect.Width - rectSideWidth * 2; i++)
                {
                    // Calculate the current hsl value of the bar segment
                    float pointX = (float)i / (float)(rect.Width - rectSideWidth * 2);

                    // Get the color of the current segment in rgb
                    Color rgb = type == 1 ? Main.hslToRgb(HairWindow.Color_Hue, pointX, HairWindow.Color_Luminosity) : Main.hslToRgb(HairWindow.Color_Hue, HairWindow.Color_Saturation, pointX);

                    // Draw the current segment with the calculated color
                    spriteBatch.Draw(ColorBlipTex, new Vector2(dimensions.X + rectSideWidth + i, dimensions.Y + 4), rgb);
                }
            }

            float SliderPos = 0f;

            // Calculate the slider's position depending on the type of this color bar type
            switch (type)
            {
                case 0: SliderPos = (dimensions.Width - 4) * HairWindow.Color_Hue; break;
                case 1: SliderPos = (dimensions.Width - 4) * HairWindow.Color_Saturation; break;
                case 2: SliderPos = (dimensions.Width - 4) * HairWindow.Color_Luminosity; break;
            }

            // Draw the Slider on top of the color bar
            spriteBatch.Draw(ColorSliderTex, new Vector2(dimensions.X + SliderPos - (ColorSliderTex.Width / 2), dimensions.Y - 4), ColorSliderTex.Bounds, Color.White);
        }

        public override void Update(GameTime gameTime)
        {
            CalculatedStyle dimensions = base.GetInnerDimensions();

            // Check whether the user has started dragging the slider and still has the mouse button held down
            if (dragging)
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
                switch (type)
                {
                    case 0:
                        HairWindow.Color_Hue = pointX;
                        Main.player[Main.myPlayer].hairColor = Main.hslToRgb(HairWindow.Color_Hue, HairWindow.Color_Saturation, HairWindow.Color_Luminosity);
                        break;

                    case 1:
                        HairWindow.Color_Saturation = pointX;
                        Main.player[Main.myPlayer].hairColor = Main.hslToRgb(HairWindow.Color_Hue, HairWindow.Color_Saturation, HairWindow.Color_Luminosity);
                        break;

                    case 2:
                        HairWindow.Color_Luminosity = pointX;
                        Main.player[Main.myPlayer].hairColor = Main.hslToRgb(HairWindow.Color_Hue, HairWindow.Color_Saturation, HairWindow.Color_Luminosity);
                        break;
                }
            }
        }

        // Rising adge on mouse down
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
            // user started 'dragging' 
            dragging = true;
        }

        // Rising adge on mouse up
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            // user stopped 'dragging' 
            dragging = false;
        }
    }

    internal class UIPrice : UIElement
    {
        private float priceAdjustment = (float)Main.player[Main.myPlayer].currentShoppingSettings.PriceAdjustment;
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            priceAdjustment = (float)Main.player[Main.myPlayer].currentShoppingSettings.PriceAdjustment;
            CalculatedStyle dimensions = base.GetInnerDimensions();

            // Return if the selected hair does not exist in the hairtable
            if (!HairLoader.HairTable.ContainsKey(HairWindow.SelectedHairID))
            {
                return;
            }

            // Return if the selected hairstyle and color is the same as the old hairstyle and color
            if (HairWindow.SelectedHairID == HairWindow.OldHairID && Main.player[Main.myPlayer].hairColor == HairWindow.OldHairColor)
            {
                return;
            }

            // Check which currency the hairstyle uses
            if (!HairLoader.HairTable[HairWindow.SelectedHairID].OverridePrice 
                || (HairLoader.HairTable[HairWindow.SelectedHairID].OverridePrice && HairLoader.HairTable[HairWindow.SelectedHairID].SpecialCurrencyID == -1))
            {
                // Regular Coins
                int price = 0;
                int platinum = 0;
                int gold = 0;
                int silver = 0;
                int copper = 0;

                // Check if the hairstyle has been changed
                if (HairWindow.SelectedHairID != HairWindow.OldHairID)
                {
                    // Increase price with hair buy price
                    price += HairLoader.HairTable[HairWindow.SelectedHairID].OverridePrice 
                        ? HairLoader.HairTable[HairWindow.SelectedHairID].HairPrice 
                        : HairWindow.SelectedHairID <= 51 ? 20000 : 200000;
                }

                // Check if the color has been changed
                if (Main.player[Main.myPlayer].hairColor != HairWindow.OldHairColor)
                {
                    // Increase price with hair re-color price
                    price += HairLoader.HairTable[HairWindow.SelectedHairID].OverridePrice
                        ? HairLoader.HairTable[HairWindow.SelectedHairID].ColorPrice
                        : 20000;
                }

                // Apply vanilla price adjustments
                price = (int)((float)price * priceAdjustment);

                // Calculate the amount of platinum coins
                if (price >= 1000000)
                {
                    platinum = price / 1000000;
                    price -= platinum * 1000000;
                }

                // Calculate the amount of gold coins
                if (price >= 10000)
                {
                    gold = price / 10000;
                    price -= gold * 10000;
                }

                // Calculate the amount of silver coins
                if (price >= 100)
                {
                    silver = price / 100;
                    price -= silver * 100;
                }

                // Amount left is the amount of copper coins
                copper = price;

                // Draw the 4 coins textures
                spriteBatch.Draw(TextureAssets.Item[ItemID.PlatinumCoin].Value, dimensions.Position() + new Vector2(80f - 60f, 0f), TextureAssets.Item[ItemID.PlatinumCoin].Value.Bounds, Color.White, 0f, TextureAssets.Item[ItemID.PlatinumCoin].Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(TextureAssets.Item[ItemID.GoldCoin].Value, dimensions.Position() + new Vector2(80f - 20f, 0f), TextureAssets.Item[ItemID.GoldCoin].Value.Bounds, Color.White, 0f, TextureAssets.Item[ItemID.PlatinumCoin].Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(TextureAssets.Item[ItemID.SilverCoin].Value, dimensions.Position() + new Vector2(80f + 20f, 2f), TextureAssets.Item[ItemID.SilverCoin].Value.Bounds, Color.White, 0f, TextureAssets.Item[ItemID.PlatinumCoin].Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(TextureAssets.Item[ItemID.CopperCoin].Value, dimensions.Position() + new Vector2(80f + 60f, 4f), TextureAssets.Item[ItemID.CopperCoin].Value.Bounds, Color.White, 0f, TextureAssets.Item[ItemID.PlatinumCoin].Size() * 0.5f, 1f, SpriteEffects.None, 0f);

                // Draw the price text underneath the coins
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, platinum.ToString(), dimensions.Position() + new Vector2(80f - 60f, 20f), Color.White, 0f, new Vector2(4f, 0f), 1f, SpriteEffects.None, 0.0f);
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, gold.ToString(), dimensions.Position() + new Vector2(80f - 20f, 20f), Color.White, 0f, new Vector2(4f, 0f), 1f, SpriteEffects.None, 0.0f);
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, silver.ToString(), dimensions.Position() + new Vector2(80f + 20f, 20f), Color.White, 0f, new Vector2(4f, 0f), 1f, SpriteEffects.None, 0.0f);
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, copper.ToString(), dimensions.Position() + new Vector2(80f + 60f, 20f), Color.White, 0f, new Vector2(4f, 0f), 1f, SpriteEffects.None, 0.0f);
            }
            else if (HairLoader.HairTable[HairWindow.SelectedHairID].OverridePrice 
                && HairLoader.HairTable[HairWindow.SelectedHairID].SpecialCurrencyID != -1 
                && CustomCurrencyManager.TryGetCurrencySystem(HairLoader.HairTable[HairWindow.SelectedHairID].SpecialCurrencyID, out CustomCurrencySystem system))
            {
                /*

                // Price is a custom currency

                // Load the texture of the currency item
                Main.instance.LoadItem(HairLoader.HairTable[HairWindow.selectMod][HairWindow.selectHair].currency);

                // Search for the chosen item's texture in the TextureAssets.Item array
                Texture2D texture = TextureAssets.Item[HairLoader.HairTable[HairWindow.selectMod][HairWindow.selectHair].currency].Value;

                // Get the texture's size
                Vector2 textureSize = new Vector2(texture.Width, texture.Height);

                // Get the size of the slot 
                Vector2 slotSize = new Vector2(24, 24);

                // Calculate the scale to apply to make the item's texture fit in the slot
                float scale = slotSize.X / textureSize.X > slotSize.Y / slotSize.Y ? slotSize.X / textureSize.X : slotSize.Y / slotSize.Y;

                int price = 0;

                // Check if the hairstyle has been changed
                if ((HairWindow.selectMod != HairWindow.OldModName || HairWindow.selectHair != HairWindow.OldHairName))
                {
                    // Increase price with hair buy price
                    price += HairLoader.HairTable[HairWindow.selectMod][HairWindow.selectHair].hairPrice;
                }

                // Check if the color has been changed
                if (Main.player[Main.myPlayer].hairColor != HairWindow.OldHairColor)
                {
                    // Increase price with hair re-color price
                    price += HairLoader.HairTable[HairWindow.selectMod][HairWindow.selectHair].colorPrice;
                }

                price = (int)((float)price * priceAdjustment);

                // Draw the item's texture with the calculated scale
                spriteBatch.Draw(texture, dimensions.Position() + new Vector2(dimensions.Width / 2 - 10, 0f), texture.Bounds, Color.White, 0f, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);

                // Draw the price text underneath the item
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, price.ToString(), dimensions.Position() + new Vector2(78f, 20f), Color.White, 0f, new Vector2(4f, 0f), 1f, SpriteEffects.None, 0.0f);
                */
            }
        }
    }
}