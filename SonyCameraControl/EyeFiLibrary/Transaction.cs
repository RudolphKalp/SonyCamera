using SonyCameraControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using ICSharpCode.SharpZipLib;
using System.Threading;

namespace EyeFiLibrary
{
    public class Transaction
    {
        public IPLCControl plcControl = PLCControl.CreatePLCControl();
        public string vtID;
        //public List<List<string>> transactions;
        public List<string> images;
        public string copyPath = @"D:\Pictures\Copied Eye-Fi Photos";
        FileSystemWatcher watchImages = new FileSystemWatcher(@"D:\Pictures\Eye-Fi Photos", "*.jpg");
        public int imageCount = 0;
        public bool previousState, currentState;

        public DispatcherTimer checkPLC;
        public DispatcherTimer vtStatus;

        public void Connect()
        {
            checkPLC = new DispatcherTimer();
            checkPLC.Interval = TimeSpan.FromMilliseconds(100);
            checkPLC.Tick += checkPLC_Tick;
            checkPLC.Start();

            //vtStatus = new DispatcherTimer();
            //vtStatus.Interval = TimeSpan.FromMilliseconds(200);
            //vtStatus.Tick += vtStatus_Tick;

            imageCount = 0;
        }

        public void LoadImages()
        {
            vtID = Guid.NewGuid().ToString();
            string targetLocation = System.IO.Path.Combine(@"D:\Pictures\Copied Eye-Fi Photos", vtID);
            if (!Directory.Exists(targetLocation))
            {
                Directory.CreateDirectory(targetLocation);
            }
            int imageCount = 0;
            
            foreach (string image in images)
            {
                imageCount++;
                string targetFile = System.IO.Path.Combine(targetLocation, imageCount.ToString() + ".jpg");
                if (!File.Exists(targetFile))
                {
                    File.Move(image, targetFile);
                }
            }
            images.Clear();
            imageCount = 0;
        }

        //void vtStatus_Tick(object sender, EventArgs e)
        //{
        //    if (images.Count >= 10)
        //    {
        //        LoadImages();
        //    }
        //}

        void checkPLC_Tick(object sender, EventArgs e)
        {
            currentState = plcControl.CameraBusy();

            if ((!previousState) && (currentState))
            {
                vtID = Guid.NewGuid().ToString();
            }
            else if ((previousState) && (!currentState))
            {
                GetImageList();
            }

            previousState = currentState;
        }

        private void GetImageList()
        {
            #region Eye-Fi Control
            if (!Directory.Exists(watchImages.Path))
            {
                throw new IOException();
            }
            //transactions = new List<List<string>>();
            images = new List<string>();
            //transactions.Add(images);
            //watchImages.Changed += new FileSystemEventHandler(OnChanged);
            watchImages.Created += new FileSystemEventHandler(OnCreated);
            watchImages.EnableRaisingEvents = true;

            while (images.Count < 10) { ;}
            LoadImages();
            
            #endregion
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            images.Add(e.FullPath);
            imageCount++;
        }
    }
}
