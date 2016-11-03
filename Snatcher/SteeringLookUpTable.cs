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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallRobots.Snatch3r
{
    public class Range
    {
        public double MinExtreme { get; set; }
        public double MaxExtreme { get; set; }

        public double Value { get; set; }

        public Range()
        {
            MinExtreme = 0;
            MaxExtreme = 0;
            Value = 0;
        }

        public Range (double theMinExtreme, double theMaxExtreme, double theValue)
        {
            MinExtreme = theMinExtreme;
            MaxExtreme = theMaxExtreme;
            Value = theValue;
        }
    }

    public class SteeringLookUpTable
    {
        List<Range> lookUpTable;

        public SteeringLookUpTable()
        {
            lookUpTable = new List<Range>();

            lookUpTable.Add(new Range( 0, 10 , 60));
            lookUpTable.Add(new Range(11, 20, 40));
            lookUpTable.Add(new Range(21, 30, 20));
            lookUpTable.Add(new Range(31, 35, 10));
            lookUpTable.Add(new Range(36, 40, 5));
            lookUpTable.Add(new Range(41, 45, -5));
            lookUpTable.Add(new Range(46, 50, -10));
            lookUpTable.Add(new Range(51, 60, -20));
            lookUpTable.Add(new Range(61, 70, -40));
            lookUpTable.Add(new Range(71, 100, -80));
        }

        public double GetValue(double theKey)
        {
            double retValue = 0.0d;

            for (int i = 0; i < lookUpTable.Count; i++)
            {
                if ((theKey >= lookUpTable[i].MinExtreme) && (theKey < lookUpTable[i].MaxExtreme))
                {
                    retValue = lookUpTable[i].Value;
                    break;
                }
            }

            return retValue;
        }
    }
}
