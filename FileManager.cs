using Microsoft.Win32;
using Svg;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Xml;

namespace SVGToGCodeGUI
{
    internal abstract class FileManager
    {
        public static string GetFilePath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            bool? success = openFileDialog.ShowDialog();
            if (success == true)
            {
                openFileDialog.OpenFile();
                return openFileDialog.FileName;
            }
            else
            {
                return null;
            }
        }
        public static XmlDocument GetSVGXml(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(path);
            }
            catch (Exception ex) 
            {
            }
            return xmlDoc;
        }
        public static string GetSVGTxt(XmlDocument xmlDoc)
        {
            return xmlDoc.DocumentElement.OuterXml;
        }
        public static BitmapSource BitmapSourcefromSVG (SvgDocument mySvg)
        {
            Bitmap bmp = new Bitmap(mySvg.Draw(100, 0));
            var handle = bmp.GetHbitmap();
            BitmapSource bmS = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return bmS;
        }
        public static Bitmap BitmapFromSVG (SvgDocument mySvg)
        {
            Bitmap bmp = new Bitmap(mySvg.Draw(100, 0));
            return bmp;
        }

    }
}
