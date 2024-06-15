using System;
using System.Threading.Tasks;

namespace MatchMaker
{
    public abstract class CallbackOnKeyPress
    {
        public static async Task Run(ConsoleKey expectedKey, Action callback)
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var readKey = Console.ReadKey();
                    if (readKey.Key == expectedKey)
                    {
                        callback();
                        break;
                    }
                }

                await Task.Delay(50);
            }
        }
    }
}