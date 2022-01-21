using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

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
                    return false;
                }
            }

            return base.PreChatButtonClicked(npc, firstButton);
        }
    }
}