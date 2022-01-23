using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.ID;
using Terraria.GameContent.UI.Elements;

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

        // Save old player hair & color
        public static int OldHairStyleID;

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

        public override void OnInitialize()
        {
            // Background of the UI
            HairWindowPanel = new UIPanel();
            HairWindowPanel.SetPadding(10);
            HairWindowPanel.Left.Set(-400f, 0.5f);
            HairWindowPanel.Top.Set(0f, 0.6f);
            HairWindowPanel.Width.Set(800f, 0f);
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
            if (HairWindowPanel.ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }

        internal void OpenHairWindow()
        {
            Main.hBar = -1f;
            Main.lBar = -1f;
            Main.sBar = -1f;
            Main.playerInventory = false;
            Main.npcChatText = "";

            UpdateModList();
            UpdateHairGrid(highlightMod);

            OldHairStyleID = Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().HairStyleID;

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

            Main.PlaySound(SoundID.MenuClose);
        }

        internal void BuyHairWindow(int index)
        {
            Main.player[Main.myPlayer].GetModPlayer<HairLoaderPlayer>().HairStyleID = index;

            if (index < Main.maxHairTotal)
            {
                Main.player[Main.myPlayer].hair = index;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            { 
                //send msg            
            }
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
            spriteBatch.Draw(Main.playerTextures[0, 0], dimensions.Center(), new Rectangle?(new Rectangle(0, 0, Main.playerTextures[0, 0].Width, 54)), Main.player[Main.myPlayer].skinColor, 0.0f, new Vector2(Main.playerTextures[0, 0].Width, dimensions.Height - 10) * 0.5f, 1f, SpriteEffects.None, 0.0f);
            
            // Player eye white
            spriteBatch.Draw(Main.playerTextures[0, 1], dimensions.Center(), new Rectangle?(new Rectangle(0, 0, Main.playerTextures[0, 1].Width, 54)), new Color(255, 255, 255, 255), 0.0f, new Vector2(Main.playerTextures[0, 1].Width, dimensions.Height - 10) * 0.5f, 1f, SpriteEffects.None, 0.0f);
            
            // Player eye pupil
            spriteBatch.Draw(Main.playerTextures[0, 2], dimensions.Center(), new Rectangle?(new Rectangle(0, 0, Main.playerTextures[0, 2].Width, 54)), Main.player[Main.myPlayer].eyeColor, 0.0f, new Vector2(Main.playerTextures[0, 2].Width, dimensions.Height - 10) * 0.5f, 1f, SpriteEffects.None, 0.0f);
            
            // Player Hair
            spriteBatch.Draw(HairLoader.HairStyles[index].hair, dimensions.Center(), new Rectangle?(new Rectangle(0, 0, HairLoader.HairStyles[index].hair.Width, 54)), Main.player[Main.myPlayer].hairColor, 0.0f, new Vector2(HairLoader.HairStyles[index].hair.Width, dimensions.Height - 10) * 0.5f, 1f, SpriteEffects.None, 0.0f);
        }
    }
}   