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



        private static void Idle(NPC npc)
        {
            GeneralState generalState = new GeneralState(npc);
            IdleState idleState = new IdleState(npc);

            Main.NewText(generalState.CurrentBehaviorType);

            Player player = Main.player[npc.target];

            float targetPositionRotation = MathF.Sin(generalState.Timer * 0.02f);
            if (generalState.Timer % 80 == 0 && generalState.Timer % 160 != 0)
            {
                switch (generalState.CurrentBehaviorType)
                {
                    case BehaviorType.Idle_StayOnTop:
                        idleState.RotationRangeMultiplier = MathHelper.Lerp(idleState.RotationRangeMultiplier, Main.rand.NextFloat(0.4f, 0.8f), 0.5f);
                        idleState.RotationOffset += Main.rand.NextFloat(-0.2f, 0.2f);
                        idleState.Distance = Main.rand.NextFloat(300f, 450f);
                        break;
                    case BehaviorType.Idle_StayToLeft:
                    case BehaviorType.Idle_StayToRight:
                        idleState.RotationRangeMultiplier = MathHelper.Lerp(idleState.RotationRangeMultiplier, Main.rand.NextFloat(0.4f, 0.6f), 0.5f);
                        idleState.RotationOffset = Main.rand.NextFloat(-0.2f, 0.2f);
                        idleState.Distance = Main.rand.NextFloat(400f, 550f);
                        break;
                }
            }
            Vector2 targetPositionOffset = new Vector2(0, idleState.Distance);
            targetPositionOffset = targetPositionOffset.RotatedBy(targetPositionRotation * idleState.RotationRangeMultiplier + idleState.RotationOffset);
            Vector2 targetPosition = player.Center - targetPositionOffset;
            Vector2 targetVelocity = (targetPosition - npc.Center).SafeNormalize(Vector2.Zero) * 7;
            npc.velocity = Vector2.Lerp(npc.velocity, targetVelocity, 0.05f);

            npc.rotation = npc.rotation.AngleLerp(npc.AngleTo(player.Center) - MathHelper.PiOver2, 0.1f);

            if (generalState.Timer >= 240)
            {
               Phase1_EnterAttackState(npc);
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

            generalState.Timer = 0;

            npc.TargetClosest();

            switch (definitiveIdleState)
            {
                case BehaviorType.Idle_StayOnTop:
                    idleState.RotationRangeMultiplier = Main.rand.NextFloat(0.4f, 0.8f);
                    idleState.RotationOffset = Main.rand.NextFloat(-0.2f, 0.2f);
                    idleState.Distance = Main.rand.NextFloat(300f, 450f);
                    break;
                case BehaviorType.Idle_StayToLeft:
                    idleState.RotationRangeMultiplier = Main.rand.NextFloat(0.4f, 0.6f);
                    idleState.RotationOffset = Main.rand.NextFloat(-0.7f, -0.4f);
                    idleState.Distance = Main.rand.NextFloat(400f, 550f);
                    break;
                case BehaviorType.Idle_StayToRight:
                    idleState.RotationRangeMultiplier = Main.rand.NextFloat(0.4f, 0.6f);
                    idleState.RotationOffset = Main.rand.NextFloat(0.4f, 0.7f);
                    idleState.Distance = Main.rand.NextFloat(400f, 550f);
                    break;
            }
        }
    }
}
