using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace TestMono.Helpers;

internal static class PlayerUtils
{
    public static List<Color> PlayerColors = new();
    
    public static void Init()
    {
        PlayerColors.Add(Color.Blue);
        PlayerColors.Add(Color.Purple);
        PlayerColors.Add(Color.IndianRed);
        PlayerColors.Add(Color.Yellow);
    }
}
