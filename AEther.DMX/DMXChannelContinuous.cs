namespace AEther.DMX
{
    public class DMXChannelContinuous : DMXChannel
    {

        public double Value;

        public override byte ByteValue => (byte)(255 * Value);

    }
}
