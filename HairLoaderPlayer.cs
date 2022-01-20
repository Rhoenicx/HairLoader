using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace HairLoader
{
	public class HairLoaderPlayer : modPlayer
	{
		public int HairStyleID = -1;
		
		public override void ModifyDrawInfo(PlayerDrawInfo drawInfo)
		{	
			// ALWAYS check if we have the vanilla hairstyle loaded
			if (!HairLoader.HairStyles.ContainsKey(drawInfo.drawPlayer.hair))
			{
				HairLoader.HairStyles.Add
					(
						drawInfo.drawPlayer.hair,
						new HairLoader.PlayerHair
							{ 
								Hair = Main.playerHairTexture[drawInfo.drawPlayer.hair],
								HairAlt = Main.playerHairAltTexture[drawInfo.drawPlayer.hair],
								Name = "Vanilla hairstyle" + drawInfo.drawPlayer.hair.ToString()
							}
					);
			}
		
			// Now we check if the custom hairstyle has been loaded
			int HairStyleID = drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().HairStyleID;
			
			if (!HairLoader.HairStyles.ContainsKey(HairStyleID))
			{
				// if not loaded reset the player's hairstyle
				drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().HairStyleID = drawInfo.drawPlayer.hair;
				HairStyleID = drawInfo.drawPlayer.hair;	
			}
			
			Main.playerHairTexture[drawInfo.drawPlayer.hair] = HairLoader.HairStyles[HairStyleID].Hair;
			Main.playerHairAltTexture[drawInfo.drawPlayer.hair] = HairLoader.HairStyles[HairStyleID].Hair;
		}
	}
}
