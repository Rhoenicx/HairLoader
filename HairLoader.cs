using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.Localization;

namespace HairLoader
{
    public class PlayerHair
    {
        public Texture2D Hair { get; set; }
        public Texture2D HairAlt { get; set; }
    }
    
    class HairLoader : Mod
    {
		// Stores all the hairstyles based on ID
		public static Dictionary<int, PlayerHair> HairStyles = new Dictionary<int, PlayerHair>();

		// stores which mod added which texture
		public static Dictionary<string, Dictionary<string, int>> HairTable = new Dictionary<string, Dictionary<string, int>>();

        public override void Load()
        {
	    	if (!Main.dedServ)
        	{
				RegisterHairTextures("HairLoader", "Example1", GetTexture("HairStyles/Example_1"), GetTexture("HairStyles/ExampleAlt_1"));
        	}
		}

		public override void Unload()
		{
			if (!Main.dedServ)
        	{
				HairStyles = null;
				HairTable = null;
        	}
		}

		public override void UpdateUI (GameTime gameTime)
		{
			if (Main.hairWindow && !Main.dedServ)
			{
				for (int i = 0; i < Main.maxHairTotal; i++)
				{
					if (HairStyles.ContainsKey(i))
					{
						Main.playerHairTexture[i] = HairLoader.HairStyles[i].Hair;
			    		Main.playerHairAltTexture[i] = HairLoader.HairStyles[i].HairAlt;
					}
				}
			}
		}
		
//-----------------------------------------------------------------------------------------------------------------------------
		public void RegisterHairTextures(string modName, string hairName, Texture2D hair, Texture2D hairAlt)
		{
			int index = 0;

			if (HairStyles.Count > 0)
			{
				index = HairStyles.Keys.Max() >= Main.maxHairTotal ? HairStyles.Keys.Max() + 1 : Main.maxHairTotal;
			}
			else
			{
				index = Main.maxHairTotal;
			}

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
				HairTable.Add(modName, new Dictionary<string, int>());
			}

			if (!HairTable[modName].ContainsKey(hairName))
			{
				HairTable[modName].Add(hairName, index);
			}
		}

		public void ChangePlayerHairStyle(string modName, string hairName, int PlayerID)
		{
			if (!HairTable.ContainsKey(modName))
			{
				return;
			}

			if (!HairTable[modName].ContainsKey(hairName))
			{
				return;
			}

			Main.player[PlayerID].GetModPlayer<HairLoaderPlayer>().HairStyleID = HairTable[modName][hairName];

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{

			}
		}
	}
}	
