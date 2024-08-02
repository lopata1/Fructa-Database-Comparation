using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

using Force.Crc32;

namespace Fructa_Database_Comparation
{
    internal class Checksum
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Get(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            return (int) Crc32Algorithm.ComputeAndWriteToEnd(bytes);
        }
    }
}
