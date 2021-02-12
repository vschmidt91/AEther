# AEther

## Description

collection of modules for Audio analysis, Visualization with DirectX/DMX and stuff.

<img src="/doc/flowchart.svg" width="100%">

## Project Layout

- **AEther**: Core library to read and analyze sample data
- **AEther.Bass**: audio input with [Bass.NET](http://www.bass.radio42.com/)
- **AEther.Benchmarks**: using [BenchmarkDotNet](https://benchmarkdotnet.org/)
- **AEther.CLI**: Command line interface via Standard Input/Output
- **AEther.CSCore**: audio input with [CSCore](https://github.com/filoe/cscore)
- **AEther.DMX**: outputs DMX commands over Serial / COM-Port
- **AEther.NAudio**: audio input with [NAudio](https://github.com/naudio/NAudio)
- **AEther.Tests**: using [NUnit](https://nunit.org/)
- **AEther.WindowsForms**: outputs DirectX visualization with [Windows Forms](https://github.com/dotnet/winforms) and [SharpDX](http://sharpdx.org/)
