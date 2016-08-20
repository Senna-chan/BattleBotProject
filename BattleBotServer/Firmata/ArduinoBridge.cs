using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Firmata.NET;

namespace BattleBotServer.Firmata
{
    public class ArduinoBridge
    {
        public static Arduino arduino;

        public ArduinoBridge()
        {
            if (File.Exists("/dev/ttyUSB0"))
            {
                arduino = new Arduino("/dev/ttyUSB0", 57600);
            }
        }

        public static void PrintAnalog()
        {
            while (true)
            {
                if (arduino.digitalRead(2) == 1)
                {
                    Console.WriteLine(arduino.analogRead(0));
                    Console.WriteLine(arduino.analogRead(1));
                    Console.WriteLine(arduino.analogRead(2));
                    Console.WriteLine(arduino.analogRead(3));
                }
                Thread.Sleep(50);
            }
        }
    }
}
