using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTheOriginOfFishies.Entities
{
    public interface Entity
    {
        string Name { get; }
        Vector2 Position { get; }
        Vector4 Color { get; }
        float Rotation { get; }

        void Update(float dt);
        Vector2 Transform(ref Vector2 pos);
    }
}
