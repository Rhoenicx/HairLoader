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
            // Get the modName and hairName of our current drawPlayer.
            string modName = drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_modName;
            string hairName = drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_hairName;
            
            // Boolean that is used to check if the hair is valid and/or found in out HairTable Dictionary
            bool valid = true;

            // Check if the player's saved hairstyle is present in the HairTable Dictionary
            if (!HairLoader.HairTable.ContainsKey(modName))
            {
                // The modName of the hairstyle stored in the player could not be found
                valid = false;
            }
            else if (!HairLoader.HairTable[modName].ContainsKey(hairName))
            {
                // The hairName of the hairstyle stored in the player could not be found
                valid = false;
            }

            // If the modName OR hairName does not exist in the HairTable:
            if (!valid)
            {
                // Search for the vanilla hairstyle, use the player's vanilla hair ID to find the modName and hairName
                // since this method uses 'ref' keyword the vanilla mod&hair names gets assigned to these variables automatically.
                if (HairLoader.Instance.getModAndHairNames(ref modName, ref hairName, drawInfo.drawPlayer.hair))
                {
                    // If the player is in the main menu we don't want to run this code, this will prevent the
                    // player from changing their hair in the character creator window.
                    
                    // TODO: This might be optimized a little better... perhaps?
                    if (!Main.gameMenu) // (Main.PendingPlayer != (Player) null) Main.menuMode ?
                    {
                        drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_modName = modName;
                        drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_hairName = hairName;
                    }
                }
                else
                {
                    // If the code ends up here something messed up badly, somehow the vanilla hairstyle is not present in the HairTable...
                    Logger.Warn("HAIRLOADER: HAIRTABLE DOES NOT CONTAIN VANILLA HAIRSTYLE: " + modName + " - " + hairName + " ! Report this to the developer!");
                    return;
                }
            }

            // Close the vanilla Hair Window UI if active, this one is a little broken due to swapping textures in the Main hair texture array anyway.
            if (Main.hairWindow)
            {
                Main.hairWindow = false;

                if (Main.player[Main.myPlayer].talkNPC > -1 && Main.npc[Main.player[Main.myPlayer].talkNPC].type == NPCID.Stylist)
                {
                    Main.player[Main.myPlayer].talkNPC = -1;
                }
            }

            // Replace the texture file in the texture slot of the player's vanilla hairstyle with our textures.
            Main.playerHairTexture[drawInfo.drawPlayer.hair] = HairLoader.HairTable[modName][hairName].hair;
            Main.playerHairAltTexture[drawInfo.drawPlayer.hair] = HairLoader.HairTable[modName][hairName].hairAlt;
        }
    }
}
