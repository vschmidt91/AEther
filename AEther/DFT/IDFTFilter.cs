using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AEther
{

    public interface IDFTFilter
    {

        int Length { get; }

        Complex GetOutput();

        void Process(double newSample);

        void Process(ReadOnlyMemory<double> samples);

    }

}
