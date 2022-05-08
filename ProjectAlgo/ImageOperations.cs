using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using NetTopologySuite.Utilities;
using System.Linq;
using System.Collections;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    public class edge : IComparable<edge>
    {
        public double distance;
        public int[] points = new int[2];
        public edge(double distance, int point1, int point2)
        {
            this.distance = distance;
            this.points[0] = point1;
            this.points[1] = point2;

        }
        public edge(double distance)
        {
            this.distance = distance;
        }
        public edge(edge e)
        {
            this.distance = e.distance;
            this.points[0] = e.points[0];
            this.points[1] = e.points[1];
        }

        public int CompareTo(edge other)
        {
            return other.distance.CompareTo(this.distance) * -1;
        }
    }
    public struct color
    {
        public double red, green, blue;

    }
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel : IComparable<RGBPixel>
    {
        public byte red, green, blue;

        public int CompareTo(RGBPixel other)
        {
            if (other.red > this.red) return 1;
            if (other.red < this.red) return -1;
            if (other.green > this.green) return 1;
            if (other.green < this.green) return -1;
            if (other.blue > this.blue) return 1;
            if (other.blue < this.blue) return -1;
            return 0;
        }
    }


    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        public List<RGBPixel> pallete;
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath, ref List<RGBPixel> palletaa)
        {
            List<edge> edges = new List<edge>();
            List<edge> MST = new List<edge>();
            SortedSet<RGBPixel> colorSet = new SortedSet<RGBPixel>();


            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;
            RGBPixel[,] Buffer = new RGBPixel[Height, Width];
            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;
                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {

                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                        colorSet.Add(Buffer[y, x]);
                    }

                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }
            int numberOfDistinctColors = colorSet.Count;
            Console.WriteLine(numberOfDistinctColors + " Distinct colors ");
            List<RGBPixel> k = new List<RGBPixel>(colorSet.ToList());
            int[] indexer = new int[numberOfDistinctColors];
            colorSet.Clear();
            for (int i = 0; i < numberOfDistinctColors; i++)
            {
                RGBPixel tempcolor = k[i];
                for (int j = 0; j < i; j++)
                {
                    RGBPixel tempcolorj = k[j];
                    edges.Add(new edge(
                            Math.Sqrt(
                                 Math.Pow(tempcolorj.red - tempcolor.red, 2) +
                                 Math.Pow(tempcolorj.green - tempcolor.green, 2) +
                                 Math.Pow(tempcolorj.blue - tempcolor.blue, 2)
                                ), j, i));
                }
            }
            edges.Sort();

            double MST_SUM = 0;
            int counter_MST_Edges = 0, tree = 1, edgesCounter = edges.Count();
            List<List<int>> trees = new List<List<int>>();
            for (int i = 0; i < edgesCounter; i++)
            {
                edge temp = edges[i];
                int p1 = temp.points[0], p2 = temp.points[1], I1 = indexer[p1], I2 = indexer[p2];
                if (indexer[p1] != 0 && indexer[p2] != 0)
                {
                    if (I1 != I2)
                    {
                        int keep = I2;
                        trees[keep - 1].ForEach(l => indexer[l] = I1);
                        trees[I1 - 1].AddRange(trees[keep - 1]);
                    }
                    else if (I1 == I2)
                    {
                        //edges.Poll();
                        continue;
                    }
                }
                else if (I1 != 0)
                {
                    indexer[p2] = I1;
                    trees[I1 - 1].Add(p2);

                }
                else if (I2 != 0)
                {
                    indexer[p1] = I2;
                    trees[I2 - 1].Add(p1);
                }
                else
                {
                    List<int> tempTree = new List<int>();
                    tempTree.AddRange(temp.points);
                    trees.Add(tempTree);
                    indexer[p1] = tree;
                    indexer[p2] = tree;
                    tree++;
                }
                MST_SUM = MST_SUM + temp.distance;
                MST.Add(temp);
                counter_MST_Edges++;
                if (counter_MST_Edges == numberOfDistinctColors - 1) break;
                //edges.Poll();
            }
            trees.Clear();
            Console.WriteLine(MST_SUM + "MST SUM");
            Console.WriteLine("-------------------------------------------------------------");
            // clustring 
            indexer = new int[numberOfDistinctColors];
            int cluster = 1, unClusterd = numberOfDistinctColors, numberOfClusters = 0;
            List<List<int>> clusters = new List<List<int>>();
            for (int i = 0; i < counter_MST_Edges; i++)
            {
                edge temp = MST[i];
                int p1 = temp.points[0], p2 = temp.points[1], I1 = indexer[p1], I2 = indexer[p2];
                if (numberOfClusters + unClusterd == 2160)
                {
                    if (unClusterd == 0) break;
                    if (I1 == 0)
                    {
                        List<int> tempCluster = new List<int>();
                        tempCluster.Add(p1);
                        clusters.Add(tempCluster);
                        indexer[p1] = cluster;
                        cluster++;
                        numberOfClusters++;
                        unClusterd--;
                    }
                    if (I2 == 0)
                    {
                        List<int> tempCluster = new List<int>();
                        tempCluster.Add(p2);
                        clusters.Add(tempCluster);
                        indexer[p2] = cluster;
                        cluster++;
                        numberOfClusters++;
                        unClusterd--;
                    }
                    continue;
                }

                if (indexer[p1] != 0 && indexer[p2] != 0)
                {
                    if (I1 != I2)
                    {
                        int keep = I2;
                        clusters[keep - 1].ForEach(l => indexer[l] = I1);
                        clusters[I1 - 1].AddRange(clusters[keep - 1]);
                        clusters[keep - 1].Clear();
                        numberOfClusters--;
                    }
                    else if (I1 == I2)
                    {
                        continue;
                    }
                }
                else if (I1 != 0)
                {
                    indexer[p2] = I1;
                    clusters[I1 - 1].Add(p2);
                    unClusterd--;

                }
                else if (I2 != 0)
                {
                    indexer[p1] = I2;
                    clusters[I2 - 1].Add(p1);
                    unClusterd--;
                }
                else
                {
                    List<int> tempCluster = new List<int>();
                    tempCluster.AddRange(temp.points);
                    clusters.Add(tempCluster);
                    indexer[p1] = cluster;
                    indexer[p2] = cluster;
                    cluster++;
                    numberOfClusters++;
                    unClusterd -= 2;
                }

            }
            palletaa = new List<RGBPixel>();
            for (int i = 0; i < clusters.Count; i++)
            {
                RGBPixel current;
                double red = 0, green = 0, blue = 0;
                foreach (var j in clusters[i])
                {
                    red = red + k[j].red;
                    green = green + k[j].green;
                    blue = blue + k[j].blue;
                }
                if (clusters[i].Count == 0) continue;
                current.red = (byte)Math.Ceiling(red / clusters[i].Count());
                current.green = (byte)Math.Ceiling(green / clusters[i].Count());
                current.blue = (byte)Math.Ceiling(blue / clusters[i].Count());
                palletaa.Add(current);
            }
            Console.WriteLine(palletaa.Count);
            ///////////////////    
            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            SaveFileDialog dialog = new SaveFileDialog();
            ImageBMP.Save("myfile.png", ImageFormat.Png);
            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public static RGBPixel[,] Quantize(RGBPixel[,] ImageMatrix, int clusters, List<RGBPixel> p)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];

            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    double min_dist = 99999;
                    int res = 0 ;
                    for (int k = 0; k < p.Count; k++)
                    {
                        double current = Math.Sqrt(
                                 Math.Pow(ImageMatrix[i, j].red - p[k].red, 2) +
                                 Math.Pow(ImageMatrix[i, j].green - p[k].green, 2) +
                                 Math.Pow(ImageMatrix[i, j].blue - p[k].blue, 2));
                        if (current < min_dist)
                        {
                            min_dist = current;
                            res = k; 
                        }
                    }
                    Filtered[i, j].red = p[res].red;
                    Filtered[i, j].green = p[res].green;
                    Filtered[i, j].blue = p[res].blue;


                }

            return Filtered;
        }


    }
}
