using System;
using System.Collections;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Kawakaze : Bot
{
    public class enemyBot{
        public double[] position=new double[2];
        public double direction;
        public double speed;
        public bool isAlive;
        public double energy;
    }
    public Random random = new Random();
    public Hashtable enemies = new Hashtable();
    public enemyBot target;
    public double[] nextSpot = new double[2];
    public double[] lastSpot = new double[2];
    const double lockedOnThreshold = 50.0;
    public bool isLockedOn = false;
    static void Main(string[] args)
    {
        new Kawakaze().Start();
    }
    Kawakaze() : base(BotInfo.FromFile("Kawakaze.json")) { }
    public override void Run()
    {
        BodyColor = Color.FromArgb(0xe0, 0xde, 0xe1);   // White
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
        // decouple components
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        AdjustRadarForBodyTurn = true;
        SetTurnRadarRight(double.MaxValue);
        Array.Copy(new double[2]{X, Y}, nextSpot, 2);
        Array.Copy(new double[2]{X, Y}, lastSpot, 2);
        target = new enemyBot();
        while (IsRunning){
            if (DistanceTo(target.position[0], target.position[1])<lockedOnThreshold){
                isLockedOn = true;
            }
            if (!isLockedOn && target.isAlive && TurnNumber>10){
                Operations();
            }
            Go();
        }
    }
    public void Operations(){
        // Gun Operations
        double currFire;
        double dis = DistanceTo(target.position[0], target.position[1]);
        if(Energy>1){
            currFire = Math.Min(Math.Min(Energy/6d, 300d/dis), target.energy/3d);
            Shooting(currFire);
        }
        double nextDis = DistanceTo(nextSpot[0], nextSpot[1]);
        if (nextDis<16){
            // Remaining bots modifier, more will lead to 1
            double modifier = 1 - random.Next((int)Math.Pow(random.NextDouble(), EnemyCount));
            double[] testSpot = new double[2];
            for(int i =0;i<200;i++){
                double testAngle = random.NextDouble()*2*Math.PI;
                double testDis = random.NextDouble()*200;
                testSpot[0] = X+Math.Min(dis*0.8,100+testDis)*Math.Sin(testAngle);
                testSpot[1] = Y+Math.Min(dis*0.8,100+testDis)*Math.Cos(testAngle);
                // boundary checking
                if (testSpot[0]>30 && testSpot[0]<ArenaWidth-30 && testSpot[1]>30 && testSpot[1]<ArenaHeight-30){
                    // lower risk is better
                    if(RiskScoring(testSpot, modifier)<RiskScoring(nextSpot, modifier)){
                        Array.Copy(testSpot, nextSpot, 2);
                    }
                }
            }
            lastSpot[0] = X;
            lastSpot[1] = Y;
        }
        else{
            
            double angle = BearingTo(nextSpot[0], nextSpot[1]);
            double frontBack = SideAngle(angle);
            if(frontBack==1){
                SetTurnLeft(angle);
            }
            else{
                // reverse
                SetTurnRight(NormalizeRelativeAngle(angle+=180));
            }
            SetForward(999*frontBack);
            // slow down to turn trick
            MaxSpeed = Math.Abs(angle)>1? 0.01 : 8d;
            Console.WriteLine("current "+X+" "+Y);
            Console.WriteLine("moving to "+nextSpot[0]+" "+nextSpot[1]);
            Console.WriteLine("angle "+angle);
            Console.WriteLine("speed "+Speed);
            Console.WriteLine("frontback"+frontBack);
        }

    }
    public void Shooting(double currFirepower){
        double deltaTime = 0;
        double bulletSpeed = CalcBulletSpeed(currFirepower);
        double[] preds = new double[2]{target.position[0], target.position[1]};
        while((++deltaTime)*bulletSpeed < DistanceTo(preds[0], preds[1])){
            preds[0] += Math.Sin(target.direction) * target.speed;
            preds[1] += Math.Cos(target.direction) * target.speed;
            // check if the bot is out of bounds
            if(preds[0] < 18 || preds[1] < 18 || preds[0] > ArenaWidth-18 || preds[1] > ArenaHeight-18){
                preds[0] = Math.Min(Math.Max(18, preds[0]), ArenaWidth-18);
                preds[1] = Math.Min(Math.Max(18, preds[1]), ArenaHeight-18);
                break;
            }
        }
        SetTurnGunLeft(GunBearingTo(preds[0], preds[1]));
        SetFire(currFirepower);
    }
    public double RiskScoring(double[] p, double modifier){
        // inverse square risk scoring
        double risk = modifier*0.08/ Math.Pow(CalcDistance(p[0],p[1],lastSpot[0],lastSpot[1]),2);
        foreach(enemyBot enemy in enemies.Values){
            if(enemy.isAlive){
                // Enemy energy compared to ours
                risk += Math.Min(enemy.energy/Energy,2)*
                // Enemy ease of turn compared to ours from the point
                // if we both can turn easily, Cos will go to 1 (high risk)
                // if we can turn easily and enemy can't, Cos will go to 0 (low risk)
                (1+Math.Abs(Math.Cos(
                    CalcBearingOf(p[0],p[1],X,Y)-
                    CalcBearingOf(p[0], p[1],enemy.position[0],enemy.position[1])
                    ))
                );
                
            }
        }
        return risk;
    }
    public void RamEvasion(){
        double[] pos = predictPosition();

        double bearing = BearingTo(target.position[0], target.position[1]);
        double tangentBearing = bearing + 90.0;
        tangentBearing = NormalizeRelativeAngle(tangentBearing);
        TurnRate = Clamp(tangentBearing, -MaxTurnRate, MaxTurnRate);

        if (Math.Abs(TargetSpeed) < 4)
            TargetSpeed = 15;

        double gunBearing = NormalizeRelativeAngle(GunBearingTo(pos[0], pos[1]));
        GunTurnRate = Clamp(gunBearing, -MaxGunTurnRate, MaxGunTurnRate);

        double radarBearing = NormalizeRelativeAngle(RadarBearingTo(target.position[0], target.position[1]));
        RadarTurnRate = Clamp(radarBearing, -MaxRadarTurnRate, MaxRadarTurnRate);

        double firePower = (DistanceTo(target.position[0],target.position[1]) < 150) ? 3 : 1;
        SetFire(firePower);
    }
    private double[] predictPosition()
    {
        double[] position = new double[2];
        position[0] = target.position[0] + target.speed * Math.Sin(target.direction);
        position[1] = target.position[1] + target.speed * Math.Cos(target.direction);
        return position;
    }
    private double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(value, max));
    }
    public override void OnScannedBot(ScannedBotEvent e)
    {
        enemyBot enemy = (enemyBot)enemies[e.ScannedBotId];
        if (enemy==null){
            enemy = new enemyBot();
            enemies.Add(e.ScannedBotId, enemy);
        }
        enemy.energy = e.Energy;
        enemy.direction = e.Direction;
        enemy.speed = e.Speed;
        enemy.isAlive = true;
        enemy.position[0] = e.X;
        enemy.position[1] = e.Y;
        if (DistanceTo(e.X, e.Y)<lockedOnThreshold){
            isLockedOn = true;
            RamEvasion();
        }
        // lock the closest enemy
        if(!target.isAlive || DistanceTo(e.X, e.Y)<DistanceTo(target.position[0], target.position[1])){
            target = enemy;
        }
        if(EnemyCount==1){
            // infinity radar lock
            SetTurnRadarLeft(RadarTurnRemaining);
        }
    }
    public override void OnHitWall(HitWallEvent e){
        // reverse
        SetTurnRight(NormalizeRelativeAngle(BearingTo(ArenaWidth/2, ArenaHeight/2)+180));
        SetForward(999);
    }
    public override void OnHitBot(HitBotEvent botHitBotEvent)
    {
        RamEvasion();
    }
    public override void OnBotDeath(BotDeathEvent e){
        ((enemyBot)enemies[e.VictimId]).isAlive = false;
    }
    /// <summary>
    /// Relative Bearing between point2 and point1
    /// </summary>
    private double CalcBearingOf(double x1, double y1, double x2, double y2){
        return NormalizeRelativeAngle(NormalizeAbsoluteAngle(180*Math.Atan2(x2-x1,y2-y1)/Math.PI));
    }
    // Custom Distance between two positions
    private double CalcDistance(double x1, double y1, double x2, double y2){
        return Math.Sqrt(Math.Pow(x1-x2, 2) + Math.Pow(y1-y2, 2));
    }
    // side check 1 for Front, -1 for Back
    private int SideAngle(double angle){
        return angle< -90 || angle>90 ? -1 : 1;
    }
}
