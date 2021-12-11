using System;
using System.IO;
using System.Threading;
using ClHcaSharp;
using CriWareFormats;
using NAudio.Wave;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //string path = @"C:\Users\jacob\Desktop\snd_bgm_gm030.awb";
            string path = @"D:\whyred\million live\com.bandainamcoent.imas_millionlive_theaterdays\files\ADX2\song3\song3_harm4u_bgm.acb";
            //string path = @"C:\Users\jacob\AppData\LocalLow\Cygames\umamusume\dat\JC\JCFMCHIFFL3XQWHZJ43DHO45JLS3ZVAR";

            var acb = new AcbReader(File.OpenRead(path));
            var awb = acb.GetAwb();
            var hcaFile = awb.GetWaveSubfileStream(awb.Waves[0]);
            //byte[] hcaBytes = new byte[hcaFile.Length];
            //hcaFile.Read(hcaBytes, 0, (int)hcaFile.Length);
            //File.WriteAllBytes("output_hca", hcaBytes);

            //key1: 2634869067942574264
            //key2: 765765765765765
            //key3: 11110638274577261161

            using var waveOut = new WaveOutEvent();

            using var hcaWaveStream = new HcaWaveStream(hcaFile, 765765765765765);
            hcaWaveStream.Loop = true;
            var hcaInfo = hcaWaveStream.Info;

            //uint loopStart = hcaInfo.LoopStartBlock * hcaInfo.SamplesPerBlock - hcaInfo.EncoderDelay + hcaInfo.LoopStartDelay;
            //uint loopEnd = hcaInfo.LoopEndBlock * hcaInfo.SamplesPerBlock - hcaInfo.EncoderDelay + (hcaInfo.SamplesPerBlock - hcaInfo.LoopEndPadding);
            //using var loopWaveStream = new LoopWaveStream(
            //    hcaWaveStream,
            //    hcaInfo.LoopStartSample - hcaInfo.EncoderDelay,
            //    hcaInfo.LoopEndSample - hcaInfo.EncoderDelay)
            //{
            //    Loop = true
            //};
            //loopWaveStream.Position = (loopWaveStream.EndSample - 100000) * 4;
            //WaveFileWriter.CreateWaveFile("test.wav", hcaWaveStream);
            //hcaFile.Position = 0;

            hcaWaveStream.Position = (hcaInfo.LoopEndSample - 100000) * 4;
            //hcaWaveStream.Position = 0;

            waveOut.Init(hcaWaveStream);
            waveOut.Play();

            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(100);
            }

            HcaDecoder decoder = new(hcaFile, 765765765765765);
            HcaInfo info = decoder.GetInfo();
            BinaryReader hcaFileReader = new(hcaFile);

            //short[][] samples = new short[info.ChannelCount][];
            //for (int i = 0; i < info.ChannelCount; i++)
            //{
            //    samples[i] = new short[info.SamplesPerBlock];
            //}

            var testFile = new FileStream("test.raw", FileMode.Create);
            var testFileWriter = new BinaryWriter(testFile);

            //for (int i = 0; i < info.BlockCount; i++)
            //{
            //    decoder.DecodeBlock(hcaFileReader.ReadBytes((int)info.BlockSize));
            //    decoder.ReadSamples16(samples);

            //    for (int sample = 0; sample < info.SamplesPerBlock; sample++)
            //    {
            //        for (int channel = 0; channel < info.ChannelCount; channel++)
            //        {
            //            testFileWriter.Write(samples[channel][sample]);
            //        }
            //    }
            //}

            //short[] samplesLinear = new short[info.SamplesPerBlock * info.ChannelCount];

            //for (int i = 0; i < info.BlockCount; i++)
            //{
            //    decoder.DecodeBlock(hcaFileReader.ReadBytes((int)info.BlockSize));
            //    decoder.ReadSamples16(samplesLinear);

            //    for (int j = 0; j < samplesLinear.Length; j++)
            //    {
            //        testFileWriter.Write(samplesLinear[j]);
            //    }
            //}

        }
    }
}
