using System;
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

        private double targetPitch;
        private double currentPitch;

        private bool run;

        public MainWindow()
        {
            _wout = new WaveOut();
            _generator = new SignalGenerator();

            _wout.Init(_generator);
            _wout.Volume = 0.01F;

            targetPitch = currentPitch = 2000;

            run = true;

            InitializeComponent();
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            KinectRegion.SetKinectRegion(this, kinectRegion);
            kinectRegion.KinectSensor = KinectSensor.GetDefault();
            BodyFrameReader multiSourceFrameReader = kinectRegion.KinectSensor.BodyFrameSource.OpenReader();
            multiSourceFrameReader.FrameArrived += (o, args) =>
            {
                
                Debug.WriteLine("got frame");
                BodyFrame frame = args.FrameReference.AcquireFrame();

                Body[] bodies = new Body[frame.BodyCount];

                frame.GetAndRefreshBodyData(bodies);

                foreach (Body body in bodies)
                {
                    Joint rightHand = body.Joints[JointType.HandRight];
                    Joint center = body.Joints[JointType.SpineMid];

                    if (center.TrackingState == TrackingState.Tracked &&
                        rightHand.TrackingState == TrackingState.Tracked)
                    {
                        float pitch = rightHand.Position.Y - center.Position.Y;

                        if (pitch > 0)
                        {
                            double newPitch = (pitch - 0) * (8600 - 80) / (0.5 - 0) + 80;

                            Debug.WriteLine(pitch + "-" + newPitch);

                            label.Content = newPitch + "HZ";
                            slider.Value = newPitch;

                            targetPitch = newPitch;

                            _generator.Frequency = newPitch;
                        }
                    }
                }
                Debug.WriteLine("frame done");
            };
            kinectRegion.KinectSensor.Open();
        }

        private void PlaybackLoop()
        {
            _wout.Play();

            while (run)
            {
                if (currentPitch != targetPitch)
                {
                    double diff = currentPitch - targetPitch;
                    if (diff > 0)
                    {
                        currentPitch -= diff / 100;
                    } else if (diff < 0)
                    {
                        currentPitch += -1 * (diff / 100);
                    }
                    Thread.Sleep(5);
                }
                _generator.Frequency = currentPitch;
            }
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(PlaybackLoop);
            t.Start();
            button.Content = "Started";
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //targetPitch = slider.Value;
        }
    }
}
