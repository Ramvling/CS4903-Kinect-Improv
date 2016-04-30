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

namespace SitStandLean
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

        // The body frame reader is used to identify the bodies
        BodyFrameReader _bodyReader = null;

        // The list of bodies identified by the sensor
        IList<Body> _bodies = null;

        int bodyCount;

        // The face frame source
        FaceFrameSource[] _faceSources = null;

        // The face frame reader
        FaceFrameReader[] _faceReaders = null;

        // Storage for face frame results
        FaceFrameResult[] _faceResults = null;

        private NetworkManager network = new NetworkManager();

        // All of the resources needed to track and scramble the hats on performers
        Person[] _persons = null;
        int personSize;
        Image[] ims = null;
        String[] paths = null;
        bool[] trackedInd = null;

        public MainWindow()
        {
            InitializeComponent();
            network.init();
            _sensor = KinectSensor.GetDefault();
            if(_sensor != null)
            {
                _sensor.Open();

                bodyCount = _sensor.BodyFrameSource.BodyCount;
                // Identify the bodies 
                _bodies = new Body[bodyCount];

                _colorReader = _sensor.ColorFrameSource.OpenReader();
                _colorReader.FrameArrived += ColorReader_FrameArrived;
                _bodyReader = _sensor.BodyFrameSource.OpenReader();
                _bodyReader.FrameArrived += BodyReader_FrameArrived;

                // Initialize the face source with the desired features.
                _faceSources = new FaceFrameSource[bodyCount];
                _faceReaders = new FaceFrameReader[bodyCount];

                for(int i = 0; i < bodyCount; i++)
                {
                    // Create the face frame source with the required features and initial tracking id of 0
                    _faceSources[i] = new FaceFrameSource(_sensor, 0, FaceFrameFeatures.BoundingBoxInColorSpace);

                    // open the corresponding reader
                    _faceReaders[i] = _faceSources[i].OpenReader();
                    _faceReaders[i].FrameArrived += FaceReader_FrameArrived;
                }

                _faceResults = new FaceFrameResult[bodyCount];

                // Set the arrays and values for person switches and timeouts
                personSize = 3;
                ims = new Image[3] {maskImage, maskImage2, maskImage3};
                trackedInd = new bool[3] { false, false, false };
                _persons = new Person[personSize];
                for(int i = 0; i < personSize; i++)
                {
                    _persons[i] = new Person(0, ims[i], -1);
                }
                paths = new String[3] { "pack://application:,,,/Images/tinfoil.png",
                                        "pack://application:,,,/Images/cowboy.png",
                                        "pack://application:,,,/Images/napolean.png"};
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
                if(frame != null)
                {
                    camera.Source = frame.ToBitmap();
                }
            }
        }

        /// <summary>
        /// This will scramble the hats on each person. Scramble is called when the network asks
        /// for a change.
        /// </summary>
        void Scramble()
        {
            Random rand = new Random();
            int r = rand.Next(0, 6);
            int[] arr = null; 
            switch (r)
            {
                case 0:
                    arr = new int[]{ 0, 1, 2 };
                    break;
                case 1:
                    arr = new int[]{ 0, 2, 1 };
                    break;
                case 2:
                    arr = new int[] { 2, 0, 1 };
                    break;
                case 3:
                    arr = new int[] { 2, 1, 0 };
                    break;
                case 4:
                    arr = new int[] { 1, 0, 2 };
                    break;
                case 5:
                    arr = new int[] { 1, 2, 0 };
                    break;
            }

            if (arr != null)
            {
                for (int i = 0; i < personSize; i++)
                {
                    _persons[i].imageRef.Source = new BitmapImage(new Uri(@paths[arr[i]]));
                }
            }
        }

        /// <summary>
        /// The body reader will read each body, connect the faces, check timeout on faces, and 
        /// call the final result of the face to attach the hat.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                // Determine if the network has sent a message
                if (network.checkChange())
                {
                    network.setChange(false);
                    Scramble();
                }

                if (frame != null)
                {

                    frame.GetAndRefreshBodyData(_bodies);

                    // Big chunk of code, finding the bodies, if not found then reassign the hat to a new body.
                    for(int i = 0; i < bodyCount; i++)
                    {
                        // Check if we have a vaild ID
                        if(_faceSources[i].IsTrackingIdValid)
                        {
                            if (_faceResults[i] != null)
                            {
                                int pInd = -1;
                                for(int j = 0; j < personSize; j++)
                                {
                                    if(_persons[j].trackingID == _faceSources[i].TrackingId)
                                    {
                                        pInd = j;
                                        break;
                                    }
                                }
                                if(pInd < 0)
                                {
                                    int newInd = -1;
                                    for (int j = 0; j < personSize; j++)
                                    {
                                        if(_persons[j].personInd < 0)
                                        {
                                            newInd = j;
                                            break;
                                        }
                                    }
                                    if(newInd > -1)
                                    {
                                        _persons[newInd].trackingID = _faceSources[i].TrackingId;
                                        _persons[newInd].personInd = i;
                                        trackedInd[newInd] = true;
                                        DrawFaceFrameResults(_persons[newInd], _faceResults[i]);
                                        
                                    }
                                }
                                else
                                {
                                    _persons[pInd].res = _faceResults[i];
                                    
                                    trackedInd[pInd] = true;
                                    DrawFaceFrameResults(_persons[pInd], _faceResults[i]);
                                }
                            }
                        }
                        else
                        {
                            if(_bodies[i].IsTracked)
                            {
                                _faceSources[i].TrackingId = _bodies[i].TrackingId;
                            }
                        }
                    }

                    // For each person, set whether they are tracked or not.
                    for (int i = 0; i < personSize; i++)
                    {
                        if (!trackedInd[i])
                        {
                            _persons[i].isTracked = false;
                        }
                        else
                        {
                            _persons[i].isTracked = true;
                        }
                        _persons[i].update();
                        trackedInd[i] = false;
                    }
                }
            }
        }


        /// <summary>
        /// When the body data is read, the face reader is executed. 
        /// Connect each face source with their face frame result.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FaceReader_FrameArrived(object sender, FaceFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if(frame != null)
                {
                    int index = -1;
                    for(int i = 0; i < bodyCount; i++)
                    {
                        if(_faceSources[i] == frame.FaceFrameSource)
                        {
                            index = i;
                            break;
                        }
                    }
                    _faceResults[index] = frame.FaceFrameResult;
                }
            }
        }


        /// <summary>
        /// Each result it drawn for each person attached to the result.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="f"></param>
        void DrawFaceFrameResults(Person p, FaceFrameResult f)
        {
            var bb = f.FaceBoundingBoxInColorSpace;

            // Gather the width of the bounding box then the size difference 
            double width = Math.Abs(bb.Right - bb.Left) * 1.8;
            double height = Math.Abs(bb.Bottom - bb.Top) * 1.8;
            double wDiff = Math.Abs(width - p.imageRef.Width);
            double hDiff = Math.Abs(height - p.imageRef.Height);

            // This will tell whether or not the image should be resized.
            if (wDiff / p.imageRef.Width > 0.25 || hDiff / p.imageRef.Height > 0.25 || Double.IsNaN(p.imageRef.Width))
            {
                if (width > 0 && height > 0)
                {
                    p.imageRef.Width = width;
                    p.imageRef.Height = height;
                }
            }
            else
            {
                width = p.imageRef.Width;
                height = p.imageRef.Height;
            }

            // Gather the coordinates of the image and the location difference
            double left = bb.Left - width * 0.2;
            double top = bb.Top - height * 0.2 - height * 0.65;
            double lDiff = Math.Abs(Canvas.GetLeft(p.imageRef) - left);
            double tDiff = Math.Abs(Canvas.GetTop(p.imageRef) - top);

            // this will tell whether or not the image should be translated.
            if (lDiff / Canvas.GetLeft(p.imageRef) > 0.07 || tDiff / Canvas.GetTop(p.imageRef) > 0.07 || Double.IsNaN(Canvas.GetTop(p.imageRef)))
            {
                if (left > 0 && top > 0)
                {
                    p.left = left;
                    p.top = top;
                }
                Canvas.SetLeft(p.imageRef, p.left);
                Canvas.SetTop(p.imageRef, p.top);
            }

            p.imageRef.Visibility = Visibility.Visible;

        }
    }
}
