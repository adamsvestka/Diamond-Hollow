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
