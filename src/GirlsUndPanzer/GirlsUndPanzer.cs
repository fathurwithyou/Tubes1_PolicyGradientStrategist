using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class GirlsUndPanzer : Bot
{
    private class LockedBot
    {
        public double Id{ get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double LastX { get; set; }
        public double LastY { get; set; }
        public double Speed { get; set; }
        public double Direction { get; set; }
        public double Energy { get; set; }
    }
    private string mode = "search";

    private int snakeStep = 0;
    private int turnDirection = 1; // clockwise (-1) or counterclockwise (1)
    
    private LockedBot lockedBot = new LockedBot();
    // The main method starts our bot
    static void Main(string[] args)
    {
        new GirlsUndPanzer().Start();
    }

    // Constructor, which loads the bot config file
    GirlsUndPanzer() : base(BotInfo.FromFile("GirlsUndPanzer.json")) { }

    // Called when a new round is started -> initialize and do some movement
    public override void Run()
    {

        BodyColor = Color.FromArgb(0xF0, 0xF0, 0xF0);   // White
        TurretColor = Color.FromArgb(0x59, 0xb3, 0x5f); // Green
        RadarColor = Color.FromArgb(0xa2, 0xf5, 0xa7);  // Light Green 
        BulletColor = Color.FromArgb(0x29, 0x27, 0x26); // Black
        ScanColor = Color.FromArgb(0xa2, 0xf5, 0xa7);   // Light Green
        TracksColor = Color.FromArgb(0x6e, 0x6a, 0x68); // Dark   
        GunColor = Color.FromArgb(0x6e, 0x6a, 0x68);    // Dark
      
        // Repeat while the bot is running
        while (IsRunning)
        {
            if(mode=="search"){
                SetTurnRadarLeft(360);
                Search(); 
            }
            else if(mode=="locked"){
                // Get Closer to the target
                LockIn();
            }
            else if(mode=="kill"){
                // Kill when close distance or no energy
                Kill();
            }
            Console.WriteLine(mode);
            Go(); // Execute the current commands
        }
    }
    public override void OnRoundStarted(RoundStartedEvent roundStartedEvent)
    {
        mode = "search";
        lockedBot.X = 0;
        lockedBot.Y = 0;
        lockedBot.LastX = 0;
        lockedBot.LastY = 0;
        lockedBot.Speed = 0;
        lockedBot.Direction = 0;
        lockedBot.Energy = 0;
        turnDirection =1;
        snakeStep = 0;
    }

    // We saw another bot -> fire!
    public override void OnScannedBot(ScannedBotEvent e)
    {
        if(mode.Equals("search"))
        {
            lockedBot.X = e.X;
            lockedBot.Y = e.Y;
            lockedBot.Speed = e.Speed;
            lockedBot.Direction = e.Direction;
            lockedBot.Energy = e.Energy;
            mode = "locked";
        }
        else{
            var distance = DistanceTo(e.X, e.Y);
            if (distance < 100 || e.Energy == 0)
            {
                mode = "kill";
            }
            else
            {
                mode = "locked";
            }
            lockedBot.LastX = lockedBot.X;
            lockedBot.LastY = lockedBot.Y;
            lockedBot.X = e.X;
            lockedBot.Y = e.Y;
            lockedBot.Speed = e.Speed;
            lockedBot.Direction = e.Direction;
            lockedBot.Energy = e.Energy;
        }
        double[] pos = PredictPosition();
        SetTurnRadarLeft(RadarBearingTo(pos[0], pos[1]));
        SetTurnGunLeft(GunBearingTo(pos[0], pos[1]));
        if (DistanceTo(lockedBot.X, lockedBot.Y) < 100)
        {
            SetFire(3);
        }
        else if (DistanceTo(lockedBot.X, lockedBot.Y) < 200)
        {
            SetFire(2);
        }
        else
        {
            SetFire(1);
        }
    }
    public override void OnHitBot(HitBotEvent e)
    {
        // Determine a shot that won't kill the bot...
        // We want to ram it instead for bonus points
        if (e.Energy > 16)
            SetFire(3);
        else if (e.Energy > 10)
            SetFire(2);
        else if (e.Energy > 4)
            SetFire(1);
        else if (e.Energy > 2)
            SetFire(.5);
        else if (e.Energy > .4)
            SetFire(.1);
        
    }
    // We were hit by a bullet -> turn perpendicular to the bullet
    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        // Calculate the bearing to the direction of the bullet

        // Turn 90 degrees to the bullet direction based on the bearing
        
    }
    private void TurnToFaceTarget(double x, double y)
    {
        var bearing = BearingTo(x, y);
        SetTurnLeft(bearing);
    }
    private void LockIn(){
        Console.WriteLine("ターゲットをロック");
        double[] pos = PredictPosition();
        SnakeMove(pos[0], pos[1]);
        SetTurnRadarLeft(RadarBearingTo(pos[0], pos[1]));
        SetRescan();
    }
    private void Kill(){
        Console.WriteLine("全部殺す");
        TurnToFaceTarget(lockedBot.X, lockedBot.Y);
        var distance = DistanceTo(lockedBot.X, lockedBot.Y);
        SetForward(distance + 5);
        SetRescan();
    }
    private void SnakeMove(double x, double y)
    {
        Console.WriteLine("蛇のように這い");
        Console.WriteLine("step: "+snakeStep);
        double amplitude = DistanceTo(x, y) / 4;
        if(snakeStep >= 300){
            turnDirection *= -1;
            snakeStep = 0;
        }
        
        double targetDirection = Direction + 45 * turnDirection;
        double[] targetPos = new double[2];
        targetPos[0] = x + amplitude/2 * Math.Sin(targetDirection);
        targetPos[1] = y + amplitude/2 * Math.Cos(targetDirection);
        if(targetPos[0] < 10){
            targetPos[0] = 10;
        }else if(targetPos[0] > ArenaWidth-10){
            targetPos[0] = ArenaWidth-10;
        }
        if(targetPos[1] < 10){
            targetPos[1] = 10;
        }else if(targetPos[1] > ArenaHeight-10){
            targetPos[1] = ArenaHeight-10;
        }
        double driveDistance = DistanceTo(targetPos[0], targetPos[1]);
        TurnToFaceTarget(targetPos[0], targetPos[1]);
        SetForward(driveDistance);
        snakeStep++;
    }
    private void Search()
    {
        // Circle around the battlefield
        SetTurnRight(90);
        SetForward(100);
        Console.WriteLine("敵を探しています");
    }
    private double[] PredictPosition(string type = "linear")
    {
        double[] pos = new double[2];
        if (type.Equals("linear")){
            pos[0] = lockedBot.X + lockedBot.Speed * Math.Sin(lockedBot.Direction);
            pos[1] = lockedBot.Y + lockedBot.Speed * Math.Cos(lockedBot.Direction);
        }
        
        return pos;
    }
}
