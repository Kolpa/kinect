using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using Microsoft.Kinect;
using Microsoft.Kinect.Wpf.Controls;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Threading;

namespace KinectWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WaveOut _wout;

        private MixingWaveProvider32 _provider;

        private SineWaveMaker _generator;
        private SineWaveMaker _generator2;

        private KinectWorker _kinectWorker;

        private double _targetPitch;

        private bool _run;

        public MainWindow()
        {
            _wout = new WaveOut();

            _provider = new MixingWaveProvider32();

            _generator = new SineWaveMaker();
            _generator2 = new SineWaveMaker();

            _provider.AddInputStream(_generator);
            _provider.AddInputStream(_generator2);
            _provider.RemoveInputStream(_generator);

            _wout.Init(_provider);
            _wout.Volume = 1F;

            _run = true;

            _kinectWorker = new KinectWorker(KinectSensor.GetDefault());
            _kinectWorker.VolumeArrived += KinectWorkerOnVolumeArrived;
            _kinectWorker.PitchArrived += KinectWorkerOnPitchArrived;

            InitializeComponent();
        }

        private void KinectWorkerOnPitchArrived(double pitch)
        {
            _generator.Frequency = pitch;
            slider.Value = pitch;
        }

        private void KinectWorkerOnVolumeArrived(float volume)
        {
            _generator.Amplitude = volume;
        }

        private void PlaybackLoop()
        {
            _wout.Play();
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(PlaybackLoop);
            t.Start();
            button.Content = "Started";
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _run = false;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _generator.Frequency = slider.Value;
        }
    }
}
