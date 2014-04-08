using System;

namespace Trashdroids
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (TrashdroidsGame game = new TrashdroidsGame())
            {
                game.Run();
            }
        }
    }
#endif
}

