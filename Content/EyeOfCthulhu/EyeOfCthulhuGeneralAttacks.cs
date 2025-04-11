using BetterVanillaBosses.Common.Utils;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
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

            Player player = npc.GetPlayerTarget();

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

                SoundEngine.PlaySound(SoundID.DD2_BetsyWindAttack, npc.Center);
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
            public static float TotalChargeAmount(NPC npc) => IsInPhase2(npc) ? 5 : 4;
            public static float TotalChargeTime(NPC npc) => IsInPhase2(npc) ? 55 : 65;
            public static float TimeUntilDash(NPC npc) => IsInPhase2(npc) ? 15 : 20;
            public static float TimeUntilPostDashSlowdown(NPC npc) => TotalChargeTime(npc) - (IsInPhase2(npc) ? 15 : 20);
            public static float ChargeUpSlowdownMultiplier => 0.95f;
            public static float PostDashSlowdownMultiplier => 0.9f;
            public static float ChargeSpeed(NPC npc) => 15f;
            //Charge up uses the same speed as the dash but multiplied, so that slower/faster charges have slower/faster charge ups
            public static float DashChargeUpMultiplier => 0.5f;

            public static int TearAmount => Main.rand.Next(6, 8 + 1);
            public static Vector2 TearVelocity(NPC npc, Vector2 direction) => direction * 14f + Main.rand.NextVector2Circular(3f, 3f);
        }

        private static void Attack_RapidDashes(NPC npc)
        {
            GeneralState generalState = new GeneralState(npc);
            RapidDashesState dashState = new RapidDashesState(npc);

            Player player = npc.GetPlayerTarget();

            if (generalState.Timer == 0)
            {
                if (!dashState.StartedCharging)
                {
                    dashState.StartedCharging = true;
                    dashState.CurrentDashAmount = 0;
                }

                Vector2 targetVelocity = player.Center - npc.Center;
                dashState.DashDirection = targetVelocity.SafeNormalize(Vector2.Zero);

                npc.velocity = -dashState.DashDirection * RapidDashValues.ChargeSpeed(npc) * RapidDashValues.DashChargeUpMultiplier;
            }
            else if (generalState.Timer < RapidDashValues.TimeUntilDash(npc))
            {
                npc.velocity *= RapidDashValues.ChargeUpSlowdownMultiplier;
            }
            else if (generalState.Timer == RapidDashValues.TimeUntilDash(npc))
            {
                Vector2 targetVelocity = player.Center - npc.Center;
                dashState.DashDirection = targetVelocity.SafeNormalize(Vector2.Zero);
                npc.velocity = dashState.DashDirection * RapidDashValues.ChargeSpeed(npc);

                SoundEngine.PlaySound(SoundID.DD2_BetsyWindAttack, npc.Center);
            }
            else if (generalState.Timer == RapidDashValues.TimeUntilPostDashSlowdown(npc))
            {
                int tearAmount = RapidDashValues.TearAmount;
                // TODO: make not look like ass
                // should add an offset on spawn so the tears come from the pupil
                // check for distance from ground? kinda unsatisfying and gross when he shoots tears immediately into a block
                for (int i = 0; i < tearAmount; i++)
                {
                    Projectile.NewProjectileDirect(npc.GetSource_FromThis(), npc.Center, RapidDashValues.TearVelocity(npc, dashState.DashDirection), ModContent.ProjectileType<Teardrop>(), 1, 1, Main.myPlayer);
                }
            }
            else if (generalState.Timer > RapidDashValues.TimeUntilPostDashSlowdown(npc) && generalState.Timer < RapidDashValues.TotalChargeTime(npc))
            {
                npc.velocity *= RapidDashValues.PostDashSlowdownMultiplier;
            }
            else if (generalState.Timer >= RapidDashValues.TotalChargeTime(npc))
            {
                dashState.CurrentDashAmount++;
                if (dashState.CurrentDashAmount >= RapidDashValues.TotalChargeAmount(npc))
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

            if (generalState.Timer > RapidDashValues.TimeUntilDash(npc) && generalState.Timer < RapidDashValues.TimeUntilPostDashSlowdown(npc))
            {
                npc.rotation = dashState.DashDirection.ToRotation() - MathHelper.PiOver2;
            }
            else
            {
                npc.rotation = npc.rotation.AngleLerp(npc.AngleTo(player.Center) - MathHelper.PiOver2, IdleValues.RotationToPlayerSpeed);
            }
        }

        #endregion

        #region Summon servants

        public Vector2 ServantSpawnDirection { get; set; }

        private ref struct SummonServantsState(NPC npc)
        {
            private NPC _npc = npc;

            public ref float CurrentSpawnDelayValue => ref _npc.localAI[0];
            public ref float DelayDecreasePerFrame => ref _npc.localAI[1];
            public int AmountOfTimesDelayReachedZero
            {
                get => (int)_npc.localAI[2];
                set
                {
                    _npc.localAI[2] = value;
                }
            }
            public Vector2 ServantSpawnDirection
            {
                get => _npc.GetGlobalNPC<EyeOfCthulhuBehaviorOverride>().ServantSpawnDirection;
                set
                {
                    _npc.GetGlobalNPC<EyeOfCthulhuBehaviorOverride>().ServantSpawnDirection = value;
                }
            }
        }

        private static class SummonServantsValues
        {
            public static float VelocitySlowdownMultiplier => 0.975f;
            //Arbitary value; doesn't exactly matter since the delay decrease is what's changed
            public static float TotalSpawnDelay => 100;
            public static int TotalAmountOfServantsToSummon => 8;

            public static float DelayDecreaseSpeed => Main.rand.NextFloat(10f, 20f);
            public static float ServantInitialSpeed => Main.rand.NextFloat(5f, 10f);
            public static Vector2 ServantInitialDirection(Vector2 eyeDirection) =>
                (Main.rand.NextBool() ? eyeDirection.RotatedBy(MathHelper.PiOver2) : eyeDirection.RotatedBy(-MathHelper.PiOver2))
                .RotatedByRandom(0.3f);
        }

        private static void Attack_SummonServants(NPC npc)
        {
            GeneralState generalState = new GeneralState(npc);
            SummonServantsState summonState = new SummonServantsState(npc);

            Player player = npc.GetPlayerTarget();

            if (generalState.Timer == 0)
            {
                summonState.CurrentSpawnDelayValue = SummonServantsValues.TotalSpawnDelay;
                summonState.DelayDecreasePerFrame = SummonServantsValues.DelayDecreaseSpeed;
                summonState.AmountOfTimesDelayReachedZero = 0;

                Vector2 targetDirection = player.Center - npc.Center;
                summonState.ServantSpawnDirection = targetDirection.SafeNormalize(Vector2.UnitY);
            }

            npc.velocity *= SummonServantsValues.VelocitySlowdownMultiplier;

            summonState.CurrentSpawnDelayValue -= summonState.DelayDecreasePerFrame;
            if (summonState.CurrentSpawnDelayValue <= 0)
            {
                summonState.CurrentSpawnDelayValue += SummonServantsValues.TotalSpawnDelay;
                NPC servant = NPC.NewNPCDirect(npc.GetSpawnSourceForNPCFromNPCAI(), npc.Center, NPCID.ServantofCthulhu, npc.whoAmI);
                servant.velocity = SummonServantsValues.ServantInitialDirection(summonState.ServantSpawnDirection.SafeNormalize(Vector2.UnitY)) * SummonServantsValues.ServantInitialSpeed;
                summonState.AmountOfTimesDelayReachedZero++;

                SoundEngine.PlaySound(SoundID.NPCDeath1, npc.Center);
            }

            npc.rotation = npc.rotation.AngleLerp(npc.AngleTo(player.Center) - MathHelper.PiOver2, IdleValues.RotationToPlayerSpeed);

            if (summonState.AmountOfTimesDelayReachedZero > SummonServantsValues.TotalAmountOfServantsToSummon)
            {
                EnterIdleState(npc);
                return;
            }
        }

        #endregion

        private static void EnterAttackState(NPC npc)
        {
            GeneralState generalState = new GeneralState(npc);

            Player player = npc.GetPlayerTarget();

            WeightedRandom<BehaviorType> randomAttackState = new WeightedRandom<BehaviorType>();
            randomAttackState.Add(BehaviorType.Attack_BigDash, 1f);
            if (npc.Center.Distance(player.Center) < RapidDashValues.DistanceNeededToSelectAttack)
            {
                randomAttackState.Add(BehaviorType.Attack_RapidDashes, 1f);
            }
            if (!Main.npc.Any((npc) => npc.active && npc.type == NPCID.ServantofCthulhu))
            {
                randomAttackState.Add(BehaviorType.Attack_SummonServants, 3f);
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
