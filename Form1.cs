using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectX
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            BlackDesertScreenCapture bdoCapture = new BlackDesertScreenCapture();

            Bitmap capture = bdoCapture.CaptureScreen();

            pictureBox1.Image = BlackDesertCooking.GetCookingStatusImage(capture);

            var inventoryRect = BlackDesertInventory.LocateInventoryPositionRect(capture);

            var point = BlackDesertInventory.LocateInventoryPositionPoint(capture);

            pictureBox2.Image = BlackDesertInventory.GetInventoryPositionImage(capture, inventoryRect);

            var bdoProcess = BlackDesertProcess.GetBlackDesertProcess();

            BlackDesertProcess.SetForegroundBlackDesert(bdoProcess);

            var inventoryImages = BlackDesertInventory.GetInventoryImages();

            foreach (var inventoryImage in inventoryImages)
            {
                var pictureBoxes = mainTab.TabPages[0].Controls.OfType<PictureBox>();
                pictureBoxes.Where(x => x.Name == (string.Format("inventoryBox{0}", inventoryImage.inventoryItemId))).First().Image = inventoryImage.inventoryItemImage;
            }




        }

        private void button2_Click(object sender, EventArgs e)
        {
            BlackDesertTrading.InitializeCrateStacking();
            inventoryBox1.Image = BlackDesertTrading.MountHasCrates().inventoryItemImage;
        }
    }
}
