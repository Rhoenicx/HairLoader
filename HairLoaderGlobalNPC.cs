using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using HairLoader.UI;

namespace HairLoader
{
    public class HairLoaderGlobalNPC : GlobalNPC
    {
        public override bool PreChatButtonClicked(NPC npc, bool firstButton)
        {
            if (npc.type == NPCID.Stylist)
            {
                if (!firstButton)
                {
                    HairLoader.Instance.HairWindow.OpenHairWindow();
                    return false;
                }
            }

            return base.PreChatButtonClicked(npc, firstButton);
        }
    }
}