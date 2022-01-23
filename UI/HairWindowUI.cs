using ReLogic.Graphics;
using Terraria.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.ID;
using Terraria.GameContent.UI.Elements;
using static Terraria.ModLoader.ModContent;

namespace HairLoader.UI
{
    internal class HairWindow : UIState
    {
        // Visibility of panel
        public static bool Visible;

        // Highlights
        public static string highlightMod = "All";

        // New Selected Entry
        public static string selectMod;
        public static string selectHair;

        // Update price elements
        public static bool priceUpdate = false;

        // Save old player hair & color
        public static int OldHairStyleID;
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
        private UIHueBar ColorHueBar;
        private UISaturationBar ColorSaturationBar;
        private UILuminosityBar ColorLuminosityBar;

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



            // Hue Bars
            ColorHueBar = new UIHueBar();
            ColorHueBar.Top.Set(20f, 0f);
            ColorHueBar.Left.Set(630f, 0f);
            HairWindowPanel.Append(ColorHueBar);

            // Saturation Slider
            ColorSaturationBar = new UISaturationBar();
            ColorSaturationBar.Top.Set(56f, 0f);
            ColorSaturationBar.Left.Set(630f, 0f);
            HairWindowPanel.Append(ColorSaturationBar);

            // Luminosity Slider
            ColorLuminosityBar = new UILuminosityBar();
            ColorLuminosityBar.Top.Set(92f, 0f);
            ColorLuminosityBar.Left.Set(630f, 0f);
            HairWindowPanel.Append(ColorLuminosityBar);

            // Buy Text
            BuyText = new UIText("Buy", 1.25f, false);
            BuyText.Top.Set(250f, 0f);
            BuyText.Left.Set(630f, 0f);
            BuyText.OnMouseOver += (a, b) =>
            {
                if (Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().HairStyleID != OldHairStyleID || Main.player[Main.myPlayer].hairColor != OldHairColor)
                {
                    BuyText.TextColor = new Color(255, 199, 0);
                }
            };
            BuyText.OnMouseOut += (a, b) =>
            {
                BuyText.TextColor = new Color(235, 235, 235);
            };
            BuyText.OnClick += (a, b) =>
            {
                if (CanBuyHair())
                {
                    BuyHairWindow();
                }
            };
            HairWindowPanel.Append(BuyText);

            // Cancel Text
            CancelText = new UIText("Cancel", 1.25f, false);
            CancelText.Top.Set(250f, 0f);
            CancelText.Left.Set(750f, 0f);
            CancelText.OnMouseOver += (a, b) =>
            {
                CancelText.TextColor = new Color(255, 199, 0);
            };
            CancelText.OnMouseOut += (a, b) =>
            {
                CancelText.TextColor = new Color(235, 235, 235);
            };
            CancelText.OnClick += (a, b) =>
            {
                CloseHairWindow();
            };
            HairWindowPanel.Append(CancelText);

            // Price
            PriceElement = new UIPrice();
            PriceElement.Top.Set(164f, 0f);
            PriceElement.Left.Set(640f, 0f);
            PriceElement.Width.Set(178f, 0f);
            PriceElement.Height.Set(84, 0f);
            HairWindowPanel.Append(PriceElement);


            Append(HairWindowPanel);
        }

