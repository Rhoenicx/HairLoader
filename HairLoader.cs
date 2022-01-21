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
    public class PlayerHairTexture
    {
        public Texture2D hair { get; set; }
        public Texture2D hairAlt { get; set; }
    }
	
	public class PlayerHairEntry
	{
		public int index { get; set; }
		public int currency { get; set; }
		public int price { get; set; }
		public bool visibility { get; set; }
	}
    
    class HairLoader : Mod
    {
		// Stores all the hairstyles based on ID
		public static Dictionary<int, PlayerHairTexture> HairStyles = new Dictionary<int, PlayerHairTexture>();

		// stores which mod added which texture
		public static Dictionary<string, Dictionary<string, int>> HairTable = new Dictionary<string, Dictionary<string, PlayerHairEntry>>();

        public override void Load()
        {
	    	if (!Main.dedServ)
        	{
				LoadVanillaHair();
				RegisterCustomHair("HairLoader", "Example1", GetTexture("HairStyles/Example_1"), GetTexture("HairStyles/ExampleAlt_1"), -1, 10000, true);
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
			// Vanilla hair window may not be opened anymore
			if (Main.hairWindow)
			{
				Main.hairWindow = !Main.hairWindow;
			}
		}

//-----------------------------------------------------------------------------------------------------------------------------
		public void LoadVanillaHair()
		{
			for (int i = 0, i < Main.maxHairTotal; i++)
			{
				if(!HairTable.ContainsKey("Vanilla"))
				{
					HairTable.Add("Vanilla", new Dictionary<string, PlayerHairEntry>());
				}
				
				if (!HairTable["Vanilla"].ContainsKey(i.ToString()))
				{
					HairTable["Vanilla"].Add(i.ToString(), new PlayerHairEntry { index = i, currency = -1, price = i <= 51 ? 10000 : 50000, visibility = true })
				}

				if (!HairStyles.ContainsKey(i))
				{
					HairStyles.Add(i, new PlayerHairTexture { hair = GetTexture("HairStyles/Vanilla/player_hair_" + i.ToString()), hairAlt = GetTexture("HairStyles/Vanilla/player_hairAlt_" + i.ToString()});
				}
			}
		}

//-----------------------------------------------------------------------------------------------------------------------------
		public void RegisterCustomHair(string _modName, string _hairName, Texture2D _hair, Texture2D _hairAlt, int _currency, int _price, bool _visibility)
		{
			int _index = 0;

			if (HairStyles.Count > 0)
			{
				_index = HairStyles.Keys.Max() >= Main.maxHairTotal ? HairStyles.Keys.Max() + 1 : Main.maxHairTotal;
			}
			else
			{
				_index = Main.maxHairTotal;
			}

			if (_modName == "Vanilla")
			{
				return;
			}
			
			if (!HairTable.ContainsKey(_modName))
			{
				HairTable.Add(_modName, new Dictionary<string, PlayerHairEntry>());
			}

			if (!HairTable[_modName].ContainsKey(_hairName))
			{
				HairTable[_modName].Add(_hairName, new PlayerHairEntry { index = _index, currency = _currency, price = _price, visibility = _visibility });
				
				if (!HairStyles.ContainsKey(_index))
				{
					HairStyles.Add(_index, new PlayerHairTexture { hair = _hair, hairAlt = _hairAlt });
				}
			}
		}

//-----------------------------------------------------------------------------------------------------------------------------
		public void ModifyPlayerHairEntryVisibility (string _modName, string _hairName, bool _visibility)
		{
			if (!HairTable.ContainsKey(_modName))
			{
				return;
			}
			
			if (!HairTable[_modName].ContainsKey(_hairName))
			{
				return;
			}
			
			HairTable[_modName][_hairName].visibility = _visibility;
		}

//-----------------------------------------------------------------------------------------------------------------------------
		public void ChangePlayerHairStyle (string modName, string hairName, int PlayerID, bool preview)
		{
			if (!HairTable.ContainsKey(modName))
			{
				return;
			}

			if (!HairTable[modName].ContainsKey(hairName))
			{
				return;
			}

			Main.player[PlayerID].GetModPlayer<HairLoaderPlayer>().HairStyleID = HairTable[modName][hairName].index;

			if (Main.player[PlayerID] == Main.myPlayer)
			{
				if (!preview)
				{
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
		
					}
				}
			}
		}
		
		public void ChangePlayerHairStyleByID (int HairStyleID, int PlayerID, bool preview)
		{
			if (!HairStyles.ContainsKey(HairStyleID))
			{
				return;
			}
			
			Main.player[PlayerID].GetModPlayer<HairLoaderPlayer>().HairStyleID = HairTable[modName][hairName].index;

			if (Main.player[PlayerID] == Main.myPlayer)
			{
				if (!preview)
				{
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
		
					}
				}
			}
		}
	}
}	
