using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;

namespace BetterVanillaBosses.Common.Utils;

internal static class WorldDifficulty
{
    public enum WorldDifficultyType
    {
        None,
        Classic,
        Expert,
        Master,
        Legendary
    }

    /// <summary>
    /// Returns the current difficulty of the world as a number.
    /// </summary>
    /// <returns>The current world difficulty.</returns>
    public static WorldDifficultyType GetWorldDifficulty()
    {
        if (Main.ActiveWorldFileData == null)
            return WorldDifficultyType.None;
        int difficulty = 1;
        if (Main.GameModeInfo.IsMasterMode || (Main._overrideForMasterMode.HasValue && Main._overrideForMasterMode.Value))
            difficulty = 3;
        else if (Main.GameModeInfo.IsExpertMode || (Main._overrideForExpertMode.HasValue && Main._overrideForExpertMode.Value))
            difficulty = 2;
        if (Main.getGoodWorld)
            difficulty++;
        return (WorldDifficultyType)difficulty;
    }
}
