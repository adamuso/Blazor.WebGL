using System;

namespace WebGame
{
    public class DebugHelper
    {
        public static void DumpArray(Array array)
        {
            for (int i = 0; i < array.Length; i++)
                Console.WriteLine($"[{i}]: {array.GetValue(i)}");   
        }
    }
}