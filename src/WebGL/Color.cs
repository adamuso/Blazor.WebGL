using System;

namespace Blazor.WebGL
{
    public struct Color
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }
    
        public Color(float r, float g, float b, float a) 
            : this()
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public uint ToUInt32()
        {
            return (uint)(R * 255) | ((uint)(G * 255) << 8) | ((uint)(B * 255) << 16) | ((uint)(A * 255) << 24);
        }
    }
}