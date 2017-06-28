using System;
using NAudio.Wave;

namespace KinectWpf
{
    public class SineWaveMaker : WaveProvider32
    {
        // Initial frequency and amplitude
        private const float DefaultInitialFrequency = 440f;
        private const float DefaultInitialAmplitude = 0.25f;

        // Variable to track the angular position in the waveform
        private float _phaseAngle = 0f;

        // Variable to store the last frquency played
        private double _lastFrequency = 0f;

        // Getters and setters for wave frequency and amplitude
        public double Frequency { get; set; }
        public double Amplitude { get; set; }

        // Default constructor
        public SineWaveMaker()
        {
            Frequency = DefaultInitialFrequency;
            Amplitude = DefaultInitialAmplitude;
            _lastFrequency = Frequency;
        }

        // Parameterized constructor for a custom default frequency
        public SineWaveMaker(float freq)
        {
            Frequency = freq;
            Amplitude = DefaultInitialAmplitude;
            _lastFrequency = Frequency;
        }

        // Parameterized constructor for custom default frequency and amplitude
        public SineWaveMaker(float freq, float amp)
        {
            Frequency = freq;
            Amplitude = amp;
            _lastFrequency = Frequency;
        }

        // Populate the wave buffer and read the data
        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            // Get the wave's sample rate
            int sampleRate = WaveFormat.SampleRate;
            // Loop through every sample
            for (int i = 0; i < sampleCount; i++)
            {
                // If the current frequency != the last frequency, transition between them smoothly.
                double freq = Frequency;
                if (Frequency != _lastFrequency)
                {
                    freq = ((sampleCount - i - 1) * _lastFrequency + Frequency) / (sampleCount - i);
                    _lastFrequency = freq;
                }
                // Determine the value of the current sample
                buffer[i + offset] = (float)(Amplitude * Math.Sin(_phaseAngle));
                // Advance our position in the waveform
                _phaseAngle += (float)(2 * Math.PI * freq / sampleRate);
                if (_phaseAngle > Math.PI * 2)
                    _phaseAngle -= (float)(Math.PI * 2);
            }
            return sampleCount;
        }
    }
}