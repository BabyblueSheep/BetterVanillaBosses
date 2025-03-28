using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace BetterVanillaBosses.Content.EyeOfCthulhu
{
    partial class EyeOfCthulhuBehaviorOverride : GlobalNPC
    {
        private ref struct IdleState(NPC npc)
        {
            private NPC _npc = npc;

            public ref float RotationOffset => ref _npc.localAI[0];
            public ref float RotationRangeMultiplier => ref _npc.localAI[1];
            public ref float Distance => ref _npc.localAI[2];
        }

        public static float TotalPhaseTime => 60 * 6;
        public static float TimeUntilRandomizeState => 60 * 3;
        //The length of the wave period at which EoC wobbles around the player
        public static float WaveSpeed => 0.02f;
        public static float TargetVelocityMultiplier => 0.05f;
        //To prevent EoC from flying when too far
        public static float MaximumTargetVelocityLength => 10f;
        public static float VelocityInterpolationChange => 0.02f;
        public static float RotationToPlayerSpeed => 0.1f;


        public static float OnTop_RotationRangeMultiplier => Main.rand.NextFloat(0.2f, 0.5f);
        public static float OnTop_RotationOffset => Main.rand.NextFloat(-0.1f, 0.1f);
        public static float OnTop_Distance => Main.rand.NextFloat(200f, 300f);
        public static float ToSide_RotationRangeMultiplier => Main.rand.NextFloat(0.3f, 0.5f);
        public static float ToSide_RotationOffset => Main.rand.NextFloat(0.7f, 1f);
        public static float ToSide_Distance => Main.rand.NextFloat(300f, 450f);


        private static void Idle(NPC npc)
        {
            GeneralState generalState = new GeneralState(npc);
            IdleState idleState = new IdleState(npc);

            Player player = Main.player[npc.target];

            float targetPositionRotation = MathF.Sin(generalState.Timer * WaveSpeed);
            if (generalState.Timer % TimeUntilRandomizeState == 0)
            {
                switch (generalState.CurrentBehaviorType)
                {
                    case BehaviorType.Idle_StayOnTop:
                        idleState.RotationRangeMultiplier = OnTop_RotationRangeMultiplier;
                        idleState.RotationOffset = OnTop_RotationOffset;
                        idleState.Distance = OnTop_Distance;
                        break;
                    case BehaviorType.Idle_StayToLeft:
                    case BehaviorType.Idle_StayToRight:
                        idleState.RotationRangeMultiplier = ToSide_RotationRangeMultiplier;
                        idleState.RotationOffset = ToSide_RotationOffset * (generalState.CurrentBehaviorType == BehaviorType.Idle_StayToLeft ? -1 : 1);
                        idleState.Distance = ToSide_Distance;
                        break;
                }
            }

            Vector2 targetPositionOffset = new Vector2(0, idleState.Distance);
            targetPositionOffset = targetPositionOffset.RotatedBy(targetPositionRotation * idleState.RotationRangeMultiplier + idleState.RotationOffset);
            Vector2 targetPosition = player.Center - targetPositionOffset;
            Vector2 targetVelocity = (targetPosition - npc.Center);
            float targetVelocityLength = targetVelocity.Length();
            targetVelocity = targetVelocity.SafeNormalize(Vector2.Zero) * MathF.Min(MaximumTargetVelocityLength, targetVelocityLength) * TargetVelocityMultiplier;
            npc.velocity =  Vector2.Lerp(npc.velocity, targetVelocity, VelocityInterpolationChange);

            npc.rotation = npc.rotation.AngleLerp(npc.AngleTo(player.Center) - MathHelper.PiOver2, RotationToPlayerSpeed);
            
            if (generalState.Timer >= TotalPhaseTime)
            {
                Phase1_EnterIdleState(npc);
            }
        }

        private static void Phase1_EnterIdleState(NPC npc)
        {
            GeneralState generalState = new GeneralState(npc);
            IdleState idleState = new IdleState(npc);
            
            WeightedRandom<BehaviorType> randomIdleState = new WeightedRandom<BehaviorType>(Main.rand);
            randomIdleState.Add(BehaviorType.Idle_StayOnTop, 1.5f);
            randomIdleState.Add(BehaviorType.Idle_StayToLeft, 1f);
            randomIdleState.Add(BehaviorType.Idle_StayToRight, 1f);
            BehaviorType definitiveIdleState = randomIdleState.Get();
            generalState.CurrentBehaviorType = definitiveIdleState;

            generalState.Timer = -1;

            npc.TargetClosest();
        }
    }
}
