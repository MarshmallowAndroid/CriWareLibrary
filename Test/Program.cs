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
            string awbPath = @"C:\\Users\\jacob\\AppData\\LocalLow\\Cygames\\umamusume\\dat\\KI\\KIJ3CC7ROKZFDGJWCPCKSQFBA3GWJBWV";
            var awb = new AwbReader(File.OpenRead(awbPath));

            //Console.ReadKey();

            var hcaFile = awb.GetWaveSubfileStream(awb.Waves[0]);
            // good stuff are 0, 8, 18, 40, 51

            using var waveOut = new WaveOutEvent();

            //ulong key = 0x1E03B570B6145D1D;
            //ushort subkey = awb.Subkey;
            //ulong mixKey = key * ((ulong)subkey << 16 | (ushort)~subkey + 2u);

            using var hcaWaveStream = new HcaWaveStream(hcaFile, 0x1d2f8d3fbb9c5985);
            //hcaWaveStream.Position = 0;

            //hcaWaveStream.Loop = false;
            //Console.WriteLine(hcaWaveStream.Info.LoopStartSample);
            //Console.WriteLine(hcaWaveStream.Info.LoopEndSample);

            //WaveFileWriter.CreateWaveFile("test.wav", hcaWaveStream);

            //hcaWaveStream.Loop = true;
            var hcaInfo = hcaWaveStream.Info;

            hcaWaveStream.Position = (hcaInfo.LoopEndSample - 100000) * 4;
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
