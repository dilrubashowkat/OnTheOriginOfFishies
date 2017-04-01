using OnTheOriginOfFishies.Entities;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnTheOriginOfFishies
{
    internal class FishManager
    {
        public const int MAX_FISH_COUNT = 16;

        public int FishCount = 16;
        public Fish[] FishList = new Fish[MAX_FISH_COUNT];
        public Fish TopPerformer;

        public List<Entity> Entities;
        public List<Fish> DeadFish;

        public Tank Tank;

        public bool trackDead = true;

        public string newTopPrefix = "New Top: ";

        //public long scheduledTick;

        public FishManager(List<Entity> entities, Tank tank)
        {
            Entities = entities;
            DeadFish = new List<Fish>();
            Tank = tank;

            var rnd = new Random();
            for (int i = 0; i < FishCount; i++)
            {
                FishList[i] = new Fish();
                FishList[i].Color = new Vector4(rnd.NextFloat(0, 1), rnd.NextFloat(0, 1), rnd.NextFloat(0, 1), 1);
                FishList[i].Position = new Vector2(rnd.NextFloat(Tank.Left, Tank.Right), rnd.NextFloat(Tank.Bottom, Tank.Top));
                FishList[i].Rotation = rnd.NextFloat(0, MathUtil.TwoPi);
                //GlowFish[i].Velocity = new Vector2(rnd.NextFloat(-1, 1), rnd.NextFloat(-1, 1));

                Entities.Add(FishList[i]);
            }

            //scheduledTick = DateTime.UtcNow.Ticks + 15 * TimeSpan.TicksPerSecond;
        }

        public void Update()
        {
            ////TEMP
            //FishList[0].Position = new Vector2(Input.XInputInput.State[0].LeftThumbX / (float)short.MaxValue, Input.XInputInput.State[0].LeftThumbY / (float)short.MaxValue);
            //FishList[0].Rotation = (float)Math.Atan2(Input.XInputInput.State[0].RightThumbY, Input.XInputInput.State[0].RightThumbX);
            //FishList[0].Velocity = 0;
            ////TEMP

            var rnd = new Random();

            if (Input.XInputInput.State != null)
            {
                if (((Input.XInputInput.State[0].Buttons & SharpDX.XInput.GamepadButtonFlags.A) == SharpDX.XInput.GamepadButtonFlags.A) && ((Input.XInputInput.PrevState[0].Buttons & SharpDX.XInput.GamepadButtonFlags.A) != SharpDX.XInput.GamepadButtonFlags.A))
                {
                    for (int i = 0; i < FishCount; i++)
                    {
                        Entities.Remove(FishList[i]);
                        FishList[i].FoodLevel = 0;
                        DeadFish.Add(FishList[i]);
                        FishList[i] = new Fish();
                        FishList[i].Color = new Vector4(rnd.NextFloat(0, 1), rnd.NextFloat(0, 1), rnd.NextFloat(0, 1), 1);
                        FishList[i].Position = new Vector2(rnd.NextFloat(Tank.Left, Tank.Right), rnd.NextFloat(Tank.Bottom, Tank.Top));
                        FishList[i].Rotation = rnd.NextFloat(0, MathUtil.TwoPi);
                        //GlowFish[i].Velocity = new Vector2(rnd.NextFloat(-1, 1), rnd.NextFloat(-1, 1));
                        Entities.Add(FishList[i]);
                    }
                }

                if (((Input.XInputInput.State[0].Buttons & SharpDX.XInput.GamepadButtonFlags.B) == SharpDX.XInput.GamepadButtonFlags.B) && ((Input.XInputInput.PrevState[0].Buttons & SharpDX.XInput.GamepadButtonFlags.B) != SharpDX.XInput.GamepadButtonFlags.B))// || DateTime.UtcNow.Ticks > scheduledTick)
                {
                    for (int i = 0; i < FishCount; i++)
                    {
                        FishList[i].Randomize();
                    }

                    //scheduledTick = DateTime.UtcNow.Ticks + 15 * TimeSpan.TicksPerSecond;
                }
            }


            for (int i = 0; i < FishCount; i++)
            {
                bool kill = false;

                if (FishList[i].FoodLevel <= 0)
                    kill = true;
                else
                {
                    FishList[i].RunBrain(Entities);
                    if (FishList[i].Position.X < Tank.Left)
                    {
                        //FishList[i].Velocity.X = Math.Abs(FishList[i].Velocity.X);
                        FishList[i].Position = new Vector2(Tank.Left, FishList[i].Position.Y);
                        if (FishList[i].Rotation < MathUtil.Pi)
                            FishList[i].Rotation = MathUtil.Pi - FishList[i].Rotation;
                        else
                            FishList[i].Rotation = MathUtil.TwoPi - (FishList[i].Rotation - MathUtil.Pi);

                        FishList[i].Velocity *= 0.5f;
                        FishList[i].FoodLevel *= 0.8f;
                        //kill = true;
                    }
                    if (FishList[i].Position.Y < Tank.Bottom)
                    {
                        //FishList[i].Velocity.Y = Math.Abs(FishList[i].Velocity.Y);
                        FishList[i].Position = new Vector2(FishList[i].Position.X, Tank.Bottom);
                        FishList[i].Rotation = MathUtil.TwoPi - FishList[i].Rotation;

                        FishList[i].Velocity *= 0.5f;
                        FishList[i].FoodLevel *= 0.8f;
                        //kill = true;
                    }

                    if (FishList[i].Position.X > Tank.Right)
                    {
                        //FishList[i].Velocity.X = -Math.Abs(FishList[i].Velocity.X);
                        FishList[i].Position = new Vector2(Tank.Right, FishList[i].Position.Y);
                        FishList[i].Rotation = MathUtil.Pi - FishList[i].Rotation;

                        FishList[i].Velocity *= 0.5f;
                        FishList[i].FoodLevel *= 0.8f;
                        //kill = true;
                    }
                    if (FishList[i].Position.Y > Tank.Top)
                    {
                        //FishList[i].Velocity.Y = -Math.Abs(FishList[i].Velocity.Y);
                        FishList[i].Position = new Vector2(FishList[i].Position.X, Tank.Top);
                        FishList[i].Rotation = MathUtil.TwoPi - FishList[i].Rotation;

                        FishList[i].Velocity *= 0.5f;
                        FishList[i].FoodLevel *= 0.8f;
                        //kill = true;
                    }
                }

                if (kill)
                {
                    if (TopPerformer == null || FishList[i].Score > TopPerformer.Score)
                        SetTopPerformer(FishList[i]);
                    Entities.Remove(FishList[i]);
                    FishList[i].FoodLevel = 0;
                    if (trackDead)
                        DeadFish.Add(FishList[i]);

                    var sorted = FishList.OrderBy( f => f?.Score );
                    /*int highestFish = 0;
                    for (int j = 1; j < FishList.Length; j++)
                        if (FishList[j].Score > FishList[highestFish].Score)
                            highestFish = j;*/
                    Fish selFish = null;
                    //selFish = SelectBrainFile(rnd);//new Fish("brains/1_12414.8833815698.brain");
                    if (rnd.NextDouble() > 0.9 && selFish != null)
                        selFish = TopPerformer;
                    for (int j = MAX_FISH_COUNT - FishCount; j < FishCount && selFish == null; j++)
                    {
                        if (TopPerformer == null || sorted.ElementAt(j).Score > TopPerformer.Score)
                            SetTopPerformer(sorted.ElementAt(j));
                        if (rnd.NextDouble() > 0.75)
                            selFish = sorted.ElementAt(j);
                    }

                    if (selFish != null)
                    {
                        FishList[i] = new Fish(selFish, rnd.NextDouble() * 1.0);
                    }
                    else
                    {
                        FishList[i] = new Fish();
                        FishList[i].Color = new Vector4(rnd.NextFloat(0, 1), rnd.NextFloat(0, 1), rnd.NextFloat(0, 1), 1);
                    }
                    FishList[i].Position = new Vector2(rnd.NextFloat(Tank.Left, Tank.Right), rnd.NextFloat(Tank.Bottom, Tank.Top));
                    FishList[i].Rotation = rnd.NextFloat(0, MathUtil.TwoPi);
                    Entities.Add(FishList[i]);
                }
                else
                {
                    foreach (var e in Entities)
                        if (e is Food)
                        {
                            var f = (Food)e;
                            if (!f.Eaten)
                                FishList[i].TryEat(f);
                        }
                }
            }

            foreach (var e in Entities)
                e.Update(1.0f / 60.0f);
        }

        private Fish SelectBrainFile(Random rnd)
        {
            var files = Directory.GetFiles("brains");
            while (files.Length > 0)
                for (int i = 0; i < files.Length; i++)
                    if (rnd.NextDouble() < 0.01)
                        return new Fish(files[i]);

            return null;
        }

        private void SetTopPerformer(Fish fish)
        {
            TopPerformer = fish;
            Console.WriteLine(newTopPrefix + fish.Score);
        }
    }
}
