using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace HairLoader
{
    public class HairLoaderPlayer : ModPlayer
    {
        public int HairStyleID = -1;

        public override void ModifyDrawInfo(ref PlayerDrawInfo drawInfo)
		{
			// ALWAYS check if we have the vanilla hairstyle loaded
			if (!HairLoader.HairStyles.ContainsKey(drawInfo.drawPlayer.hair))
			{
				HairLoader.HairStyles.Add
				(
					drawInfo.drawPlayer.hair,
					new PlayerHair
					{
						Hair = Main.playerHairTexture[drawInfo.drawPlayer.hair],
						HairAlt = Main.playerHairAltTexture[drawInfo.drawPlayer.hair]
					}
				);
			}

			// Now we check if the custom hairstyle has been loaded
			int HairStyleID = HairLoader.PlayerDisplayHairStyle[drawInfo.drawPlayer.whoAmI];

			if (!HairLoader.HairStyles.ContainsKey(HairStyleID))
			{
				// if not loaded reset the player's hairstyle to the default one
				HairLoader.PlayerDisplayHairStyle[drawInfo.drawPlayer.whoAmI] = drawInfo.drawPlayer.hair;
				HairStyleID = drawInfo.drawPlayer.hair;
			}

			Main.playerHairTexture[drawInfo.drawPlayer.hair] = HairLoader.HairStyles[HairStyleID].Hair;
			Main.playerHairAltTexture[drawInfo.drawPlayer.hair] = HairLoader.HairStyles[HairStyleID].Hair;
		}
    }
}
