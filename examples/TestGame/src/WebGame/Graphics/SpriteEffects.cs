using System;

namespace WebGame.Graphics
{
    [Flags]
    public enum SpriteEffects
    {
        None = 0,
        FlipVertically = 1,
        FlipHorizontally = 1 << 1,
    }
}