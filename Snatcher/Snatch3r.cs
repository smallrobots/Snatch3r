//////////////////////////////////////////////////////////////////////////////////////////////////
// SNATCH3R                                                                                     //
// Version 1.0                                                                                  //
//                                                                                              //
// Happily shared under the MIT License (MIT)                                                   //
//                                                                                              //
// Copyright(c) 2016 SmallRobots.it                                                             //
//                                                                                              //
// Permission is hereby granted, free of charge, to any person obtaining                        //
//a copy of this software and associated documentation files (the "Software"),                  //
// to deal in the Software without restriction, including without limitation the rights         //
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies             //
// of the Software, and to permit persons to whom the Software is furnished to do so,           //      
// subject to the following conditions:                                                         //
//                                                                                              //
// The above copyright notice and this permission notice shall be included in all               //
// copies or substantial portions of the Software.                                              //
//                                                                                              //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,          //
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR     //
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE           //
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,          //
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE        //
// OR OTHER DEALINGS IN THE SOFTWARE.                                                           //
//                                                                                              //
// Visit http://wwww.smallrobots.it for tutorials and videos                                    //
//                                                                                              //
// Credits                                                                                      //
// The SNATCH3R is built with Lego Mindstorms Ev3 retail set                                    //
// Building instructions can be found on                                                        //
// "The Lego Mindstorms EV3 Discovery book" written by Laurens Valk                             //
//////////////////////////////////////////////////////////////////////////////////////////////////

using MonoBrickFirmware.Display;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Sensors;
using MonoBrickFirmware.Sound;
using MonoBrickFirmware.UserInput;
using SmallRobots.Ev3ControlLib;
using System.Threading;
using System;

namespace SmallRobots.Snatch3r
{
    /// <summary>
    /// Ev3 IR Remote Command
    /// </summary>
    public enum Direction
    {
        Stop = 0,
        Straight_Forward,
        Left_Forward,
        Right_Forward,
        Straight_Backward,
        Left_Backward,      // This will be used to raise the Snatch3r gripper
        Right_Backward,     // This will be used to lower the Snacth3r gripper
        Beacon_ON
    }

    /// <summary>
    /// Behaviour supported by the Snatch3r
    /// </summary>
    public enum Snatch3rBehaviour
    {
        CommandedRemotely = 0,
        GarbageCollection,
        LineFollowing
    }

    public class Snatch3r : Robot
    {
        #region Fields
        /// <summary>
        /// Behaviour selected
        /// </summary>
        Snatch3rBehaviour behaviour;
        #endregion

        #region Public fields
        /// <summary>
        /// Direction of Snatch3r motion
        /// </summary>
        public Direction direction;

        /// <summary>
        /// Ev3 Speaker
        /// </summary>
        public Speaker speaker;

        /// <summary>
        /// Left caterpillar motor
        /// </summary>
        public Motor leftMotor;

        /// <summary>
        /// Right caterpillar motor
        /// </summary>
        public Motor rightMotor;

        /// <summary>
        /// Gripper motor
        /// </summary>
        public Motor gripperMotor;

        /// <summary>
        /// EV3 IR Sensor
        /// </summary>
        public EV3IRSensor irSensor;

        /// <summary>
        /// EV3 Color Sensor
        /// </summary>
        public EV3ColorSensor colorSensor;

        // Garbage collection behaviour related
        public GarbageCollection_States currentGarbageCollecionState;

        // Line following behaviour related
        public LineFollowingTask_States currentLineFollowingState;
        // public PIDController lineFollowingPIDTask;
        public sbyte steering;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Snatch3r(Snatch3rBehaviour behaviour)
        {
            this.behaviour = behaviour;
            currentGarbageCollecionState = GarbageCollection_States.starting;

            LcdConsole.Clear();
            LcdConsole.WriteLine("Snatch3r init");

            // Motors initialization
            leftMotor = new Motor(MotorPort.OutB);
            rightMotor = new Motor(MotorPort.OutC);
            gripperMotor = new Motor(MotorPort.OutA);
            LcdConsole.WriteLine("Motors ok");

            // Sensors initialization
            irSensor = new EV3IRSensor(SensorPort.In4);
            colorSensor = new EV3ColorSensor(SensorPort.In2);
            colorSensor.Mode = ColorMode.Reflection;
            LcdConsole.WriteLine("Sensors ok");

            // Speaker initialization
            speaker = new Speaker(100);
            LcdConsole.WriteLine("Speaker ok");

            switch (behaviour)
            {
                case Snatch3rBehaviour.CommandedRemotely:
                    {
                        // IR Remote task initialization
                        TaskScheduler.Add(new IRRemoteTask());
                        LcdConsole.WriteLine("IRRemoteTask OK");

                        // Drive task initialization
                        TaskScheduler.Add(new ExecuteCommandFromIRRemote());
                        LcdConsole.WriteLine("DriveTask OK");

                        // Keyboard task
                        TaskScheduler.Add(new KeyboardTask());
                        LcdConsole.WriteLine("Keyboard Task OK");

                        // HMI task
                        TaskScheduler.Add(new HmiTask(behaviour));
                        LcdConsole.WriteLine("HMI Task OK");
                        break;
                    }
                case Snatch3rBehaviour.GarbageCollection:
                    {
                        // State machine task
                        TaskScheduler.Add(new GarbageCollectionSMTask());
                        LcdConsole.WriteLine("Garb Collection Task OK");

                        // HMI task
                        TaskScheduler.Add(new HmiTask(behaviour));
                        LcdConsole.WriteLine("HMI Task OK");

                        // Keyboard task
                        TaskScheduler.Add(new KeyboardTask());
                        LcdConsole.WriteLine("Keyboard Task OK");
                        break;
                    }
                case Snatch3rBehaviour.LineFollowing:
                    {
                        // State machine task
                        TaskScheduler.Add(new LineFollowingSMTask());
                        LcdConsole.WriteLine("Line following Task OK");

                        //// Line following PID Task
                        //lineFollowingPIDTask = new PIDController(50);
                        //TaskScheduler.Add(lineFollowingPIDTask);
                        //LcdConsole.WriteLine("Line following PID Task OK");

                        // HMI task
                        TaskScheduler.Add(new HmiTask(behaviour));
                        LcdConsole.WriteLine("HMI Task OK");

                        // Keyboard task
                        TaskScheduler.Add(new KeyboardTask());
                        LcdConsole.WriteLine("Keyboard Task OK");

                        break;
                    }
                default:
                    {
                        // Keyboard task
                        TaskScheduler.Add(new KeyboardTask());
                        LcdConsole.WriteLine("Keyboard Task OK");
                        break;
                    }
            }

        }
        #endregion

