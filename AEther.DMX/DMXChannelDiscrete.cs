﻿namespace AEther.DMX
{
    public class DMXChannelDiscrete : DMXChannel
    {

        public int Count => Values.Length;

        readonly byte[] Values;

        public int Index;

        public DMXChannelDiscrete(byte[] values)
        {
            Values = values;
        }

        public override byte ByteValue => Values[Index % Values.Length];

    }
}
