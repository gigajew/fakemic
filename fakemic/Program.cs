using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace fakemic
{
    class Program
    {
        // stream audio to speaker (custom speaker)

        static WaveFileReader waveReader;
        static int deviceNumber;
        static bool playing;
        static WaveOut waveOut;
        static string soundsFolder = "sounds";
        static Random random = new Random();

        static List<string> sounds = new List<string>();

        static void Main(string[] args)
        {
            string[] parts = new string[] {
                "   ___     _                 _      ",
                "  / __\\_ _| | _____    /\\/\\ (_) ___ ",
                " / _\\/ _` | |/ / _ \\  /    \\| |/ __|",
                "/ / | (_| |   <  __/ / /\\/\\ \\ | (__ ",
                "\\/   \\__,_|_|\\_\\___| \\/    \\/_|\\___|"
            };
            for (int i = 0; i < parts.Length; i ++)
            {
                ColorfulConsole.WriteLine(parts[i], random.Next(1, 255), random.Next(1, 255), random.Next(1, 255));
            }
            ColorfulConsole.WriteLine("Created by gigajew. Repo at gigajew/fakemic. Requires VB-Audio Virtual Cable.", random.Next(1, 255), random.Next(1, 255), random.Next(1, 255));
            Console.WriteLine();

            LoadSounds();
            foreach(var sound in sounds )
            {
                Console.WriteLine("Loaded: {0}" , Path.GetFileName(sound));
            }

            GetDevice();
            Console.WriteLine("Located sound device: {0}", deviceNumber);

            Hooky.CUp += Hooky_CUp;
            Hooky.CDown += Hooky_CDown;
            
            Thread t = new Thread(() => Hooky.Initialize());
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();

            Console.WriteLine("Hooked to Key C! Bind your mic to C in the game you want.");
            Console.WriteLine("Hold C to play a random sound. Let go to stop it.");

            Console.WriteLine("Ready!");
            Console.ReadLine();
        }

        static void LoadSounds()
        {
            foreach(var file in Directory.GetFiles(soundsFolder, "*.wav"))
            {
                sounds.Add(file);   
            }
        }

        private static string GetRandomSound()
        {
            return Path.GetFullPath(sounds[random.Next(0, sounds.Count)]);
        }

        private static void Hooky_CDown(object sender, EventArgs e)
        {
            if (playing) return;
            playing = !playing;

            PlaySound(GetRandomSound());

            Console.WriteLine("Playing");
        }

        private static void Hooky_CUp(object sender, EventArgs e)
        {
            if (!playing) return;
            playing = !playing;

            Stop();


            Console.WriteLine("Stopped");
        }

        static void Stop()
        {
            waveOut.Stop();
            waveOut.Dispose();
        }

        static void GetDevice()
        {
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var caps = WaveOut.GetCapabilities(i);
                Console.WriteLine("Device: {0}, Name: {1}", i, caps.ProductName);
                if (caps.ProductName.Contains("VB-Audio Virtual"))
                {
                    deviceNumber = i;
                }
            }
        }

        static void PlaySound(string soundfile)
        {
            if (waveReader != null)
                waveReader.Dispose();
            waveReader = new WaveFileReader(soundfile);
            waveOut = new WaveOut();
            waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
            waveOut.DeviceNumber = deviceNumber;
            var output = waveOut;
            output.Init(waveReader);
            output.Play();
        }

        private static void WaveOut_PlaybackStopped(object sender, NAudio.Wave.StoppedEventArgs e)
        {
            Console.WriteLine("Stopped");
            Stop(); // temproary
        }
    }

    internal static class ColorfulConsole
    {
        private static object WriteLock = new object();

        public static void WriteLine(string text, int r, int g, int b, params string[] args )
        {
           if (  Monitor.TryEnter(WriteLock))
            {
                ConsoleColor lastColor = Console.ForegroundColor;
                Console.ForegroundColor = FromColor(r, g, b);
                Console.WriteLine(text, args);
                Console.ForegroundColor = lastColor;
            }
            Monitor.Exit(WriteLock);
        }

        private static ConsoleColor FromColor(int r, int g, int b)
        {
            int index = (r > 128 | g > 128 | b > 128) ? 8 : 0; // Bright bit
            index |= (r > 64) ? 4 : 0; // Red bit
            index |= (g > 64) ? 2 : 0; // Green bit
            index |= (b > 64) ? 1 : 0; // Blue bit
            return (ConsoleColor)index;
        }
    }
}
