using System;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Ellipsis : Bot
{
    int turnCounter = 0;

    // Variables for locking on the nearest target.
    private bool locked = false;
    private int lockedTargetId = -1;
    private double lockedTargetX = 0;
    private double lockedTargetY = 0;
    private double lockedTargetDistance = double.MaxValue;

    // Threshold in degrees for determining if enemy is heading toward us.
    const double HeadOnThreshold = 10.0;

    public static void Main(string[] args)
    {
        new Ellipsis().Start();
    }

    public Ellipsis() : base(BotInfo.FromFile("Ellipsis.json")) { }

    public override void Run()
    {
        while (IsRunning)
        {
            // Always keep gun and radar at maximum turn rates.
            GunTurnRate = MaxGunTurnRate;
            RadarTurnRate = MaxRadarTurnRate;

            // If no target is locked, use default orbiting movement.
            if (!locked)
            {
                if (turnCounter % 64 == 0)
                {
                    TurnRate = 5;
                    TargetSpeed = MaxSpeed;
                }
                else if (turnCounter % 64 == 32)
                {
                    TargetSpeed = -MaxSpeed;
                }
                turnCounter++;
            }

            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double scannedDistance = DistanceTo(e.X, e.Y);

        // Update lock: if the scanned bot is already locked or is closer than current lock, update.
        if (locked && e.ScannedBotId == lockedTargetId)
        {
            lockedTargetX = e.X;
            lockedTargetY = e.Y;
            lockedTargetDistance = scannedDistance;
        }
        else if (!locked || scannedDistance < lockedTargetDistance)
        {
            locked = true;
            lockedTargetId = e.ScannedBotId;
            lockedTargetX = e.X;
            lockedTargetY = e.Y;
            lockedTargetDistance = scannedDistance;
        }
        double enemyToMe = Math.Atan2(X - e.X, Y - e.Y) * (180.0 / Math.PI);
        double angleDiff = NormalizeRelativeAngle(e.Direction - enemyToMe);

        // If the enemy is heading nearly directly toward us, switch to dodge mode.
        if (Math.Abs(angleDiff) < HeadOnThreshold)
        {
            Console.WriteLine("Head-on threat detected. Initiating dodge maneuver.");
            double bearingToEnemy = BearingTo(e.X, e.Y);
            double dodgeBearing = NormalizeRelativeAngle(bearingToEnemy + 90);
            TurnRate = Clamp(dodgeBearing, -MaxTurnRate, MaxTurnRate);
            TargetSpeed = MaxSpeed;
            SetFire(3);  
            return;
        }

        double bearing = BearingTo(lockedTargetX, lockedTargetY);
        double tangentBearing = bearing + 90.0;
        tangentBearing = NormalizeRelativeAngle(tangentBearing);
        TurnRate = Clamp(tangentBearing, -MaxTurnRate, MaxTurnRate);

        if (Math.Abs(TargetSpeed) < 4)
            TargetSpeed = 5;

        double gunBearing = NormalizeRelativeAngle(GunBearingTo(lockedTargetX, lockedTargetY));
        GunTurnRate = Clamp(gunBearing, -MaxGunTurnRate, MaxGunTurnRate);

        double radarBearing = NormalizeRelativeAngle(RadarBearingTo(lockedTargetX, lockedTargetY));
        RadarTurnRate = Clamp(radarBearing, -MaxRadarTurnRate, MaxRadarTurnRate);

        double firePower = (lockedTargetDistance < 150) ? 3 : 1;
        SetFire(firePower);
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        TurnRate = 5;
    }

    public override void OnHitWall(HitWallEvent e)
    {
        TurnRate = MaxTurnRate;
        TargetSpeed = -TargetSpeed;
    }

    public override void OnHitBot(HitBotEvent e)
    {
        double escapeBearing = NormalizeRelativeAngle(BearingTo(e.X, e.Y) + 180.0);
        TurnRate = Clamp(escapeBearing, -MaxTurnRate, MaxTurnRate);
        TargetSpeed = -Math.Abs(TargetSpeed);
        Console.WriteLine("Collision detected. Executing escape maneuver.");
    }

    private double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(value, max));
    }
}
