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
using System;
using System.IO;
using System.Threading;

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

    public class Snatch3r : Robot
    {
        #region Fields
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
        public Motor leftCaterpillarMotor;

        /// <summary>
        /// Right caterpillar motor
        /// </summary>
        public Motor rightCaterpillarMotor;

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
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Snatch3r()
        {
            LcdConsole.Clear();
            LcdConsole.WriteLine("Snatch3r init");

            // Motors initialization
            leftCaterpillarMotor = new Motor(MotorPort.OutB);
            rightCaterpillarMotor = new Motor(MotorPort.OutC);
            gripperMotor = new Motor(MotorPort.OutA);
            LcdConsole.WriteLine("Motors ok");

            // Sensors initialization
            irSensor = new EV3IRSensor(SensorPort.In4);
            colorSensor = new EV3ColorSensor(SensorPort.In3);
            colorSensor.Mode = ColorMode.Reflection;
            LcdConsole.WriteLine("Sensors ok");

            // Speaker initialization
            speaker = new Speaker(100);
            LcdConsole.WriteLine("Speaker ok");

            // IR Remote task initialization
            TaskScheduler.Add(new IRRemoteTask());
            LcdConsole.WriteLine("IRRemoteTask OK");

            // Drive task initialization
            TaskScheduler.Add(new DriveTask());
            LcdConsole.WriteLine("DriveTask OK");

            // Keyboard task
            TaskScheduler.Add(new KeyboardTask());
            LcdConsole.WriteLine("Keyboard Task OK");

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
            LcdConsole.WriteLine("*                           *");
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
                LcdConsole.WriteLine("*                           *");
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
                        LcdConsole.WriteLine("Stop");
                        previousDirection = Direction.Stop;
                        beaconActivated = false;
                    }
                    break;
                case 1:
                    ((Snatch3r)robot).direction = Direction.Left_Forward;
                    if (previousDirection != Direction.Left_Forward)
                    {
                        LcdConsole.WriteLine("Left_Forward");
                        previousDirection = Direction.Left_Forward;
                        beaconActivated = false;
                    }
                    break;
                case 3:
                    ((Snatch3r)robot).direction = Direction.Right_Forward;
                    if (previousDirection != Direction.Right_Forward)
                    {
                        LcdConsole.WriteLine("Right_Forward");
                        previousDirection = Direction.Right_Forward;
                        beaconActivated = false;
                    }
                    break;
                case 5:
                    ((Snatch3r)robot).direction = Direction.Straight_Forward;
                    if (previousDirection != Direction.Straight_Forward)
                    {
                        LcdConsole.WriteLine("Straight_Forward");
                        previousDirection = Direction.Straight_Forward;
                        beaconActivated = false;
                    }
                    break;
                case 2:
                    ((Snatch3r)robot).direction = Direction.Left_Backward;
                    if (previousDirection != Direction.Left_Backward)
                    {
                        LcdConsole.WriteLine("Left_Backward");
                        previousDirection = Direction.Left_Backward;
                        beaconActivated = false;
                    }
                    break;
                case 4:
                    ((Snatch3r)robot).direction = Direction.Right_Backward;
                    if (previousDirection != Direction.Right_Backward)
                    {
                        LcdConsole.WriteLine("Right_Backward");
                        previousDirection = Direction.Right_Backward;
                        beaconActivated = false;
                    }
                    break;
                case 8:
                    ((Snatch3r)robot).direction = Direction.Straight_Backward;
                    if (previousDirection != Direction.Straight_Backward)
                    {
                        LcdConsole.WriteLine("Straight_Backward");
                        previousDirection = Direction.Straight_Backward;
                        beaconActivated = false;
                    }
                    break;
                case 9:
                    ((Snatch3r)robot).direction = Direction.Beacon_ON;
                    if (previousDirection != Direction.Beacon_ON)
                    {
                        LcdConsole.WriteLine("Beacon_ON");
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
    public class DriveTask : PeriodicTask
    {
        #region Fields
        sbyte forwardSpeed;
        sbyte backwardSpeed;
        sbyte turnSpeed;
        sbyte leftCaterpillarSpeed;
        sbyte rightCaterpillarSpeed;
        sbyte gripSpeed;
        sbyte gripperSpeed;
        #endregion

        #region Constructors
        public DriveTask() : base()
        {
            // Set the action
            Action = OnTimer;

            // Set the Period
            Period = 100;

            // Maximum speed
            forwardSpeed = 80;
            backwardSpeed = 50;
            turnSpeed = 25;
            gripSpeed = 100;

            // Current speed initialization
            leftCaterpillarSpeed = 0;
            rightCaterpillarSpeed = 0;
            gripperSpeed = 0;
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
                    leftCaterpillarSpeed = 0;
                    rightCaterpillarSpeed = 0;
                    gripperSpeed = 0;
                    break;
                case Direction.Stop:
                    leftCaterpillarSpeed = 0;
                    rightCaterpillarSpeed = 0;
                    gripperSpeed = 0;
                    break;
                case Direction.Straight_Forward:
                    leftCaterpillarSpeed = forwardSpeed;
                    rightCaterpillarSpeed = forwardSpeed;
                    gripperSpeed = 0;
                    break;
                case Direction.Straight_Backward:
                    leftCaterpillarSpeed = (sbyte) - backwardSpeed;
                    rightCaterpillarSpeed = (sbyte) - backwardSpeed;
                    gripperSpeed = 0;
                    break;
                case Direction.Left_Forward:
                    leftCaterpillarSpeed = (sbyte) -turnSpeed;
                    rightCaterpillarSpeed = (sbyte) turnSpeed;
                    gripperSpeed = 0;
                    break;
                case Direction.Right_Forward:
                    leftCaterpillarSpeed = (sbyte) turnSpeed;
                    rightCaterpillarSpeed = (sbyte) -turnSpeed;
                    gripperSpeed = 0;
                    break;
                case Direction.Left_Backward:
                    leftCaterpillarSpeed = 0;
                    rightCaterpillarSpeed = 0;
                    gripperSpeed = (sbyte) gripSpeed;
                    break;
                case Direction.Right_Backward:
                    leftCaterpillarSpeed = 0;
                    rightCaterpillarSpeed = 0;
                    gripperSpeed = (sbyte) - gripSpeed;
                    break;
                default:
                    leftCaterpillarSpeed = 0;
                    rightCaterpillarSpeed = 0;
                    gripperSpeed = 0;
                    break;
            }
            // Send the updated set point to the legs controller
            ((Snatch3r)robot).leftCaterpillarMotor.SetPower(leftCaterpillarSpeed);
            ((Snatch3r)robot).rightCaterpillarMotor.SetPower(rightCaterpillarSpeed);
            ((Snatch3r)robot).gripperMotor.SetPower(gripperSpeed);
        }
    }

    /// <summary>
    /// Periodic Task that checks for keyboards
    /// </summary>
    public class KeyboardTask : PeriodicTask
    {
        #region Constrcuctors
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
                ((Snatch3r)robot).leftCaterpillarMotor.Off();
                ((Snatch3r)robot).rightCaterpillarMotor.Off();
                ((Snatch3r)robot).gripperMotor.Off();
            }
        }
        #endregion
    }
    #endregion
}

