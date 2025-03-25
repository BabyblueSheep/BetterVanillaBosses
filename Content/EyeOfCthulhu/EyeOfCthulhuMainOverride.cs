using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BetterVanillaBosses.Content.EyeOfCthulhu
{
    public partial class EyeOfCthulhuBehaviorOverride : GlobalNPC
    {
        public enum EyeOfCthulhuState
        {
            Phase1_Idle_SwayAroundPlayer,
            Phase1_Idle_GetCloseToPlayer,
            Phase1_Idle_GetFarFromPlayer,

            Phase1_Attack_BigCharge,
        }

        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return lateInstantiation && entity.type == NPCID.EyeofCthulhu;
        }

        public override void SpawnNPC(int npc, int tileX, int tileY)
        {
            base.SpawnNPC(npc, tileX, tileY);
        }

        public override bool PreAI(NPC npc)
        {
            EyeOfCthulhuState currentState = (EyeOfCthulhuState)(int)npc.ai[0];
            ref float generalTimer = ref npc.ai[1];

            if (npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead || !Main.player[npc.target].active)
            {
                npc.TargetClosest();
            }

            Player player = Main.player[npc.target];

            if (player.dead)
            {
                npc.velocity.Y -= 0.04f;
                npc.EncourageDespawn(10);
                return false;
            }

            switch (currentState)
            {
                case EyeOfCthulhuState.Phase1_Idle_SwayAroundPlayer:
                case EyeOfCthulhuState.Phase1_Idle_GetCloseToPlayer:
                case EyeOfCthulhuState.Phase1_Idle_GetFarFromPlayer:
                    Phase1_Idle(npc);
                    break;
                case EyeOfCthulhuState.Phase1_Attack_BigCharge:
                    Phase1_Attack_BigCharge(npc);
                    break;
            }

            generalTimer++;

            return false;
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D eyeTexture = ModContent.Request<Texture2D>("BetterVanillaBosses/Assets/EyeOfCthulhu/EyeOfCthulhu").Value;

            spriteBatch.Draw(eyeTexture, npc.Center - Main.screenPosition, eyeTexture.Frame(), npc.GetAlpha(drawColor), npc.rotation, eyeTexture.Size() / 2, 1f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
