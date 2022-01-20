using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.XNA.Graphics;

//MOD
namespace HairLoader
{
	public class PlayerHair
	{
		public Texture2D Hair { get; set; }
		public Texture2D HairAlt { get; set; }
	}

	public class HairLoader
	{
		// Stores all the hairstyles based on ID
		public static Dictionary<int, PlayerHair> HairStyles = new Dictionary<int, PlayerHair>();

		// stores which mod added which texture
		public static Dictionary<string, Dictionary<int, int>> HairTable = new Dictionary<string, Dictionary<int, int>>();
		
		// stores which hairStyle each player is wearing
		public static int[] PlayerDisplayHairStyle = new int[128];



		//-----------------------------------------------------------------------------------------------------------------------------
		public void RegisterCustomHair(string modName, Texture2D hair, Texture2D hairAlt)
		{
			int index = HairStyles.Keys.Max() >= Main.maxHairLoaded ? HairStyles.Keys.Max() + 1 : Main.maxHairLoaded;

			HairLoader.HairStyles.Add
			(
				index,
				new PlayerHair
				{
					Hair = hair,
					HairAlt = hairAlt,
				}
			);

			if (!HairTable.ContainsKey(modName))
			{
				HairTable.Add(modName, new Dictionary<int, int>());
			}

			int subindex = 0;
			if (HairTable[modName].Count != 0)
			{
				subindex = HairTable[modName].Keys.Max() + 1;
			}

			HairTable[modName].Add(subindex, index);
		}



		//-----------------------------------------------------------------------------------------------------------------------------
		public bool ChangePlayerHairStyle(string modName, int modHairID, int PlayerID)
		{
			if (!HairTable.ContainsKey(modName))
			{
				return false;
			}

			if (!HairTable[modName].ContainsKey(modHairID))
			{
				return false;
			}

			PlayerDisplayHairStyle[PlayerID] = HairTable[modName][modHairID];

			return true;
		}



		//-----------------------------------------------------------------------------------------------------------------------------
		public void ClearPlayerDisplayHairStyle()
		{
			PlayerDisplayHairStyle = new int[128];
		}


		//-----------------------------------------------------------------------------------------------------------------------------
		public void Draw(PlayerDrawInfo drawInfo)
		{
			// ALWAYS check if we have the vanilla hairstyle loaded
			if (!HairStyles.ContainsKey(drawInfo.drawPlayer.hair))
			{
				HairStyles.Add
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
			int HairStyleID = PlayerIsWearingHairStyle[drawInfo.drawPlayer.whoAmI];

			if (!HairStyles.ContainsKey(HairStyleID))
			{
				// if not loaded reset the player's hairstyle to the default one
				PlayerIsWearingHairStyle[drawInfo.drawPlayer.whoAmI] = drawInfo.drawPlayer.hair;
				HairStyleID = drawInfo.drawPlayer.hair;
			}

			Main.playerHairTexture[drawInfo.drawPlayer.hair] = HairLoader.HairStyles[HairStyleID].Hair;
			Main.playerHairAltTexture[drawInfo.drawPlayer.hair] = HairLoader.HairStyles[HairStyleID].Hair;
		}
	}
}