using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrixOriginal, ImageMatrixQuantized;
        List<RGBPixel> p = new List<RGBPixel>() , colorSet = new List<RGBPixel>();
        List<List<int>> clustersList = new List<List<int>>();
        List<edge> MST;
        int[,,] mapper = new int[256, 256, 256];
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                
                ImageMatrixOriginal = ImageOperations.OpenImage(OpenedFilePath, ref mapper, ref colorSet);
                ImageOperations.DisplayImage(ImageMatrixOriginal, pictureBox1);
                MST = ImageOperations.CalculateMST(colorSet.Count, colorSet);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrixOriginal).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrixOriginal).ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            int clusters = (int)nudMaskSize.Value;
            clustersList = ImageOperations.clustering(colorSet.Count, MST, clusters);
            p = ImageOperations.generatePallete(clustersList, colorSet, ref mapper);
            ImageMatrixQuantized = ImageOperations.Quantize(ImageMatrixOriginal, p, mapper);
            ImageOperations.DisplayImage(ImageMatrixQuantized, pictureBox2);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void txtHeight_TextChanged(object sender, EventArgs e)
        {

        }

        private void nudMaskSize_ValueChanged(object sender, EventArgs e)
        {

        }

        private void txtWidth_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void txtGaussSigma_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}