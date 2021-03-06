﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;                  
using Emgu.CV.CvEnum;           
using Emgu.CV.Structure;
using Emgu.CV.Tracking;
using Emgu.CV.UI;
using System.Threading;
using System.IO;

namespace RedBallTracker
{
    public partial class frmMain : Form
    {

        VideoCapture capWebcam;
        private static string VIDEO_DIR = "C:\\FoosballGeneral\\TestVideo\\testvideo3.mp4";
        bool blnCapturingInProcess = false;

        ScoreCounter scoreCounter = new ScoreCounter();
        public frmMain()
        {
            InitializeComponent();
            try
            {

                capWebcam = new VideoCapture(VIDEO_DIR);

            }
            catch (Exception ex)
            {
                MessageBox.Show("unable to read from webcam, error: " + Environment.NewLine + Environment.NewLine +
                                ex.Message + Environment.NewLine + Environment.NewLine +
                                "exiting program");
                Environment.Exit(0);
                return;
            }
            Application.Idle += processFrameAndUpdateGUI;    // add process image function to the application's list of tasks   
            blnCapturingInProcess = true;
        }


        void processFrameAndUpdateGUI(object sender, EventArgs arg)
        {
            Mat imgOriginal;
            imgOriginal = capWebcam.QueryFrame();
            Thread.Sleep(1000 / 180);
            if (imgOriginal == null)
            {
                new FileIO().writeToFile(scoreCounter.scoreTeamRed, scoreCounter.scoreTeamBlue);
                Environment.Exit(0);
                return;
            }

            Mat imgHSV = new Mat(imgOriginal.Size, DepthType.Cv8U, 3);

            Mat imgThreshLow = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);
            Mat imgThreshHigh = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);

            Mat imgThresh = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);

            CvInvoke.CvtColor(imgOriginal, imgHSV, ColorConversion.Bgr2Hsv);

            CvInvoke.InRange(imgHSV, new ScalarArray(new MCvScalar(0, 155, 155)), new ScalarArray(new MCvScalar(18, 255, 255)), imgThreshLow);
            CvInvoke.InRange(imgHSV, new ScalarArray(new MCvScalar(165, 155, 155)), new ScalarArray(new MCvScalar(179, 255, 255)), imgThreshHigh);

            CvInvoke.Add(imgThreshLow, imgThreshHigh, imgThresh);

            CvInvoke.GaussianBlur(imgThresh, imgThresh, new Size(3, 3), 500);

            Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));

            CvInvoke.Dilate(imgThresh, imgThresh, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
            CvInvoke.Erode(imgThresh, imgThresh, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0, 0, 0));

            CircleF[] circles = CvInvoke.HoughCircles(imgThresh, HoughType.Gradient, 2.0, imgThresh.Rows / 4, 100, 5, 4, 8);

            foreach (CircleF circle in circles)
            {
                //txtXYRadius.AppendText("ball position x = " + circle.Center.X.ToString().PadLeft(4) + ", y = " + circle.Center.Y.ToString().PadLeft(4) + ", radius = " + circle.Radius.ToString("###.000").PadLeft(7));
              
                    CvInvoke.Circle(imgOriginal, new Point((int)circle.Center.X, (int)circle.Center.Y), (int)circle.Radius, new MCvScalar(255, 0, 0), 2, LineType.AntiAlias);
                    CvInvoke.Circle(imgOriginal, new Point((int)circle.Center.X, (int)circle.Center.Y), 3, new MCvScalar(0, 255, 0), -1);
                    scoreCounter.countScore(circle.Center.X);
                    lTeamBox.Text = ("Red team score: " + scoreCounter.scoreTeamRed);
                    rTeamBox.Text = ("Blue team score: " + scoreCounter.scoreTeamBlue);

            }
            ibOriginal.Image = imgOriginal;
            ibThresh.Image = imgThresh;
        }



        private void tlpOuter_Paint(object sender, PaintEventArgs e)
        {

        }
        
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void loadScore_Click(object sender, EventArgs e)
        {
            loadedScore.Text= (new FileIO().readFromFile());
        }
    }
}