        public override void Update(GameTime gameTime)
        {
            if (Main.npcChatText != "" || Main.playerInventory || (Main.player[Main.myPlayer].chest != -1 || Main.npcShop != 0) || (Main.player[Main.myPlayer].talkNPC == -1 || Main.InGuideCraftMenu))
            {
                Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().HairStyleID = OldHairStyleID;
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
            OldHairStyleID = Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().HairStyleID;
            OldHairColor = Main.player[Main.myPlayer].hairColor;

            // Seach for the hair that the player is weaing and get the modname and hairname
            bool found = false;
            foreach (var mod in HairLoader.HairTable)
            {
                foreach (var hair in HairLoader.HairTable[mod.Key])
                {
                    if (HairLoader.HairTable[mod.Key][hair.Key].index == OldHairStyleID)
                    {
                        selectMod = mod.Key;
                        selectHair = hair.Key;
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    break;
                }
            }

            Vector3 hsl = Main.rgbToHsl(OldHairColor);
            Color_Hue = hsl.X;
            Color_Saturation = hsl.Y;
            Color_Luminosity = hsl.Z;

            Main.playerInventory = false;
            Main.npcChatText = "";

            UpdateModList();
            UpdateHairGrid(highlightMod);

            Visible = true;
            Main.PlaySound(SoundID.MenuOpen);
        }

        internal void CloseHairWindow()
        {
            if (!Visible)
            {
                return;
            }

            Visible = false;

            if (Main.player[Main.myPlayer].talkNPC > -1 && Main.npc[Main.player[Main.myPlayer].talkNPC].type == NPCID.Stylist)
            {
                Main.player[Main.myPlayer].talkNPC = -1;
            }

            Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().HairStyleID = OldHairStyleID;
            Main.player[Main.myPlayer].hairColor = OldHairColor;

            Main.PlaySound(SoundID.MenuClose);
        }

        internal bool CanBuyHair()
        {
            if (Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().HairStyleID != OldHairStyleID || Main.player[Main.myPlayer].hairColor != OldHairColor)
            {
                if (HairLoader.HairTable[selectMod][selectHair].currency == -1)
                {
                    int price = HairLoader.HairTable[selectMod][selectHair].price;

                    if (Main.player[Main.myPlayer].hairColor != OldHairColor)
                    {
                        price += 10000;
                    }

                    if (Main.player[Main.myPlayer].BuyItem(price))
                    {
                        return true;
                    }
                }
                else
                { 
                
                }
            }

            return false;
        }

        internal void BuyHairWindow()
        {
            int index = HairLoader.HairTable[selectMod][selectHair].index;

            Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().HairStyleID = index;
            Main.player[Main.myPlayer].hairColor = Main.hslToRgb(Color_Hue, Color_Saturation, Color_Luminosity);

            OldHairStyleID = index;
            OldHairColor = Main.hslToRgb(Color_Hue, Color_Saturation, Color_Luminosity);

            if (index < Main.maxHairTotal)
            {
                Main.player[Main.myPlayer].hair = index;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            { 
                //send msg            
            }

            Visible = false;

            if (Main.player[Main.myPlayer].talkNPC > -1 && Main.npc[Main.player[Main.myPlayer].talkNPC].type == NPCID.Stylist)
            {
                Main.player[Main.myPlayer].talkNPC = -1;
            }

            Main.PlaySound(SoundID.Coins);
        }

        internal void UpdateModList()
        {
            ModList.Clear();

            ModSlot modListEntryAll = new ModSlot("All");
            modListEntryAll.OnClick += (a, b) =>
            {
                highlightMod = "All";
                UpdateHairGrid(highlightMod);
                Main.PlaySound(SoundID.MenuTick);
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
                    }
                }

                if (visible)
                {
                    ModSlot modListEntry = new ModSlot(_modName.Key);

                    modListEntry.OnClick += (a, b) =>
                    {
                        highlightMod = _modName.Key;
                        UpdateHairGrid(_modName.Key);
                        Main.PlaySound(SoundID.MenuTick);
                    };

                    ModList.Add(modListEntry);
                }
            }
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
                            if (_hairName.Value.index < Main.maxHairTotal)
                            {
                                if (_hairName.Value.index > Main.UnlockedMaxHair())
                                {
                                    continue;
                                }
                            }

                            HairSlot slot = new HairSlot(_modName.Key, _hairName.Key, _hairName.Value.index);

                            slot.OnClick += (a, b) =>
                            {
                                Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().HairStyleID = _hairName.Value.index;
                                selectMod = _modName.Key;
                                selectHair = _hairName.Key;
                                HairWindow.priceUpdate = true;
                                Main.PlaySound(SoundID.MenuTick);
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

        private int index;
        private string modName;
        private string hairName;

        public static Texture2D backgroundTexture = Main.inventoryBack8Texture;
        public static Texture2D myBackgroundTexture = Main.inventoryBack14Texture;

        public HairSlot(string _modName, string _hairName, int _index)
        {
            this.modName = _modName;
            this.hairName = _hairName;
            this.index = _index;

            this.Width.Set(backgroundTexture.Width * scale, 0f);
            this.Height.Set(backgroundTexture.Height * scale, 0f);
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = base.GetInnerDimensions();
            Texture2D texture;

            if (Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().HairStyleID == index)
            {
                texture = myBackgroundTexture;
            }
            else
            {
                texture = backgroundTexture;
            }

            if (this.IsMouseHovering)
            {
                //Main.NewText(hairName);
            }

            // Slot back Texture
            spriteBatch.Draw(texture, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            // Player skin
            spriteBatch.Draw(Main.playerTextures[0, 0], dimensions.Center(), new Rectangle?(new Rectangle(0, 0, Main.playerTextures[0, 0].Width, 54)), Main.player[Main.myPlayer].skinColor, 0.0f, new Vector2(Main.playerTextures[0, 0].Width, dimensions.Height - 10) * 0.5f, 1f, SpriteEffects.None, 0f);
            
            // Player eye white
            spriteBatch.Draw(Main.playerTextures[0, 1], dimensions.Center(), new Rectangle?(new Rectangle(0, 0, Main.playerTextures[0, 1].Width, 54)), new Color(255, 255, 255, 255), 0.0f, new Vector2(Main.playerTextures[0, 1].Width, dimensions.Height - 10) * 0.5f, 1f, SpriteEffects.None, 0f);
            
            // Player eye pupil
            spriteBatch.Draw(Main.playerTextures[0, 2], dimensions.Center(), new Rectangle?(new Rectangle(0, 0, Main.playerTextures[0, 2].Width, 54)), Main.player[Main.myPlayer].eyeColor, 0.0f, new Vector2(Main.playerTextures[0, 2].Width, dimensions.Height - 10) * 0.5f, 1f, SpriteEffects.None, 0f);
            
            // Player Hair
            spriteBatch.Draw(HairLoader.HairStyles[index].hair, dimensions.Center(), new Rectangle?(new Rectangle(0, 0, HairLoader.HairStyles[index].hair.Width, 54)), Main.player[Main.myPlayer].hairColor, 0.0f, new Vector2(HairLoader.HairStyles[index].hair.Width, dimensions.Height - 10) * 0.5f, 1f, SpriteEffects.None, 0f);
        }
    }

    internal class UIHueBar : UIElement
    {
        Texture2D HueBarTex = GetTexture("Terraria/Hue");
        Texture2D SliderHighlightTex = GetTexture("Terraria/UI/Slider_Highlight");
        Texture2D ColorSliderTex = GetTexture("Terraria/ColorSlider");

        private bool dragging;

        public UIHueBar()
        {
            this.Width.Set(HueBarTex.Width, 0f);
            this.Height.Set(HueBarTex.Height, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = base.GetInnerDimensions();

            spriteBatch.Draw(HueBarTex, dimensions.Center(), HueBarTex.Bounds, Color.White, 0f, HueBarTex.Size() * 0.5f, 1f, SpriteEffects.None, 0f);

            if (this.IsMouseHovering)
            {
                spriteBatch.Draw(SliderHighlightTex, dimensions.Center(), SliderHighlightTex.Bounds, Main.OurFavoriteColor, 0f, SliderHighlightTex.Size() * 0.5f, 1f, SpriteEffects.None, 0f);

                //Main.NewText(dimensions.Position() - Main.MouseScreen);
            }

            float SliderPos = (dimensions.Width - 4) * HairWindow.Color_Hue;
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

                HairWindow.Color_Hue = pointX;
                Main.player[Main.myPlayer].hairColor = Main.hslToRgb(HairWindow.Color_Hue, HairWindow.Color_Saturation, HairWindow.Color_Luminosity);
            }
        }

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
            dragging = true;
        }

        private void DragEnd(UIMouseEvent evt)
        {
            Vector2 end = evt.MousePosition;
            dragging = false;
        }
    }

    internal class UISaturationBar : UIElement
    {
        Texture2D ColorBarTex = GetTexture("Terraria/ColorBar");
        Texture2D SliderHighlightTex = GetTexture("Terraria/UI/Slider_Highlight");
        Texture2D ColorBlipTex = GetTexture("Terraria/ColorBlip");
        Texture2D ColorSliderTex = GetTexture("Terraria/ColorSlider");

        private bool dragging;

        public UISaturationBar()
        {
            this.Width.Set(ColorBarTex.Width, 0f);
            this.Height.Set(ColorBarTex.Height, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = base.GetInnerDimensions();

            spriteBatch.Draw(ColorBarTex, dimensions.Center(), ColorBarTex.Bounds, Color.White, 0f, ColorBarTex.Size() * 0.5f, 1f, SpriteEffects.None, 0f);

            if (this.IsMouseHovering)
            {
                spriteBatch.Draw(SliderHighlightTex, dimensions.Center(), SliderHighlightTex.Bounds, Main.OurFavoriteColor, 0f, SliderHighlightTex.Size() * 0.5f, 1f, SpriteEffects.None, 0f);

                //Main.NewText(dimensions.Position() - Main.MouseScreen);
            }

            for (int i = 0; i < 168; i++)
            {
                float Saturation2 = (float)i / (float)168;
                Color rgb = Main.hslToRgb(HairWindow.Color_Hue, Saturation2, HairWindow.Color_Luminosity);
                spriteBatch.Draw(ColorBlipTex, new Vector2(dimensions.X + 5 + i, dimensions.Y + 4), rgb);
            }

            float SliderPos = (dimensions.Width - 4) * HairWindow.Color_Saturation;
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

                HairWindow.Color_Saturation = pointX;
                Main.player[Main.myPlayer].hairColor = Main.hslToRgb(HairWindow.Color_Hue, HairWindow.Color_Saturation, HairWindow.Color_Luminosity);
            }
        }

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
            dragging = true;
        }

        private void DragEnd(UIMouseEvent evt)
        {
            Vector2 end = evt.MousePosition;
            dragging = false;
        }
    }

    internal class UILuminosityBar : UIElement
    {
        Texture2D ColorBarTex = GetTexture("Terraria/ColorBar");
        Texture2D SliderHighlightTex = GetTexture("Terraria/UI/Slider_Highlight");
        Texture2D ColorBlipTex = GetTexture("Terraria/ColorBlip");
        Texture2D ColorSliderTex = GetTexture("Terraria/ColorSlider");

        private bool dragging;

        public UILuminosityBar()
        {
            this.Width.Set(ColorBarTex.Width, 0f);
            this.Height.Set(ColorBarTex.Height, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = base.GetInnerDimensions();

            spriteBatch.Draw(ColorBarTex, dimensions.Center(), ColorBarTex.Bounds, Color.White, 0f, ColorBarTex.Size() * 0.5f, 1f, SpriteEffects.None, 0f);

            if (this.IsMouseHovering)
            {
                spriteBatch.Draw(SliderHighlightTex, dimensions.Center(), SliderHighlightTex.Bounds, Main.OurFavoriteColor, 0f, SliderHighlightTex.Size() * 0.5f, 1f, SpriteEffects.None, 0f);

                //Main.NewText(dimensions.Position() - Main.MouseScreen);
            }

            for (int i = 0; i < 168; i++)
            {
                float Luminosity2 = (float)i / (float)168;
                Color rgb = Main.hslToRgb(HairWindow.Color_Hue, HairWindow.Color_Saturation, Luminosity2);
                spriteBatch.Draw(ColorBlipTex, new Vector2(dimensions.X + 5 + i, dimensions.Y + 4), rgb);
            }

            float SliderPos = (dimensions.Width - 4) * HairWindow.Color_Luminosity;
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

                HairWindow.Color_Luminosity = pointX;
                Main.player[Main.myPlayer].hairColor = Main.hslToRgb(HairWindow.Color_Hue, HairWindow.Color_Saturation, HairWindow.Color_Luminosity);
            }
        }

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
            dragging = true;
        }

        private void DragEnd(UIMouseEvent evt)
        {
            Vector2 end = evt.MousePosition;
            dragging = false;
        }
    }