        #region Public methods
        /// <summary>
        /// Starts the robot behaviour
        /// </summary>
        public void Start()
        {
            // Welcome messages
            LcdConsole.Clear();
            LcdConsole.WriteLine("*****************************");
            LcdConsole.WriteLine("*                           *");
            LcdConsole.WriteLine("*      SmallRobots.it       *");
            LcdConsole.WriteLine("*                           *");
            LcdConsole.WriteLine("*       SNATCH3R  1.0       *");
            switch (behaviour)
            {
                case Snatch3rBehaviour.CommandedRemotely:
                    LcdConsole.WriteLine("*         IR Remote         *");
                    break;
                case Snatch3rBehaviour.GarbageCollection:
                    LcdConsole.WriteLine("*        GCollection        *");
                    break;
                case Snatch3rBehaviour.LineFollowing:
                    LcdConsole.WriteLine("*       Line Following      *");
                    break;
                default:
                    LcdConsole.WriteLine("*                           *");
                    break;
            }        
            LcdConsole.WriteLine("*                           *");
            LcdConsole.WriteLine("*   Enter to start          *");
            LcdConsole.WriteLine("*   Escape to quit          *");
            LcdConsole.WriteLine("*                           *");
            LcdConsole.WriteLine("*****************************");

            // Busy wait for user
            bool enterButtonPressed = false;
            bool escapeButtonPressed = false;
            while (!(enterButtonPressed || escapeButtonPressed))
            {
                // Either the user presses the touch sensor, or presses the escape button
                // If users presses both, escape button will prevale
                enterButtonPressed = (Buttons.ButtonStates.Enter == Buttons.GetKeypress(new CancellationToken(true)));
                escapeButtonPressed = (Buttons.ButtonStates.Escape == Buttons.GetKeypress(new CancellationToken(true)));
            }

            if (escapeButtonPressed)
            {
                return;
            }

            if (enterButtonPressed)
            {
                LcdConsole.Clear();
                LcdConsole.WriteLine("*****************************");
                LcdConsole.WriteLine("*                           *");
                LcdConsole.WriteLine("*      SmallRobots.it       *");
                LcdConsole.WriteLine("*                           *");
                LcdConsole.WriteLine("*       SNATCH3R  1.0       *");
                switch (behaviour)
                {
                    case Snatch3rBehaviour.CommandedRemotely:
                        LcdConsole.WriteLine("*         IR Remote         *");
                        break;
                    case Snatch3rBehaviour.GarbageCollection:
                        LcdConsole.WriteLine("*        GCollection        *");
                        break;
                    case Snatch3rBehaviour.LineFollowing:
                        LcdConsole.WriteLine("*       Line Following      *");
                        break;
                    default:
                        LcdConsole.WriteLine("*                           *");
                        break;
                }
                LcdConsole.WriteLine("*                           *");
                LcdConsole.WriteLine("*        Starting....       *");
                LcdConsole.WriteLine("*                           *");
                LcdConsole.WriteLine("*                           *");
                LcdConsole.WriteLine("*****************************");

                // Acually starts the robot
                TaskScheduler.Start();
            }
        }
        #endregion
    }

    /// <summary>
    /// Periodic Task that checks for keyboards
    /// </summary>
    public class KeyboardTask : PeriodicTask
    {
        #region Constructors
        public KeyboardTask() : base()
        {
            // Set the Action
            Action = OnTimer;

            // Set the period
            Period = 500;
        }
        #endregion

        #region Private methods
        private void OnTimer(Robot robot)
        {
            if (Buttons.ButtonStates.Escape == Buttons.GetKeypress(new CancellationToken(true)))
            {
                ((Snatch3r)robot).TaskScheduler.Stop();

                // Shutdown
                Buttons.LedPattern(0);
                ((Snatch3r)robot).leftMotor.Off();
                ((Snatch3r)robot).rightMotor.Off();
                ((Snatch3r)robot).gripperMotor.Off();
            }
            if (Buttons.ButtonStates.Down == Buttons.GetKeypress(new CancellationToken(true)))
            {
                // Reset Tachos
                ((Snatch3r)robot).leftMotor.ResetTacho();
                ((Snatch3r)robot).rightMotor.ResetTacho();
                ((Snatch3r)robot).gripperMotor.ResetTacho();
            }
        }
        #endregion
    }

    /// <summary>
    /// Task that draws values of sensors and actuators on screen
    /// </summary>
    public class HmiTask : PeriodicTask
    {
        #region Fields
        // Data to visualize
        int leftTacho;
        int rightTacho;
        int gripperTacho;
        int targetDistance;
        int reflection;

