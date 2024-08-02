using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Fructa_Database_Comparation
{
    internal class NetworkSpeed
    {
        public static int GetDatabaseSpeed(Database database, int rowSize, int rowCount, string table)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            database.executeReadQuery($"SELECT TOP {rowCount} * FROM {table}");
            sw.Stop();

            int speed = (int)Math.Round((double) (rowSize * rowCount / sw.Elapsed.Milliseconds) * 1000);

            return speed;
        }
    }
}
