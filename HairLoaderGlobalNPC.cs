using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
                    HairloaderSystem.Instance.HairWindow.OpenHairWindow(npc);
                    return false;
                }
            }

            return base.PreChatButtonClicked(npc, firstButton);
        }
    }
}