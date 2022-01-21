using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace HairLoader
{
    public class HairLoaderPlayer : ModPlayer
    {
        public int HairStyleID = -1;
        public int oldHairStyleID = -1;

        public bool hairWindow = false;

        public override TagCompound Save()
        {
            return new TagCompound
            {
                { "HairStyleID", HairStyleID }
            };
        }

        public override void Load(TagCompound tag)
        {
            HairStyleID = tag.GetInt("HairStyleID");
        }

        public override void OnEnterWorld(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                
            }
        }

        public override void PlayerConnect(Player player)
        {
            //Send the hairstyleID of this player to other players
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                
            }
        }

        public override void ModifyDrawInfo(ref PlayerDrawInfo drawInfo)
		{
			int HairStyleID = drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().HairStyleID;

            // This code will make sure the player's hair is valid, if not it will reset the player's hairstyle and load possible missing textures
			if (!HairLoader.HairStyles.ContainsKey(HairStyleID))
			{
                if (!Main.gameMenu)
                {
                    drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().HairStyleID = drawInfo.drawPlayer.hair;
                }

				HairStyleID = drawInfo.drawPlayer.hair;
			}

            if (Main.hairWindow)
            {
                Main.hairWindow = false;

                if (Main.player[Main.myPlayer].talkNPC > -1 && Main.npc[Main.player[Main.myPlayer].talkNPC].type == NPCID.Stylist)
                {
                    Main.player[Main.myPlayer].talkNPC = -1;
                }
            }

            Main.playerHairTexture[drawInfo.drawPlayer.hair] = HairLoader.HairStyles[HairStyleID].hair;
			Main.playerHairAltTexture[drawInfo.drawPlayer.hair] = HairLoader.HairStyles[HairStyleID].hairAlt;
		}
    }
}
