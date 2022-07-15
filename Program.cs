/*
 * Diamond Hollow
 * Adam Švestka, I. ročník, 34. st. skupina
 * letní semestr 2021/2022
 * Programování 2 (NPRG031)
 */

using System;

namespace DiamondHollow
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            // Game component entry point
            new DiamondHollowGame().Run();
        }
    }
}
