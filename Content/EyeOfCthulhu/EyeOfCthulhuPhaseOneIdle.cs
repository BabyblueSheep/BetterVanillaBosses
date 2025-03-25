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
        private void Phase1_Idle(NPC npc)
        {
            EyeOfCthulhuState currentState = (EyeOfCthulhuState)(int)npc.ai[0];
            ref float generalTimer = ref npc.ai[1];

            switch (currentState)
            {
                case EyeOfCthulhuState.Phase1_Idle_SwayAroundPlayer:
                    Phase1_Idle_SwayAroundPlayer(npc);
                    break;
                case EyeOfCthulhuState.Phase1_Idle_GetCloseToPlayer:
                    Phase1_Idle_GetCloseToPlayer(npc);
                    break;
                case EyeOfCthulhuState.Phase1_Idle_GetFarFromPlayer:
                    Phase1_Idle_GetFarFromPlayer(npc);
                    break;
            }

            if (generalTimer >= 240)
            {
               Phase1_EnterAttackState(npc);
            }
        }

        private void Phase1_EnterIdleState(NPC npc)
        {
            ref float generalTimer = ref npc.ai[1];

            WeightedRandom<EyeOfCthulhuState> randomIdleState = new WeightedRandom<EyeOfCthulhuState>(Main.rand);
            randomIdleState.Add(EyeOfCthulhuState.Phase1_Idle_SwayAroundPlayer, 1.5f);
            randomIdleState.Add(EyeOfCthulhuState.Phase1_Idle_GetFarFromPlayer, 1f);
            randomIdleState.Add(EyeOfCthulhuState.Phase1_Idle_GetCloseToPlayer, 0.5f);
            EyeOfCthulhuState definitiveIdleState = randomIdleState.Get();
            npc.ai[0] = (int)definitiveIdleState;

            generalTimer = 0;

            npc.TargetClosest();

            switch (definitiveIdleState)
            {
                case EyeOfCthulhuState.Phase1_Idle_SwayAroundPlayer:
                    float angleToTarget = (Main.player[npc.target].Center - npc.Center).ToRotation();

                    npc.localAI[1] = angleToTarget;
                    npc.localAI[2] = Main.rand.NextFloat(0.6f, 0.8f);
                    break;
                case EyeOfCthulhuState.Phase1_Idle_GetCloseToPlayer:
                    npc.localAI[1] = Main.rand.NextFloat(MathHelper.TwoPi);
                    break;
                case EyeOfCthulhuState.Phase1_Idle_GetFarFromPlayer:
                    npc.localAI[1] = Main.rand.NextFloat(MathHelper.TwoPi);
                    break;
            }
        }

        private void Phase1_Idle_SwayAroundPlayer(NPC npc)
        {
            ref float swayingTimer = ref npc.ai[1];
            ref float rotationOffset = ref npc.localAI[1];
            ref float rotationMult = ref npc.localAI[2];

            Player player = Main.player[npc.target];

            float targetPositionRotation = MathF.Sin(swayingTimer * 0.02f);
            if (swayingTimer % 80 == 0 && swayingTimer % 160 != 0)
            {
                rotationOffset += Main.rand.NextFloat(-0.6f, 0.6f);
                rotationMult = MathHelper.Lerp(rotationMult, Main.rand.NextFloat(0.6f, 0.8f), 0.5f);
            }
            Vector2 targetPositionOffset = new Vector2(0, 400f * rotationMult);
            targetPositionOffset = targetPositionOffset.RotatedBy(targetPositionRotation * rotationMult + rotationOffset);
            Vector2 targetPosition = player.Center - targetPositionOffset;
            Vector2 targetVelocity = (targetPosition - npc.Center).SafeNormalize(Vector2.Zero) * 7;
            npc.velocity = Vector2.Lerp(npc.velocity, targetVelocity, 0.05f);

            npc.rotation = npc.rotation.AngleLerp(npc.AngleTo(player.Center) - MathHelper.PiOver2, 0.1f);
        }

        private void Phase1_Idle_GetCloseToPlayer(NPC npc)
        {
            ref float rotationOffset = ref npc.localAI[1];

            Player player = Main.player[npc.target];

            Vector2 targetPosition = player.Center - new Vector2(0, 150f).RotatedBy(rotationOffset);
            Vector2 difference = targetPosition - npc.Center;
            Vector2 targetVelocity = difference.SafeNormalize(Vector2.Zero) * Utils.Remap(difference.Length(), 250f, 1000f, 20f, 5f);
            npc.velocity = Vector2.Lerp(npc.velocity, targetVelocity, 0.02f);

            npc.rotation = npc.rotation.AngleLerp(npc.AngleTo(player.Center) - MathHelper.PiOver2, 0.05f);
        }

        private void Phase1_Idle_GetFarFromPlayer(NPC npc)
        {
            ref float rotationOffset = ref npc.localAI[1];

            Player player = Main.player[npc.target];

            Vector2 targetPosition = player.Center - new Vector2(0, 500f).RotatedBy(rotationOffset);
            Vector2 difference = targetPosition - npc.Center;
            Vector2 targetVelocity = difference.SafeNormalize(Vector2.Zero) * Utils.Remap(difference.Length(), 1000f, 5000f, 7f, 4f);
            npc.velocity = Vector2.Lerp(npc.velocity, targetVelocity, 0.02f);

            npc.rotation = npc.rotation.AngleLerp(npc.AngleTo(player.Center) - MathHelper.PiOver2, 0.2f);
        }
    }
}
