using System;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class lowestEnergyChaser : Bot
{
  private bool locked = false;
  private int lockedTargetId = -1;
  private double lockedTargetX = 0, lockedTargetY = 0;
  private double lockedTargetEnergy = double.MaxValue;
  private double lockedTargetDistance = double.MaxValue;
  private double lockedTargetVelocity = 0, lockedTargetHeading = 0;

  private int turnCounter = 0, lastSeenTurn = 0;

  const int LockTimeout = 10;
  const double CloseRangeDistance = 150.0;
  const double EnemyRammingThreshold = 20.0;
  const double BulletSpeedFactor = 20.0;

  public static void Main(string[] args)
  {
    new lowestEnergyChaser().Start();
  }

  public lowestEnergyChaser() : base(BotInfo.FromFile("lowestEnergyChaser.json")) { }

  public override void Run()
  {
    while (IsRunning)
    {
      turnCounter++;

      if (locked && (turnCounter - lastSeenTurn > LockTimeout))
      {
        locked = false;
        lockedTargetId = -1;
      }

      GunTurnRate = MaxGunTurnRate;
      RadarTurnRate = MaxRadarTurnRate;

      if (locked)
      {
        double bearingToTarget = BearingTo(lockedTargetX, lockedTargetY);
        TurnRate = Clamp(bearingToTarget, -MaxTurnRate, MaxTurnRate);

        if (lockedTargetEnergy < EnemyRammingThreshold)
        {
          SetForward(1000);
        }
        else
        {
          double distance = lockedTargetDistance;
          double moveDistance = (distance > CloseRangeDistance) ? distance - CloseRangeDistance : -10;
          SetForward(moveDistance);
        }


        double predictedX, predictedY;
        PredictEnemyPosition(out predictedX, out predictedY);

        double gunBearing = NormalizeRelativeAngle(GunBearingTo(predictedX, predictedY));
        GunTurnRate = Clamp(gunBearing, -MaxGunTurnRate, MaxGunTurnRate);

        double radarBearing = NormalizeRelativeAngle(RadarBearingTo(lockedTargetX, lockedTargetY));
        RadarTurnRate = Clamp(radarBearing, -MaxRadarTurnRate, MaxRadarTurnRate);

        double firePower = (lockedTargetEnergy < EnemyRammingThreshold) ? 3 : 1;
        SetFire(firePower);
      }
      else
      {
        TurnRate = MaxTurnRate;
      }

      Go();
    }
  }

  public override void OnScannedBot(ScannedBotEvent e)
  {
    double scannedDistance = DistanceTo(e.X, e.Y);
    double enemyEnergy = e.Energy;

    if (locked && e.ScannedBotId == lockedTargetId)
    {
      lockedTargetX = e.X;
      lockedTargetY = e.Y;
      lockedTargetDistance = scannedDistance;
      lockedTargetEnergy = enemyEnergy;
      lockedTargetVelocity = e.Speed;
      lockedTargetHeading = e.Direction;
      lastSeenTurn = turnCounter;
    }
    else if (!locked || enemyEnergy < lockedTargetEnergy)
    {
      locked = true;
      lockedTargetId = e.ScannedBotId;
      lockedTargetX = e.X;
      lockedTargetY = e.Y;
      lockedTargetDistance = scannedDistance;
      lockedTargetEnergy = enemyEnergy;
      lockedTargetVelocity = e.Speed;
      lockedTargetHeading = e.Direction;
      lastSeenTurn = turnCounter;
    }
  }

  public override void OnHitByBullet(HitByBulletEvent e)
  {
    TurnRate = 5;
  }

  public override void OnHitWall(HitWallEvent e)
  {
    SetForward(-100);
  }

  public override void OnHitBot(HitBotEvent e)
  {
    if (locked && e.VictimId == lockedTargetId && lockedTargetEnergy < EnemyRammingThreshold)
    {
      SetForward(50);
    }
    else
    {
      double escapeBearing = NormalizeRelativeAngle(BearingTo(e.X, e.Y) + 180.0);
      TurnRate = Clamp(escapeBearing, -MaxTurnRate, MaxTurnRate);
      SetForward(-50);
    }
  }


  private void PredictEnemyPosition(out double predictedX, out double predictedY)
  {
    double bulletSpeed = BulletSpeedFactor - (3 * 3);
    double timeToImpact = lockedTargetDistance / bulletSpeed;

    predictedX = lockedTargetX + (lockedTargetVelocity * timeToImpact * Math.Cos(DegreesToRadians(lockedTargetHeading)));
    predictedY = lockedTargetY + (lockedTargetVelocity * timeToImpact * Math.Sin(DegreesToRadians(lockedTargetHeading)));

    predictedX = Clamp(predictedX, 0, ArenaWidth);
    predictedY = Clamp(predictedY, 0, ArenaHeight);
  }

  private double DegreesToRadians(double degrees)
  {
    return degrees * (Math.PI / 180);
  }

  private double Clamp(double value, double min, double max)
  {
    return Math.Max(min, Math.Min(value, max));
  }
}
