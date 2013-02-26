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

namespace YoutubeCrawler
{
    public partial class frmYoutubeCrawler : Form
    {
        public frmYoutubeCrawler()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //If Direct Channel Name is given..
            
            string channelName = textBox1.Text;
            if (!channelName.Equals(""))
            {
                //Todo: Do Validation of Channel Name, if it is Valid Channel Name or not.
                //For the time I am assuming it as a Valid Channel Name
                if (CrawlChannel(channelName))
                    MessageBox.Show("Congratulations, Channel has been crawled Successfully", "Success");
            }
            else
                MessageBox.Show("Please Enter a Valid Channel Name","Error");
            
            //---------------------------------------------------------------------------------------------------------

            //If Url of Channel is given

        }

        private bool CrawlChannel(string pChannelName)
        {
            try
            {
                string developerKey = ConfigurationManager.AppSettings["developerKey"].ToString();
                YouTubeRequestSettings settings = new YouTubeRequestSettings("Youtube Crawler App", developerKey);
                YouTubeRequest request = new YouTubeRequest(settings);
                if (Channel.ParseChannel(request, pChannelName))
                    return true;
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
