using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HairLoader
{
    public class HairLoaderGlobalNPC : GlobalNPC
    {
        public override bool PreChatButtonClicked(NPC npc, bool firstButton)
        {
            // When the NPC is the stylist
            if (npc.type == NPCID.Stylist)
            {
                // Determine which button was clicked
                if (!firstButton)
                {
                    // The second button (Hair Style) was clicked, open the hairwindow
                    HairloaderSystem.Instance.HairWindow.OpenHairWindow();

                    // Return false here to prevent the vanilla code from opening
                    // the vanilla HairWindow
                    return false;
                }
            }

            // return base
            return base.PreChatButtonClicked(npc, firstButton);
        }
    }
}