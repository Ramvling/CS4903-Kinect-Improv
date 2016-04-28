﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Microsoft.Kinect.Face;
using System.Timers;

namespace SitStandLean
{
    class Person
    {
        public ulong trackingID;
        public Image imageRef;
        public int personInd;
        public FaceFrameResult res;
        public bool isTracked;
        private int timeOut;
        public double left, top;

        public Person(ulong id, Image iRef, int ind)
        {
            this.trackingID = id;
            this.imageRef = iRef;
            this.personInd = ind;
            this.isTracked = false;
            this.left = this.top = 0.0;
            this.timeOut = 0;

        }

        public bool checkTrack()
        {
            
            return isTracked;
        }

        public void update()
        {
            if(!isTracked && personInd > -1)
            {
                timeOut++;
                if(timeOut > 90)
                {
                    timeOut = 0;
                    personInd = -1;
                    imageRef.Width = 0;
                    imageRef.Height = 0;
                }
            }
            else
            {
                timeOut = 0;
            }
        }

    }
}
