using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther.WindowsForms
{
    interface IPoissonSolver
    {

        void Solve(Texture2D target, Texture2D destination);

    }
}
