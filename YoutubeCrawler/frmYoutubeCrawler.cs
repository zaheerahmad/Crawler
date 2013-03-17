using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.GData.Extensions.MediaRss;
using Google.YouTube;
using YoutubeCrawler.Utilities;
using System.Threading;
using System.IO;
using HtmlAgilityPack;

namespace YoutubeCrawler
{
    public partial class frmYoutubeCrawler : Form
    {
        ManualResetEvent signal = new ManualResetEvent(false);
        public delegate void UpdateControlsDelegate();
        public static bool flagDone = true;
        public frmYoutubeCrawler()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread childThread = new Thread(new ThreadStart(StartWorking));
            childThread.IsBackground = true;
            childThread.Name = "Youtube Crawler";
            childThread.Start();
        }

        public void StartWorking()
        {
            flagDone = false;
            InvokeUpdateControls();
            
            //If Direct Channel Name is given..

            string channelName = textBox1.Text;
            string appName = textBox3.Text;
            string devKey = textBox2.Text;
            if (!channelName.Equals(""))
            {
                //Todo: Do Validation of Channel Name, if it is Valid Channel Name or not.
                //For the time I am assuming it as a Valid Channel Name
                if (!Directory.Exists(channelName))
                {
                    Directory.CreateDirectory(channelName);
                }
                else
                {
                    Directory.Delete(channelName, true);
                    Directory.CreateDirectory(channelName);
                }
                if (CrawlChannel(channelName, appName, devKey))
                    MessageBox.Show("Congratulations, Channel has been crawled Successfully", "Success");
            }
            else
                MessageBox.Show("Please Enter a Valid Channel Name", "Error");

            flagDone = true;
            InvokeUpdateControls();
            //---------------------------------------------------------------------------------------------------------

            //If Url of Channel is given
        }

        public void InvokeUpdateControls()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateControlsDelegate(UpdateControls));
            }
            else
            {
                UpdateControls();
            }
        }

        private void UpdateControls()
        {
            // update your controls here
            if (flagDone)
            {
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                textBox3.Enabled = true;
                button1.Enabled = true;
                button2.Enabled = true;
                UseWaitCursor = false;
            }
            else
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
                button1.Enabled = false;
                button2.Enabled = false;
                UseWaitCursor = true;
            }
        }

        private bool CrawlChannel(string pChannelName, string appName, string devKey)
        {
            try
            {
                //string developerKey = ConfigurationManager.AppSettings["developerKey"].ToString();
                //YouTubeRequestSettings settings = new YouTubeRequestSettings("Youtube Crawler App", developerKey);
                //YouTubeRequest request = new YouTubeRequest(settings);
                if (Channel.ParseChannel(pChannelName, appName, devKey, GlobalConstants._level))
                    return true;
                return false;
            }
            catch(UnauthorizedAccessException ex)
            {
                MessageBox.Show("Please Close the Driectory '" + pChannelName + "' and Try Crawling Data Again, All Data need to be removed to Crawl this Channel Again!", "Alert");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception Occured : " + ex.Message, "Exception");
                return false;
            }
        }
    }
}
