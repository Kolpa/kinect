using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace KinectWpf
{
    class PitchMultiplexer : IWaveProvider
    {
        private IWaveProvider _inputWaveProvider;
        private IWaveProvider _lfoWaveProvider;

        private WaveFormat waveFormat;
        private int bytesPerSample;


        public PitchMultiplexer(IWaveProvider inputWaveProvider, IWaveProvider lfoWaveProvider)
        {
            this._inputWaveProvider = inputWaveProvider;
            this._lfoWaveProvider = lfoWaveProvider;

            this.waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(_inputWaveProvider.WaveFormat.SampleRate, _inputWaveProvider.WaveFormat.Channels);

            this.bytesPerSample = 4;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (count % bytesPerSample != 0)
                throw new ArgumentException(@"Must read an whole number of samples", nameof(count));

            Array.Clear(buffer, offset, count);
            int bytesReadInput = 0;
            int bytesReadLfo = 0;

            byte[] readBufferinput = new byte[count];
            byte[] readBufferLfo = new byte[count];

            int readFromThisStreamInput = _inputWaveProvider.Read(readBufferinput, 0, count);
            int readFromThisStreamLfo = _lfoWaveProvider.Read(readBufferLfo, 0, count);

            bytesReadInput = Math.Max(bytesReadInput, readFromThisStreamInput);
            if (readFromThisStreamInput > 0)
            {
                Sum32BitAudio(buffer, offset, readBufferinput, readFromThisStreamInput, readBufferLfo);
            }
            return bytesReadInput;
        }

        static unsafe void Sum32BitAudio(byte[] destBuffer, int offset, byte[] sourceBufferInput, int bytesReadInput, byte[] sourceBufferLfo)
        {
            fixed (byte* pDestBuffer = &destBuffer[offset],
                      pSourceBufferInput = &sourceBufferInput[0],
                      pSourceBufferLfo = &sourceBufferLfo[0])
            {
                float* pfDestBuffer = (float*)pDestBuffer;
                float* pfReadBufferInput = (float*)pSourceBufferInput;
                float* pfReadBufferLfo = (float*)pSourceBufferLfo;
                int samplesRead = bytesReadInput / 4;
                for (int n = 0; n < samplesRead; n++)
                {
                    pfDestBuffer[n] = pfReadBufferInput[n] * pfReadBufferLfo[n];
                }
            }
        }

        public WaveFormat WaveFormat => waveFormat;
    }
}