        // Renderer fields
        Font f;
        Point offset;
        Point p;
        Point boxSize;
        Rectangle box;

        // Behaviour selected
        Snatch3rBehaviour selectedBehaviour;
        #endregion

        #region Constructors
        public HmiTask(Snatch3rBehaviour selectedBehaviour) : base()
        {
            // Store the desired behaviour
            this.selectedBehaviour = selectedBehaviour;
            
            // Set the Action
            Action = OnTimer;

            // Set the period
            Period = 500;

            // Rendere initialization
            f = Font.MediumFont;
            offset = new Point(0, 25);
            p = new Point(10, Lcd.Height - 125);
            boxSize = new Point(100, 24);
            box = new Rectangle(p, p + boxSize);
        }
        #endregion

        #region Private methods
        private void OnTimer(Robot robot)
        {
            // Retrieve data to visualize
            leftTacho = ((Snatch3r)robot).leftMotor.GetTachoCount();
            rightTacho = ((Snatch3r)robot).rightMotor.GetTachoCount();
            gripperTacho = ((Snatch3r)robot).gripperMotor.GetTachoCount();
            reflection = ((Snatch3r)robot).colorSensor.Read();

            if ((selectedBehaviour == Snatch3rBehaviour.CommandedRemotely) || (selectedBehaviour == Snatch3rBehaviour.LineFollowing))
            {
                targetDistance = ((Snatch3r)robot).irSensor.ReadDistance();
            }
            else
            {
                if (((Snatch3r)robot).currentGarbageCollecionState == GarbageCollection_States.drivingTowardBeacon)
                    targetDistance = ((Snatch3r)robot).irSensor.ReadBeaconLocation().Distance;
                else
                    targetDistance = ((Snatch3r)robot).irSensor.ReadDistance();
            }

            switch (selectedBehaviour)
            {
                case Snatch3rBehaviour.CommandedRemotely:
                    // Prepare for drawing
                    Lcd.Clear();
                    Lcd.WriteTextBox(f, box + offset * 0, "Gripper tc: " + gripperTacho.ToString(), true);
                    Lcd.WriteTextBox(f, box + offset * 1, "Left    tc: " + leftTacho.ToString(), true);
                    Lcd.WriteTextBox(f, box + offset * 2, "Right   tc: " + rightTacho.ToString(), true);
                    Lcd.WriteTextBox(f, box + offset * 3, "Target distance: " + targetDistance.ToString(), true);
                    Lcd.WriteTextBox(f, box + offset * 4, "Down to reset " + rightTacho.ToString(), true);
                    Lcd.Update();
                    break;
                case Snatch3rBehaviour.GarbageCollection:
                    // Prepare for drawing
                    offset = new Point(0, 20);
                    Lcd.Clear();
                    Lcd.WriteTextBox(f, box + offset * 0, "Gripper tc: " + gripperTacho.ToString(), true);
                    Lcd.WriteTextBox(f, box + offset * 1, "Left    tc: " + leftTacho.ToString(), true);
                    Lcd.WriteTextBox(f, box + offset * 2, "Right   tc: " + rightTacho.ToString(), true);
                    Lcd.WriteTextBox(f, box + offset * 3, "Target distance: " + targetDistance.ToString(), true);
                    Lcd.WriteTextBox(f, box + offset * 4, "State: " + ((Snatch3r)robot).currentGarbageCollecionState.ToString(), true);
                    Lcd.WriteTextBox(f, box + offset * 5, "Down to reset " + rightTacho.ToString(), true);
                    Lcd.Update();
                    break;
                case Snatch3rBehaviour.LineFollowing:
                    // Prepare for drawing
                    offset = new Point(0, 20);
                    Lcd.Clear();
                    Lcd.WriteTextBox(f, box + offset * 0, "Left    tc: " + leftTacho.ToString(), true);
                    Lcd.WriteTextBox(f, box + offset * 1, "Reflection: " + reflection.ToString(), true);
                    Lcd.WriteTextBox(f, box + offset * 2, "Steering  : " + ((Snatch3r)robot).steering.ToString(), true);
                    Lcd.WriteTextBox(f, box + offset * 3, "Target distance: " + targetDistance.ToString(), true);
                    Lcd.WriteTextBox(f, box + offset * 4, "State: " + ((Snatch3r)robot).currentLineFollowingState.ToString(), true);
                    Lcd.WriteTextBox(f, box + offset * 5, "Down to reset " + rightTacho.ToString(), true);
                    Lcd.Update();
                    break;
            }
        }
        #endregion
    }


    #region Tasks to command the Snatch3r with the IR Remote
    /// <summary>
    /// Periodic Task that receives commands from the Ev3 IR Remote
    /// </summary>
    public class IRRemoteTask : PeriodicTask
    {
        #region Fields
        /// <summary>
        /// Last command received from the Ev3 IR Remote
        /// </summary>
        byte remoteCommand;

        bool beaconActivated;

        Direction previousDirection;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public IRRemoteTask() : base()
        {
            // Fields initialization
            remoteCommand = 0;
            beaconActivated = false;

            // Set the action
            Action = OnTimer;

            // Set the period
            Period = 100;

            // Previous direction
            previousDirection = Direction.Stop;
        }
        #endregion

