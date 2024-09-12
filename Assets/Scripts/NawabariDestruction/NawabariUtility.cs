namespace NawabariDestruction
{
    public class NawabariUtility
    {
        public static float DistanceSqrt(NawabariPoint point, float x, float y, float z)
        {
            float xd = x - point.x;
            float yd = y - point.y;
            float zd = z - point.z;
            return (xd * xd) + (yd * yd) + (zd * zd);
        }
    }
}