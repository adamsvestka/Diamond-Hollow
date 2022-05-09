using System;

namespace DiamondHollow
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            // using (var game = new DiamondHollowGame())
            //     game.Run();
            new DiamondHollowGame().Run();
        }
    }
}
