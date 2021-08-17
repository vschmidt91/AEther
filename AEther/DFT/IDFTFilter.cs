using System.Numerics;

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
