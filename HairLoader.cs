using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace HairLoader
{
    public class PlayerHair
		{
			  public Texture2D Hair { get; set; }
			  public Texture2D HairAlt { get; set; }
			  public string Name { get; set; }
		}
    
    class HairLoader : Mod
	  {
		    public static Dictionary<int, PlayerHair> HairStyles = new Dictionary<int, PlayerHair>();
    
		    public override void load()
		    {
			      if (!Main.dedServ)
            {

            }
		    }
		
		    public int AddCustomHairStyle(Texture2D hair, Texture2D hairAlt, string name)
		    {
			      int index = HairStyles.Keys.Max() >= Main.maxHairLoaded ? HairStyles.Keys.Max() + 1 : Main.maxHairLoaded;
			
			      HairLoader.HairStyles.Add
				        (
					          index,
					          new HairLoader.PlayerHair
						        { 
							          Hair = hair,
							          HairAlt = hairAlt,
							          Name = name
						        }
				        );
			
			      return index;
		    }
	  }
}	
