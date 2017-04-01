using System;
using SharpDX;
using System.Collections.Generic;

namespace OnTheOriginOfFishies.Entities
{
    public class Fish : Entity
    {
        public string Name { get; set; }
        public Vector2 Position { get; set; }
        public float Velocity;
        public float Rotation { get; set; }
        public Vector4 Color { get; set; }
        public Vector2 EyePosition;
        public EntitySight[] Sights;

        public int MeshIndex;

        public float Roll;

        public Vector2 Size = new Vector2(0.05f, 0.05f);

        public Neural.Brain Brain;

        public double Score;

        public float FoodLevel = 20.0f;

        public int Seed;

        public double Time;

        public Fish()
        {
            SetupSight();
            Randomize();
            Seed = (int)Util.RndFloat(-10000, 10000);
            MeshIndex = Util.RndInt(0, 3);
        }

        public Fish(Fish other, double rndAmount)
        {
            SetupSight();
            Brain = new Neural.Brain(other.Brain);
            Brain.Randomize(rndAmount);
            rndAmount /= 5.0;
            Color = new Vector4(MathUtil.Clamp(other.Color.X + Util.RndFloat(-(float)rndAmount, (float)rndAmount), 0.0f, 1.0f), MathUtil.Clamp(other.Color.Y + Util.RndFloat(-(float)rndAmount, (float)rndAmount), 0.0f, 1.0f), MathUtil.Clamp(other.Color.Z + Util.RndFloat(-(float)rndAmount, (float)rndAmount), 0.0f, 1.0f), 1.0f);

            //Temp
            var mag = Color.X * Color.X + Color.Y * Color.Y + Color.Z * Color.Z;
            mag = (float)Math.Sqrt(mag);
            Color = new Vector4(Color.X / mag, Color.Y / mag, Color.Z / mag, Color.W);
            //====

            MeshIndex = Util.RndInt(0, 3);
        }

        public Fish(string filename)
        {
            SetupSight();
            Brain = new Neural.Brain(4);
            Brain.FullyConnect(5 + 5 * 4 + 1, 30, 15, 2);
            Brain.ReadWeights(filename);

            Color = new Vector4(Util.RndFloat(0, 1), Util.RndFloat(0, 1), Util.RndFloat(0, 1), 1.0f);

            //Temp
            var mag = Color.X * Color.X + Color.Y * Color.Y + Color.Z * Color.Z;
            mag = (float)Math.Sqrt(mag);
            Color = new Vector4(Color.X / mag, Color.Y / mag, Color.Z / mag, Color.W);
            //====

            MeshIndex = Util.RndInt(0, 3);
        }

        private void SetupSight()
        {
            EyePosition = new Vector2(Size.X, 0);

            Sights = new EntitySight[]
            {
                //new EntitySight(this, EyePosition, 0.20f, 0.0f, MathUtil.DegreesToRadians(30.0f))
                new EntitySight(this, EyePosition, 1.00f, MathUtil.DegreesToRadians(-40.0f), MathUtil.DegreesToRadians(20.0f)),
                new EntitySight(this, EyePosition, 1.00f, MathUtil.DegreesToRadians(-20.0f), MathUtil.DegreesToRadians(20.0f)),
                new EntitySight(this, EyePosition, 1.00f, MathUtil.DegreesToRadians(0.0f), MathUtil.DegreesToRadians(20.0f)),
                new EntitySight(this, EyePosition, 1.00f, MathUtil.DegreesToRadians(20.0f), MathUtil.DegreesToRadians(20.0f)),
                new EntitySight(this, EyePosition, 1.00f, MathUtil.DegreesToRadians(40.0f), MathUtil.DegreesToRadians(20.0f)),
            };
        }

        public void Update(float dt)
        {
            Time += dt;

            Velocity += (float)Brain.Layers[3][0].Sum * dt;
            if (Velocity < 0.1f)
                Velocity = 0.1f;
            else if (Velocity > 1.0f)
                Velocity = 1.0f;
            Rotation += MathUtil.Clamp((float)Brain.Layers[3][1].Sum, -15.0f, 15.0f) * dt;
            Rotation = MathUtil.Wrap(Rotation, 0, MathUtil.TwoPi);

            Velocity *= 0.99f;

            Position += Velocity * dt * new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));

            Score += dt;
            FoodLevel -= dt;
            FoodLevel -= Math.Abs(MathUtil.Clamp((float)Brain.Layers[3][1].Sum, -15.0f, 15.0f) * dt) * 0.25f;

            if (Rotation > MathUtil.PiOverTwo && Rotation <= MathUtil.PiOverTwo * 3)
                Roll = MathUtil.Clamp(Roll + dt * 5, 0, MathUtil.Pi);
            else
                Roll = MathUtil.Clamp(Roll - dt * 5, 0, MathUtil.Pi);
        }

        public void TryEat(Food food)
        {
            var dx = food.Position.X - Position.X;
            var dy = food.Position.Y - Position.Y;

            if (dx * dx + dy * dy < Size.X * Size.Y)
            {
                Score += 30.0;

                food.Eaten = true;
                FoodLevel = 20.0f;
            }
        }

        public void RunBrain(IList<Entity> entities)
        {
            for (int i = 0; i < Sights.Length; i++)
                Sights[i].Update(entities);

            Brain.Reset();
            Brain.Step(Position.X, Position.Y, Velocity, MathUtil.Mod2PI(Rotation), FoodLevel,
                Sights[0].Color.X * 10.0f, Sights[0].Color.Y * 10.0f, Sights[0].Color.Z * 10.0f, Sights[0].Distance * 10.0f,
                Sights[1].Color.X * 10.0f, Sights[1].Color.Y * 10.0f, Sights[1].Color.Z * 10.0f, Sights[1].Distance * 10.0f,
                Sights[2].Color.X * 10.0f, Sights[2].Color.Y * 10.0f, Sights[2].Color.Z * 10.0f, Sights[2].Distance * 10.0f,
                Sights[3].Color.X * 10.0f, Sights[3].Color.Y * 10.0f, Sights[3].Color.Z * 10.0f, Sights[3].Distance * 10.0f,
                Sights[4].Color.X * 10.0f, Sights[4].Color.Y * 10.0f, Sights[4].Color.Z * 10.0f, Sights[4].Distance * 10.0f,
                Math.Sin(Time) * 10.0);
        }

        internal void Randomize()
        {
            Brain = new Neural.Brain(4);
            Brain.FullyConnect(5 + 5 * 4 + 1, 30, 15, 2);
            Brain.Randomize(1);
        }

        public Vector2 Transform(ref Vector2 pos)
        {
            return new Vector2((float)Math.Cos(Rotation) * pos.X + (float)Math.Sin(Rotation) * pos.Y + Position.X, (float)Math.Sin(Rotation) * pos.X + (float)Math.Cos(Rotation) * pos.Y + Position.Y);
        }
    }
}
