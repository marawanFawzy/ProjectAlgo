﻿using System;
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
        public double distance = 0;
        public int[] points = new int[2];
        public edge(double distance, int point1, int point2)
        {
            this.distance = distance;
            this.points[0] = point1;
            this.points[1] = point2;
        }
        public edge(float distance)
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
        public byte red, green, blue;
    }
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        public int[,,] mapper = new int[256, 256, 256];
        public List<RGBPixel> pallete;
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath, ref int[,,] mapper , ref List<RGBPixel> colorSet)
        {
            //List<edge> edges = new List<edge>();
            colorSet = new List<RGBPixel>();
            bool[,,] colorsBoolArray = new bool[256, 265, 265];
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
                        if (colorsBoolArray[Buffer[y, x].red, Buffer[y, x].green, Buffer[y, x].blue] == false)
                        {
                            colorsBoolArray[Buffer[y, x].red, Buffer[y, x].green, Buffer[y, x].blue] = true;
                            colorSet.Add(Buffer[y, x]);
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }
            Console.WriteLine(colorSet.Count + " Distinct colors ");
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
        public static RGBPixel[,] Quantize(RGBPixel[,] ImageMatrix, List<RGBPixel> p, int[,,] mapper)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    RGBPixel quantized = new RGBPixel();
                    quantized = p[mapper[ImageMatrix[i, j].red, ImageMatrix[i, j].green, ImageMatrix[i, j].blue]];
                    Filtered[i, j].red = quantized.red;
                    Filtered[i, j].green = quantized.green;
                    Filtered[i, j].blue = quantized.blue;
                }
            return Filtered;
        }
        static void Sorting(edge[] collection, int N) => QSort(0, N - 1, collection, N);
        static void Swap(int i, int j, edge[] collection)
        {
            edge tmp = collection[i];
            collection[i] = collection[j];
            collection[j] = tmp;
        }
        public static void QSort(int startIndex, int finalIndex, edge[] collection, int N)
        {
            if (startIndex >= finalIndex) return;
            int i = startIndex, j = finalIndex;
            while (i <= j)
            {
                while (i < N && collection[startIndex].distance >= collection[i].distance) i++;
                while (j > -1 && collection[startIndex].distance < collection[j].distance) j--;
                if (i <= j) Swap(i, j, collection);
            }
            Swap(startIndex, j, collection);
            QSort(startIndex, j - 1, collection, N);
            QSort(i, finalIndex, collection, N);
        }
        public static List<edge> CalculateMST(int numberOfDistinctColors , List<RGBPixel> colorSet)
        {
            List<edge> MST;
            bool[] partnersOfTheTree = new bool[numberOfDistinctColors];
            int[] indexer = new int[numberOfDistinctColors];
            double min, dist;
            int least = 0;
            MST = new List<edge>();
            MST.Add( new edge(0, -1, -1));
            partnersOfTheTree[0] = true;
            for (int i = 0; i < numberOfDistinctColors; i++)
            {
                min = 99999;
                int localLeast = least;
                for (int j = 0; j < numberOfDistinctColors; j++)
                {
                    if (partnersOfTheTree[j] == false)
                    {
                        dist = Math.Sqrt(Math.Pow(colorSet[localLeast].red - colorSet[j].red, 2) +
                                      Math.Pow(colorSet[localLeast].green - colorSet[j].green, 2) +
                                      Math.Pow(colorSet[localLeast].blue - colorSet[j].blue, 2));

                        if (i == 0) MST.Add( new edge(dist, i, j));
                        else
                        {
                            edge tmp = MST[j];
                            if (dist <= tmp.distance)
                            {
                                MST[j].distance = dist;
                                MST[j].points[0] = localLeast;
                            }
                            if (tmp.distance <= min)
                            {
                                min = tmp.distance;
                                least = tmp.points[1];
                            }
                        }
                    }
                }
                partnersOfTheTree[least] = true;
            }
            double MST_SUM = 0;
            for (int i = 0; i < MST.Count; i++) MST_SUM = MST_SUM + MST[i].distance;
            Console.WriteLine(MST_SUM + " MST SUM");
            Console.WriteLine("-------------------------------------------------------------");
            return MST;
        }
        public static List<List<int>> clustring(int numberOfDistinctColors , List<edge> MST, int k)
        {
            MST.Sort();
            List<List<int>> clusters = new List<List<int>>();
            int[] indexer = new int[numberOfDistinctColors];
            int cluster = 1, unClusterd = numberOfDistinctColors, numberOfClusters = 0;
            for (int i = 1; i < numberOfDistinctColors; i++)
            {
                edge temp = MST[i];
                int p1 = temp.points[0], p2 = temp.points[1], I1 = indexer[p1], I2 = indexer[p2];
                if (numberOfClusters + unClusterd == k)
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
                    tempCluster.Add(temp.points[0]);
                    tempCluster.Add(temp.points[1]);
                    clusters.Add(tempCluster);
                    indexer[p1] = cluster;
                    indexer[p2] = cluster;
                    cluster++;
                    numberOfClusters++;
                    unClusterd -= 2;
                }
            }
            return clusters;
        }
        public static List<RGBPixel> generatePallete( List<List<int>> clusters, List<RGBPixel> colorSet , ref int[,,] mapper)
        {
            mapper = new int[256, 256, 256];
            List<RGBPixel> palletaa = new List<RGBPixel>();
            int clusterCounter = 0 , count  = clusters.Count;
            for (int i = 0; i < count; i++)
            {
                RGBPixel current;
                double red = 0, green = 0, blue = 0;
                int numberOfClusterElements = clusters[i].Count();
                foreach (var j in clusters[i])
                {
                    red = red + colorSet[j].red;
                    green = green + colorSet[j].green;
                    blue = blue + colorSet[j].blue;
                    mapper[colorSet[j].red, colorSet[j].green, colorSet[j].blue] = clusterCounter;
                }
                if (clusters[i].Count == 0) continue;
                current.red = (byte)Math.Ceiling(red / numberOfClusterElements);
                current.green = (byte)Math.Ceiling(green / numberOfClusterElements);
                current.blue = (byte)Math.Ceiling(blue / numberOfClusterElements);
                palletaa.Add(current);
                clusterCounter++;
            }
            Console.WriteLine(palletaa.Count);
            return palletaa;
        }
    }
}
