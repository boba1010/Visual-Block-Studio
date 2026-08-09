using System.Numerics;

namespace Visual_Block_Studio.DTOs
{
    public sealed class Vector2Dto
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2Dto() { }

        public Vector2Dto(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static explicit operator Vector2(Vector2Dto v)
        {
            return new(v.X, v.Y);
        }

        public static explicit operator Vector2Dto(Vector2 v)
        {
            return new(v.X, v.Y);
        }
    }
}
