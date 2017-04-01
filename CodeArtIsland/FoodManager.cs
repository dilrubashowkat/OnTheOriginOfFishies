using OnTheOriginOfFishies.Entities;
using SharpDX;
using System;
using System.Collections.Generic;

namespace OnTheOriginOfFishies
{
    public class FoodManager
    {
        internal List<Food> FoodList;
        private List<Food> FoodListTR;
        private float SpawnChance = 0.025f;

        public List<Entity> Entities;

        public Tank Tank;

        public FoodManager(List<Entity> entities, Tank tank)
        {
            Entities = entities;
            Tank = tank;

            FoodList = new List<Food>();
            FoodListTR = new List<Food>();
        }

        public void Update()
        {
            if (SpawnChance * 2 > Util.RndDouble())
            {
                var f = new Food(new Vector2(Util.RndFloat(Tank.Left, Tank.Right), Tank.Top));
                FoodList.Add(f);
                Entities.Add(f);
            }

            foreach (var f in FoodList)
                if (f.Position.Y < Tank.Bottom || f.Eaten)
                {
                    FoodListTR.Add(f);
                    Entities.Remove(f);
                }
                else
                {
                    f.Update(1.0f / 60.0f);
                }
            foreach (var f in FoodListTR)
                FoodList.Remove(f);
            FoodListTR.Clear();
        }
    }
}
