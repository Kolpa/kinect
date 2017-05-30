using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Kinect;
using Microsoft.Kinect.Wpf.Controls;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace KinectWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WaveOut _wout;
        private SignalGenerator _generator;
        public MainWindow()
        {
            _wout = new WaveOut();
            _generator = new SignalGenerator();

            _wout.Init(_generator);
            _wout.Volume = 0.01F;

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
                            double newPitch = (pitch - 0) * (20000 - 20) / (0.5 - 0) + 20;

                            Debug.WriteLine(pitch + "-" + newPitch);

                            label.Content = newPitch + "HZ";
                            slider.Value = newPitch;

                            _generator.Frequency = newPitch;
                        }
                    }
                }
                Debug.WriteLine("frame done");
            };
            kinectRegion.KinectSensor.Open();
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            _wout.Stop();
            _wout.Dispose();
        }
    }
}