    internal class UIPrice : UIElement
    {

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = base.GetInnerDimensions();

            if (HairLoader.HairTable[HairWindow.selectMod][HairWindow.selectHair].currency == -1)
            {
                int price = HairLoader.HairTable[HairWindow.selectMod][HairWindow.selectHair].price;
                int platinum = 0;
                int gold = 0;
                int silver = 0;
                int copper = 0;

                if (HairLoader.HairTable[HairWindow.selectMod][HairWindow.selectHair].index != HairWindow.OldHairStyleID)
                {
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
                }

                if (Main.player[Main.myPlayer].hairColor != HairWindow.OldHairColor)
                {
                    gold += 1;
                }

                spriteBatch.Draw(Main.itemTexture[ItemID.PlatinumCoin], dimensions.Position() + new Vector2(80f - 60f, 0f), Main.itemTexture[ItemID.PlatinumCoin].Bounds, Color.White, 0f, Main.itemTexture[ItemID.PlatinumCoin].Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(Main.itemTexture[ItemID.GoldCoin], dimensions.Position() + new Vector2(80f - 20f, 0f), Main.itemTexture[ItemID.GoldCoin].Bounds, Color.White, 0f, Main.itemTexture[ItemID.PlatinumCoin].Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(Main.itemTexture[ItemID.SilverCoin], dimensions.Position() + new Vector2(80f + 20f, 2f), Main.itemTexture[ItemID.SilverCoin].Bounds, Color.White, 0f, Main.itemTexture[ItemID.PlatinumCoin].Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(Main.itemTexture[ItemID.CopperCoin], dimensions.Position() + new Vector2(80f + 60f, 4f), Main.itemTexture[ItemID.CopperCoin].Bounds, Color.White, 0f, Main.itemTexture[ItemID.PlatinumCoin].Size() * 0.5f, 1f, SpriteEffects.None, 0f);

                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, Main.fontMouseText, platinum.ToString(), dimensions.Position() + new Vector2(80f - 60f, 20f), Color.White, 0f, new Vector2(4f, 0f), 1f, SpriteEffects.None, 0.0f);
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, Main.fontMouseText, gold.ToString(), dimensions.Position() + new Vector2(80f - 20f, 20f), Color.White, 0f, new Vector2(4f, 0f), 1f, SpriteEffects.None, 0.0f);
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, Main.fontMouseText, silver.ToString(), dimensions.Position() + new Vector2(80f + 20f, 20f), Color.White, 0f, new Vector2(4f, 0f), 1f, SpriteEffects.None, 0.0f);
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, Main.fontMouseText, copper.ToString(), dimensions.Position() + new Vector2(80f + 60f, 20f), Color.White, 0f, new Vector2(4f, 0f), 1f, SpriteEffects.None, 0.0f);
            }
            else
            {
                

            }
        }
    }
}   