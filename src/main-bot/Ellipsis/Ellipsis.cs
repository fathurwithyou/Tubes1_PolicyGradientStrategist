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
    private double lockedTargetSpeed = 0;
    private double lockedTargetDirection = 0;
    private double lockedTargetDistance = double.MaxValue;

    public static void Main(string[] args)
    {
        new Ellipsis().Start();
    }

    public Ellipsis() : base(BotInfo.FromFile("Ellipsis.json")) { }

    public override void Run()
    {
        while (IsRunning)
        {
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

    private double[] predictPosition()
    {
        double[] position = new double[2];
        position[0] = lockedTargetX + lockedTargetSpeed * Math.Sin(lockedTargetDirection);
        position[1] = lockedTargetY + lockedTargetSpeed * Math.Cos(lockedTargetDirection);
        return position;
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
            lockedTargetSpeed = e.Speed;
            lockedTargetDirection = e.Direction;
        }
        else if (!locked || scannedDistance < lockedTargetDistance)
        {
            locked = true;
            lockedTargetId = e.ScannedBotId;
            lockedTargetX = e.X;
            lockedTargetY = e.Y;
            lockedTargetDistance = scannedDistance;
            lockedTargetSpeed = e.Speed;
            lockedTargetDirection = e.Direction;
        }

        double[] pos = predictPosition();

        double bearing = BearingTo(lockedTargetX, lockedTargetY);
        double tangentBearing = bearing + 90.0;
        tangentBearing = NormalizeRelativeAngle(tangentBearing);
        TurnRate = Clamp(tangentBearing, -MaxTurnRate, MaxTurnRate);

        if (Math.Abs(TargetSpeed) < 4)
            TargetSpeed = 15;

        double gunBearing = NormalizeRelativeAngle(GunBearingTo(pos[0], pos[1]));
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
        TargetSpeed = -TargetSpeed;
    }

    public override void OnHitBot(HitBotEvent e)
    {
        TargetSpeed = -Math.Abs(TargetSpeed);
        Console.WriteLine("Collision detected. Executing escape maneuver.");
    }

    private double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(value, max));
    }
}
