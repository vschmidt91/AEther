using System.Security.Cryptography;

namespace AEther.CLI
{
    class Program
    {
        static async void Main(string path)
        {

            //path = Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..", "TestFiles", "test_input.wav");
            //path = new FileInfo(path).FullName;

            var options = new AnalyzerOptions();

            using var inputStream = Console.OpenStandardInput();
            using var outputStream = new MemoryStream();
            using var standardOutput = Console.OpenStandardOutput();
            using var sampleSource = new WAVReader(File.OpenRead(path));

            var analyzer = new Analyzer(sampleSource.Format, options);
            var hash = MD5.Create();
            byte[] buffer = Array.Empty<byte>();

            analyzer.SamplesAnalyzed += async (obj, evt) =>
            {
                var byteCount = sizeof(double) * evt.SampleCount;
                if (buffer.Length < byteCount)
                {
                    buffer = new byte[byteCount];
                }
                Buffer.BlockCopy(evt.Samples, 0, buffer, 0, byteCount);

                Console.WriteLine(BitConverter.ToString(hash.ComputeHash(buffer, 0, byteCount)));
                await outputStream.WriteAsync(buffer.AsMemory(0, byteCount));
            };

            sampleSource.Start();
            await analyzer.Completion;

        }

    }
}
