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
            string acbPath = @"C:\Users\jacob\Desktop\CueSheet_BGM.acb";
            string awbPath1 = @"C:\Users\jacob\Desktop\CueSheet_BGM.awb";
            string awbPath2 = @"C:\Users\jacob\Desktop\CueSheet_BGM_DLC01.awb";

            var utf = new UtfTable(File.OpenRead(acbPath), out uint _, out string _);

            var acb = new AcbReader(File.OpenRead(acbPath));
            var awb = new AwbReader(File.OpenRead(awbPath1));
            var hcaFile = awb.GetWaveSubfileStream(awb.Waves[18]);
            // good stuff are 0, 8, 18, 40, 51

            using var waveOut = new WaveOutEvent();

            ulong key = 0x1E03B570B6145D1D;
            ushort subkey = awb.Subkey;
            ulong mixKey = key * ((ulong)subkey << 16 | (ushort)~subkey + 2u);

            using var hcaWaveStream = new HcaWaveStream(hcaFile, mixKey);
            //hcaWaveStream.Position = 0;

            //hcaWaveStream.Loop = false;
            //Console.WriteLine(hcaWaveStream.Info.LoopStartSample);
            //Console.WriteLine(hcaWaveStream.Info.LoopEndSample);

            //WaveFileWriter.CreateWaveFile("test.wav", hcaWaveStream);

            hcaWaveStream.Loop = true;
            var hcaInfo = hcaWaveStream.Info;

            //hcaWaveStream.Position = (hcaInfo.LoopEndSample - 100000) * 4;
            //hcaWaveStream.Position = 0;

            waveOut.Init(hcaWaveStream);
            waveOut.Play();

            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(100);
            }
        }
    }
}
