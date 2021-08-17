namespace AEther.DMX
{
    public static class EuroliteLEDMultiFXBar
    {

        public static DMXChannel[] Create() => new DMXChannel[]
        {
            new DMXChannelContinuous(),
            new DMXChannelContinuous(),
            new DMXChannelDiscrete(new byte[]{ 0 }),
            new DMXChannelDiscrete(new byte[]{ 0, 10, 18, 26, 34, 42, 50, 58, 66, 74, 82, 90, 98, 106, 114, 122, 130, 138, 146, 154, 162, 170, 178, 186, 194, 202, 210, 218, 226, 234, 242, 250 }),
            new DMXChannelDiscrete(new byte[]{ 0 }),
            new DMXChannelDiscrete(new byte[]{ 0, 27, 69, 111, 153, 195, 237 }),
            new DMXChannelDiscrete(new byte[]{ 0 }),
            //new DMXChannelContinuous(),
            new DMXChannelDiscrete(new byte[]{ 0, 127, 255 }),
            //new DMXChannelDiscrete(new byte[]{ 0, 18, 40, 62, 84, 106, 128, 150, 172, 200, 222, 242 }),
            new DMXChannelDiscreteThresholds(new (byte, double)[]{ (0, 0.0), (200, 0.5), (20, 1.0) }),
            new DMXChannelDiscrete(new byte[]{ 0 }),
        };

    }
}
