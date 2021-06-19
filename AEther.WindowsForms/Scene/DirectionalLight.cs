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

        public Vector3 Direction => Vector3.Normalize(Transform.Translation);

        public override IEnumerable<string> GetSwitches()
        {
            foreach(var define in base.GetSwitches())
            {
                yield return define;
            }
            yield return "DIRECTIONAL";
        }

    }
}
