namespace AEther
{
    public record SampleFormat
    (
        SampleType Type,
        int SampleRate,
        int ChannelCount
    )
    {

        public int Size => Type.Size * ChannelCount;

    }
}
