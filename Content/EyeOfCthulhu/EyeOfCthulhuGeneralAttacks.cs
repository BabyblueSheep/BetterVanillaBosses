using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
            public static float TotalChargeTime => 100;
            public static float TimeUntilDash => 25;
            public static float TimeUntilPostDashSlowdown => TotalChargeTime - 15;
            public static float ChargeUpSlowdownMultiplier => 0.95f;
            public static float PostDashSlowdownMultiplier => 0.975f;
            //Charge up uses the same speed as the dash but multiplied, so that slower/faster charges have slower/faster charge ups
            public static float DashChargeUpMultiplier => 0.5f;
            public static float DistanceFromPlayerToDashSpeed(float distance) => Utils.Remap(distance, 400, 1200, 15, 40);
        }

        private static void Attack_BigDash(NPC npc)
        {
            GeneralState generalState = new GeneralState(npc);
            BigDashState dashState = new BigDashState(npc);

            Player player = Main.player[npc.target];

            if (generalState.Timer == 0)
            {
                Vector2 targetVelocity = player.Center - npc.Center;
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
                Vector2 targetVelocity = player.Center - npc.Center;
                dashState.DashDirection = targetVelocity.SafeNormalize(Vector2.Zero);
                npc.velocity = dashState.DashDirection * dashState.DashSpeed;
            }
            else if (generalState.Timer > BigDashValues.TimeUntilPostDashSlowdown && generalState.Timer < BigDashValues.TotalChargeTime)
            {
                npc.velocity *= BigDashValues.PostDashSlowdownMultiplier;
            }
            else if (generalState.Timer >= BigDashValues.TotalChargeTime)
            {
                EnterIdleState(npc);
                return;
            }

            if (generalState.Timer < BigDashValues.TimeUntilPostDashSlowdown)
            {
                npc.rotation = dashState.DashDirection.ToRotation() - MathHelper.PiOver2;
            }
            else
            {
                npc.rotation = npc.rotation.AngleLerp(npc.AngleTo(player.Center) - MathHelper.PiOver2, IdleValues.RotationToPlayerSpeed);
            }
        }

        #endregion

        #region Rapid dashes

        private ref struct RapidDashesState(NPC npc)
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
            public int CurrentDashAmount
            {
                get => (int)_npc.localAI[2];
                set
                {
                    _npc.localAI[2] = value;
                }
            }
            public bool StartedCharging
            {
                get => _npc.localAI[3] == 1f;
                set => _npc.localAI[3] = value ? 1f : 0f;
            }
        }

        private static class RapidDashValues
        {
            public static float DistanceNeededToSelectAttack => 1200f;
            public static float TotalChargeAmount => 4;
            public static float TotalChargeTime => 55;
            public static float TimeUntilDash => 20;
            public static float TimeUntilPostDashSlowdown => TotalChargeTime - 15;
            public static float ChargeUpSlowdownMultiplier => 0.95f;
            public static float PostDashSlowdownMultiplier => 0.9f;
            public static float ChargeSpeed => 18f;
            //Charge up uses the same speed as the dash but multiplied, so that slower/faster charges have slower/faster charge ups
            public static float DashChargeUpMultiplier => 0.5f;

            #region Tears
            public static int tearShotgunMin = 6;
            public static int tearShotgunMax = 8;
            public static int tearSpread = 3;
            public static int tearSpeed = 14;
            #endregion
        }

        private static void Attack_RapidDashes(NPC npc)
        {
            GeneralState generalState = new GeneralState(npc);
            RapidDashesState dashState = new RapidDashesState(npc);

            Player player = Main.player[npc.target];

            if (generalState.Timer == 0)
            {
                if (!dashState.StartedCharging)
                {
                    dashState.StartedCharging = true;
                    dashState.CurrentDashAmount = 0;
                }

                Vector2 targetVelocity = player.Center - npc.Center;
                dashState.DashDirection = targetVelocity.SafeNormalize(Vector2.Zero);

                npc.velocity = -dashState.DashDirection * RapidDashValues.ChargeSpeed * RapidDashValues.DashChargeUpMultiplier;
            }
            else if (generalState.Timer < RapidDashValues.TimeUntilDash)
            {
                npc.velocity *= RapidDashValues.ChargeUpSlowdownMultiplier;
            }
            else if (generalState.Timer == RapidDashValues.TimeUntilDash)
            {
                Vector2 targetVelocity = player.Center - npc.Center;
                dashState.DashDirection = targetVelocity.SafeNormalize(Vector2.Zero);
                npc.velocity = dashState.DashDirection * RapidDashValues.ChargeSpeed;
            }
            else if (generalState.Timer == RapidDashValues.TimeUntilPostDashSlowdown)
            {
                int tearAmount = Main.rand.Next(RapidDashValues.tearShotgunMin, RapidDashValues.tearShotgunMax);
                // TODO: make not look like ass
                // should add an offset on spawn so the tears come from the pupil
                for (int i = 0; i < tearAmount; i++)
                {
                    Projectile.NewProjectileDirect(npc.GetSource_FromThis(), npc.Center, dashState.DashDirection * RapidDashValues.tearSpeed + Main.rand.NextVector2Circular(RapidDashValues.tearSpread, RapidDashValues.tearSpread), ModContent.ProjectileType<Teardrop>(), 1, 1, Main.myPlayer);
                }
            }
            else if (generalState.Timer > RapidDashValues.TimeUntilPostDashSlowdown && generalState.Timer < RapidDashValues.TotalChargeTime)
            {
                npc.velocity *= RapidDashValues.PostDashSlowdownMultiplier;
            }
            else if (generalState.Timer >= RapidDashValues.TotalChargeTime)
            {
                dashState.CurrentDashAmount++;
                if (dashState.CurrentDashAmount >= RapidDashValues.TotalChargeAmount)
                {
                    EnterIdleState(npc);
                    dashState.StartedCharging = false;
                    return;
                }
                else
                {
                    Vector2 targetVelocity = player.Center - npc.Center;
                    dashState.DashDirection = targetVelocity.SafeNormalize(Vector2.Zero);

                    generalState.Timer = -1;
                }
            }

            if (generalState.Timer > RapidDashValues.TimeUntilDash && generalState.Timer < RapidDashValues.TimeUntilPostDashSlowdown)
            {
                npc.rotation = dashState.DashDirection.ToRotation() - MathHelper.PiOver2;
            }
            else
            {
                npc.rotation = npc.rotation.AngleLerp(npc.AngleTo(player.Center) - MathHelper.PiOver2, IdleValues.RotationToPlayerSpeed);
            }
        }

        #endregion

        private static void EnterAttackState(NPC npc)
        {
            GeneralState generalState = new GeneralState(npc);

            Player player = Main.player[npc.target];

            WeightedRandom<BehaviorType> randomAttackState = new WeightedRandom<BehaviorType>();
            randomAttackState.Add(BehaviorType.Attack_BigDash, 1f);
            if (npc.Center.Distance(player.Center) < RapidDashValues.DistanceNeededToSelectAttack)
            {
                randomAttackState.Add(BehaviorType.Attack_RapidDashes, 1f);
            }
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
