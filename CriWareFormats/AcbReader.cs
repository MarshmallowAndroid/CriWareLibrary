using CriWareFormats.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace CriWareFormats
{
    public sealed class AcbReader : IDisposable
    {
        private readonly Stream innerStream;
        private readonly long positionOffset;
        private readonly uint awbOffset;
        private readonly uint awbLength;

        public AcbReader(Stream acbStream) : this(acbStream, 0) { }

        public AcbReader(Stream acbStream, long offset)
        {
            innerStream = acbStream;
            positionOffset = offset;

            acbStream.Position = positionOffset;

            UtfTable utfTable = new(acbStream, out uint rows, out string name);

            if (rows != 1 || !name.Equals("Header"))
                throw new InvalidDataException("No Header table.");

            if (!utfTable.Query(0, "AwbFile", out VLData awbValueData))
                throw new InvalidDataException("No embedded AWB file.");

            awbOffset = awbValueData.Offset;
            awbLength = awbValueData.Size;
        }

        public AwbReader GetAwb()
        {
            return new AwbReader(new SpliceStream(innerStream, awbOffset, awbLength), true);
        }

        public void Dispose()
        {
            innerStream.Dispose();
        }
    }
}