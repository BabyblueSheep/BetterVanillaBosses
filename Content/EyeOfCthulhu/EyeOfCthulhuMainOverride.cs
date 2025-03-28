using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BetterVanillaBosses.Content.EyeOfCthulhu;

internal sealed partial class EyeOfCthulhuBehaviorOverride : GlobalNPC
{
    public enum BehaviorType
    {
        Idle_StayOnTop,
        Idle_StayToLeft,
        Idle_StayToRight,

        Phase1_Attack_BigCharge,
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
    }

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return lateInstantiation && entity.type == NPCID.EyeofCthulhu;
    }

    public override void SpawnNPC(int npc, int tileX, int tileY)
    {
        base.SpawnNPC(npc, tileX, tileY);
    }

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

        switch (currentState.CurrentBehaviorType)
        {
            case BehaviorType.Idle_StayOnTop:
            case BehaviorType.Idle_StayToLeft:
            case BehaviorType.Idle_StayToRight:
                Idle(npc);
                break;
            case BehaviorType.Phase1_Attack_BigCharge:
                Phase1_Attack_BigCharge(npc);
                break;
        }

        currentState.Timer++;

        return false;
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D eyeTexture = ModContent.Request<Texture2D>("BetterVanillaBosses/Assets/EyeOfCthulhu/EyeOfCthulhu").Value;

        spriteBatch.Draw(eyeTexture, npc.Center - Main.screenPosition, eyeTexture.Frame(), npc.GetAlpha(drawColor), npc.rotation, eyeTexture.Size() / 2, 1f, SpriteEffects.None, 0f);

        return false;
    }
}