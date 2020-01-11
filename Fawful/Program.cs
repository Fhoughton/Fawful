using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nintaco;

namespace Fawful
{
    class Program
    {
        private static RemoteAPI api;

        private static int strWidth;
        private static int strX;
        private static int strY;

        private static int marioX; //Can be used as success criteria as higher X = closer to goal
        private static int marioY;

        private static string facing="right";

        public static void Main(string[] args)
        {
            ApiSource.initRemoteAPI("localhost", 9999);
            api = ApiSource.API;
            Launch(api);
        }

        public static void Launch(RemoteAPI api)
        {
            api.addFrameListener(renderFinished);
            api.addStatusListener(statusChanged);
            api.addActivateListener(apiEnabled);
            api.addDeactivateListener(apiDisabled);
            api.addStopListener(dispose);
            api.run();
        }

        private static void apiEnabled()
        {
            Console.WriteLine("API enabled");
        }

        private static void apiDisabled()
        {
            Console.WriteLine("API disabled");
        }

        private static void dispose()
        {
            Console.WriteLine("API stopped");
        }

        private static void statusChanged(String message)
        {
            Console.WriteLine("Status message: {0}", message);
        }

        private static void renderFinished()
        {
            var k = api.readCPU(0x0003).ToString();

            if (k == "2")
            {
                facing = "left";
            }
            if (k == "1")
            {
                facing = "right";
            }

            api.setColor(Colors.WHITE);

            strWidth = api.getStringWidth(k, false);
            strX = (256 - strWidth) / 2;
            strY = (240 - 8) / 2;

            marioX = api.readCPU(0x6D) * 0x100 + api.readCPU(0x86);
            marioY = api.readCPU(0x03B8) + 16;

            api.drawString("Facing: " + facing, strX, strY, false);
            api.drawString("Time left: " + api.readCPU(0x07F8).ToString() + api.readCPU(0x07F9).ToString() + api.readCPU(0x07FA).ToString(), strX, strY - 16, true);
            api.drawString("Mario X: " + marioX.ToString(), strX, strY + 16, false);
            api.drawString("Mario Y: " + marioY.ToString(), strX, strY + 32, false);
            api.drawString("Powerup state: " + api.readCPU(0x0756).ToString(), strX, strY + 48, false);
        }

        private static bool fetchTile(int tileX, int tileY)
        {
            double page = Math.Floor(tileX / 256d) % 2;
            double subx = Math.Floor(tileX % 256d / 16);
            double suby = Math.Floor((tileY - 32d) / 16);

            double addr = 0x500 + page * 13 * 16 + suby * 16 + subx;

            if (suby >= 13 || suby < 0)
            {
                return false;
            }

            return (api.readCPU((int)addr) != 0);
        }
    }
}
