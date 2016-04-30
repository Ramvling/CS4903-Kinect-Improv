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


        // Create our Network Manager
        private NetworkManager network = new NetworkManager();

        public MainWindow()
        {
            InitializeComponent();
            network.init();
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
                _faceSource = new FaceFrameSource(_sensor, 0, FaceFrameFeatures.BoundingBoxInColorSpace);// |
                /*
                                                                FaceFrameFeatures.FaceEngagement |
                                                                FaceFrameFeatures.Glasses |
                                                                FaceFrameFeatures.Happy |
                                                                FaceFrameFeatures.LeftEyeClosed |
                                                                FaceFrameFeatures.MouthOpen |
                                                                FaceFrameFeatures.PointsInColorSpace |
                                                                FaceFrameFeatures.RightEyeClosed);
                                                                */

                _faceReader = _faceSource.OpenReader();
                _faceReader.FrameArrived += FaceReader_FrameArrived;
            }
        }


        /// <summary>
        /// Applying the camera feed to the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


        /// <summary>
        /// The Body Reader will gather all of the bodies that the kinect reads, it will then give the
        /// best body that is displayed and set it's tracking ID.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


        /// <summary>
        /// The Face Reader connects to the body reader and will reaturn a bounding box for us to reference the 
        /// image to. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FaceReader_FrameArrived(object sender, FaceFrameArrivedEventArgs e)
        {
            maskImage.Visibility = Visibility.Hidden;
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if(frame != null)
                {
                    // Get the face frame result
                    FaceFrameResult result = frame.FaceFrameResult;

                    if(result != null)
                    {
                        if(network.checkChange())
                        {
                            network.setChange(false);
                            string path = "pack://application:,,/Images/" + network.getPath();
                            //string path = network.getPath();
                            try
                            {
                                //maskImage.Source = new BitmapImage(new Uri(path, UriKind.Absolute));
                                maskImage.Source = new BitmapImage(new Uri(@path));
                            }
                            catch(System.IO.IOException exc)
                            {
                                Debug.WriteLine("System IO Exception ");
                            }
                            catch(System.UriFormatException exc)
                            {
                                Debug.WriteLine("UriFormatException");
                            }
                        }


                        var bb = result.FaceBoundingBoxInColorSpace;

                        double width = Math.Abs(bb.Right - bb.Left) * 1.8;
                        double height = Math.Abs(bb.Bottom - bb.Top) * 1.8;
                        double wDiff = Math.Abs(width - maskImage.Width);
                        double hDiff = Math.Abs(height - maskImage.Height);

                        // This will tell whether or not the image should be resized.
                        if(wDiff/maskImage.Width > 0.35 || hDiff/maskImage.Height > 0.35 || Double.IsNaN(maskImage.Width))
                        {
                            if (width > 0 && height > 0)
                            {
                                maskImage.Width = width;
                                maskImage.Height = height;
                            }
                        }
                        else
                        {
                            width = maskImage.Width;
                            height = maskImage.Height;
                        }
                        double left = bb.Left - width * 0.25;
                        double top = bb.Top - height * 0.30;
                        double lDiff = Math.Abs(Canvas.GetLeft(maskImage) - left);
                        double tDiff = Math.Abs(Canvas.GetTop(maskImage) - top);

                        // this will tell whether or not the image should be translated.
                        if(lDiff/Canvas.GetLeft(maskImage) > 0.08 || tDiff/Canvas.GetTop(maskImage) > 0.08 || Double.IsNaN(Canvas.GetTop(maskImage)))
                        {
                            if (left > 0 && top > 0)
                            {
                                Canvas.SetLeft(maskImage, left);
                                Canvas.SetTop(maskImage, top);
                            }
                        }

                        maskImage.Visibility = Visibility.Visible;
                    }
                }
            }
        }


    }
}
