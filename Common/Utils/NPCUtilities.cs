using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace BetterVanillaBosses.Common.Utils;

public static class NPCUtilities
{
    public static Player GetPlayerTarget(this NPC npc) => npc.target == -1 ? null : Main.player[npc.target];
}
