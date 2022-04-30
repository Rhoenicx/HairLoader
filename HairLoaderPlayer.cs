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
        public string Hair_modName = "";
        public string Hair_hairName = "";

        public override void Load()
        {
            Hair_modName = "";
            Hair_hairName = "";
        }

        public override void Unload()
        {
            Hair_modName = null;
            Hair_hairName = null;
        }

        public override void SaveData(TagCompound tag)
        {
            tag.Add("modName", Hair_modName);
            tag.Add("hairName", Hair_hairName);
        }

        public override void LoadData(TagCompound tag)
        {
            Hair_modName = tag.GetString("modName");
            Hair_hairName = tag.GetString("hairName");
        }

        public override void OnEnterWorld(Player player)
        {
            HairWindow.Visible = false;

            if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer)
            {
                HairLoader.Instance.ChangePlayerHairStyle(player.GetModPlayer<HairLoaderPlayer>().Hair_modName, player.GetModPlayer<HairLoaderPlayer>().Hair_hairName, player.whoAmI);
            }
        }

        public override void PlayerConnect(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer)
            {
                HairLoader.Instance.ChangePlayerHairStyle(player.GetModPlayer<HairLoaderPlayer>().Hair_modName, player.GetModPlayer<HairLoaderPlayer>().Hair_hairName, player.whoAmI);
            }
        }
    }
}
