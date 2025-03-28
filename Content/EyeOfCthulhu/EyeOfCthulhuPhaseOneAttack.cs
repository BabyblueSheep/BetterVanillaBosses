using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace BetterVanillaBosses.Content.EyeOfCthulhu
{
    partial class EyeOfCthulhuBehaviorOverride : GlobalNPC
    {
        private ref struct BigDashState(NPC npc)
        {
            private NPC _npc = npc;

            public Vector2 DashDirection
            {
                get => new Vector2(_npc.localAI[0], _npc.localAI[1]);
                set
                {
                    _npc.localAI[0] = value.X;
                    _npc.localAI[1] = value.Y;
                }
            }
            public ref float DashSpeed => ref _npc.localAI[2];
        }

        private static void Phase1_EnterAttackState(NPC npc)
        {
            GeneralState generalState = new GeneralState(npc);

            WeightedRandom<BehaviorType> randomAttackState = new WeightedRandom<BehaviorType>();
            randomAttackState.Add(BehaviorType.Phase1_Attack_BigCharge, 1f);
            BehaviorType definitiveAttackState = randomAttackState.Get();
            generalState.CurrentBehaviorType = definitiveAttackState;

            generalState.Timer = -1;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                npc.netUpdate = true;
            }
        }

        private static void Phase1_Attack_BigCharge(NPC npc)
        {
            GeneralState generalState = new GeneralState(npc);
            BigDashState dashState = new BigDashState(npc);

            switch (generalState.Timer)
            {
                case 0:
                    Vector2 targetVelocity = Main.player[npc.target].Center - npc.Center;
                    float dashVelocityLength = targetVelocity.Length();
                    dashState.DashSpeed = Utils.Remap(dashVelocityLength, 400, 1000, 15, 45);
                    dashState.DashDirection = targetVelocity.SafeNormalize(Vector2.Zero);

                    npc.velocity = -dashState.DashDirection * (dashState.DashSpeed / 2f);
                    break;
                case < 30:
                    npc.velocity *= 0.95f;
                    break;
                case 30:
                    npc.velocity = dashState.DashDirection * dashState.DashSpeed;
                    break;
                case > 60 and < 75:
                    npc.velocity *= 0.975f;
                    break;
                case >= 75:
                    Phase1_EnterIdleState(npc);
                    break;
            }

            npc.rotation = dashState.DashDirection.ToRotation() - MathHelper.PiOver2;
            /*
            if (chargeTimer == 0)
            {
                Vector2 targetVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.One);
                npc.localAI[0] = targetVelocity.X;
                npc.localAI[1] = targetVelocity.Y;

                npc.velocity = -targetVelocity * 7f;
                npc.rotation = npc.velocity.ToRotation() + MathHelper.PiOver2;
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
                npc.rotation = dashState.DashDirection.ToRotation() - MathHelper.PiOver2;
            }*/
        }
    }
}
