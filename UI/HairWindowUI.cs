using ReLogic.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.ID;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using static Terraria.ModLoader.ModContent;

namespace HairLoader.UI
{
    public class HairWindow : UIState
    {
        // Visibility of panel
        public static bool Visible;

        // Highlights
        public static string highlightMod = "All";

        // New Selected Entry
        public static string selectMod;
        public static string selectHair;

        // Save old player hair & color
        public static string OldModName;
        public static string OldHairName;
        public static Color OldHairColor;

        // Saved Colors
        public static float Color_Hue;
        public static float Color_Saturation;
        public static float Color_Luminosity;

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
        private UIColorBar ColorHueBar;
        private UIColorBar ColorSaturationBar;
        private UIColorBar ColorLuminosityBar;

        // Text
        private UIText BuyText;
        private UIText CancelText;

        // Price
        private UIPrice PriceElement;

        public override void OnInitialize()
        {
            // Background of the UI
            HairWindowPanel = new UIPanel();
            HairWindowPanel.SetPadding(10);
            HairWindowPanel.Left.Set(-426f, 0.5f);
            HairWindowPanel.Top.Set(0f, 0.6f);
            HairWindowPanel.Width.Set(852f, 0f);
            HairWindowPanel.Height.Set(300f, 0f);
            //HairWindowBackground.BorderColor = new Color(63, 84, 161);
            HairWindowPanel.BackgroundColor = new Color(73, 94, 171);

            // MODS
            ModListText = new UIText("Categories:");
            ModListText.Top.Set(-4f, 0f);
            ModListText.Left.Pixels = 0f;
            HairWindowPanel.Append(ModListText);
           
            ModListPanel = new UIPanel();
            ModListPanel.Top.Set(20f, 0f);
            ModListPanel.Left.Set(0f, 0f);
            ModListPanel.Width.Set(200f, 0f);
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
            HairListText = new UIText("Hair Style:");
            HairListText.Top.Set(-4f, 0f);
            HairListText.Left.Pixels = 208f;
            HairWindowPanel.Append(HairListText);
            
            HairListPanel = new UIPanel();
            HairListPanel.Top.Set(20f, 0f);
            HairListPanel.Left.Set(208f, 0f);
            HairListPanel.Width.Set(390f, 0f);
            HairListPanel.Height.Set(-20f, 1f);
            HairListPanel.SetPadding(10);
            HairListPanel.BackgroundColor = new Color(53, 74, 151);
            
            HairGrid = new UIGrid(6);
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

            // Hue Slider
            ColorHueBar = new UIColorBar(0);
            ColorHueBar.Top.Set(20f, 0f);
            ColorHueBar.Left.Set(630f, 0f);
            HairWindowPanel.Append(ColorHueBar);

            // Saturation Slider
            ColorSaturationBar = new UIColorBar(1);
            ColorSaturationBar.Top.Set(56f, 0f);
            ColorSaturationBar.Left.Set(630f, 0f);
            HairWindowPanel.Append(ColorSaturationBar);

            // Luminosity Slider
            ColorLuminosityBar = new UIColorBar(2);
            ColorLuminosityBar.Top.Set(92f, 0f);
            ColorLuminosityBar.Left.Set(630f, 0f);
            HairWindowPanel.Append(ColorLuminosityBar);

            // Buy Text
            BuyText = new UIText("Buy", 1.25f, false);
            BuyText.Top.Set(250f, 0f);
            BuyText.Left.Set(630f, 0f);
            BuyText.OnMouseOver += (a, b) => 
            {
                if ((Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_modName != OldModName || Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_hairName != OldHairName) || Main.player[Main.myPlayer].hairColor != OldHairColor)
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
            BuyText.OnClick += (a, b) => { if (CanBuyHair()) BuyHairWindow(); };
            HairWindowPanel.Append(BuyText);

            // Cancel Text
            CancelText = new UIText("Cancel", 1.25f, false);
            CancelText.Top.Set(250f, 0f);
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
            CancelText.OnClick += (a, b) => { CloseHairWindow(); };
            HairWindowPanel.Append(CancelText);

            // Price
            PriceElement = new UIPrice();
            PriceElement.Top.Set(164f, 0f);
            PriceElement.Left.Set(640f, 0f);
            PriceElement.Width.Set(178f, 0f);
            PriceElement.Height.Set(84f, 0f);
            HairWindowPanel.Append(PriceElement);

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
        }

        internal void OpenHairWindow()
        {
            OldModName = Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_modName;
            OldHairName = Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_hairName;
            OldHairColor = Main.player[Main.myPlayer].hairColor;

            selectMod = OldModName;
            selectHair = OldHairName;

            Vector3 hsl = Main.rgbToHsl(OldHairColor);
            Color_Hue = hsl.X;
            Color_Saturation = hsl.Y;
            Color_Luminosity = hsl.Z;

            Main.playerInventory = false;
            Main.npcChatText = "";

            UpdateUnlocks();
            UpdateModList();
            UpdateHairGrid(highlightMod);

            Visible = true;

            SoundEngine.PlaySound(SoundID.MenuOpen);
        }

        internal void CloseHairWindow()
        {
            if (!Visible)
            {
                return;
            }

            Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_modName = OldModName;
            Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_hairName = OldHairName;
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
            Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_modName = selectMod;
            Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_hairName = selectHair;
            Main.player[Main.myPlayer].hairColor = Main.hslToRgb(Color_Hue, Color_Saturation, Color_Luminosity);

            HairLoader.Instance.ChangePlayerHairStyle(selectMod, selectHair, Main.myPlayer);

            Visible = false;

            if (Main.player[Main.myPlayer].talkNPC > -1 && Main.npc[Main.player[Main.myPlayer].talkNPC].type == NPCID.Stylist)
            {
                Main.player[Main.myPlayer].SetTalkNPC(-1);
            }

            SoundEngine.PlaySound(SoundID.Coins);
        }

        internal bool CanBuyHair()
        {
            if ((Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_modName != OldModName || Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_hairName != OldHairName) || Main.player[Main.myPlayer].hairColor != OldHairColor)
            {
                if (HairLoader.HairTable[selectMod][selectHair].currency == -1)
                {
                    int price = 0;

                    if ((Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_modName != OldModName && Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_hairName != OldHairName))
                    {
                        price += HairLoader.HairTable[selectMod][selectHair].hairPrice;
                    }

                    if (Main.player[Main.myPlayer].hairColor != OldHairColor)
                    {
                        price += HairLoader.HairTable[selectMod][selectHair].colorPrice;
                    }

                    if (Main.player[Main.myPlayer].BuyItem(price))
                    {
                        return true;
                    }
                }
                else
                {
                    int itemCount = 0;
                    int price = 0;

                    if ((Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_modName != OldModName && Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_hairName != OldHairName))
                    {
                        price += HairLoader.HairTable[selectMod][selectHair].hairPrice;
                    }

                    if (Main.player[Main.myPlayer].hairColor != OldHairColor)
                    {
                        price += HairLoader.HairTable[selectMod][selectHair].colorPrice;
                    }


                    for (int i = 0; i < Main.player[Main.myPlayer].inventory.Length; i++)
                    {
                        if (Main.player[Main.myPlayer].inventory[i].type == HairLoader.HairTable[selectMod][selectHair].currency)
                        {
                            itemCount += Main.player[Main.myPlayer].inventory[i].stack;
                        }
                    }

                    if (itemCount >= price)
                    {
                        for (int i = 0; i < Main.player[Main.myPlayer].inventory.Length; i++)
                        {
                            if (Main.player[Main.myPlayer].inventory[i].type == HairLoader.HairTable[selectMod][selectHair].currency)
                            {
                                int amount = Main.player[Main.myPlayer].inventory[i].stack;
                                if (amount > price)
                                {
                                    Main.player[Main.myPlayer].inventory[i].stack -= price;
                                    price = 0;
                                }
                                else
                                {
                                    price -= Main.player[Main.myPlayer].inventory[i].stack;
                                    Main.player[Main.myPlayer].inventory[i].TurnToAir();
                                }
                            }

                            if (price <= 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        internal void UpdateModList()
        {
            ModList.Clear();

            ModSlot modListEntryAll = new ModSlot("All");
            modListEntryAll.OnClick += (a, b) =>
            {
                highlightMod = "All";
                UpdateHairGrid(highlightMod);
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
            ModList.Add(modListEntryAll);

            foreach (var _modName in HairLoader.HairTable)
            {
                bool visible = false;

                foreach (var _hairName in HairLoader.HairTable[_modName.Key])
                {
                    if (HairLoader.HairTable[_modName.Key][_hairName.Key].visibility)
                    {
                        visible = true;
                        break;
                    }
                }

                if (visible)
                {
                    ModSlot modListEntry = new ModSlot(_modName.Key);

                    modListEntry.OnClick += (a, b) =>
                    {
                        highlightMod = _modName.Key;
                        UpdateHairGrid(_modName.Key);
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    };

                    ModList.Add(modListEntry);
                }
            }
        }

        private void UpdateUnlocks()
        {
            Main.hairWindow = true;
            Main.Hairstyles.UpdateUnlocks();
            Main.hairWindow = false;
        }

        internal void UpdateHairGrid(string modName)
        {
            HairGrid.Clear();

            foreach (var _modName in HairLoader.HairTable)
            {
                foreach (var _hairName in HairLoader.HairTable[_modName.Key])
                {
                    if (HairLoader.HairTable[_modName.Key][_hairName.Key].visibility)
                    {
                        if (modName == "All" || _modName.Key == modName)
                        {
                            if (_modName.Key == "Vanilla")
                            {
                                UpdateUnlocks();

                                if (HairLoader.HairTable[_modName.Key][_hairName.Key].index >= Main.Hairstyles.AvailableHairstyles.Count)
                                {
                                    continue;
                                }
                            }

                            HairSlot slot = new HairSlot(_modName.Key, _hairName.Key);

                            slot.OnClick += (a, b) =>
                            {
                                Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_modName = _modName.Key;
                                Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_hairName = _hairName.Key;
                                selectMod = _modName.Key;
                                selectHair = _hairName.Key;
                                SoundEngine.PlaySound(SoundID.MenuTick);
                            };

                            HairGrid._items.Add(slot);
                            HairGrid._innerList.Append(slot);
                        }
                    }
                }
            }

            HairGrid.UpdateOrder();
            HairGrid._innerList.Recalculate();
        }
    }

    internal class ModSlot : UITextPanel<string>
    {
        // Elements
        private UIPanel ModEntryPanel;

        public ModSlot(string _modName) : base (_modName)
        {

        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (Text == HairWindow.highlightMod)
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

    internal class HairSlot : UIElement
    {
        private float scale = 1f;

        private string modName;
        private string hairName;

        public static Texture2D backgroundTexture = TextureAssets.InventoryBack8.Value;
        public static Texture2D myBackgroundTexture = TextureAssets.InventoryBack14.Value;

        public HairSlot(string _modName, string _hairName)
        {
            this.modName = _modName;
            this.hairName = _hairName;

            this.Width.Set(backgroundTexture.Width * scale, 0f);
            this.Height.Set(backgroundTexture.Height * scale, 0f);
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = base.GetInnerDimensions();
            Texture2D texture = (Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_modName == modName && Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().Hair_hairName == hairName) ? myBackgroundTexture : backgroundTexture;

            const int offsetX = 38;

            spriteBatch.Draw(texture, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Players[0, 0].Value, dimensions.Center(), new Rectangle?(new Rectangle(0, 0, TextureAssets.PlayerHair[Main.player[Main.myPlayer].hair].Width(), 56)), Main.player[Main.myPlayer].skinColor, 0.0f, new Vector2(offsetX, dimensions.Height - 10) * 0.5f, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Players[0, 1].Value, dimensions.Center(), new Rectangle?(new Rectangle(0, 0, TextureAssets.PlayerHair[Main.player[Main.myPlayer].hair].Width(), 56)), new Color(255, 255, 255, 255), 0.0f, new Vector2(offsetX, dimensions.Height - 10) * 0.5f, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Players[0, 2].Value, dimensions.Center(), new Rectangle?(new Rectangle(0, 0, TextureAssets.PlayerHair[Main.player[Main.myPlayer].hair].Width(), 56)), Main.player[Main.myPlayer].eyeColor, 0.0f, new Vector2(offsetX, dimensions.Height - 10) * 0.5f, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(HairLoader.HairTable[modName][hairName].hair.Value, dimensions.Center() + new Vector2(HairLoader.HairTable[modName][hairName].hairOffset, 0f), new Rectangle?(new Rectangle(0, 0, HairLoader.HairTable[modName][hairName].hair.Width(), 56)), Main.player[Main.myPlayer].hairColor, 0.0f, new Vector2(HairLoader.HairTable[modName][hairName].hair.Width(), dimensions.Height - 10) * 0.5f, 1f, SpriteEffects.None, 0f);

            if (IsMouseHovering)
            {
                Main.hoverItemName = hairName;
            }
        }
    }

    internal class UIColorBar : UIElement
    {
        Rectangle rect = new Rectangle(0, 0, 178, 16);
        Texture2D HueBarTex = TextureAssets.Hue.Value;
        Texture2D ColorBarTex = TextureAssets.ColorBar.Value;
        Texture2D SliderHighlightTex = TextureAssets.ColorHighlight.Value;
        Texture2D ColorBlipTex = TextureAssets.ColorBlip.Value;
        Texture2D ColorSliderTex = TextureAssets.ColorSlider.Value;

        public bool dragging;
        public int type;

        public UIColorBar(int _type)
        {
            this.Width.Set(rect.Width, 0f);
            this.Height.Set(rect.Height, 0f);
            type = _type;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = base.GetInnerDimensions();

            if (type > 0)
            {
                spriteBatch.Draw(ColorBarTex, dimensions.Center(), rect, Color.White, 0f, ColorBarTex.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }
            else 
            {
                spriteBatch.Draw(HueBarTex, dimensions.Center(), HueBarTex.Bounds, Color.White, 0f, HueBarTex.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }

            if (this.IsMouseHovering)
            {
                spriteBatch.Draw(SliderHighlightTex, dimensions.Center(), SliderHighlightTex.Bounds, Main.OurFavoriteColor, 0f, SliderHighlightTex.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }

            if (type > 0)
            {
                for (int i = 0; i < 168; i++)
                {
                    float pointX = (float)i / (float)168;
                    Color rgb = type == 1 ? Main.hslToRgb(HairWindow.Color_Hue, pointX, HairWindow.Color_Luminosity) : Main.hslToRgb(HairWindow.Color_Hue, HairWindow.Color_Saturation, pointX);
                    spriteBatch.Draw(ColorBlipTex, new Vector2(dimensions.X + 5 + i, dimensions.Y + 4), rgb);
                }
            }

            float SliderPos = 0f;

            switch (type)
            {
                case 0: SliderPos = (dimensions.Width - 4) * HairWindow.Color_Hue; break;
                case 1: SliderPos = (dimensions.Width - 4) * HairWindow.Color_Saturation; break;
                case 2: SliderPos = (dimensions.Width - 4) * HairWindow.Color_Luminosity; break;
            }

            spriteBatch.Draw(ColorSliderTex, new Vector2(dimensions.X + SliderPos - (ColorSliderTex.Width / 2), dimensions.Y - 4), ColorSliderTex.Bounds, Color.White);
        }

        public override void Update(GameTime gameTime)
        {
            CalculatedStyle dimensions = base.GetInnerDimensions();

            if (dragging)
            {
                float pointX = (Main.MouseScreen.X - dimensions.Position().X) / dimensions.Width;

                if (Main.MouseScreen.X < dimensions.Position().X)
                {
                    pointX = 0f;
                }

                if (Main.MouseScreen.X > dimensions.Position().X + dimensions.Width)
                {
                    pointX = 1f;
                }

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

        public override void MouseDown(UIMouseEvent evt)
        {
            base.MouseDown(evt);
            dragging = true;
        }

        public override void MouseUp(UIMouseEvent evt)
        {
            base.MouseUp(evt);
            dragging = false;
        }
    }

    internal class UIPrice : UIElement
    {
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = base.GetInnerDimensions();

            if ((HairWindow.selectMod == HairWindow.OldModName && HairWindow.selectHair == HairWindow.OldHairName) && Main.player[Main.myPlayer].hairColor == HairWindow.OldHairColor)
            {
                return;
            }

            if (HairLoader.HairTable[HairWindow.selectMod][HairWindow.selectHair].currency == -1)
            {
                int price = 0;
                int platinum = 0;
                int gold = 0;
                int silver = 0;
                int copper = 0;

                if ((HairWindow.selectMod != HairWindow.OldModName || HairWindow.selectHair != HairWindow.OldHairName))
                {
                    price += HairLoader.HairTable[HairWindow.selectMod][HairWindow.selectHair].hairPrice;
                }

                if (Main.player[Main.myPlayer].hairColor != HairWindow.OldHairColor)
                {
                    price += HairLoader.HairTable[HairWindow.selectMod][HairWindow.selectHair].colorPrice;
                }

                if (price >= 1000000)
                {
                    platinum = price / 1000000;
                    price -= platinum * 1000000;
                }

                if (price >= 10000)
                {
                    gold = price / 10000;
                    price -= gold * 10000;
                }

                if (price >= 1000000)
                {
                    silver = price / 100;
                    price -= silver * 100;
                }

                copper = price;

                spriteBatch.Draw(TextureAssets.Item[ItemID.PlatinumCoin].Value, dimensions.Position() + new Vector2(80f - 60f, 0f), TextureAssets.Item[ItemID.PlatinumCoin].Value.Bounds, Color.White, 0f, TextureAssets.Item[ItemID.PlatinumCoin].Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(TextureAssets.Item[ItemID.GoldCoin].Value, dimensions.Position() + new Vector2(80f - 20f, 0f), TextureAssets.Item[ItemID.GoldCoin].Value.Bounds, Color.White, 0f, TextureAssets.Item[ItemID.PlatinumCoin].Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(TextureAssets.Item[ItemID.SilverCoin].Value, dimensions.Position() + new Vector2(80f + 20f, 2f), TextureAssets.Item[ItemID.SilverCoin].Value.Bounds, Color.White, 0f, TextureAssets.Item[ItemID.PlatinumCoin].Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(TextureAssets.Item[ItemID.CopperCoin].Value, dimensions.Position() + new Vector2(80f + 60f, 4f), TextureAssets.Item[ItemID.CopperCoin].Value.Bounds, Color.White, 0f, TextureAssets.Item[ItemID.PlatinumCoin].Size() * 0.5f, 1f, SpriteEffects.None, 0f);

                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, platinum.ToString(), dimensions.Position() + new Vector2(80f - 60f, 20f), Color.White, 0f, new Vector2(4f, 0f), 1f, SpriteEffects.None, 0.0f);
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, gold.ToString(), dimensions.Position() + new Vector2(80f - 20f, 20f), Color.White, 0f, new Vector2(4f, 0f), 1f, SpriteEffects.None, 0.0f);
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, silver.ToString(), dimensions.Position() + new Vector2(80f + 20f, 20f), Color.White, 0f, new Vector2(4f, 0f), 1f, SpriteEffects.None, 0.0f);
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, copper.ToString(), dimensions.Position() + new Vector2(80f + 60f, 20f), Color.White, 0f, new Vector2(4f, 0f), 1f, SpriteEffects.None, 0.0f);
            }
            else
            {
                Texture2D texture = TextureAssets.Item[HairLoader.HairTable[HairWindow.selectMod][HairWindow.selectHair].currency].Value;

                Vector2 textureSize = new Vector2(texture.Width, texture.Height);
                Vector2 slotSize = new Vector2(24, 24);
                float scale = slotSize.X / textureSize.X > slotSize.Y / slotSize.Y ? slotSize.X / textureSize.X : slotSize.Y / slotSize.Y;
                int price = 0;

                if ((HairWindow.selectMod != HairWindow.OldModName || HairWindow.selectHair != HairWindow.OldHairName))
                {
                    price += HairLoader.HairTable[HairWindow.selectMod][HairWindow.selectHair].hairPrice;
                }

                if (Main.player[Main.myPlayer].hairColor != HairWindow.OldHairColor)
                {
                    price += HairLoader.HairTable[HairWindow.selectMod][HairWindow.selectHair].colorPrice;
                }

                spriteBatch.Draw(texture, dimensions.Position() + new Vector2(dimensions.Width/2 - 10, 0f), texture.Bounds, Color.White, 0f, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, price.ToString(), dimensions.Position() + new Vector2(78f, 20f), Color.White, 0f, new Vector2(4f, 0f), 1f, SpriteEffects.None, 0.0f);
            }
        }
    }
}   