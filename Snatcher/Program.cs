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

using System;
using MonoBrickFirmware;
using MonoBrickFirmware.Display.Dialogs;
using MonoBrickFirmware.Display;
using MonoBrickFirmware.Movement;
using System.Threading;
using MonoBrickFirmware.Display.Menus;
using SmallRobots.Ev3ControlLib.Menu;

namespace SmallRobots.Snatch3r
{
    class Program
    {
        #region Static fields
        static public MenuContainer container;
        #endregion
        static void Main(string[] args)
        {
            Menu menu = new Menu("SNATCH3R");
            container = new MenuContainer(menu);

            menu.AddItem(new MainMenuItem("Command with IR", CommandWithIR_OnEnterPressed));
            menu.AddItem(new MainMenuItem("Line follwing", LineFollowing_OnEnterPressed));
            menu.AddItem(new MainMenuItem("Garbage collection", GarbageCollection_OnEnterPressed));
            menu.AddItem(new MainMenuItem("Quit", Quit_OnEnterPressed));

            container.Show();
        }

        private static void LineFollowing_OnEnterPressed()
        {
            container.SuspendButtonEvents();
            Snatch3r snatch3r = new Snatch3r(Snatch3rBehaviour.LineFollowing);
            snatch3r.Start();
            container.ResumeButtonEvents();
        }

        private static void Quit_OnEnterPressed()
        {
            LcdConsole.Clear();
            LcdConsole.WriteLine("Terminating");
            // Wait a bit
            Thread.Sleep(1000);
            TerminateMenu();
        }

        public static void TerminateMenu()
        {
            container.Terminate();
        }

        private static void CommandWithIR_OnEnterPressed()
        {
            container.SuspendButtonEvents();
            Snatch3r snatch3r = new Snatch3r(Snatch3rBehaviour.CommandedRemotely);
            snatch3r.Start();
            container.ResumeButtonEvents();
        }


        private static void GarbageCollection_OnEnterPressed()
        {
            container.SuspendButtonEvents();
            Snatch3r snatch3r = new Snatch3r(Snatch3rBehaviour.GarbageCollection);
            snatch3r.Start();
            container.ResumeButtonEvents();
        }
    }
}
