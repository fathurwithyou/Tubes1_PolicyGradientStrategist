using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Hebi : Bot
{
    double[] enemy = new double[2];
    double lastDirection;
    bool changeDirection;
    int turnsSinceShot=0;
    static void Main(string[] args)
    {
        new Hebi().Start();
    }
    Hebi() : base(BotInfo.FromFile("Hebi.json")) { }
    public override void Run()
    {
        BodyColor = Color.FromArgb(0x60, 0x5e, 0xa1);   
        TurretColor = Color.FromArgb(0x29, 0xb9, 0xdb); // Light Blue
        RadarColor = Color.FromArgb(0x1d, 0x1b, 0x43);  // Dark Blue
        BulletColor = Color.FromArgb(0x29, 0x27, 0x26); // Black
        ScanColor = Color.FromArgb(0x8c, 0x4f, 0x61);   // Blue
        TracksColor = Color.FromArgb(0x6e, 0x6a, 0x68); // Dark   
        GunColor = Color.FromArgb(0x6e, 0x6a, 0x68);   
        // initialize variables
        GunTurnRate = MaxGunTurnRate;
        RadarTurnRate = MaxRadarTurnRate;
        // center of the arena
        enemy = new double[2]{ArenaWidth/2, ArenaHeight/2};
        // decouple components
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        AdjustRadarForBodyTurn = true;
        int turnDirection = SideAngle(RadarBearingTo(enemy[0], enemy[1]));
        SetTurnRadarLeft(360 * turnDirection);
        SetTurnGunLeft(GunBearingTo(enemy[0], enemy[1]));
        while (IsRunning){
            turnsSinceShot++;
            double angle = BearingTo(enemy[0], enemy[1]);
            turnDirection = 1;
            if(turnDirection!=lastDirection){
                changeDirection = true;
            }
            if(changeDirection && Math.Abs(Speed)>=2){
                // turning is faster on low speed, see docs
                // so this can snap the bot to the other direction
                MaxSpeed = 0.0001;
            }
            else{
                MaxSpeed = 9;
                changeDirection = false;
            }
            lastDirection = turnDirection;
            SetForward(999*turnDirection);
            if (turnsSinceShot > 10){
                SetTurnRadarLeft(360 * turnDirection);
            }else{
                SetTurnLeft(angle);
            }
            SetRescan();
            Go();
        }
    }
    public override void OnScannedBot(ScannedBotEvent e)
    {
        turnsSinceShot = 0;
        double angleToEnemy = BearingTo(e.X, e.Y);
        SetTurnRadarLeft(RadarBearingTo(e.X, e.Y));
        double[] preds = new double[2]{e.X, e.Y};
        double deltaTime = 0;
        double currFirepower;
        if (DistanceTo(e.X, e.Y) < 100)
        {
            currFirepower = 3;
        }
        else if (DistanceTo(e.X, e.Y) < 200)
        {
            currFirepower = 2;
        }
        else
        {
            currFirepower = 1.1;
        }
        // Shot prediction
        double bulletSpeed = CalcBulletSpeed(currFirepower);
        while((++deltaTime)*bulletSpeed < DistanceTo(preds[0], preds[1])){
            preds[0] += Math.Sin(e.Direction) * e.Speed;
            preds[1] += Math.Cos(e.Direction) * e.Speed;
            // check if the bot is out of bounds
            if(preds[0] < 18 || preds[1] < 18 || preds[0] > ArenaWidth-18 || preds[1] > ArenaHeight-18){
                preds[0] = Math.Min(Math.Max(18, preds[0]), ArenaWidth-18);
                preds[1] = Math.Min(Math.Max(18, preds[1]), ArenaHeight-18);
                break;
            }
        }
        SetTurnGunLeft(GunBearingTo(preds[0], preds[1]));
        SetFire(currFirepower);
        Array.Copy(preds, enemy, 2);

    }
    // side check 1 for Left, -1 for Right
    private int SideAngle(double angle){
        return angle > 0 ? 1 : -1;
    }
}
