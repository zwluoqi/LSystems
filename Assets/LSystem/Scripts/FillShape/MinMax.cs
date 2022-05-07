namespace LSystem.Scripts.FillShape
{
    public class MinMax
    {
        public float min = float.MaxValue;
        public float max = float.MinValue;

        public void AddValue(float v)
        {
            if (v < min)
            {
                min = v;
            }

            if (v > max)
            {
                max = v;
            }
        }
    }
}