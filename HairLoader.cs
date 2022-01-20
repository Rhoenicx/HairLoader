using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework.Graphics;

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
		public static Dictionary<string, Dictionary<int, int>> HairTable = new Dictionary<string, Dictionary<int, int>>();
		
		// stores which hairStyle each player is wearing
		public static int[] PlayerDisplayHairStyle = new int[128];

        public override void Load()
        {
	    	if (!Main.dedServ)
        	{

        	}
		}

		public override void Unload()
		{
			if (!Main.dedServ)
        	{
				HairStyles = null;
				HairTable = null;
				PlayerDisplayHairStyle = null;
        	}
		}
		
//-----------------------------------------------------------------------------------------------------------------------------
		public void RegisterCustomHair(string modName, Texture2D hair, Texture2D hairAlt)
		{
			int index = HairStyles.Keys.Max() >= Main.maxHairTotal ? HairStyles.Keys.Max() + 1 : Main.maxHairTotal;

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

		public void ClearPlayerDisplayHairStyle()
		{
			PlayerDisplayHairStyle = new int[128];
		}
	}
}	
