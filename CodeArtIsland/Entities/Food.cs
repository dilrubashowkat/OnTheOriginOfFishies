using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace OnTheOriginOfFishies.Entities
{
    public class Food : Entity
    {
        public string Name { get { return "Food"; } }
        public Vector2 Position { get; set; }
        public Vector2 Velocity;
        public float Rotation { get; set; }
        private static readonly Vector4 color = new Vector4(1.0f, 1.0f, 0.1f, 1.0f);
        public Vector4 Color { get { return color; } }

        public bool Eaten = false;

        public Vector2 Size = new Vector2(0.01f, 0.01f);

        private static Vector2 Gravity = new Vector2(0.0f, -0.1f);

        public Food(Vector2 pos)
        {
            Position = pos;
        }

        public void Update(float dt)
        {
            Velocity += Gravity * dt;
            Velocity *= 0.98f;

            Position += Velocity * dt;

            Rotation += dt;
        }

        public Vector2 Transform(ref Vector2 pos)
        {
            throw new NotImplementedException();
        }
    }
}
