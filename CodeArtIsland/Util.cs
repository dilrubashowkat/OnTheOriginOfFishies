using System;

namespace OnTheOriginOfFishies
{
    internal static class Util
    {
        private static Random rnd = new Random();

        public const float PI = (float)Math.PI;

        public static float RndFloat(float min, float max)
        {
            return (float)(rnd.NextDouble() * (max - min) + min);
        }

        public static double RndDouble()
        {
            return rnd.NextDouble();
        }

        public static double RndDouble(double min, double max)
        {
            return rnd.NextDouble() * (max - min) + min;
        }

        public static float Clamp(float v, float min, float max)
        {
            return v < min ? min : v > max ? max : v;
        }

        internal static int RndInt(int min, int max)
        {
            return rnd.Next(min, max);
        }
    }
}
