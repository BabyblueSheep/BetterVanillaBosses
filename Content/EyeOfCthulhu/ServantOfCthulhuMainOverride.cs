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
    private ref struct ServantState(NPC npc)
    {
        private NPC _npc = npc;

        public bool IsAttachedToBoss
        {
            get => _npc.ai[0] == 1f;
            set => _npc.ai[0] = value ? 1f : 0f;
        }

        public int BossWhoAmI
        {
            get => (int)_npc.ai[1];
            set => _npc.ai[1] = value;
        }
    }

    private static class ServantValues
    {
        public static float TargetVelocityLength => 5;
        public static float VelocityInterpolationValue => 0.01f;
    }

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return lateInstantiation && entity.type == NPCID.ServantofCthulhu;
    }

    public override bool InstancePerEntity => true;

    public override bool PreAI(NPC npc)
    {
        if (!npc.HasValidTarget)
        {
            npc.TargetClosest();
        }
        Vector2 targetDirection = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.Zero);
        npc.velocity = Vector2.Lerp(npc.velocity, targetDirection * ServantValues.TargetVelocityLength, ServantValues.VelocityInterpolationValue);
        npc.rotation = npc.velocity.ToRotation() - MathHelper.PiOver2;

        return false;
    }

    

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D eyeTexture = ModContent.Request<Texture2D>("BetterVanillaBosses/Assets/EyeOfCthulhu/ServantOfCthulhu").Value;

        spriteBatch.Draw(eyeTexture, npc.Center - Main.screenPosition, eyeTexture.Frame(), npc.GetAlpha(drawColor), npc.rotation, eyeTexture.Size() / 2, 1f, SpriteEffects.None, 0f);

        return false;
    }
}
