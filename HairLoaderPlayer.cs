using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.GameContent;
using HairLoader.UI;
using Terraria.DataStructures;

namespace HairLoader
{
    public class HairLoaderPlayer : ModPlayer
    {
        public string Hair_modClassName = "";
        public string Hair_hairEntryName = "";

        public override void Load()
        {
            Hair_modClassName = "";
            Hair_hairEntryName = "";
        }

        public override void Unload()
        {
            Hair_modClassName = null;
            Hair_hairEntryName = null;
        }

        public override void SaveData(TagCompound tag)
        {
            tag.Add("modClassName", Hair_modClassName);
            tag.Add("hairEntryName", Hair_hairEntryName);
        }

        public override void LoadData(TagCompound tag)
        {
            Hair_modClassName = tag.GetString("modClassName");
            Hair_hairEntryName = tag.GetString("hairEntryName");
        }

        public override void OnEnterWorld(Player player)
        {
            HairWindow.Visible = false;

            if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer)
            {
                HairLoader.Instance.ChangePlayerHairStyle(player.GetModPlayer<HairLoaderPlayer>().Hair_modClassName, player.GetModPlayer<HairLoaderPlayer>().Hair_hairEntryName, player.whoAmI);
            }
        }

        public override void PlayerConnect(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer)
            {
                HairLoader.Instance.ChangePlayerHairStyle(player.GetModPlayer<HairLoaderPlayer>().Hair_modClassName, player.GetModPlayer<HairLoaderPlayer>().Hair_hairEntryName, player.whoAmI);
            }
        }
    }
}
