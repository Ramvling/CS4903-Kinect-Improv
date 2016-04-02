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

        public MainWindow()
        {
            InitializeComponent();
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);

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
                _faceSource = new FaceFrameSource(_sensor, 0, FaceFrameFeatures.BoundingBoxInColorSpace | FaceFrameFeatures.MouthOpen);// |
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

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                maskImage.Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/nickhorror.png"));
            }
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
            maskImage.Visibility = Visibility.Hidden;
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if(frame != null)
                {
                    // Get the face frame result
                    FaceFrameResult result = frame.FaceFrameResult;

                    if(result != null)
                    {
                        /*
                        // Get the face points, mapped in the color space
                        var eyeLeft = result.FacePointsInColorSpace[FacePointType.EyeLeft];
                        var eyeRight = result.FacePointsInColorSpace[FacePointType.EyeRight];
                        var nose = result.FacePointsInColorSpace[FacePointType.Nose];
                        var mouthLeft = result.FacePointsInColorSpace[FacePointType.MouthCornerLeft];
                        var mouthRight = result.FacePointsInColorSpace[FacePointType.MouthCornerRight];

                        // Get the face characteristics
                        var eyeLeftClosed = result.FaceProperties[FaceProperty.LeftEyeClosed];
                        var eyeRightClosed = result.FaceProperties[FaceProperty.RightEyeClosed];
                        var mouthOpen = result.FaceProperties[FaceProperty.MouthOpen];
                        */

                        var mouthOpen = result.FaceProperties[FaceProperty.MouthOpen];

                        /*
                        if(mouthOpen == DetectionResult.Yes && maskImage.Source.ToString() != "pack://application:,,,/Images/nickhorror.png")
                        {
                            maskImage.Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/nickhorror.png"));
                        }
                        if(mouthOpen == DetectionResult.No && maskImage.Source.ToString() != "pack://application:,,,/Images/nicksmile.png")
                        {
                            maskImage.Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/nicksmile.png"));
                        }
                        */
                        // Get the Bounding Box
                        var bb = result.FaceBoundingBoxInColorSpace;

                        //Debug.WriteLine("Width: " + width);
                        //Debug.WriteLine("Height: " + height);

                        // Set the values, replace this with the image sourcef
                        /*
                        boundingBox.Width = Math.Abs(bb.Right - bb.Left);
                        boundingBox.Height = Math.Abs(bb.Bottom - bb.Top);
                        Canvas.SetLeft(boundingBox, bb.Left);
                        Canvas.SetRight(boundingBox, bb.Right);
                        Canvas.SetTop(boundingBox, bb.Top);
                        Canvas.SetBottom(boundingBox, bb.Bottom);
                        */
                        double width = Math.Abs(bb.Right - bb.Left) * 1.8;
                        double height = Math.Abs(bb.Bottom - bb.Top) * 1.8;
                        double wDiff = Math.Abs(width - maskImage.Width);
                        double hDiff = Math.Abs(height - maskImage.Height);

                        // This will tell whether or not the image should be resized.
                        if(wDiff/maskImage.Width > 0.15 || hDiff/maskImage.Height > 0.15 || Double.IsNaN(maskImage.Width))
                        {
                            maskImage.Width = width;
                            maskImage.Height = height;
                        }
                        else
                        {
                            width = maskImage.Width;
                            height = maskImage.Height;
                        }
                        double left = bb.Left - width * 0.2;
                        double top = bb.Top - height * 0.2;
                        double lDiff = Math.Abs(Canvas.GetLeft(maskImage) - left);
                        double tDiff = Math.Abs(Canvas.GetTop(maskImage) - top);

                        // this will tell whether or not the image should be translated.
                        if(lDiff/Canvas.GetLeft(maskImage) > 0.03 || tDiff/Canvas.GetTop(maskImage) > 0.03 || Double.IsNaN(Canvas.GetTop(maskImage)))
                        {
                            Canvas.SetLeft(maskImage, bb.Left - width * 0.2);
                            Canvas.SetTop(maskImage, bb.Top - height * 0.2);

                            // Below may/may not be necessary.
                            // Canvas.SetRight(maskImage, bb.Right + width * 0.2);
                            //Canvas.SetBottom(maskImage, bb.Bottom + height * 0.2);
                        }

                        maskImage.Visibility = Visibility.Visible;
                        
                        /*
                        // Position the canvas UI elements
                        Canvas.SetLeft(ellipseEyeLeft, eyeLeft.X - ellipseEyeLeft.Width / 2.0);
                        Canvas.SetTop(ellipseEyeLeft, eyeLeft.Y - ellipseEyeLeft.Width / 2.0);

                        Canvas.SetLeft(ellipseEyeRight, eyeRight.X - ellipseEyeRight.Width / 2.0);
                        Canvas.SetTop(ellipseEyeRight, eyeRight.Y - ellipseEyeRight.Height / 2.0);

                        Canvas.SetLeft(ellipseNose, nose.X - ellipseNose.Width / 2.0);
                        Canvas.SetTop(ellipseNose, nose.Y - ellipseNose.Height / 2.0);

                        Canvas.SetLeft(ellipseMouth, ((mouthRight.X + mouthLeft.X) / 2.0) - ellipseMouth.Width / 2.0);
                        Canvas.SetTop(ellipseMouth, ((mouthRight.Y + mouthLeft.Y) / 2.0) - ellipseMouth.Height / 2.0);
                        ellipseMouth.Width = Math.Abs(mouthRight.X - mouthLeft.X);
                        
                        // Display or hide the ellipses
                        if (eyeLeftClosed == DetectionResult.Yes || eyeLeftClosed == DetectionResult.Maybe)
                        {
                            ellipseEyeLeft.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            ellipseEyeLeft.Visibility = Visibility.Visible;
                        }

                        if (eyeRightClosed == DetectionResult.Yes || eyeRightClosed == DetectionResult.Maybe)
                        {
                            ellipseEyeRight.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            ellipseEyeRight.Visibility = Visibility.Visible;
                        }

                        if (mouthOpen == DetectionResult.Yes || mouthOpen == DetectionResult.Maybe)
                        {
                            ellipseMouth.Height = 50.0;
                        }
                        else
                        {
                            ellipseMouth.Height = 20.0;
                        }
                        */
                    }
                }
            }
        }


    }
}
