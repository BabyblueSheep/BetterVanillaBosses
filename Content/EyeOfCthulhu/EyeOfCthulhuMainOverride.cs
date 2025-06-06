﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BetterVanillaBosses.Content.EyeOfCthulhu;

internal sealed partial class EyeOfCthulhuBehaviorOverride : GlobalNPC
{
    public enum BehaviorType
    {
        Idle_StayOnTop,
        Idle_StayToLeft,
        Idle_StayToRight,

        Attack_BigDash,
        Attack_RapidDashes,
        Attack_SummonServants,

        Idle_Phase2Transition,
    }

    public enum StageType
    {
        Spawned,
        Phase1,
        Phase2,
    }

    public ref struct GeneralState(NPC npc)
    {
        private NPC _npc = npc;

        public BehaviorType CurrentBehaviorType
        {
            get => (BehaviorType) (int) _npc.ai[0];
            set => _npc.ai[0] = (float)value;
        }
        public ref float Timer => ref _npc.ai[1];
        public StageType CurrentStageType
        {
            get => (StageType)(int)_npc.ai[2];
            set => _npc.ai[2] = (float)value;
        }
    }

    private static class GeneralValues
    {
        public static float PercentageOfHealthForPhase2Transition => 0.75f;
    }

    private static bool IsInPhase2(NPC npc) => npc.ai[2] == (int)StageType.Phase2;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return lateInstantiation && entity.type == NPCID.EyeofCthulhu;
    }

    public override void SetDefaults(NPC entity)
    {
        entity.width = 76;
        entity.height = 76;
    }

    public override bool InstancePerEntity => true;

    public override bool PreAI(NPC npc)
    {
        GeneralState currentState = new GeneralState(npc);

        if (!npc.HasValidTarget)
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

        switch (currentState.CurrentStageType)
        {
            case StageType.Spawned:
                EnterIdleState(npc);
                currentState.CurrentStageType = StageType.Phase1;
                break;
            case StageType.Phase1:
                break;
            case StageType.Phase2:
                break;
        }

        switch (currentState.CurrentBehaviorType)
        {
            case BehaviorType.Idle_StayOnTop:
            case BehaviorType.Idle_StayToLeft:
            case BehaviorType.Idle_StayToRight:
                Idle(npc);
                break;
            case BehaviorType.Idle_Phase2Transition:
                Idle_Phase2Transition(npc);
                break;
            case BehaviorType.Attack_BigDash:
                Attack_BigDash(npc);
                break;
            case BehaviorType.Attack_RapidDashes:
                Attack_RapidDashes(npc);
                break;
            case BehaviorType.Attack_SummonServants:
                Attack_SummonServants(npc);
                break;
        }

        currentState.Timer++;

        return false;
    }

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
    {
        GeneralState generalState = new GeneralState(npc);

        switch (generalState.CurrentBehaviorType)
        {
            case BehaviorType.Attack_BigDash:
                return generalState.Timer > BigDashValues.TimeUntilDash(npc) && generalState.Timer < BigDashValues.TimeUntilPostDashSlowdown(npc);
            case BehaviorType.Attack_RapidDashes:
                return generalState.Timer > RapidDashValues.TimeUntilDash(npc) && generalState.Timer < RapidDashValues.TimeUntilPostDashSlowdown(npc);
            default:
                return false;
        }
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        binaryWriter.Write(npc.localAI[0]);
        binaryWriter.Write(npc.localAI[1]);
        binaryWriter.Write(npc.localAI[2]);
        binaryWriter.Write(npc.localAI[3]);
        binaryWriter.Write(ServantSpawnDirection.X);
        binaryWriter.Write(ServantSpawnDirection.Y);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        npc.localAI[0] = binaryReader.Read();
        npc.localAI[1] = binaryReader.Read();
        npc.localAI[2] = binaryReader.Read();
        npc.localAI[3] = binaryReader.Read();
        ServantSpawnDirection = new Vector2(binaryReader.ReadSingle(), binaryReader.ReadSingle());
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D eyeTexture = ModContent.Request<Texture2D>("BetterVanillaBosses/Assets/EyeOfCthulhu/EyeOfCthulhu").Value;

        spriteBatch.Draw(eyeTexture, npc.Center - Main.screenPosition, eyeTexture.Frame(1, 2, 0, IsInPhase2(npc) ? 1 : 0, 0, 1), npc.GetAlpha(drawColor), npc.rotation, new Vector2(eyeTexture.Width / 2f, eyeTexture.Height / 4f), 1f, SpriteEffects.None, 0f);

        return false;
    }
}