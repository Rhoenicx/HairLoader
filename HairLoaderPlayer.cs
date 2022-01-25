using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using HairLoader.UI;

namespace HairLoader
{
    public class HairLoaderPlayer : ModPlayer
    {
        public string Hair_modName = "";
        public string Hair_hairName = "";

        public override TagCompound Save()
        {
            return new TagCompound
            {
                { "modName", Hair_modName },
                { "hairName", Hair_hairName }
            };
        }

        public override void Load(TagCompound tag)
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

        public override void ModifyDrawInfo(ref PlayerDrawInfo drawInfo)
		{
			string modName = drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_modName;
            string hairName = drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_hairName;

            // This code will make sure the player's hair is valid, if not it will reset the player's hairstyle
            if (!HairLoader.HairTable.ContainsKey(modName))
            {
                if (HairLoader.Instance.getModAndHairNames(ref modName, ref hairName, drawInfo.drawPlayer.hair))
                {
                    if (!Main.gameMenu)
                    {
                        drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_modName = modName;
                        drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_hairName = hairName;
                    }
                }
			}

            if (Main.hairWindow)
            {
                Main.hairWindow = false;

                if (Main.player[Main.myPlayer].talkNPC > -1 && Main.npc[Main.player[Main.myPlayer].talkNPC].type == NPCID.Stylist)
                {
                    Main.player[Main.myPlayer].talkNPC = -1;
                }
            }

            Main.playerHairTexture[drawInfo.drawPlayer.hair] = HairLoader.HairTable[modName][hairName].hair;
			Main.playerHairAltTexture[drawInfo.drawPlayer.hair] = HairLoader.HairTable[modName][hairName].hairAlt;
		}
    }
}
