using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVGToGCodeGUI
{
    internal abstract class LineLogic
    {
        public static List<string> SearchForLines(Bitmap bmp)
        {
            List<string> lines = new List<string>();

            lines.Add("G90\n");
            lines.Add("G21\n");
            lines.Add("G28\n");
            lines.Add("G00 X0 Y0 Z15\n");
            lines.Add("M206 X-20 Y-80\n");

            for (int row = 0; row < bmp.Height; row++)
            {
                if (row % 2 == 0)
                {
                    for (int col = 0; col < bmp.Width; col++)
                    {
                        col = findLine(row, col, bmp, lines);
                    }
                }
                else
                {
                    for (int col = bmp.Width - 1; col >= 0; col--)
                    {
                        col = findLine(row, col, bmp, lines);
                    }
                }
            }
            lines.Add("G00 X0 Y0 Z50\n");

            return lines;


        }
        private static int findLine(int row, int col, Bitmap bmp, List<string> lines)
        {
            Color pixel = bmp.GetPixel(col, row);

            if (pixel.A != 0 || pixel.R != 0 || pixel.G != 0 || pixel.B != 0)
            {
                Debug.Print(pixel.ToString());
                Debug.Print(row + " " + col);
            }

            if (pixel.A != 0 && pixel.R < 255 && pixel.G < 255 && pixel.B < 255)
            {
                int startLineY = col;

                col = SearchForLineEnd(row, startLineY, bmp);

                lines.Add("G00 X" + row + " Y" + startLineY + " Z15 \n");
                lines.Add("G00 X" + row + " Y" + startLineY + " Z10 \n");
                Debug.Print("G00 X" + row + " Y" + startLineY + " Z10 \n");

                lines.Add("G01 X" + row + " Y" + col + " Z10 \n");
                lines.Add("G01 X" + row + " Y" + col + " Z15 \n");
                Debug.Print("G01 X" + row + " Y" + col + " Z04 \n");
            }
            return col;
        }
        private static int SearchForLineEnd(int startLineX, int startLineY, Bitmap bmp)
        {
            System.Drawing.Color pixel;
            if (startLineX % 2 == 0)
            {
                for (int y = startLineY; y < bmp.Width; y++)
                {
                    pixel = bmp.GetPixel(y, startLineX);
                    if (pixel.R == 255 && pixel.G == 255 && pixel.B == 255 || pixel.A == 0)
                    {
                        return y - 1;
                    }
                }
                return bmp.Width - 1;
            } 
            else
            {
                for (int y = startLineY; y >= 0; y--)
                {
                    pixel = bmp.GetPixel(y, startLineX);
                    if (pixel.R == 255 && pixel.G == 255 && pixel.B == 255 || pixel.A == 0)
                    {
                        return y + 1;
                    }
                }
                return 0;
            }
        }
    }
}
