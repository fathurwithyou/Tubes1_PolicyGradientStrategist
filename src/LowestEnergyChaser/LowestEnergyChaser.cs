using System;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class lowestEnergyChaser : Bot
{

  private bool locked = false;
  private int lockedTargetId = -1;
  private double lockedTargetX = 0;
  private double lockedTargetY = 0;
  private double lockedTargetEnergy = double.MaxValue;
  private double lockedTargetDistance = double.MaxValue;


  private int turnCounter = 0;
  private int lastSeenTurn = 0;

  const int LockTimeout = 10;


  const double SelfRammingThreshold = 10.0;

  const double CloseRangeDistance = 150.0;

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

      if (Energy < SelfRammingThreshold)
      {


        if (locked)
        {
          double bearingToTarget = BearingTo(lockedTargetX, lockedTargetY);
          TurnRate = Clamp(bearingToTarget, -MaxTurnRate, MaxTurnRate);
        }

        SetForward(1000);
        SetFire(3);
      }
      else
      {

        if (locked)
        {
          double bearingToTarget = BearingTo(lockedTargetX, lockedTargetY);
          TurnRate = Clamp(bearingToTarget, -MaxTurnRate, MaxTurnRate);

          double distance = lockedTargetDistance;

          if (distance > CloseRangeDistance)
          {

            double moveDistance = distance - CloseRangeDistance;
            SetForward(moveDistance);
          }
          else
          {
            SetForward(-10);
          }


          double gunBearing = NormalizeRelativeAngle(GunBearingTo(lockedTargetX, lockedTargetY));
          GunTurnRate = Clamp(gunBearing, -MaxGunTurnRate, MaxGunTurnRate);

          double radarBearing = NormalizeRelativeAngle(RadarBearingTo(lockedTargetX, lockedTargetY));
          RadarTurnRate = Clamp(radarBearing, -MaxRadarTurnRate, MaxRadarTurnRate);


          double firePower = (distance < CloseRangeDistance) ? 3 : 1;
          SetFire(firePower);
        }
        else
        {
          TurnRate = MaxTurnRate;
        }
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
      lastSeenTurn = turnCounter;
    }
    else
    {

      if (!locked || enemyEnergy < lockedTargetEnergy)
      {
        locked = true;
        lockedTargetId = e.ScannedBotId;
        lockedTargetX = e.X;
        lockedTargetY = e.Y;
        lockedTargetDistance = scannedDistance;
        lockedTargetEnergy = enemyEnergy;
        lastSeenTurn = turnCounter;
      }
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

    double escapeBearing = NormalizeRelativeAngle(BearingTo(e.X, e.Y) + 180.0);
    TurnRate = Clamp(escapeBearing, -MaxTurnRate, MaxTurnRate);
    SetForward(-50);
  }

  private double Clamp(double value, double min, double max)
  {
    return Math.Max(min, Math.Min(value, max));
  }
}