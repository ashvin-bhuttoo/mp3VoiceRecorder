using NAudio.Wave;
using NAudio.Lame;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace VoiceRecord
{
    public partial class frmMain : Form
    {
        public WaveIn waveSource = null;
        public WaveFileWriter waveFile = null;
        public MemoryStream memStream = null;

        public frmMain()
        {
            InitializeComponent();
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            StartBtn.Enabled = false;
            StopBtn.Enabled = true;

            waveSource = new WaveIn();

            //for (int n = 0; n < WaveIn.DeviceCount; n++)
            //{
            //    var caps = WaveIn.GetCapabilities(n);
            //    Console.WriteLine($"{n}: {caps.ProductName} {caps.Channels}");
            //}  

            waveSource.WaveFormat = new WaveFormat(44100, 1);

            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);

            memStream = new MemoryStream();
            waveFile = new WaveFileWriter(memStream, waveSource.WaveFormat);
            
            waveSource.StartRecording();
        }

        private void StopBtn_Click(object sender, EventArgs e)
        {
            StopBtn.Enabled = false;

            waveSource.StopRecording();

            byte[] outB = memStream.ToArray();
            File.WriteAllBytes("test.mp3", ConvertWavToMp3(outB));
        }

        void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveFile != null)
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile.Flush();
            }
        }

        void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }

            StartBtn.Enabled = true;
        }


        public byte[] ConvertWavToMp3(byte[] wavFile)
        {
            using (var retMs = new MemoryStream())
            using (var ms = new MemoryStream(wavFile))
            using (var rdr = new WaveFileReader(ms))
            using (var wtr = new LameMP3FileWriter(retMs, rdr.WaveFormat, 128))
            {
                rdr.CopyTo(wtr);
                wtr.Flush();
                return retMs.ToArray();
            }
        }
    }
}
