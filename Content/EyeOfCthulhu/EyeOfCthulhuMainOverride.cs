using Microsoft.Xna.Framework;
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
    }

    public enum StageType
    {
        Spawned,
        Phase1,
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

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return lateInstantiation && entity.type == NPCID.EyeofCthulhu;
    }

    public override bool InstancePerEntity => true;

    public override bool PreAI(NPC npc)
    {
        GeneralState currentState = new GeneralState(npc);

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

        if (currentState.CurrentStageType == StageType.Spawned)
        {
            EnterIdleState(npc);
            currentState.CurrentStageType = StageType.Phase1;
        }

        switch (currentState.CurrentBehaviorType)
        {
            case BehaviorType.Idle_StayOnTop:
            case BehaviorType.Idle_StayToLeft:
            case BehaviorType.Idle_StayToRight:
                Idle(npc);
                break;
            case BehaviorType.Attack_BigDash:
                Attack_BigDash(npc);
                break;
            case BehaviorType.Attack_RapidDashes:
                Attack_RapidDashes(npc);
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
            case BehaviorType.Idle_StayOnTop:
            case BehaviorType.Idle_StayToLeft:
            case BehaviorType.Idle_StayToRight:
                return false;
            default:
                return true;
        }
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        binaryWriter.Write(npc.localAI[0]);
        binaryWriter.Write(npc.localAI[1]);
        binaryWriter.Write(npc.localAI[2]);
        binaryWriter.Write(npc.localAI[3]);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        npc.localAI[0] = binaryReader.Read();
        npc.localAI[1] = binaryReader.Read();
        npc.localAI[2] = binaryReader.Read();
        npc.localAI[3] = binaryReader.Read();
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D eyeTexture = ModContent.Request<Texture2D>("BetterVanillaBosses/Assets/EyeOfCthulhu/EyeOfCthulhu").Value;

        spriteBatch.Draw(eyeTexture, npc.Center - Main.screenPosition, eyeTexture.Frame(), npc.GetAlpha(drawColor), npc.rotation, eyeTexture.Size() / 2, 1f, SpriteEffects.None, 0f);

        return false;
    }
}