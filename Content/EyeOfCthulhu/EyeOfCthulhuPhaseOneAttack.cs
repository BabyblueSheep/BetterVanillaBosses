using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace BetterVanillaBosses.Content.EyeOfCthulhu
{
    partial class EyeOfCthulhuBehaviorOverride : GlobalNPC
    {
        private static void Phase1_EnterAttackState(NPC npc)
        {
            ref float generalTimer = ref npc.ai[1];

            WeightedRandom<BehaviorType> randomAttackState = new WeightedRandom<BehaviorType>();
            randomAttackState.Add(BehaviorType.Phase1_Attack_BigCharge);
            BehaviorType definitiveAttackState = randomAttackState.Get();
            npc.ai[0] = (int)definitiveAttackState;

            generalTimer = -1;

            switch (definitiveAttackState)
            {
                case BehaviorType.Phase1_Attack_BigCharge:
                    Vector2 targetVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.One);
                    npc.localAI[0] = targetVelocity.X;
                    npc.localAI[1] = targetVelocity.Y;

                    npc.velocity = -targetVelocity * 7f;
                    npc.rotation = npc.velocity.ToRotation() + MathHelper.PiOver2;
                    break;
            }
        }

        private static void Phase1_Attack_BigCharge(NPC npc)
        {
            ref float chargeTimer = ref npc.ai[1];
            float chargeX = npc.localAI[0];
            float chargeY = npc.localAI[1];
            Vector2 chargeDirection = new Vector2(chargeX, chargeY);

            if (chargeTimer == 0)
            {

            }
            else if (chargeTimer < 15)
            {
                npc.velocity *= 0.95f;
            }
            else if (chargeTimer < 45)
            {
                npc.velocity = chargeDirection * 15f;
            }
            else if (chargeTimer < 60)
            {
                npc.velocity *= 0.95f;
            }
            else
            {
                Phase1_EnterIdleState(npc);
            }

            if (chargeTimer < 15)
            {
                npc.rotation = npc.velocity.ToRotation() + MathHelper.PiOver2;
            }
            else
            {
                npc.rotation = npc.velocity.ToRotation() - MathHelper.PiOver2;
            }
        }
    }
}
