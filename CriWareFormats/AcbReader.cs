using CriWareFormats.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace CriWareFormats
{
    public sealed class AcbReader : IDisposable
    {
        private readonly Stream outerStream;
        private readonly long offset;
        private readonly uint awbOffset;
        private readonly uint awbLength;

        private readonly AcbParser acbParser;

        public AcbReader(Stream acbStream) : this(acbStream, 0) { }

        public AcbReader(Stream acbStream, long positionOffset)
        {
            outerStream = acbStream;
            offset = positionOffset;

            acbStream.Position = offset;

            UtfTable utfTable = new(acbStream, (uint)offset, out int rows, out string name);

            if (rows != 1 || !name.Equals("Header"))
                throw new InvalidDataException("No Header table.");

            if (!utfTable.Query(0, "AwbFile", out VLData awbValueData))
                throw new InvalidDataException("No embedded AWB file.");

            awbOffset = awbValueData.Offset;
            awbLength = awbValueData.Size;

            outerStream.Position = positionOffset;
            acbParser = new AcbParser(outerStream);
        }

        public AwbReader GetAwb()
        {
            return new AwbReader(new SpliceStream(outerStream, awbOffset, awbLength), true);
        }

        public string GetWaveName(int waveId, int port, bool memory)
        {
            outerStream.Position = offset;
            return acbParser.LoadWaveName(waveId, port, memory);
        }

        public int GetWaveIdFromCueId(int cueId)
        {
            outerStream.Position = offset;
            return acbParser.WaveIdFromCueId(cueId);
        }

        public void Dispose()
        {
            outerStream.Dispose();
        }
    }
}