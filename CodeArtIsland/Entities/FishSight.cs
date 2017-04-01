using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTheOriginOfFishies.Entities
{
    public class EntitySight
    {
        public Entity Owner;

        public Vector4 Color { get; private set; }
        public float Distance { get; private set; }

        public Vector2 Start;
        public float MidAngle;
        public float WidthAngle;
        public float MaxDistance;

        public EntitySight(Entity owner, Vector2 start, float maxDist, float midAngle, float widthAngle)
        {
            Owner = owner;
            Start = start;
            MidAngle = midAngle;
            WidthAngle = widthAngle;
            MaxDistance = maxDist;
            Distance = MaxDistance;
            Color = new Vector4(0, 0, 0, 0);
        }

        public void Update(IList<Entity> entities)
        {
            Distance = MaxDistance * MaxDistance;
            Color = Vector4.Zero;
            Vector2 startPos = Owner.Transform(ref Start);
            foreach (var e in entities)
            {
                if (e == Owner)
                    continue;

                var dx = e.Position.X - startPos.X;
                var dy = e.Position.Y - startPos.Y;
                var angle = MathUtil.Mod2PI((float)Math.Atan2(dy, dx));
                if (angle < 0)
                    angle += MathUtil.TwoPi;

                var minAngle = MathUtil.Mod2PI(MidAngle - WidthAngle / 2.0f + Owner.Rotation);
                if (minAngle < 0)
                    minAngle += MathUtil.TwoPi;
                var maxAngle = MathUtil.Mod2PI(MidAngle + WidthAngle / 2.0f + Owner.Rotation);
                if (maxAngle < 0)
                    maxAngle += MathUtil.TwoPi;

                if (maxAngle < minAngle)
                {
                    maxAngle += MathUtil.TwoPi;
                    angle += MathUtil.TwoPi;
                }

                if (minAngle < maxAngle)
                    if (angle < minAngle || angle > maxAngle)
                        continue;

                var distSqr = dx * dx + dy * dy;
                if (distSqr < Distance)
                {
                    Distance = distSqr;
                    Color = e.Color;
                }
            }

            Distance = (float)Math.Sqrt(Distance);
        }
    }
}
