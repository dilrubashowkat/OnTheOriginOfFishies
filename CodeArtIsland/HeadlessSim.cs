using OnTheOriginOfFishies.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTheOriginOfFishies
{
    class HeadlessSim
    {
        private FishManager fishManager;
        private FoodManager foodManager;

        private int ID;

        public HeadlessSim(int id)
        {
            ID = id;

            var entities = new List<Entity>();
            var tank = new Tank();
            fishManager = new FishManager(entities, tank);
            fishManager.trackDead = false;
            fishManager.newTopPrefix = "[" + ID + "] New Top: ";
            foodManager = new FoodManager(entities, tank);
        }

        public void UpdateLoop(ulong iterations)
        {
            while (iterations-- > 0)
            {
                fishManager.Update();
                foodManager.Update();

                if (iterations % (60 * 60) == 0)
                    Console.WriteLine("[" + ID + "] Time: " + (iterations / 60 / 60));
            }

            fishManager.TopPerformer.Brain.WriteWeights(ID + "_" + fishManager.TopPerformer.Score + ".brain");
        }
    }
}