        #region Private methods
        private void OnTimer(Robot robot)
        {
            if (beaconActivated && ((Snatch3r)robot).irSensor.ReadBeaconLocation().Distance > -100)
            {
                // Don't change Ev3 IR Sensor mode
                return;
            }
            else
            {
                // Ev3 IR Sensor mode can be changed because it's not detected anymore
                beaconActivated = false;
            }

            remoteCommand = ((Snatch3r)robot).irSensor.ReadRemoteCommand();
            switch (remoteCommand)
            {
                case 0:
                    ((Snatch3r)robot).direction = Direction.Stop;
                    if (previousDirection != Direction.Stop)
                    {
                        // LcdConsole.WriteLine("Stop");
                        previousDirection = Direction.Stop;
                        beaconActivated = false;
                    }
                    break;
                case 1:
                    ((Snatch3r)robot).direction = Direction.Left_Forward;
                    if (previousDirection != Direction.Left_Forward)
                    {
                        // LcdConsole.WriteLine("Left_Forward");
                        previousDirection = Direction.Left_Forward;
                        beaconActivated = false;
                    }
                    break;
                case 3:
                    ((Snatch3r)robot).direction = Direction.Right_Forward;
                    if (previousDirection != Direction.Right_Forward)
                    {
                        // LcdConsole.WriteLine("Right_Forward");
                        previousDirection = Direction.Right_Forward;
                        beaconActivated = false;
                    }
                    break;
                case 5:
                    ((Snatch3r)robot).direction = Direction.Straight_Forward;
                    if (previousDirection != Direction.Straight_Forward)
                    {
                        // LcdConsole.WriteLine("Straight_Forward");
                        previousDirection = Direction.Straight_Forward;
                        beaconActivated = false;
                    }
                    break;
                case 2:
                    ((Snatch3r)robot).direction = Direction.Left_Backward;
                    if (previousDirection != Direction.Left_Backward)
                    {
                        // LcdConsole.WriteLine("Left_Backward");
                        previousDirection = Direction.Left_Backward;
                        beaconActivated = false;
                    }
                    break;
                case 4:
                    ((Snatch3r)robot).direction = Direction.Right_Backward;
                    if (previousDirection != Direction.Right_Backward)
                    {
                        // LcdConsole.WriteLine("Right_Backward");
                        previousDirection = Direction.Right_Backward;
                        beaconActivated = false;
                    }
                    break;
                case 8:
                    ((Snatch3r)robot).direction = Direction.Straight_Backward;
                    if (previousDirection != Direction.Straight_Backward)
                    {
                        // LcdConsole.WriteLine("Straight_Backward");
                        previousDirection = Direction.Straight_Backward;
                        beaconActivated = false;
                    }
                    break;
                case 9:
                    ((Snatch3r)robot).direction = Direction.Beacon_ON;
                    if (previousDirection != Direction.Beacon_ON)
                    {
                        // LcdConsole.WriteLine("Beacon_ON");
                        previousDirection = Direction.Beacon_ON;
                        beaconActivated = true;
                    }
                    break;
                default:
                    ((Snatch3r)robot).direction = Direction.Stop;
                    break;
            }
        }
        #endregion
    }

    /// <summary>
    /// Periodic Task that drives the Sentin3l
    /// </summary>
    public class ExecuteCommandFromIRRemote : PeriodicTask
    {
        #region Fields
        sbyte forwardPower;
        sbyte backwardPower;
        sbyte turnPower;
        sbyte currentLeftPower;
        sbyte currentRightPower;
        sbyte gripperPower;
        sbyte currentGripperPower;
        #endregion

        #region Constructors
        public ExecuteCommandFromIRRemote() : base()
        {
            // Set the action
            Action = OnTimer;

            // Set the Period
            Period = 100;

            // Maximum speed
            forwardPower = 80;
            backwardPower = 50;
            turnPower = 25;
            gripperPower = 100;

            // Current speed initialization
            currentLeftPower = 0;
            currentRightPower = 0;
            currentGripperPower = 0;
        }
        #endregion

        #region Private methods
        private void OnTimer(Robot robot)
        {
            // Adjust the LED Pattern
            if (((Snatch3r)robot).direction == Direction.Stop)
            {
                Buttons.LedPattern(3);
            }
            else if (((Snatch3r)robot).direction == Direction.Beacon_ON)
            {
                Buttons.LedPattern(2);
            }
            else
            {
                Buttons.LedPattern(1);
            }

            // Move the Sentin3l
            // Updates the set point
            switch (((Snatch3r)robot).direction)
            {
                case Direction.Beacon_ON:
                    currentLeftPower = 0;
                    currentRightPower = 0;
                    currentGripperPower = 0;
                    break;
                case Direction.Stop:
                    currentLeftPower = 0;
                    currentRightPower = 0;
                    currentGripperPower = 0;
                    break;
                case Direction.Straight_Forward:
                    currentLeftPower = forwardPower;
                    currentRightPower = forwardPower;
                    currentGripperPower = 0;
                    break;
                case Direction.Straight_Backward:
                    currentLeftPower = (sbyte)-backwardPower;
                    currentRightPower = (sbyte)-backwardPower;
                    currentGripperPower = 0;
                    break;
                case Direction.Left_Forward:
                    currentLeftPower = (sbyte)-turnPower;
                    currentRightPower = (sbyte)turnPower;
                    currentGripperPower = 0;
                    break;
                case Direction.Right_Forward:
                    currentLeftPower = (sbyte)turnPower;
                    currentRightPower = (sbyte)-turnPower;
                    currentGripperPower = 0;
                    break;
                case Direction.Left_Backward:
                    currentLeftPower = 0;
                    currentRightPower = 0;
                    currentGripperPower = (sbyte)gripperPower;
                    break;
                case Direction.Right_Backward:
                    currentLeftPower = 0;
                    currentRightPower = 0;
                    currentGripperPower = (sbyte)-gripperPower;
                    break;
                default:
                    currentLeftPower = 0;
                    currentRightPower = 0;
                    currentGripperPower = 0;
                    break;
            }
            // Send the updated set point to the legs controller
            ((Snatch3r)robot).leftMotor.SetPower(currentLeftPower);
            ((Snatch3r)robot).rightMotor.SetPower(currentRightPower);
            ((Snatch3r)robot).gripperMotor.SetPower(currentGripperPower);
        }
        #endregion
    }
    #endregion

