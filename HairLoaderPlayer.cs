using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.GameContent;
using HairLoader.UI;
using Terraria.DataStructures;

namespace HairLoader
{
    public class HairLoaderPlayer : ModPlayer
    {
        public string Hair_modName = "";
        public string Hair_hairName = "";

        public override void SaveData(TagCompound tag)
        {
            tag.Add("modName", Hair_modName);
            tag.Add("hairName", Hair_hairName);
        }

        public override void LoadData(TagCompound tag)
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

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            // Get the modName and hairName of our current drawPlayer.
            string modName = drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_modName;
            string hairName = drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_hairName;

            // Boolean that is used to check if the hair is valid and/or found in out HairTable Dictionary
            if (HairLoader.VanillaTextureSlot[drawInfo.drawPlayer.hair].modName != modName || HairLoader.VanillaTextureSlot[drawInfo.drawPlayer.hair].hairName != hairName)
            {
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
                    if (HairLoader.getModAndHairNames(ref modName, ref hairName, drawInfo.drawPlayer.hair))
                    {
                        // If the player is in the main menu we don't want to run this code, this will prevent the
                        // player from changing their hair in the character creator window.

                        if (!Main.gameMenu)
                        {
                            // Assign the valid vanilla hairstyle to this player
                            drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_modName = modName;
                            drawInfo.drawPlayer.GetModPlayer<HairLoaderPlayer>().Hair_hairName = hairName;
                        }
                    }
                    else
                    {
                        // If the code ends up here something messed up badly, somehow the vanilla hairstyle is not present in the HairTable...
                        HairLoader.Instance.Logger.Warn("HAIRLOADER: HAIRTABLE DOES NOT CONTAIN VANILLA HAIRSTYLE: " + modName + " - " + hairName + " ! Report this to the developer!");
                        return;
                    }
                }

                // Replace the texture file in the texture slot of the player's vanilla hairstyle with our textures.
                TextureAssets.PlayerHair[drawInfo.drawPlayer.hair] = HairLoader.HairTable[modName][hairName].hair;
                TextureAssets.PlayerHairAlt[drawInfo.drawPlayer.hair] = HairLoader.HairTable[modName][hairName].hairAlt;               

                // Update Internal vanilla texture array
                HairLoader.VanillaTextureSlot[drawInfo.drawPlayer.hair].modName = modName;
                HairLoader.VanillaTextureSlot[drawInfo.drawPlayer.hair].hairName = hairName;
            }
                
            // Apply the drawOffset for this hairstyle
            drawInfo.hairOffset.X = HairLoader.HairTable[modName][hairName].hairOffset;

            // Close the vanilla Hair Window UI if active, this one is a little broken due to swapping textures in the Main hair texture array anyway.
            if (Main.hairWindow)
            {
                Main.hairWindow = false;

                if (Main.player[Main.myPlayer].talkNPC > -1 && Main.npc[Main.player[Main.myPlayer].talkNPC].type == NPCID.Stylist)
                {
                    Main.player[Main.myPlayer].SetTalkNPC(-1);
                }
            }
        }
    }
}
