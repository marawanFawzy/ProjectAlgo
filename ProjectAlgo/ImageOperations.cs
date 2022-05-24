using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Linq;
using System.Diagnostics;
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
        public int CompareTo(edge other) => other.distance.CompareTo(this.distance) * 1;
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
        public static Stopwatch overAllTime = new Stopwatch();
        public static double phase1Time;
        public static List<List<int>> graph = new List<List<int>>();
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath, ref int[,,] mapper, ref List<RGBPixel> colorSet)
        {
            //List<edge> edges = new List<edge>();
            colorSet = new List<RGBPixel>();
            bool[,,] colorsBoolArray = new bool[256, 265, 265];
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;
            RGBPixel[,] Buffer = new RGBPixel[Height, Width];
            graph = new List<List<int>>();
            Stopwatch DistinctTime = new Stopwatch();
            DistinctTime.Start();
            overAllTime.Start();
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
                for (y = 0; y < Height; y++)//O(N)
                {
                    for (x = 0; x < Width; x++)//O(N)
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
                        // check if the color is present in the list 
                        if (colorsBoolArray[Buffer[y, x].red, Buffer[y, x].green, Buffer[y, x].blue] == false)//O(1)
                        {
                            // sets the color is present in the list 
                            colorsBoolArray[Buffer[y, x].red, Buffer[y, x].green, Buffer[y, x].blue] = true;//O(1)
                            // adds the color to list
                            colorSet.Add(Buffer[y, x]);//O(1)
                            graph.Add(new List<int>());
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }


            //Console.WriteLine(colorSet.Count + " Distinct colors ");
            DistinctTime.Stop();
            overAllTime.Stop();
            MessageBox.Show(colorSet.Count.ToString() + " Number of distinct colors in time " + DistinctTime.ElapsedMilliseconds + " ms");

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
        public static List<edge> CalculateMST(int numberOfDistinctColors, List<RGBPixel> colorSet)
        {
            Stopwatch MSTTime = new Stopwatch();
            MSTTime.Start();
            overAllTime.Start();
            // bool array to check optimized vertcies 
            bool[] partnersOfTheTree = new bool[numberOfDistinctColors];
            int least = 0, localLeast = 0;
            double min = 99999, dist;
            // holds the edge to reduce access time
            edge tmp;
            List<edge> MST = new List<edge>();
            // adds a fake edge to start the MST indexing from 1 
            MST.Add(new edge(0, 0, 0));//O(1)
            int k = 1;
            // loops from 0 to all other vertcies
            while (k < numberOfDistinctColors)//O(N)
            {
                // calculating the distance 
                byte red1 = colorSet[localLeast].red;//O(1)
                byte green1 = colorSet[localLeast].green;//O(1)
                byte blue1 = colorSet[localLeast].blue;//O(1)
                byte red2 = colorSet[k].red;//O(1)
                byte green2 = colorSet[k].green;//O(1)
                byte blue2 = colorSet[k].blue;//O(1)
                double red = (red1 - red2) * (red1 - red2);//O(1)
                double green = (green1 - green2) * (green1 - green2);//O(1)
                double blue = (blue1 - blue2) * (blue1 - blue2);//O(1)
                dist = Math.Sqrt(red + green + blue);//O(1)
                // adding the edge to the initial MST
                MST.Add(new edge(dist, 0, k));//O(1)
                //adding each point to other to create graph 
                graph[localLeast].Add(k);//O(1)
                graph[k].Add(localLeast);//O(1)
                // checks if it is the minimmum edge to start from its end
                if (dist < min)//O(1)
                {
                    // change the minimum
                    min = dist;//O(1)
                    // save the index of the next start over
                    least = k;//O(1)
                }
                k++;
            }
            k = 1;
            // looping from every vertix exept of 0 to the others 
            while (k < numberOfDistinctColors)//O(N)
            {
                //reset the min for every vertix 
                min = 99999;
                //set the startPoint as optiml way found 
                partnersOfTheTree[least] = true;
                // reserve the start point index to prevent fomr modifications 
                localLeast = least;
                // check the distance to all others from the current start point 
                for (int j = 1; j < numberOfDistinctColors; j++)//O(D)
                {
                    // exclude the optimal points if found 
                    if (partnersOfTheTree[j] == false)//O(D)
                    {
                        // calculating the distance 
                        byte red1 = colorSet[localLeast].red;//O(1)
                        byte green1 = colorSet[localLeast].green;//O(1)
                        byte blue1 = colorSet[localLeast].blue;//O(1)
                        byte red2 = colorSet[j].red;//O(1)
                        byte green2 = colorSet[j].green;//O(1)
                        byte blue2 = colorSet[j].blue;//O(1)
                        double red = (red1 - red2) * (red1 - red2);//O(1)
                        double green = (green1 - green2) * (green1 - green2);//O(1)
                        double blue = (blue1 - blue2) * (blue1 - blue2);//O(1)
                        dist = Math.Sqrt(red + green + blue);//O(1)
                        // hold the edge to reduce access time
                        tmp = MST[j];//O(1)
                        //check if it is a better way to this vertix 
                        if (dist < tmp.distance)//O(1)
                        {
                            // removing each point from each other lists 
                            graph[j].Remove(MST[j].points[0]);//O(N)
                            graph[MST[j].points[0]].Remove(j);//O(N)
                            //replace the distance 
                            MST[j].distance = dist;//O(1)
                            //replace the starting index 
                            MST[j].points[0] = localLeast;//O(1)
                            // adding each point from the new edge to other's lists 
                            graph[j].Add(MST[j].points[0]);//O(1)
                            graph[MST[j].points[0]].Add(j);//O(1)
                        }
                        //check if it is the best edge in the tree to set the next start point
                        if (tmp.distance < min)//O(1)
                        {
                            //change the min 
                            min = tmp.distance;//O(1)
                            // reserve the index 
                            least = tmp.points[1];//O(1)
                        }
                    }
                }
                k++;
            }
            MST_SUM = 0;
            //calculating the MST SUM 
            for (int i = 0; i < MST.Count; i++) //O(N)
                MST_SUM = MST_SUM + MST[i].distance;
            MSTTime.Stop();
            overAllTime.Stop();
            phase1Time = overAllTime.ElapsedMilliseconds;
            //Console.WriteLine(MST_SUM + " MST SUM");
            MessageBox.Show(MST_SUM.ToString() + " MST SUM IN TIME " + MSTTime.ElapsedMilliseconds + " ms");
            //Console.WriteLine(MSTCOUNT - 1 + " MST COUNT");
            //Console.WriteLine(MEAN + " MST MEAN");
            ////List<double> std = new List<double>();
            //double stdSUM = 0;
            //int current = 0;

            return MST;
        }
        public static double MST_SUM = 0;
        public static List<List<int>> clustering(int numberOfDistinctColors, List<edge> MST, int k)
        {
            Stopwatch clusterTime = new Stopwatch();
            clusterTime.Start();
            overAllTime.Restart();
            int cluster_counter = 0;//O(1)
            List<List<int>> clusters = new List<List<int>>();//O(1)
            Queue<int> q = new Queue<int>();//O(1)
            bool[] partnersOfTheTree = new bool[numberOfDistinctColors];//O(1)
            MST.Sort();//O(Nlog(N))
            int MSTCOUNT = MST.Count() - 1;//O(1)
            double MEAN = MST_SUM / MSTCOUNT;//O(1)
            double stdSUM = 0;//O(1)
            int STDcount = MSTCOUNT - 1;//O(1)
            // calculate the standerd deveation  
            for (int i = 0; i < MSTCOUNT; i++)//O(E) bouns loop 
                stdSUM += (MST[i].distance - MEAN) * (MST[i].distance - MEAN);
            stdSUM /= STDcount;
            stdSUM = Math.Sqrt(stdSUM);
            ////////////////////////////////////
            double stdSUM_loop = stdSUM;
            int h = 0;//O(1)
            int r = MSTCOUNT - 1;//O(1)
            while (Math.Abs(stdSUM - stdSUM_loop) >= 0.0001 || h == 0)//O(E) bouns loop // to check if stoping 
            {
                stdSUM = stdSUM_loop;
                stdSUM_loop = 0;
                if (Math.Abs(MST[r].distance - MEAN) >= MST[h].distance - MEAN) // decide which makes more reduction 
                {
                    // breaking the edge 
                    graph[MST[r].points[0]].Remove(MST[r].points[1]);//O(N)
                    graph[MST[r].points[1]].Remove(MST[r].points[0]);//O(N)
                    //calculate new mean 
                    MEAN = MEAN * (STDcount + 1);
                    MEAN = MEAN - MST[r].distance;
                    MEAN = MEAN / STDcount;
                    r--;// skipping this edge 
                    //calculate new standerd deveation
                    for (int i = h; i < r; i++)//O(E) bouns loop 
                        stdSUM_loop += (MST[i].distance - MEAN) * (MST[i].distance - MEAN);
                    stdSUM_loop /= STDcount;
                    stdSUM_loop = Math.Sqrt(stdSUM_loop);
                }
                else
                {
                    // breaking the edge 
                    graph[MST[h].points[0]].Remove(MST[h].points[1]);//O(N)
                    graph[MST[h].points[1]].Remove(MST[h].points[0]);//O(N)
                    //calculate new mean
                    MEAN = MEAN * (STDcount + 1);
                    MEAN = MEAN - MST[h].distance;
                    MEAN = MEAN / STDcount;
                    h++; // skipping this edge 
                    //calculate new standerd deveation
                    for (int i = h; i < r; i++)//O(E) bouns loop 
                        stdSUM_loop += (MST[i].distance - MEAN) * (MST[i].distance - MEAN);
                    stdSUM_loop /= STDcount;
                    stdSUM_loop = Math.Sqrt(stdSUM_loop);
                }
                STDcount--;
                if (STDcount == 0) break; // handle error 
            }
            for (int i = 0; i < graph.Count; i++)//O(D)
                if (partnersOfTheTree[i] == false)
                {
                    //Console.WriteLine(stdSUM + " MST STD");
                    //Console.WriteLine("-------------------------------------------------------------");
                    clusters.Add(new List<int>());//O(1)
                    // adding start point 
                    q.Enqueue(i);//O(1)
                    //checking if all connected subtree is computed 
                    while (q.Count != 0)//O(N) 
                    {
                        //for (int i = 1; i < MSTCOUNT; i++) stdSUM += Math.Pow((MST[i].distance - MEAN), 2);
                        // temp to hold peek to reduce time then Dequeue 
                        var tmpQ = q.Dequeue();
                        // adding all direct connactions 
                        for (int j = 0; j < graph[tmpQ].Count; j++)//O(N)
                            if (partnersOfTheTree[graph[tmpQ][j]] == false)//O(1) //to avoid retake 
                                q.Enqueue(graph[tmpQ][j]);//O(1)
                        partnersOfTheTree[tmpQ] = true;//O(1)// mark as taken 
                        clusters[cluster_counter].Add(tmpQ);//O(1) // add to cluster 
                        //double currentMean = MEAN;
                        //int current_MSTCOUNT = MSTCOUNT;
                    }
                    cluster_counter++;
                }
            clusterTime.Stop();
            overAllTime.Stop();
            MessageBox.Show("number of cluster "+ clusters.Count + " --> clustring done in " + clusterTime.ElapsedMilliseconds + " ms");
            return clusters;
        }
        public static List<RGBPixel> generatePallete(List<List<int>> clusters, List<RGBPixel> colorSet, ref int[,,] mapper)
        {
            overAllTime.Start();
            Stopwatch generatePalleteTime = new Stopwatch();
            generatePalleteTime.Start();
            //re intiailize the mapper to over write any old data 
            mapper = new int[256, 256, 256];//O(1)
            List<RGBPixel> palletaa = new List<RGBPixel>();
            int clusterCounter = 0, count = clusters.Count;
            for (int i = 0; i < count; i++)//O(N)
            {
                // holds the color 
                RGBPixel current;//O(1)
                double red = 0, green = 0, blue = 0;//O(1)
                int numberOfClusterElements = clusters[i].Count();//O(1)
                // clculates the avarage of red , green and blue
                foreach (var j in clusters[i])
                {
                    red = red + colorSet[j].red;
                    green = green + colorSet[j].green;
                    blue = blue + colorSet[j].blue;
                    //sets the referance to the color to it's cluster number
                    mapper[colorSet[j].red, colorSet[j].green, colorSet[j].blue] = clusterCounter;//O(1)
                }
                // overcoming the error from clustring step by skipping every empty cluster 
                if (clusters[i].Count == 0) continue;
                // round to handle double numbers error 
                current.red = (byte)Math.Round(red / numberOfClusterElements);//O(1)
                current.green = (byte)Math.Round(green / numberOfClusterElements);//O(1)
                current.blue = (byte)Math.Round(blue / numberOfClusterElements);//O(1)
                // add the color to the pallet list in the same refernce to the mapper 
                palletaa.Add(current);//O(1)
                // change ther referance 
                clusterCounter++;//O(1)
            }
            generatePalleteTime.Stop();
            overAllTime.Stop();
            MessageBox.Show("generating Pallet done in  time " + generatePalleteTime.ElapsedMilliseconds + " ms");
            return palletaa;
        }
        public static RGBPixel[,] Quantize(RGBPixel[,] ImageMatrix, List<RGBPixel> p, int[,,] mapper)
        {
            Stopwatch QuantizeTime = new Stopwatch();
            QuantizeTime.Start();
            overAllTime.Start();
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];
            //loop to get every pixel in the picture to replace 
            for (int i = 0; i < Height; i++)//O(N)
                for (int j = 0; j < Width; j++)//O(N)
                {
                    RGBPixel quantized = new RGBPixel();
                    //gets the pallet referance from the mapper then saves the color 
                    quantized = p[mapper[ImageMatrix[i, j].red, ImageMatrix[i, j].green, ImageMatrix[i, j].blue]];//O(1)
                    //replace the old color by it's representitive 
                    Filtered[i, j].red = quantized.red;//O(1)
                    Filtered[i, j].green = quantized.green;//O(1)
                    Filtered[i, j].blue = quantized.blue;//O(1)
                }
            QuantizeTime.Stop();
            overAllTime.Stop();
            MessageBox.Show("image mapping colors done in time " + QuantizeTime.ElapsedMilliseconds + " ms");
            MessageBox.Show("over all time is  " + (overAllTime.ElapsedMilliseconds + phase1Time) + " ms");
            MessageBox.Show("over all time is  " + ((overAllTime.ElapsedMilliseconds + phase1Time) / 1000) + " S");
            MessageBox.Show("over all time is  " + ((overAllTime.ElapsedMilliseconds + phase1Time) / 1000 / 60) + " M");
            return Filtered;
        }
    }
}
