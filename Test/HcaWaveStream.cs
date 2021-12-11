using ClHcaSharp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class HcaWaveStream : WaveStream
    {
        private readonly Stream hcaFileStream;
        private readonly BinaryReader hcaFileReader;
        private readonly HcaDecoder decoder;
        private readonly HcaInfo info;
        private readonly long dataStart;
        private readonly object positionLock = new();

        private readonly short[][] sampleBuffer;
        private readonly short[] sampleBuffer1D;

        private long samplePosition;

        public HcaWaveStream(Stream hcaFile, ulong key)
        {
            hcaFileStream = hcaFile;
            hcaFileReader = new(hcaFile);
            decoder = new(hcaFile, key);
            info = decoder.GetInfo();
            dataStart = hcaFile.Position;

            sampleBuffer = new short[info.ChannelCount][];
            for (int i = 0; i < info.ChannelCount; i++)
            {
                sampleBuffer[i] = new short[info.SamplesPerBlock];
            }

            sampleBuffer1D = new short[info.ChannelCount * info.SamplesPerBlock];

            samplePosition = info.EncoderDelay;

            int block = (int)(samplePosition / info.SamplesPerBlock);
            FillBuffer(block);

            WaveFormat = new WaveFormat((int)info.SamplingRate, (int)info.ChannelCount);
        }

        public HcaInfo Info => info;

        public bool Loop { get; set; }

        public override WaveFormat WaveFormat { get; }

        public override long Length => info.SampleCount * info.ChannelCount * sizeof(short);

        public override long Position
        {
            get
            {
                lock (positionLock)
                {
                    return (samplePosition - info.EncoderDelay) * info.ChannelCount * sizeof(short);
                }
            }
            set
            {
                lock (positionLock)
                {
                    samplePosition = value / info.ChannelCount / sizeof(short);
                    samplePosition += info.EncoderDelay;

                    int block = (int)(samplePosition / info.SamplesPerBlock);
                    FillBuffer(block);
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (positionLock)
            {
                int read = 0;

                for (int i = 0; i < count / info.ChannelCount / sizeof(short); i++)
                {
                    if (Position >= Length) break;

                    if (samplePosition - info.EncoderDelay == info.SampleCount)
                        break;
                    else if (samplePosition - info.EncoderDelay == info.LoopEndSample && Loop)
                    {
                        FillBuffer((int)info.LoopStartBlock);

                        samplePosition = info.LoopStartSample + info.EncoderDelay;
                    }

                    if (samplePosition % info.SamplesPerBlock == 0) FillBuffer();

                    for (int j = 0; j < info.ChannelCount; j++)
                    {
                        //int bufferOffset = (int)((i * info.ChannelCount + j) * sizeof(short));
                        //buffer[offset + bufferOffset] = (byte)sampleBuffer[j][samplePosition % info.SamplesPerBlock];
                        //buffer[offset + bufferOffset + 1] = (byte)(sampleBuffer[j][samplePosition % info.SamplesPerBlock] >> 8);

                        int bufferIndex = (int)((i * info.ChannelCount + j) * sizeof(short));
                        int sampleBufferIndex = (int)(samplePosition % info.SamplesPerBlock);
                        buffer[offset + bufferIndex] = (byte)sampleBuffer1D[sampleBufferIndex * info.ChannelCount + j];
                        buffer[offset + bufferIndex + 1] = (byte)(sampleBuffer1D[sampleBufferIndex * info.ChannelCount + j] >> 8);

                        read += sizeof(short);
                    }

                    samplePosition++;
                }

                return read;
            }
        }

        private void FillBuffer(int block = -1)
        {
            if (block >= 0) hcaFileStream.Position = dataStart + block * info.BlockSize;

            decoder.DecodeBlock(hcaFileReader.ReadBytes((int)info.BlockSize));
            //decoder.ReadSamples16(sampleBuffer);
            decoder.ReadSamples16(sampleBuffer1D);
        }
    }
}
