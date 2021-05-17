using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace grafikaLab7p2
{
    public partial class Form1 : Form
    {
        Image basee;
        Bitmap podstawaBitmap;
        Bitmap znormalizowanaBitmap;
        Bitmap gaussBitmap;
        int x;
        int srednia;
        int odcchylenie;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            modifyHistogram modify = new modifyHistogram(new Bitmap(pictureBox1.Image), new Bitmap(pictureBox1.Image), chart1, chart2);
            Blur blur = new Blur(new Bitmap(pictureBox1.Image), new Bitmap(pictureBox1.Image), chart1, chart2);
            pictureBox2.Image = modify.kontrast();
            pictureBox3.Image = blur.wyrownanie(5);
            pictureBox4.Image = blur.gauss(5);
        }
    }
    class Blur
    {
        Bitmap podstawaBitmap;
        Chart podstawaHistogram;
        Chart znormalizowanaHistogram;
        private int[] intensity = new int[3];
        private int[] odchylenie = new int[3];

        public Blur(Bitmap postawaBitmap, Bitmap znormalizowanaBitmap, Chart podstawaHistogram, Chart znormalizowanaHistogram)
        {
            this.podstawaBitmap = postawaBitmap;
            this.podstawaHistogram = podstawaHistogram;
            this.znormalizowanaHistogram = znormalizowanaHistogram;
        }


        public Bitmap wyrownanie(int n)
        {
            Color[,] maska = new Color[n, n];
            int[] srednia;
            for (int i = n - 1; i < podstawaBitmap.Width; i += n)
            {
                for (int j = n - 1; j < podstawaBitmap.Height; j += n)
                {
                    for (int x = 0; x < n; x++)
                    {
                        for (int y = 0; y < n; y++)
                        {
                            maska[x, y] = podstawaBitmap.GetPixel(i - x, j - y);
                        }
                    }
                    srednia = this.srednia(maska);
                    for (int x = 0; x < n; x++)
                    {
                        for (int y = 0; y < n; y++)
                        {
                            podstawaBitmap.SetPixel(i - x, j - y, Color.FromArgb(srednia[0],
                                srednia[1],
                                srednia[2]));
                        }
                    }
                }
            }
            return podstawaBitmap;
        }

        private int[] srednia(Color[,] mask)
        {
            int maskSize = mask.Length / mask.GetLength(1);
            int[] srednie = new int[maskSize];
            for (int i = 0; i < maskSize; i++)
            {
                for (int j = 0; j < maskSize; j++)
                {
                    srednie[0] += mask[i, j].R;
                    srednie[1] += mask[i, j].G;
                    srednie[2] += mask[i, j].B;
                }
            }
            srednie[0] /= mask.Length;
            srednie[1] /= mask.Length;
            srednie[2] /= mask.Length;

            return srednie;
        }

        public Bitmap gauss(int radius)
        {
            var size = (radius * 2) + 1;
            var deviation = radius / 2;
            var mask = new double[size, size];
            double sum = 0.0;
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    int numerator = -(i * i + j * j);
                    int denominator = 2 * deviation * deviation;
                    var eExpre = Math.Pow(Math.E, numerator / denominator);
                    var value = (eExpre / (2 * Math.PI * deviation * deviation));

                    mask[i + radius, j + radius] = value;
                    sum += value;
                }
            }

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    mask[i, j] /= sum;
                }
            }

            for (int x = radius; x < podstawaBitmap.Width - radius; x++)
            {
                for (int y = radius; y < podstawaBitmap.Height - radius; y++)
                {
                    double red = 0, green = 0, blue = 0;

                    for (int i = -radius; i <= radius; i++)
                    {
                        for (int j = -radius; j <= radius; j++)
                        {
                            double temp = mask[i + radius, j + radius];
                            var pixel = podstawaBitmap.GetPixel(x - i, y - j);

                            red += pixel.R * temp;
                            green += pixel.G * temp;
                            blue += pixel.B * temp;
                        }
                    }
                    podstawaBitmap.SetPixel(x, y, Color.FromArgb(
                        checkIfInRgb(red), checkIfInRgb(green), checkIfInRgb(blue)));
                }
            }
            return podstawaBitmap;
        }

        private int checkIfInRgb(double temp)
        {
            if (temp > 255) return 255;
            else if (temp < 0) return 0;
            return (int)temp;
        }
    }
    class modifyHistogram
    {
        Bitmap podstawaBitmap;
        Bitmap znormalizowanaBitmap;
        int NM;
        private Chart podstawaHistogram;
        private Chart znormalizowanaHistogram;
        double[] histogramRed = new double[256];
        double[] histogramGreen = new double[256];
        double[] histogramBlue = new double[256];

        public modifyHistogram(Bitmap podstawaBitmap, Bitmap znormalizowanaBitmap, Chart podstawaHistogram, Chart znormalizowanaHistogram)
        {
            this.podstawaBitmap = podstawaBitmap;
            this.znormalizowanaBitmap = znormalizowanaBitmap;
            NM = podstawaBitmap.Width * podstawaBitmap.Height;
            this.podstawaHistogram = podstawaHistogram;
            this.znormalizowanaHistogram = znormalizowanaHistogram;
            histogramPodstawa();
            fillHistogram();
        }


        private void histogramPodstawa()
        {
            double[] red = new double[256];
            double[] green = new double[256];
            double[] blue = new double[256];
            for (int x = 0; x < podstawaBitmap.Width; x++)
            {
                for (int y = 0; y < podstawaBitmap.Height; y++)
                {
                    Color pixel = podstawaBitmap.GetPixel(x, y);
                    red[pixel.R]++;
                    green[pixel.G]++;
                    blue[pixel.B]++;
                }
            }


            podstawaHistogram.Series["red"].Points.Clear();
            podstawaHistogram.Series["green"].Points.Clear();
            podstawaHistogram.Series["blue"].Points.Clear();
            for (int i = 0; i < 256; i++)
            {
                podstawaHistogram.Series["red"].Points.AddXY(i, red[i] / NM);
                podstawaHistogram.Series["green"].Points.AddXY(i, green[i] / NM);
                podstawaHistogram.Series["blue"].Points.AddXY(i, blue[i] / NM);
            }
            podstawaHistogram.Invalidate();
        }

        private void fillHistogram()
        {

            for (int i = 0; i < 256; i++)
            {
                histogramRed[i] = kumulacjaHistogramu(i, "red");
                histogramGreen[i] = kumulacjaHistogramu(i, "green");
                histogramBlue[i] = kumulacjaHistogramu(i, "blue");
                znormalizowanaHistogram.Series["red"].Points.AddXY(i, histogramRed[i]);
                znormalizowanaHistogram.Series["green"].Points.AddXY(i, histogramGreen[i]);
                znormalizowanaHistogram.Series["blue"].Points.AddXY(i, histogramBlue[i]);
            }
        }

        private double kumulacjaHistogramu(int poziom, string kolor)
        {
            if (poziom == 0) return podstawaHistogram.Series[kolor].Points[0].YValues[podstawaHistogram.Series[kolor].Points[0].YValues.Length - 1];
            else return podstawaHistogram.Series[kolor].Points[poziom].YValues[podstawaHistogram.Series[kolor].Points[0].YValues.Length - 1]
                    + kumulacjaHistogramu(poziom - 1, kolor);
        }

        public Bitmap kontrast()
        {

            int r, g, b;
            for (int x = 0; x < podstawaBitmap.Width; x++)
            {
                for (int y = 0; y < podstawaBitmap.Height; y++)
                {
                    Color pixel = podstawaBitmap.GetPixel(x, y);
                    r = (int)(255 * histogramRed[pixel.R]);
                    g = (int)(255 * histogramGreen[pixel.G]);
                    b = (int)(255 * histogramBlue[pixel.B]);
                    znormalizowanaBitmap.SetPixel(x, y, Color.FromArgb(pixel.A, r, g, b));
                }
            }
            return znormalizowanaBitmap;
        }
    }

}