    #region Tasks for the garabage collection behaviour
    public enum GarbageCollection_States
    {
        starting = 0,
        searchingNextTarget,
        drivingTowardNextTarget,
        collectingTarget,
        searchingBeacon,
        drivingTowardBeacon,
        deliveringTarget,
        garbageCollectionComplete,
        stop
    }

    public class GarbageCollectionSMTask : PeriodicTask
    {
        #region Fields
        // Mission related
        GarbageCollection_States currentState;
        GarbageCollection_States previousState;
        int objectsToCollect;
        int objectsCollected;
        int waitingTimeBeforeStart;

        // Manouvering related
        sbyte forwardPower;
        sbyte backwardPower;
        sbyte turnPower;
        sbyte currentLeftPower;
        sbyte currentRightPower;
        sbyte gripperPower;
        sbyte currentGripperPower;
        int halfSwipe;

        // Searching for next target
        bool swipeRightCompleted;
        bool swipeLeftCompleted;

        // Delivering target
        bool delivered;
        bool halfTurnCompleted;
        bool backStepped;

        // Distance from target
        int targetDistance;
        #endregion

        #region Constructors
        public GarbageCollectionSMTask()
        {
            // Set the Action
            Action = OnTimer;

            // Set the period
            Period = 100;

            // Set the current mission state
            waitingTimeBeforeStart = 1000;
            objectsToCollect = 2;
            objectsCollected = 0;
            currentState = GarbageCollection_States.starting;
            previousState = GarbageCollection_States.garbageCollectionComplete;

            // Maximum speed
            forwardPower = 70;
            backwardPower = 40;
            turnPower = 25;
            gripperPower = 100;

            // Ninety degrees turn tacho count (approximately)
            halfSwipe = 450;

            // Current speed initialization
            currentLeftPower = 0;
            currentRightPower = 0;
            currentGripperPower = 0;

            // Searching for next target
            swipeRightCompleted = false;
            swipeLeftCompleted = false;

        }
        #endregion

        #region Private methods
        private void OnTimer(Robot robot)
        {
            switch (currentState)
            {
                case GarbageCollection_States.starting:
                    Starting_Case();
                    break;
                case GarbageCollection_States.searchingNextTarget:
                    SearchingNextTarget_Case(robot);
                    break;
            
                case GarbageCollection_States.drivingTowardNextTarget:
                    DrivingTowardNextTarget_Case(robot);
                    break;
                case GarbageCollection_States.collectingTarget:
                    CollectingTarget_Case(robot);
                    break;             
                case GarbageCollection_States.searchingBeacon:                
                    SearchingBeacon_Case(robot);
                    break;
                case GarbageCollection_States.drivingTowardBeacon:
                    DrivingTowardBeacon_Case(robot);
                    break;
                case GarbageCollection_States.deliveringTarget:
                    DeliveringTarget_Case(robot);
                    break;
                case GarbageCollection_States.stop:
                    currentLeftPower = 0;
                    currentRightPower = 0;
                    break;
                default:
                    break;
            }

            // Update the robot current status for display purposes
            ((Snatch3r)robot).currentGarbageCollecionState = currentState;

            // Send the updated set point to the legs controller
            ((Snatch3r)robot).leftMotor.SetPower(currentLeftPower);
            ((Snatch3r)robot).rightMotor.SetPower(currentRightPower);
            ((Snatch3r)robot).gripperMotor.SetPower(currentGripperPower);
        }

        private void Starting_Case()
        {
            if (previousState != GarbageCollection_States.starting)
            {
                Thread.Sleep(waitingTimeBeforeStart);
                currentState = GarbageCollection_States.searchingNextTarget;
                previousState = GarbageCollection_States.starting;

                // LcdConsole.WriteLine(currentState.ToString());
            }
        }

        private void SearchingNextTarget_Case(Robot robot)
        {
            if (previousState != GarbageCollection_States.searchingNextTarget)
            {
                // Initialize the ir sensor as proximity sensor
                ((Snatch3r)robot).irSensor.Mode = IRMode.Proximity;
                previousState = GarbageCollection_States.searchingNextTarget;

                // Reset the tacho count
                ((Snatch3r)robot).leftMotor.ResetTacho();

                // LcdConsole.WriteLine(currentState.ToString());
            }

            // Rotate right
            if (!swipeRightCompleted)
            {
                currentLeftPower = (sbyte)turnPower;
                currentRightPower = (sbyte)-turnPower;

                if (((Snatch3r)robot).leftMotor.GetTachoCount() > halfSwipe)
                {
                    swipeRightCompleted = true;
                }
            }

            // Rotate left
            if (swipeRightCompleted && !swipeLeftCompleted)
            {
                currentLeftPower = (sbyte)-turnPower;
                currentRightPower = (sbyte)turnPower;

                if (((Snatch3r)robot).leftMotor.GetTachoCount() < -halfSwipe)
                {
                    swipeLeftCompleted = true;
                }
            }

            // LcdConsole.WriteLine(((Snatch3r)robot).leftMotor.GetTachoCount().ToString());

            targetDistance = ((Snatch3r)robot).irSensor.ReadDistance();
            // LcdConsole.WriteLine("Target distance: " + targetDistance.ToString());
            if ((targetDistance > 10) && (targetDistance < 60))
            {
                currentLeftPower = 0;
                currentRightPower = 0;
                swipeLeftCompleted = true;
                swipeRightCompleted = true;
                currentState = GarbageCollection_States.drivingTowardNextTarget;
                previousState = GarbageCollection_States.searchingNextTarget;
            }
        }

