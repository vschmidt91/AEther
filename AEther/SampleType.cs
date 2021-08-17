namespace AEther
{

    public abstract class SampleType
    {

        public abstract int Size { get; }
        public abstract double ReadFrom(ReadOnlySpan<byte> source);

        public class Float32 : SampleType
        {
            public static readonly Float32 Instance = new();
            public override int Size => 4;
            public override double ReadFrom(ReadOnlySpan<byte> source) => BitConverter.ToSingle(source);
        }

        public class UInt16 : SampleType
        {
            public const double Scale = 1 / (double)short.MaxValue;
            public static readonly UInt16 Instance = new();
            public override int Size => 2;
            public override double ReadFrom(ReadOnlySpan<byte> source) => BitConverter.ToInt16(source) * Scale;
        }

    }

}
