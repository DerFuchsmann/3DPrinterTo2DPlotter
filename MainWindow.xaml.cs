using Svg;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Xml;

namespace SVGToGCodeGUI
{
    public delegate void PercentEventHandler(object sender, LineSentEventArgs e);
    /// ==============================================================================================LoadSVGStart=================================================================================================
    public partial class MainWindow : Window
    {
        private XmlDocument xmlDoc;
        public Bitmap bmp;
        private List<string> lines = new List<string>();
        private bool writerFlag;
        private SendGCodeManager gCodeSender;
        public MainWindow()
        {
            InitializeComponent();
            gCodeSender = new SendGCodeManager();
            writerFlag = false;
        }
        private void LoadSVG(object sender, RoutedEventArgs e)
        {
            string path = FileManager.GetFilePath();
            if (path != null && path.Length > 0)
            {
                this.xmlDoc = FileManager.GetSVGXml(path);
                DateiPfad.Text = xmlDoc.BaseURI;
                string SvgTxt = FileManager.GetSVGTxt(xmlDoc);
                Ausgabe.Text = SvgTxt;
                var mySvg = SvgDocument.FromSvg<SvgDocument>(SvgTxt);
                this.bmp = FileManager.BitmapFromSVG(mySvg);
                Vorschau.Source = FileManager.BitmapSourcefromSVG(mySvg);
                writerFlag = true;
                GCodeButton.IsEnabled = true;
            }
            else
            {
                Ausgabe.Text = "Es wurde keine Datei ausgewählt!";
            }
        }

        private void GCodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (writerFlag)
            {
                setList(LineLogic.SearchForLines(this.bmp));
                SendGCodeButton.IsEnabled = true;
                printLines();
            }
            else
            {
                Ausgabe.Text = "Es wurde keine Datei gefunden!";
            }
        }

        private void SendGCode(object sender, RoutedEventArgs e)
        {
            gCodeSender.LineEvent += new PercentEventHandler(LineSent);
            this.gCodeSender.ManageSendingGCode(lines);
        }

        public async void LineSent(object sender, LineSentEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ProgressBar1.Value = e.Percentage;
            });
        }

        private void setList(List<string>lines)
        {
            this.lines = lines;
        }

        private void printLines()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                sb.Append(lines[i]);
            }
            AusgabeGcode.Text = sb.ToString();
        }
        private void Ausgabe_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (Ausgabe.Text.Length < 1) 
            {
                writerFlag = false;
                GCodeButton.IsEnabled = false;
            }
        }
        private void AusgabeGCode_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (lines.Count > 0)
            {
                Delete.IsEnabled = true;
            } 
            else
            {
                if (Delete != null)
                {
                Delete.IsEnabled = false;
                }
            }
        }
        private void DeletButtonClick(object sender, RoutedEventArgs e)
        {
            AusgabeGcode.Text = "Vorgang abgebrochen!";
            gCodeSender.CancelSendingGCode();
        }
    }
}