        private void DrivingTowardNextTarget_Case(Robot robot)
        {
            if (previousState != GarbageCollection_States.drivingTowardNextTarget)
            {
                previousState = GarbageCollection_States.drivingTowardNextTarget;
            }

            currentLeftPower = (sbyte)(0.5 * forwardPower);
            currentRightPower = (sbyte)(0.5 * forwardPower);

            targetDistance = ((Snatch3r)robot).irSensor.ReadDistance();
            // LcdConsole.WriteLine("Target distance: " + targetDistance.ToString());

            if (targetDistance < 15)
            {
                currentLeftPower = 0;
                currentRightPower = 0;
                currentState = GarbageCollection_States.collectingTarget;
                previousState = GarbageCollection_States.drivingTowardNextTarget;
            }
        }

        private void CollectingTarget_Case(Robot robot)
        {
            if (previousState != GarbageCollection_States.collectingTarget)
            {
                previousState = GarbageCollection_States.collectingTarget;
                ((Snatch3r)robot).gripperMotor.ResetTacho();
                Thread.Sleep(100);
            }

            int gripperTachoCount = ((Snatch3r)robot).gripperMotor.GetTachoCount();
            if (gripperTachoCount < 3500)
            {
                currentGripperPower = gripperPower;
            }
            else
            {
                currentGripperPower = 0;
                currentState = GarbageCollection_States.searchingBeacon;
            }
        }

        private void SearchingBeacon_Case(Robot robot)
        {
            if (previousState != GarbageCollection_States.searchingBeacon)
            {
                // Initialize the ir sensor as beacon seeker
                ((Snatch3r)robot).irSensor.Mode = IRMode.Seek;
                previousState = GarbageCollection_States.searchingBeacon;

                // Reset the tacho count
                ((Snatch3r)robot).leftMotor.ResetTacho();
                swipeLeftCompleted = false;
                swipeRightCompleted = false;

                Thread.Sleep(200);
            }

            // Rotate right
            if (!swipeRightCompleted)
            {
                currentLeftPower = (sbyte)turnPower;
                currentRightPower = (sbyte)-turnPower;

                if (((Snatch3r)robot).leftMotor.GetTachoCount() > 2 * halfSwipe)
                {
                    swipeRightCompleted = true;
                }
            }

            // Rotate left
            if (swipeRightCompleted && !swipeLeftCompleted)
            {
                currentLeftPower = (sbyte)-turnPower;
                currentRightPower = (sbyte)turnPower;

                if (((Snatch3r)robot).leftMotor.GetTachoCount() < -2 * halfSwipe)
                {
                    swipeLeftCompleted = true;
                }
            }

            BeaconLocation beaconLocation = ((Snatch3r)robot).irSensor.ReadBeaconLocation();
            int beaconHeading = beaconLocation.Location;
            targetDistance = beaconLocation.Distance;

            if ((beaconHeading < 5) && (beaconHeading > -5) && (beaconHeading != 0))
            {
                currentLeftPower = 0;
                currentRightPower = 0;
                swipeLeftCompleted = true;
                swipeRightCompleted = true;
                currentState = GarbageCollection_States.drivingTowardBeacon;
                previousState = GarbageCollection_States.searchingBeacon;
            }
        }

        private void DrivingTowardBeacon_Case(Robot robot)
        {
            double kP = 0.2;
            int powerDifference;
            BeaconLocation beaconLocation;

            beaconLocation = ((Snatch3r)robot).irSensor.ReadBeaconLocation();
            if (beaconLocation.Distance > 30)
            {
                powerDifference = (int)(-kP * beaconLocation.Location * 0.3 * forwardPower);
                currentLeftPower = (sbyte)(0.3 * forwardPower - powerDifference) > 0 ? (sbyte)(0.3 * forwardPower - powerDifference) : (sbyte) 0;
                currentRightPower = (sbyte)(0.3 * forwardPower + powerDifference) > 0 ? (sbyte)(0.3 * forwardPower + powerDifference) : (sbyte)0;
            }
            else
            {
                currentRightPower = 0;
                currentLeftPower = 0;
                currentState = GarbageCollection_States.deliveringTarget;
            }
        }

        private void DeliveringTarget_Case(Robot robot)
        {
            if (previousState != GarbageCollection_States.deliveringTarget)
            {
                previousState = GarbageCollection_States.deliveringTarget;
                ((Snatch3r)robot).leftMotor.ResetTacho();
                halfTurnCompleted = false;
                delivered = false;
                backStepped = false;
            }

            if (!delivered)
            {
                int gripperTachoCount = ((Snatch3r)robot).gripperMotor.GetTachoCount();
                if (gripperTachoCount > 0)
                {
                    currentGripperPower = (sbyte)-gripperPower;
                }
                else
                {
                    currentGripperPower = 0;
                    delivered = true;
                    objectsCollected = objectsCollected + 1;
                }
            }

            if (delivered && !backStepped)
            {
                currentLeftPower = (sbyte) -backwardPower;
                currentRightPower = (sbyte) -backwardPower;
                if (((Snatch3r)robot).leftMotor.GetTachoCount() < -300)
                {
                    backStepped = true;
                    currentLeftPower = 0;
                    currentRightPower = 0;
                    ((Snatch3r)robot).leftMotor.ResetTacho();
                }
            }

            if (delivered && backStepped && !halfTurnCompleted)
            {
                currentLeftPower = (sbyte)turnPower;
                currentRightPower = (sbyte)-turnPower;

                if (((Snatch3r)robot).leftMotor.GetTachoCount() > 2*halfSwipe)
                {
                    halfTurnCompleted = true;
                    if (objectsCollected >= objectsToCollect)
                        currentState = GarbageCollection_States.stop;
                    else
                        currentState = GarbageCollection_States.starting;
                    
                }
            }
        }

