using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Graphics.Effects;
using Terraria.UI;
using Terraria.ObjectData;

namespace HairLoader
{
    public class HairLoaderPlayer : ModPlayer
    {
        public int HairStyleID = Main.maxHairTotal;
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

			    if (!HairLoader.HairTable.ContainsKey("Vanilla"))
			    {
			    	HairLoader.HairTable.Add("Vanilla", new Dictionary<string, int>());
			    }

			    if (!HairLoader.HairTable["Vanilla"].ContainsKey(drawInfo.drawPlayer.hair.ToString()))
			    {
			    	HairLoader.HairTable["Vanilla"].Add(drawInfo.drawPlayer.hair.ToString(), drawInfo.drawPlayer.hair);
			    }
			}

			// Now we check if the custom hairstyle has been loaded
			int HairStyleID = drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().HairStyleID;

            // This code will make sure the player's hair is valid, if not it will reset the player's hairstyle and load possible missing textures
			if (!HairLoader.HairStyles.ContainsKey(HairStyleID))
			{
				drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().HairStyleID = drawInfo.drawPlayer.hair;
				HairStyleID = drawInfo.drawPlayer.hair;
			}

            if (Main.hairWindow && drawInfo.drawPlayer.whoAmI == Main.myPlayer)
            {
                if (hairWindow = false)
                {
                    oldHairStyleID = drawInfo.drawPlayer.hair;
                    hairWindow = true;
                }

                Main.playerHairTexture[drawInfo.drawPlayer.hair] = HairLoader.HairStyles[drawInfo.drawPlayer.hair].Hair;
                Main.playerHairAltTexture[drawInfo.drawPlayer.hair] = HairLoader.HairStyles[drawInfo.drawPlayer.hair].HairAlt;
            }
            else
            {
                if (drawInfo.drawPlayer.whoAmI == Main.myPlayer)
                {
                    if (hairWindow = true)
                    {
                        if (drawInfo.drawPlayer.hair != oldHairStyleID)
                        {
                            HairStyleID = drawInfo.drawPlayer.hair;
                            drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().HairStyleID = drawInfo.drawPlayer.hair;

                            if (Main.netMode == NetmodeID.MultiplayerClient)
                            {

                            }
                        }

                        hairWindow = false;
                    }
                }

                Main.playerHairTexture[drawInfo.drawPlayer.hair] = HairLoader.HairStyles[HairStyleID].Hair;
			    Main.playerHairAltTexture[drawInfo.drawPlayer.hair] = HairLoader.HairStyles[HairStyleID].HairAlt;
            }
		}
    }
}
