namespace Runtime.Utility
{
    public static class Curves
    {
        public static float Smootherstep(float x) => x * x * x * (x * (6.0f * x - 15.0f) + 10.0f);
    }
}