        #endregion
    }
    #endregion

    #region Tasks fot the line following behaviour
    public enum LineFollowingTask_States
    {
        starting = 0,
        lineFollowing,
        grabbingObstacle,
        rotating,
        advancing,
        deliveringObstacle,
        retracting,
        counterRotating
    }

    public class LineFollowingSMTask : PeriodicTask
    {
        #region Private fields
        // Mission related
        LineFollowingTask_States currentState;
        LineFollowingTask_States previousState;

        // Manouvering related
        readonly sbyte forwardPower;
        readonly sbyte backwardPower;
        readonly sbyte gripperPower;
        readonly sbyte turnPower;
        readonly int halfSwipe;
        readonly sbyte minDistanceFromObstacle;
        sbyte currentLeftPower;
        sbyte currentRightPower;
        sbyte currentGripperPower;

        bool thisTimeToLeft;

        // Starting state
        readonly int waitingTimeBeforeStarting;

        // Line Following state
        int colorSensorReading;
        int setPoint;
        int threshold;
        SteeringLookUpTable lookUpTable;
        #endregion

        #region Public fields
        #endregion

        #region Constructors
        public LineFollowingSMTask()
        {
            // Set the Action
            Action = OnTimer;

            // Set the period
            Period = 50;

            // Fields initialization
            // Manouvering related
            forwardPower = 25;
            backwardPower = 20;
            gripperPower = 100;
            turnPower = 20;
            halfSwipe = 400;
            minDistanceFromObstacle = 8;

            // State initialization
            currentState = LineFollowingTask_States.starting;
            previousState = LineFollowingTask_States.starting;

            // Starting state
            waitingTimeBeforeStarting = 500;

            // Line Following State
            lookUpTable = new SteeringLookUpTable();
        }
        #endregion

        #region Private Methods
        private void OnTimer(Robot robot)
        {
            switch (currentState)
            {
                case LineFollowingTask_States.starting:
                    Buttons.LedPattern(1);
                    Starting_Case();
                    break;
                case LineFollowingTask_States.lineFollowing:
                    Buttons.LedPattern(1);
                    LineFollowing_Case(robot);
                    break;
                case LineFollowingTask_States.grabbingObstacle:
                    Buttons.LedPattern(2);
                    GrabbingObstacle_Case(robot);
                    break;
                case LineFollowingTask_States.rotating:
                    Buttons.LedPattern(2);
                    Rotating_Case(robot);
                    break;
                case LineFollowingTask_States.advancing:
                    Buttons.LedPattern(2);
                    Advancing_Case(robot);
                    break;
                case LineFollowingTask_States.deliveringObstacle:
                    Buttons.LedPattern(2);
                    DeliveringObstacle_Case(robot);
                    break;
                case LineFollowingTask_States.retracting:
                    Buttons.LedPattern(3);
                    Retracting_Case(robot);
                    break;
                case LineFollowingTask_States.counterRotating:
                    Buttons.LedPattern(3);
                    CounterRotating_Case(robot);
                    break;
                default:
                    currentLeftPower = 0;
                    currentRightPower = 0;
                    currentGripperPower = 0;
                    break;
            }

            // Update the robot current status for display purposes
            ((Snatch3r)robot).currentLineFollowingState = currentState;

            // Send the updated set point to the legs controller
            ((Snatch3r)robot).leftMotor.SetPower(currentLeftPower);
            ((Snatch3r)robot).rightMotor.SetPower(currentRightPower);
            ((Snatch3r)robot).gripperMotor.SetPower(currentGripperPower);
        }

        private void CounterRotating_Case(Robot robot)
        {
            Snatch3r snatcher = (Snatch3r)robot;
            if (previousState != LineFollowingTask_States.counterRotating)
            {
                previousState = LineFollowingTask_States.counterRotating;
                snatcher.leftMotor.ResetTacho();
            }

            if (!thisTimeToLeft)
            {
                currentLeftPower = (sbyte)-turnPower;
                currentRightPower = (sbyte)+turnPower;

                if (snatcher.leftMotor.GetTachoCount() < -0.8 * halfSwipe)
                {
                    currentLeftPower = 0;
                    currentRightPower = 0;
                    currentState = LineFollowingTask_States.lineFollowing;
                    thisTimeToLeft = !thisTimeToLeft;
                }
            }
            else
            {
                currentLeftPower = (sbyte)turnPower;
                currentRightPower = (sbyte)-turnPower;

                if (snatcher.leftMotor.GetTachoCount() > 0.8 * halfSwipe)
                {
                    currentLeftPower = 0;
                    currentRightPower = 0;
                    currentState = LineFollowingTask_States.lineFollowing;
                    thisTimeToLeft = !thisTimeToLeft;
                }
            }
        }

        private void Retracting_Case(Robot robot)
        {
            Snatch3r snatcher = (Snatch3r)robot;
            if (previousState != LineFollowingTask_States.retracting)
            {
                previousState = LineFollowingTask_States.retracting;
                snatcher.leftMotor.ResetTacho();
            }
            currentLeftPower = (sbyte)-backwardPower;
            currentRightPower = (sbyte)-backwardPower;

            if (snatcher.leftMotor.GetTachoCount() < -halfSwipe)
            {
                currentLeftPower = 0;
                currentRightPower = 0;
                currentState = LineFollowingTask_States.counterRotating;
            }
        }

