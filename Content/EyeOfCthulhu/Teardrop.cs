using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BetterVanillaBosses.Content.EyeOfCthulhu;

public class Teardrop : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 8;
        Projectile.height = 8;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.timeLeft = 180;
    }
    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
        if (!Main.dedServ)
        {
            if (Main.rand.NextBool(10))
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, 0f, 0f, Scale: Main.rand.NextFloat(0.4f, 0.8f));
            }
        }
        if (Projectile.velocity.Y > 12)
        {
            Projectile.velocity.Y = 12;
        }
        else
        {
            Projectile.velocity.Y += 0.12f;
        }
    }
    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Drip, Projectile.Center);
        for (int i = 0; i < 10; i++)
        {
            Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, 0f, 0f);
            d.velocity = new Vector2(Main.rand.Next(-4, 5), Main.rand.Next(-4, 5));
        }

    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        return Projectile.timeLeft < 175;
    }
}