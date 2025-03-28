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
        #region Big Dash

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

        private static class BigDashValues
        {
            public static float TotalChargeTime => 75;
            public static float TimeUntilDash => 30;
            public static float TimeUntilPostDashSlowdown => 60;
            public static float ChargeUpSlowdownMultiplier => 0.95f;
            public static float PostDashSlowdownMultiplier => 0.975f;
            //Charge up uses the same speed as the dash but multiplied, so that slower/faster charges have slower/faster charge ups
            public static float DashChargeUpMultiplier => 0.5f;
            public static float DistanceFromPlayerToDashSpeed(float distance) => Utils.Remap(distance, 400, 1000, 15, 45);
        }

        private static void Phase1_Attack_BigDash(NPC npc)
        {
            GeneralState generalState = new GeneralState(npc);
            BigDashState dashState = new BigDashState(npc);

            if (generalState.Timer == 0)
            {
                Vector2 targetVelocity = Main.player[npc.target].Center - npc.Center;
                float dashVelocityLength = targetVelocity.Length();
                dashState.DashSpeed = BigDashValues.DistanceFromPlayerToDashSpeed(dashVelocityLength);
                dashState.DashDirection = targetVelocity.SafeNormalize(Vector2.Zero);

                npc.velocity = -dashState.DashDirection * (dashState.DashSpeed * BigDashValues.DashChargeUpMultiplier);
            }
            else if (generalState.Timer < BigDashValues.TimeUntilDash)
            {
                npc.velocity *= BigDashValues.ChargeUpSlowdownMultiplier;
            }
            else if  (generalState.Timer == BigDashValues.TimeUntilDash)
            {
                npc.velocity = dashState.DashDirection * dashState.DashSpeed;
            }
            else if (generalState.Timer > BigDashValues.TimeUntilPostDashSlowdown && generalState.Timer < BigDashValues.TotalChargeTime)
            {
                npc.velocity *= BigDashValues.PostDashSlowdownMultiplier;
            }
            else if (generalState.Timer >= BigDashValues.TotalChargeTime)
            {
                Phase1_EnterIdleState(npc);
            }

            npc.rotation = dashState.DashDirection.ToRotation() - MathHelper.PiOver2;
        }

        #endregion

        private static void Phase1_EnterAttackState(NPC npc)
        {
            GeneralState generalState = new GeneralState(npc);

            WeightedRandom<BehaviorType> randomAttackState = new WeightedRandom<BehaviorType>();
            randomAttackState.Add(BehaviorType.Phase1_Attack_BigDash, 1f);
            BehaviorType definitiveAttackState = randomAttackState.Get();
            generalState.CurrentBehaviorType = definitiveAttackState;

            generalState.Timer = -1;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                npc.netUpdate = true;
            }
        }
    }
}
