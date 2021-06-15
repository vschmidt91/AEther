using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther.WindowsForms
{
    public class DirectionalLight : Light
    {

        public Vector3 Direction => Vector3.Normalize(Position);

        public override IEnumerable<string> GetDefines()
        {
            foreach(var define in base.GetDefines())
            {
                yield return define;
            }
            yield return "DIRECTIONAL";
        }

    }
}
