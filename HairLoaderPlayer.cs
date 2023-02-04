using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
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

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            string modClassName = drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_modClassName;
            string hairEntryName = drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_hairEntryName;

            bool valid = true;
            if (!HairLoader.HairTable.ContainsKey(modClassName))
            {
                valid = false;
            }
            else if (!HairLoader.HairTable[modClassName].ContainsKey(hairEntryName))
            {
                valid = false;
            }

            if (!valid)
            {
                if (HairLoader.GetModAndHairNames(ref modClassName, ref hairEntryName, drawInfo.drawPlayer.hair))
                {
                    drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_modClassName = modClassName;
                    drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_hairEntryName = hairEntryName;
                }
                else
                {
                    return;
                }
            }

            drawInfo.hairOffset = new Vector2(drawInfo.drawPlayer.direction == 1 ? HairLoader.HairTable[modClassName][hairEntryName].hairOffset : -HairLoader.HairTable[modClassName][hairEntryName].hairOffset, 0f);
        }
    }
}
