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
        private SignalGenerator _generator;

        private KinectWorker _kinectWorker;

        private double _targetPitch;

        private bool _run;

        public MainWindow()
        {
            _wout = new WaveOut();
            _generator = new SignalGenerator();
            _generator.Type = SignalGeneratorType.Sin;

            _wout.Init(_generator);
            _wout.Volume = 1F;

            _run = true;

            _kinectWorker = new KinectWorker(KinectSensor.GetDefault());
            _kinectWorker.VolumeArrived += KinectWorkerOnVolumeArrived;
            _kinectWorker.PitchArrived += KinectWorkerOnPitchArrived;

            InitializeComponent();
        }

        private void KinectWorkerOnPitchArrived(double pitch)
        {
            _targetPitch = pitch;
            slider.Value = pitch;
        }

        private void KinectWorkerOnVolumeArrived(float volume)
        {
            _wout.Volume = volume;
        }

        private void PlaybackLoop()
        {
            _wout.Play();

            while (_run)
            {
                _generator.Frequency = _targetPitch;
            }
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
    }
}
