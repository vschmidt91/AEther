using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.DXGI;
using SharpDX.Direct3D11;

using Feature = SharpDX.Direct3D11.Feature;

namespace AEther.WindowsForms
{
    public class GraphicsCapabilities
    {

        static readonly int[] MultiSampleLevelCandididates = new[] { 1, 2, 4, 8 };

        public readonly Dictionary<Format, ComputeShaderFormatSupport> ComputeShaderFormats = new Dictionary<Format, ComputeShaderFormatSupport>();
        public readonly FeatureDataD3D11Options D3D11Features;
        public readonly FeatureDataD3D11Options1 D3D11Features1;
        public readonly FeatureDataD3D11Options2 D3D11Features2;
        public readonly FeatureDataD3D11Options3 D3D11Features3;
        public readonly Dictionary<Format, FormatSupport> Formats = new Dictionary<Format, FormatSupport>();
        public readonly Dictionary<Format, Dictionary<int, int>> MultiSampleLevel = new Dictionary<Format, Dictionary<int, int>>();
        public readonly bool FullNonPow2Textures;
        public readonly FeatureDataShaderMinimumPrecisionSupport ShaderMinimumPrecision;
        public readonly bool Threading;
        public readonly bool ConcurrentResources;
        public readonly bool CommandLists;
        public readonly bool TileBasedDeferredRenderer;

        public GraphicsCapabilities(SharpDX.Direct3D11.Device device)
        {

            foreach (var format in Enum.GetValues(typeof(Format)).Cast<Format>())
            {

                Formats[format] = device.CheckFormatSupport(format);
                ComputeShaderFormats[format] = device.CheckComputeShaderFormatSupport(format);

                MultiSampleLevel[format] = new Dictionary<int, int>();
                foreach (int n in MultiSampleLevelCandididates)
                    MultiSampleLevel[format][n] = device.CheckMultisampleQualityLevels(format, n);

            }

            D3D11Features = device.CheckD3D11Feature();
            D3D11Features1 = device.CheckD3D112Feature();
            D3D11Features2 = device.CheckD3D113Features2();
            D3D11Features3 = device.CheckD3D113Features3();

            FullNonPow2Textures = device.CheckFullNonPow2TextureSupport();
            ShaderMinimumPrecision = device.CheckShaderMinimumPrecisionSupport();
            Threading = device.CheckThreadingSupport(out ConcurrentResources, out CommandLists).Success;
            TileBasedDeferredRenderer = device.CheckTileBasedDeferredRendererSupport();
            
        }

    }
}
