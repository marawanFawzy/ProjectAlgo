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
    public class color
    {
        public double red, green, blue;
        public color(double red, double green, double blue)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
        }

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
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            PriorityQueue<edge> edges = new PriorityQueue<edge>();
            PriorityQueue<edge> MST = new PriorityQueue<edge>();
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
            double MST_SUM = 0;
            int counterEdges = 0, tree = 1, edgesCounter = edges.Count();
            List<List<int>> trees = new List<List<int>>();
            for (int i = 0; i < edgesCounter; i++)
            {
                edge temp = edges.Peek();
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
                        edges.Poll();
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
                counterEdges++;
                if (counterEdges == numberOfDistinctColors - 1) break;
                edges.Poll();
            }
            Console.WriteLine(MST_SUM + "MST SUM");
            Console.WriteLine("-------------------------------------------------------------");
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
            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            color[,] VerFiltered = new color[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            color Sum;
            RGBPixel Item1;
            color Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum = new color(0, 0, 0);
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================

            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum = new color(0, 0, 0);
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;

                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;


                }

            return Filtered;
        }


    }
}
