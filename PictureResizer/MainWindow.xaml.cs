using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PictureResizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OpenFileDialog ofd = new OpenFileDialog
        {
            Multiselect = true,
            Title = "Sélectionner les fichiers à redimensionner"
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Execute(bool askFiles = true)
        {
            if (!int.TryParse(valX.Text, out int width))
            {
                valX.Text = "640";
                width = 640;
            }
            if (!int.TryParse(valY.Text, out int height))
            {
                valY.Text = "640";
                height = 640;
            }

            if (!askFiles || ofd.ShowDialog() == true)
            {
                if (ofd.FileNames.Length == 0)
                    return;

                string dir = string.Empty;
                foreach (var fileName in ofd.FileNames)
                {
                    Bitmap img;
                    if (cbResize.IsChecked == true)
                    {
                        img = ResizeImage(Image.FromFile(fileName), width, height);
                    }
                    else
                    {
                        img = new Bitmap(fileName);
                    }
                    dir = Path.Combine(Path.GetDirectoryName(fileName), "resized");
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    string file = Path.GetFileNameWithoutExtension(fileName);
                    string path = Path.Combine(dir, file + "-resize.jpg");
                    if (cbCompress.IsChecked == true)
                    {
                        CompressImage(img, path, (int)cmbQuality.SelectedItem);
                    }
                    else
                    {
                        CompressImage(img, path, 0);
                    }
                }

                MessageBox.Show($"{ofd.FileNames.Length} image(s) redimensionnée(s)\nLe dossier va s'ouvrir quand cette fenêtre est fermée\n[{dir}]");
                if (Directory.Exists(dir))
                    Process.Start(dir);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Execute();
        }

        public Bitmap ResizeImage(Image image, int width, int height)
        {
            if (image.Width < width)
                width = image.Width;
            if (image.Height < height)
                height = image.Height;

            float factor = (float)image.Width / image.Height;
            bool res = factor >= 1;
            if (valMinMax.IsChecked == false)
                res = !res;
            if (res)
            {
                height = (int)(width / factor);
            }
            else
            {
                width = (int)(height * factor);
            }

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public void CompressImage(Bitmap img, string DestPath, int quality)
        {
            if (quality == 0)
            {
                img.Save(DestPath, ImageFormat.Jpeg);
            }
            else
            {
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                System.Drawing.Imaging.Encoder QualityEncoder = System.Drawing.Imaging.Encoder.Quality;

                EncoderParameters myEncoderParameters = new EncoderParameters(1);

                EncoderParameter myEncoderParameter = new EncoderParameter(QualityEncoder, quality);

                myEncoderParameters.Param[0] = myEncoderParameter;
                img.Save(DestPath, jpgEncoder, myEncoderParameters);
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 10; i <= 100; i = i + 10)
            {
                cmbQuality.Items.Add(i);
            }
            cmbQuality.SelectedIndex = 4;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Execute(false);
        }
    }
}
