using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectWpf
{
    class KinectWorker : IDisposable
    {
        private readonly KinectSensor _sensor;
        private readonly MultiSourceFrameReader _frameReader;
        private Body[] _bodies;

        public delegate void PitchEvent(double pitch);
        public delegate void VolumeEvent(float volume);

        public event PitchEvent PitchArrived;
        public event VolumeEvent VolumeArrived;

        public KinectWorker(KinectSensor sensor)
        {
            _sensor = sensor;
            _frameReader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body);
            _frameReader.MultiSourceFrameArrived += _frameReader_MultiSourceFrameArrived;
            _sensor.Open();
        }

        private void _frameReader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame().BodyFrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (_bodies == null)
                    {
                        _bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(_bodies);
                    dataReceived = true;
                }
            }

            if (!dataReceived) return;

            foreach (Body body in _bodies)
            {
                if (body == null)
                    return;

                Joint rightHand = body.Joints[JointType.HandRight];
                Joint leftHand = body.Joints[JointType.HandLeft];
                Joint center = body.Joints[JointType.SpineMid];

                if (center.TrackingState == TrackingState.Tracked &&
                    rightHand.TrackingState == TrackingState.Tracked &&
                    leftHand.TrackingState == TrackingState.Tracked)
                {
                    float pitch = rightHand.Position.Y - center.Position.Y;
                    float volume = center.Position.X - leftHand.Position.X;

                    if (pitch > 0)
                    {
                        double newPitch = (pitch - 0) * (1000 - 150) / (0.5 - 0) + 150;
                        PitchArrived?.Invoke(newPitch);
                    }

                    if (volume >= 0)
                    {
                        if (volume > 0.5)
                            volume = 0.5f;

                        float newVolume = (volume - 0) / (0.5f - 0);
                        VolumeArrived?.Invoke(newVolume);
                    }
                }
            }
        }

        public void Dispose()
        {
            _frameReader?.Dispose();
            _sensor?.Close();
        }
    }
}
