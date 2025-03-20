using System;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Ellipsis : Bot
{
    int turnCounter = 0;

    public static void Main(string[] args)
    {
        new Ellipsis().Start();
    }

    public Ellipsis() : base(BotInfo.FromFile("Ellipsis.json")) { }

    public override void Run()
    {
        while (IsRunning)
        {
            GunTurnRate = MaxGunTurnRate;
            RadarTurnRate = MaxRadarTurnRate;
            if (turnCounter % 64 == 0)
            {
                TurnRate = 0;
                TargetSpeed = 4;
            }
            else if (turnCounter % 64 == 32)
            {
                TargetSpeed = -6;
            }

            turnCounter++;
            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // 1. Compute direct bearing to the enemy
        double bearing = BearingTo(e.X, e.Y);

        // 2. Offset by 90 deg to move tangentially (orbit around them)
        double tangentBearing = bearing + 90.0;
        tangentBearing = NormalizeRelativeAngle(tangentBearing);
        tangentBearing = Clamp(tangentBearing, -MaxTurnRate, MaxTurnRate);

        // Set turn rate to orbit around the enemy
        TurnRate = tangentBearing;

        if (Math.Abs(TargetSpeed) < 4)
            TargetSpeed = 5;

        // 3. Keep the GUN locked on the enemy
        double gunBearing = GunBearingTo(e.X, e.Y);
        gunBearing = NormalizeRelativeAngle(gunBearing);
        gunBearing = Clamp(gunBearing, -MaxGunTurnRate, MaxGunTurnRate);
        GunTurnRate = gunBearing;

        // 4. Keep the RADAR locked on the enemy
        double radarBearing = RadarBearingTo(e.X, e.Y);
        radarBearing = NormalizeRelativeAngle(radarBearing);
        radarBearing = Clamp(radarBearing, -MaxRadarTurnRate, MaxRadarTurnRate);
        RadarTurnRate = radarBearing;

        // 5. Fire if close enough and gun is cooled down
        double distance = DistanceTo(e.X, e.Y);
        double firePower;
        if (distance < 50)
        {
            firePower = 3;
        }
        else if (distance < 100)
        {
            firePower = 2;
        }
        else
        {
            firePower = 1;
        }
        if (GunHeat == 0)
            Fire(firePower);

    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        TurnRate = 5;
    }

    public override void OnHitWall(HitWallEvent e)
    {
        TargetSpeed = -TargetSpeed;
    }

    // Called when we collide with another bot
    public override void OnHitBot(HitBotEvent e)
    {
        // If we are the one ramming, attempt an escape
        if (e.IsRammed)
        {
            // TurnRate = MaxTurnRate;
            // TargetSpeed = -10;
        }
    }

    private double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(value, max));
    }
}
