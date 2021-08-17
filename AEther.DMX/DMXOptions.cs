namespace AEther.DMX
{
    public class DMXOptions
    {

        public bool Enabled { get; set; } = false;
        public int COMPort { get; set; } = 0;
        public double SinuoidThreshold { get; set; } = 0.1;
        public double TransientThreshold { get; set; } = 0.1;

    }
}
