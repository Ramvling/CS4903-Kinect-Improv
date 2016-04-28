using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using System.Diagnostics;

/*
    Notes:

*/

namespace TrackEyes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // The sensor objects.
        KinectSensor _sensor = null;

        // The color frame reader is 
        ColorFrameReader _colorReader = null;

        // The body fram reader is used to identify the bodies
        BodyFrameReader _bodyReader = null;

        // The list of bodies identified by the sensor
        IList<Body> _bodies = null;

        // The face frame source
        FaceFrameSource _faceSource = null;

        // The face frame reader
        FaceFrameReader _faceReader = null;

        // Image image = null;


        // Create our Network Manager
        //private NetworkManager network = new NetworkManager();

        // public static Image myMask = null;

        double top, left;
        public MainWindow()
        {
            InitializeComponent();
            //this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
            //network.init();
             _sensor = KinectSensor.GetDefault();
            if(_sensor != null)
            {
                _sensor.Open();

                // Identify the bodies
                _bodies = new Body[_sensor.BodyFrameSource.BodyCount];

                _colorReader = _sensor.ColorFrameSource.OpenReader();
                _colorReader.FrameArrived += ColorReader_FrameArrived;
                _bodyReader = _sensor.BodyFrameSource.OpenReader();
                _bodyReader.FrameArrived += BodyReader_FrameArrived;

                // Initialize the face source with the desired features, some are commented out, include later.
                _faceSource = new FaceFrameSource(_sensor, 0, FaceFrameFeatures.BoundingBoxInColorSpace |
                                                  FaceFrameFeatures.FaceEngagement |
                                                  FaceFrameFeatures.Glasses |
                                                  FaceFrameFeatures.Happy |
                                                  FaceFrameFeatures.LeftEyeClosed |
                                                  FaceFrameFeatures.MouthOpen |
                                                  FaceFrameFeatures.PointsInColorSpace |
                                                  FaceFrameFeatures.RightEyeClosed);

                _faceReader = _faceSource.OpenReader();
                _faceReader.FrameArrived += FaceReader_FrameArrived;
                monocle.Width = 55;
                monocle.Height = 55;
            }
            top = left = 0.0;
        }


        void ColorReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    camera.Source = frame.ToBitmap();
                }
            }
        }


        void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if(frame != null)
                {
                    frame.GetAndRefreshBodyData(_bodies);

                    // Detect the default body
                    Body body = _bodies.Where(b => b.IsTracked).FirstOrDefault();

                    if(!_faceSource.IsTrackingIdValid)
                    {
                        if(body != null)
                        {
                            // Assign/update a tracking ID to the face source
                            _faceSource.TrackingId = body.TrackingId;
                        }
                    }
                }
            }
        }

        // Since the face source is connected with the body, we can specify what happens when a face frame is available.
        // Face frames work exactly like the color, depth, infared, and body frames: firstly, you get a reference to the
        // frame, then you acquire the frame, and if the frame is not empty, you can grab the FaceFrameResult object. 
        // FaceFrameResult object encapsulates all available face info.

        void FaceReader_FrameArrived(object sender, FaceFrameArrivedEventArgs e)
        {
            //maskImage.Visibility = Visibility.Hidden;
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if(frame != null)
                {
                    // Get the face frame result
                    FaceFrameResult result = frame.FaceFrameResult;

                    if(result != null)
                    {
                        // Get the Bounding Box
                        var bb = result.FaceBoundingBoxInColorSpace;
                        var eyeLeft = result.FacePointsInColorSpace[FacePointType.EyeRight];
                        var mouthLeft = result.FacePointsInColorSpace[FacePointType.MouthCornerLeft];
                        var mouthRight = result.FacePointsInColorSpace[FacePointType.MouthCornerRight];

                        double width = Math.Abs(bb.Right - bb.Left) * 1.8;
                        double height = Math.Abs(bb.Bottom - bb.Top) * 1.8;
                        double wDiff = Math.Abs(width - maskImage.Width);
                        double hDiff = Math.Abs(height - maskImage.Height);

                        // This will tell whether or not the image should be resized.
                        if(wDiff/maskImage.Width > 0.15 || hDiff/maskImage.Height > 0.15 || Double.IsNaN(maskImage.Width))
                        {
                            if (width > 0 && height > 0)
                            {
                                if(width > maskImage.Width)
                                {
                                    maskImage.Width = width * 0.85;
                                    maskImage.Height = height * 0.85;
                                }
                                else
                                {
                                    maskImage.Width = width * 1.15;
                                    maskImage.Height = height * 1.15;
                                }
                            }
                        }
                        width = maskImage.Width;
                        height = maskImage.Height;

                        double tleft = bb.Left - width * 0.2;
                        double ttop = bb.Top - height * 0.2 - height * 0.70;
                        if (tleft > 0)
                            left = tleft;
                        if (ttop > 0)
                            top = ttop;
                        double lDiff = Math.Abs(Canvas.GetLeft(maskImage) - left);
                        double tDiff = Math.Abs(Canvas.GetTop(maskImage) - top);

                        // this will tell whether or not the image should be translated.
                        if(lDiff/Canvas.GetLeft(maskImage) > 0.07 || tDiff/Canvas.GetTop(maskImage) > 0.07 || Double.IsNaN(Canvas.GetTop(maskImage)))
                        {
                            Canvas.SetLeft(maskImage, left);
                            Canvas.SetTop(maskImage, top);
                        }

                        maskImage.Visibility = Visibility.Visible;

                        double stacheWid = 3 * Math.Abs(mouthRight.X - mouthLeft.X);
                        if(stacheWid > 0)
                        {
                            stache.Width = stacheWid;
                            stache.Height = stacheWid / 3.0;
                            Canvas.SetLeft(stache, ((mouthRight.X + mouthLeft.X) / 2.0) - stache.Width / 2.0);
                            Canvas.SetTop(stache, ((mouthRight.Y + mouthLeft.Y) / 2.0) - stache.Height / 2.0 - 5);
                            stache.Visibility = Visibility.Visible;
                        }

                        if (eyeLeft.X > 0 && eyeLeft.X > 0)
                        {
                            Canvas.SetLeft(monocle, eyeLeft.X - monocle.Width / 2.0);
                            Canvas.SetTop(monocle, eyeLeft.Y - monocle.Height / 2.0);
                            Debug.WriteLine(eyeLeft.X + ", " + eyeLeft.Y);
                            monocle.Visibility = Visibility.Visible;
                        }

                    }
                }
            }

        }


    }
}
