using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CriWareFormats
{
    struct CueName
    {
        public ushort CueIndex;
        public string Name;
    }

    struct Cue
    {
        public byte ReferenceType;
        public ushort ReferenceIndex;
    }

    struct BlockSequence
    {
        public ushort NumTracks;
        public uint TrackIndexOffset;
        public uint TrackIndexSize;
        public ushort NumBlocks;
        public uint BlockIndexOffset;
        public uint BlockIndexSize;
    }

    struct Block
    {
        public ushort NumTracks;
        public uint TrackIndexOffset;
        public uint TrackIndexSize;
    }

    struct Sequence
    {
        public ushort NumTracks;
        public uint TrackIndexOffset;
        public uint TrackIndexSize;
        public byte Type;
    }

    struct Track
    {
        public ushort EventIndex;
    }

    struct TrackCommand
    {
        public uint CommandOffset;
        public uint CommandSize;
    }

    struct Synth
    {
        public byte Type;
        public uint ReferenceItemsOffset;
        public uint ReferenceItemsSize;
    }

    struct WaveForm
    {
        public ushort Id;
        public ushort PortNo;
        public byte Streaming;
    }

    static class AcbNameLoader
    {
        static Stream acbStream;

        static UtfTable header;
        static UtfTable cueNames;

        static BinaryReader cueReader;
        static BinaryReader cueNameReader;
        static BinaryReader blockSequenceReader;
        static BinaryReader blockReader;
        static BinaryReader sequenceReader;
        static BinaryReader trackReader;
        static BinaryReader trackCommandReader;
        static BinaryReader synthReader;
        static BinaryReader waveFormReader;

        static Cue[] cue;
        static CueName[] cueName;
        static BlockSequence[] blockSequence;
        static Block[] block;
        static Sequence[] sequence;
        static Track[] track;
        static TrackCommand[] trackCommand;
        static Synth[] synth;
        static WaveForm[] waveForm;

        static int cueRows = -1;
        static int cueNameRows = -1;
        static int blockSequenceRows = -1;
        static int blockRows = -1;
        static int sequenceRows = -1;
        static int trackRows = -1;
        static int trackCommandRows = -1;
        static int synthRows = -1;
        static int waveFormRows = -1;

        static bool isMemory;
        static int targetWaveId;
        static int targetPort;

        static int synthDepth;
        static int sequenceDepth;

        static short cueNameIndex;
        static string cueNameName;
        static int awbNameCount;
        static short[] awbNameList;
        static string name;

        static bool OpenUtfSubtable(out BinaryReader tableReader, out UtfTable table, string tableName, out uint rows)
        {
            if (!header.Query(0, tableName, out VLData data))
                throw new ArgumentException("Error reading table.");

            tableReader = new(acbStream);
            table = new(tableReader.BaseStream, data.Offset, out rows, out string _);

            return true;
        }

        static void PreloadAcbWaveForm()
        {
            if (!OpenUtfSubtable(out waveFormReader, out UtfTable table, "WaveFormTable", out uint pRows))
                throw new Exception();

            int cId = table.GetColumn("Id");
            int cMemoryAwbId = table.GetColumn("MemoryAwbId");
            int cStreamAwbId = table.GetColumn("StreamAwbId");
            int cStreamAwbPortNo = table.GetColumn("StreamAwbPortNo");
            int cStreaming = table.GetColumn("Streaming");

            waveForm = new WaveForm[pRows];
            for (int i = 0; i < pRows; i++)
            {
                if (!table.Query(i, cId, out waveForm[i].Id))
                {
                    if (isMemory)
                    {
                        table.Query(i, cMemoryAwbId, out waveForm[i].Id);
                        waveForm[i].PortNo = 0xFFFF;
                    }
                    else
                    {
                        table.Query(i, cStreamAwbId, out waveForm[i].Id);
                        table.Query(i, cStreamAwbPortNo, out waveForm[i].PortNo);
                    }
                }
                else
                    waveForm[i].PortNo = 0xFFFF;

                table.Query(i, cStreaming, out waveForm[i].Streaming);
            }

            table.Dispose();
        }
    }
}
