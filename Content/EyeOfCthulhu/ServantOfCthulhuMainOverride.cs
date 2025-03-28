using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BetterVanillaBosses.Content.EyeOfCthulhu;

internal sealed class ServantOfCthulhuMainOverride : GlobalNPC
{
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return lateInstantiation && entity.type == NPCID.ServantofCthulhu;
    }

    public override bool PreAI(NPC npc)
    {
        return false;
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D eyeTexture = ModContent.Request<Texture2D>("BetterVanillaBosses/Assets/EyeOfCthulhu/ServantOfCthulhu").Value;

        spriteBatch.Draw(eyeTexture, npc.Center - Main.screenPosition, eyeTexture.Frame(), npc.GetAlpha(drawColor), npc.rotation, eyeTexture.Size() / 2, 1f, SpriteEffects.None, 0f);

        return false;
    }
}