        private void DeliveringObstacle_Case(Robot robot)
        {
            Snatch3r snatcher = (Snatch3r)robot;
            if (previousState != LineFollowingTask_States.deliveringObstacle)
            {
                previousState = LineFollowingTask_States.deliveringObstacle;
                snatcher.leftMotor.ResetTacho();
            }

            int gripperTachoCount = snatcher.gripperMotor.GetTachoCount();
            if (gripperTachoCount > 0)
            {
                currentGripperPower = (sbyte)-gripperPower;
            }
            else
            {
                currentGripperPower = 0;
                currentState = LineFollowingTask_States.retracting;
            }
        }

        private void Advancing_Case(Robot robot)
        {
            Snatch3r snatcher = (Snatch3r)robot;
            if (previousState != LineFollowingTask_States.advancing)
            {
                previousState = LineFollowingTask_States.advancing;
                snatcher.leftMotor.ResetTacho();
            }
            currentLeftPower = (sbyte)forwardPower;
            currentRightPower = (sbyte)forwardPower;

            if (snatcher.leftMotor.GetTachoCount() > halfSwipe)
            {
                currentLeftPower = 0;
                currentRightPower = 0;
                currentState = LineFollowingTask_States.deliveringObstacle;
            }
        }

        private void Rotating_Case(Robot robot)
        {
            Snatch3r snatcher = (Snatch3r)robot;
            if (previousState != LineFollowingTask_States.rotating)
            {
                previousState = LineFollowingTask_States.rotating;
                snatcher.leftMotor.ResetTacho();
            }

            if (!thisTimeToLeft)
            {
                currentLeftPower = (sbyte)turnPower;
                currentRightPower = (sbyte)-turnPower;

                if (snatcher.leftMotor.GetTachoCount() > halfSwipe)
                {
                    currentLeftPower = 0;
                    currentRightPower = 0;
                    currentState = LineFollowingTask_States.advancing;
                }
            }
            else
            {
                currentLeftPower = (sbyte)-turnPower;
                currentRightPower = (sbyte)turnPower;

                if (snatcher.leftMotor.GetTachoCount() < -halfSwipe)
                {
                    currentLeftPower = 0;
                    currentRightPower = 0;
                    currentState = LineFollowingTask_States.advancing;
                }
            }
        }

        private void LineFollowing_Case(Robot robot)
        {
            Snatch3r snatcher = (Snatch3r)robot;
            if (previousState != LineFollowingTask_States.lineFollowing)
            {
                // Initialize the state
                setPoint = 40;
                threshold = 10;

                //snatcher.lineFollowingPIDTask.Kp = 0.8f;
                //snatcher.lineFollowingPIDTask.Ki = 0.3f;
                //snatcher.lineFollowingPIDTask.Kd = 0.1f;
                //snatcher.lineFollowingPIDTask.LowPassConstant = 0.7f;
                //snatcher.lineFollowingPIDTask.SetPoint = setPoint;
                //snatcher.lineFollowingPIDTask.MaxPower = (sbyte) (1.5*forwardPower);
                //snatcher.lineFollowingPIDTask.MinPower = (sbyte) (-1.5*forwardPower);

                Thread soundThread = new Thread(new ThreadStart(() => { snatcher.speaker.PlaySoundFile("/home/root/apps/Snatch3r/trucks001.wav",250); }));
                soundThread.Start();
                // snatcher.speaker.PlaySoundFile("/home/root/apps/Snatch3r/trucks001.wav");

                previousState = LineFollowingTask_States.lineFollowing;
            }

            // Give inputs to the PID
            colorSensorReading = snatcher.colorSensor.Read();
            //snatcher.lineFollowingPIDTask.ProcessVariableSignal = (Math.Abs(colorSensorReading - setPoint) < threshold) ? 0 : colorSensorReading;

            //// Set outputs to the motors
            //snatcher.steering = snatcher.lineFollowingPIDTask.OutputSignal;

            snatcher.steering = (sbyte)lookUpTable.GetValue(colorSensorReading);

            currentLeftPower = (sbyte) (forwardPower + 0.5*snatcher.steering);
            currentRightPower = (sbyte) (forwardPower - 0.5* snatcher.steering);

            // Checks for obstacles
            int targetDistance = snatcher.irSensor.ReadDistance();
            if ((targetDistance < minDistanceFromObstacle) && (targetDistance>0))
            {
                currentLeftPower = 0;
                currentRightPower = 0;
                currentState = LineFollowingTask_States.grabbingObstacle;
            }
            
        }

        private void Starting_Case()
        {
            Thread.Sleep(waitingTimeBeforeStarting);
            currentState = LineFollowingTask_States.lineFollowing;
            previousState = LineFollowingTask_States.starting;
        }

        private void GrabbingObstacle_Case(Robot robot)
        {
            Snatch3r snatcher = (Snatch3r)robot;
            if (previousState != LineFollowingTask_States.grabbingObstacle)
            {
                previousState = LineFollowingTask_States.grabbingObstacle;
                snatcher.gripperMotor.ResetTacho();

                // This function does nothing, body is completely commented into the library
                snatcher.speaker.StopSoundPlayback();

                Thread.Sleep(500);
                Thread soundThread = new Thread(new ThreadStart(() => { snatcher.speaker.PlaySoundFile("/home/root/apps/Snatch3r/BeepBeep.wav", 250); }));
                soundThread.Start();
            }

            int gripperTachoCount = ((Snatch3r)robot).gripperMotor.GetTachoCount();
            if (gripperTachoCount < 3500)
            {
                currentGripperPower = gripperPower;
            }
            else
            {
                currentGripperPower = 0;
                currentState = LineFollowingTask_States.rotating;
            }
        }
        #endregion
    }
    #endregion
}

