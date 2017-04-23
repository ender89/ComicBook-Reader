using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Drawing;
using Ionic.Zip;
using System.Windows.Controls.Primitives;


namespace CBZ_Reader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        zipHandler zip = new zipHandler();
        double pagewidth = 467;
        bool widthchanged = false;
        bool full = false;
        bool filmshowing = false;
        string[] books = null;
        int booknumber = 0;
        Popup codePopup = new Popup();
        TextBlock popupText = new TextBlock();
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //Right Button
            next();
        }
        private void b_Left_Click(object sender, RoutedEventArgs e)
        {
            //left button
            previous();
        }


        private void b_Right_KeyDown(object sender, KeyEventArgs e)
        {
            Key button = e.Key;
            

           
            if (button == Key.Right)
            {
                //page to the right
                EndPopup.IsOpen = false;
                next();

            }
            else if (button == Key.Left)
            {
                //page to the left
                EndPopup.IsOpen = false;
                previous();
            }
            else if (button == Key.D1)
            {
                //show button areas
                if (b_Left.Opacity != 0)
                {
                    b_Left.Opacity = 0;
                    b_Right.Opacity = 0;
                    OpenFileButton.Opacity = 0;
                    BookLeft.Opacity = 0;
                    BookRight.Opacity = 0;
                }
                else
                {
                    b_Left.Opacity = 1;
                    b_Right.Opacity = 1;
                    OpenFileButton.Opacity = 1;
                    BookLeft.Opacity = 1;
                    BookRight.Opacity = 1;
                }
            }
            else if (button == Key.F11)
            {
                // full screen
                if (!full)
                {
                    full = true;

                    Main.WindowStyle = WindowStyle.None;
                    Main.WindowState = WindowState.Maximized;
                }
                else
                {
                    full = false;

                    Main.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                    Main.WindowState = System.Windows.WindowState.Normal;

                }

            }
            else if (button == Key.Escape && full == true)
            {
                full = false;
                //escape full screen
                Main.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                Main.WindowState = System.Windows.WindowState.Normal;
            }
            else if (button == Key.Add || button == Key.OemPlus)
            {
                //zoom in
                var st = (ScaleTransform)((TransformGroup)MainPage.RenderTransform).Children.ElementAt(1);

                double zoom = .2;

                st.ScaleX += zoom;
                st.ScaleY += zoom;
            }
            else if (button == Key.Subtract || button == Key.OemMinus)
            {
                //zoom out
                var st = (ScaleTransform)((TransformGroup)MainPage.RenderTransform).Children.ElementAt(1);

                double zoom = -.2;

                st.ScaleX += zoom;
                st.ScaleY += zoom;
            }
            else if (button == Key.D0)
            {
                //reset zoom and pan
                var st = (ScaleTransform)((TransformGroup)MainPage.RenderTransform).Children.ElementAt(1);

                st.ScaleX = 1;
                st.ScaleY = 1;
                var tt = (TranslateTransform)((TransformGroup)MainPage.RenderTransform).Children.First(tr => tr is TranslateTransform);

                tt.X = 0.0;
                tt.Y = 0.0;
            }           
        }

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog opendlg = new Microsoft.Win32.OpenFileDialog();
            opendlg.DefaultExt = ".zip";
            opendlg.Filter = "ComicBook Archives (*.cbz; *.cbr; *.zip; *.rar)|*.cbz;*.cbr;*.zip;*.rar|All files (*.*)|*.*";
            Nullable<bool> result = opendlg.ShowDialog();
            if (result == true)
            {
                bool success = openbook(opendlg.FileName);
            }
        }

       //Pan And zoom

        private void Main_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var st = (ScaleTransform)((TransformGroup)MainPage.RenderTransform).Children.ElementAt(1);
            double zoom = e.Delta > 0 ? .2 : -.2;
            System.Windows.Point pos = Mouse.GetPosition(Main);
            st.CenterX = pos.X;
            st.CenterY = pos.Y;
            st.ScaleX += zoom;
            st.ScaleY += zoom;
        }
        System.Windows.Point start;
        System.Windows.Point origin;
        private void Main_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            var tt = (TranslateTransform)((TransformGroup)MainPage.RenderTransform)
                .Children.First(tr => tr is TranslateTransform);
            start = e.GetPosition(Main);
            origin = new System.Windows.Point(tt.X, tt.Y);
            Main.CaptureMouse();
        }

        private void Main_MouseMove(object sender, MouseEventArgs e)
        {
            if (Main.IsMouseCaptured)
            {
                var tt = (TranslateTransform)((TransformGroup)MainPage.RenderTransform).Children.First(tr => tr is TranslateTransform);
                Vector v = start - e.GetPosition(Main);
                tt.X = origin.X - v.X;
                tt.Y = origin.Y - v.Y;
            }
        }

        private void Main_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Main.ReleaseMouseCapture();
        }

        //Page turn functions
        private void next()
        {
            if (zip.curpagenum != zip.totalpages - 1 && zip.dir != null)
            {
                zip.curpagenum++;
                BitmapImage pagerendered = new BitmapImage();


                pagerendered.BeginInit();
                string pageURI = (string)zip.dir[zip.curpagenum];
                pagerendered.UriSource = new Uri(@pageURI);
                pagerendered.CacheOption = BitmapCacheOption.OnLoad;
                pagerendered.EndInit();
                if (widthchanged == true && (float)pagerendered.Width / (float)pagerendered.Height > 1)
                {
                    float nw = ((float)pagerendered.Width * (float)MainPage.ActualHeight) / (float)pagerendered.Height;
                    if (nw > (float)System.Windows.SystemParameters.PrimaryScreenWidth)
                        nw = (float)System.Windows.SystemParameters.PrimaryScreenWidth;
                    Main.Width = (double)nw;
                }
                else if (widthchanged == true && (float)pagerendered.Width / (float)pagerendered.Height < 1)
                {
                    Main.Width = pagewidth;
                    widthchanged = false;
                }
                else if ((float)pagerendered.Width / (float)pagerendered.Height > 1 && widthchanged == false)
                {
                    pagewidth = Main.Width;
                    float nw = ((float)pagerendered.Width * (float)MainPage.ActualHeight) / (float)pagerendered.Height;
                    if (nw > (float)System.Windows.SystemParameters.PrimaryScreenWidth)
                        nw = (float)System.Windows.SystemParameters.PrimaryScreenWidth;
                    Main.Width = (double)nw;
                    widthchanged = true;
                }
                else if (Main.Width != pagewidth)
                    Main.Width = pagewidth;
                //Reset zoom
                var st = (ScaleTransform)((TransformGroup)MainPage.RenderTransform).Children.ElementAt(1);

                st.ScaleX = 1;
                st.ScaleY = 1;
                // reset pan
                var tt = (TranslateTransform)((TransformGroup)MainPage.RenderTransform).Children.First(tr => tr is TranslateTransform);

                tt.X = 0.0;
                tt.Y = 0.0;
                MainPage.Source = pagerendered;

            }
            else if (zip.dir != null)
            {
                EndPopup.StaysOpen = false;
                poptext.Text = "End of Book";
                EndPopup.IsOpen = true;
            }
            else
            {
                EndPopup.StaysOpen = false;
                poptext.Text = "No Open Book";
                EndPopup.IsOpen = true;
            }
        }
        private void previous()
        {
            if (zip.curpagenum != 0 && zip.dir != null)
            {
                zip.curpagenum--;
                BitmapImage pagerendered = new BitmapImage();


                pagerendered.BeginInit();
                string pageURI = (string)zip.dir[zip.curpagenum];
                pagerendered.UriSource = new Uri(@pageURI);
                pagerendered.CacheOption = BitmapCacheOption.OnLoad;
                pagerendered.EndInit();
                if (widthchanged == true && (float)pagerendered.Width / (float)pagerendered.Height > 1)
                {
                    float nw = ((float)pagerendered.Width * (float)MainPage.ActualHeight) / (float)pagerendered.Height;
                    if (nw > (float)System.Windows.SystemParameters.PrimaryScreenWidth)
                        nw = (float)System.Windows.SystemParameters.PrimaryScreenWidth;
                    Main.Width = (double)nw;
                }
                else if (widthchanged == true && (float)pagerendered.Width / (float)pagerendered.Height < 1)
                {

                    Main.Width = pagewidth;
                    widthchanged = false;
                }
                else if ((float)pagerendered.Width / (float)pagerendered.Height > 1 && widthchanged == false)
                {
                    pagewidth = Main.Width;
                    float nw = ((float)pagerendered.Width * (float)MainPage.ActualHeight) / (float)pagerendered.Height;
                    if (nw > (float)System.Windows.SystemParameters.PrimaryScreenWidth)
                        nw = (float)System.Windows.SystemParameters.PrimaryScreenWidth;
                    Main.Width = (double)nw;
                    widthchanged = true;
                }
                else if (Main.Width != pagewidth)
                    Main.Width = pagewidth;
                //Reset zoom
                var st = (ScaleTransform)((TransformGroup)MainPage.RenderTransform).Children.ElementAt(1);

                st.ScaleX = 1;
                st.ScaleY = 1;
                // reset pan
                var tt = (TranslateTransform)((TransformGroup)MainPage.RenderTransform).Children.First(tr => tr is TranslateTransform);

                tt.X = 0.0;
                tt.Y = 0.0;
                MainPage.Source = pagerendered;


            }
            else if (zip.dir != null)
            {
                EndPopup.StaysOpen = false;
                poptext.Text = "Start of Book";
                EndPopup.IsOpen = true;
            }
            else
            {
                EndPopup.StaysOpen = false;
                poptext.Text = "No Open Book";
                EndPopup.IsOpen = true;
            }
        }
        private bool openbook(string filepath)
        {
            bool success = false;
            books = Directory.GetFiles(System.IO.Path.GetDirectoryName(filepath));
            booknumber = Array.IndexOf(books, filepath);
            if (System.IO.Path.GetExtension(filepath) == ".zip" || System.IO.Path.GetExtension(filepath) == ".cbz")
            {

                zip.openArchive(filepath);


            }
            else if (System.IO.Path.GetExtension(filepath) == ".rar" || System.IO.Path.GetExtension(filepath) == ".cbr")
            {
                zip.openRAR(filepath);
            }
            BitmapImage pagerendered = new BitmapImage();
            pagerendered.CacheOption = BitmapCacheOption.OnDemand;
            pagerendered.BeginInit();
            string pageURI = (string)zip.dir[0];
            pagerendered.UriSource = new Uri(@pageURI);
            pagerendered.EndInit();

            if ((float)pagerendered.Width / (float)pagerendered.Height > 1)
            {
                pagewidth = Main.Width;
                float nw = ((float)pagerendered.Width * (float)Main.Height) / (float)pagerendered.Height;
                Main.Width = (double)nw;
                widthchanged = true;
            }
            //Reset zoom
            var st = (ScaleTransform)((TransformGroup)MainPage.RenderTransform).Children.ElementAt(1);

            st.ScaleX = 1;
            st.ScaleY = 1;
            // reset pan
            var tt = (TranslateTransform)((TransformGroup)MainPage.RenderTransform).Children.First(tr => tr is TranslateTransform);

            tt.X = 0.0;
            tt.Y = 0.0;
            MainPage.Source = pagerendered;
            success = true;
            return success;
        }

        private void messagepopup(string msg)
        {
        }
        private void BookRight_Click(object sender, RoutedEventArgs e)
        {
            if (books != null && booknumber != books.GetLength(0)-1)
            {
                EndPopup.StaysOpen = false;
                poptext.Text = "Opening Next Book";
                EndPopup.IsOpen = true;
                booknumber++;
                while (openbook(books[booknumber]) != true)
                {
                    if (booknumber != books.GetLength(0)-1)
                        booknumber++;
                    else
                    {
                        EndPopup.StaysOpen = false;
                        poptext.Text = "No More Books";
                        EndPopup.IsOpen = true;
                        break;
                    }
                }
               
            }
        }

        private void BookLeft_Click(object sender, RoutedEventArgs e)
        {
            
            if (books != null && booknumber != 0)
            {
                EndPopup.StaysOpen = false;
                poptext.Text = "Opening Previous Book";
                EndPopup.IsOpen = true;
                booknumber--;
                while (openbook(books[booknumber]) != true)
                {
                    if (booknumber != books.GetLength(0))
                        booknumber++;
                    else
                    {
                        EndPopup.StaysOpen = false;
                        poptext.Text = "No More Books";
                        EndPopup.IsOpen = true;
                        break;
                    }
                }
                
            }
            else if (books != null && booknumber == 0)
            {
                poptext.Text = "Reached First Book";
                EndPopup.StaysOpen = false;
                EndPopup.IsOpen = true;
            }

        }

        private void button1_Click_2(object sender, RoutedEventArgs e)
        {
           
            if (filmshowing == false)
            {
                
            }
        }


    }
    public class zipHandler
    {
        public static string bpath = System.IO.Path.GetTempPath() + "CBZ\\";
        public ArrayList dir = new ArrayList();
        public string path = bpath + "int";
        
        public int totalpages = 0;
        public int curpagenum = 0;
        public ZipFile zip = null;
        public void openArchive(string filename)
        {
            curpagenum = 0;
            dir = new ArrayList();
            FileStream zipStream = new FileStream(@filename, FileMode.Open);
            zip = ZipFile.Read(zipStream);
            totalpages = zip.Entries.Count;
            if (System.IO.Directory.Exists(bpath))
            {
                DeleteDirectory(bpath);
            }
            System.IO.Directory.CreateDirectory(bpath);
            Random rand = new Random();
            int r = rand.Next();
            path = bpath + r.ToString();
            zip.ExtractAll(path);
            
            string[] dirs = Directory.GetDirectories(path);
            if (dirs.GetLength(0) == 0)
            {
                string[] filelist = Directory.GetFiles(path);
                foreach (string p in filelist)
                {
                    if (System.IO.Path.GetExtension(p) == ".jpg" || System.IO.Path.GetExtension(p) == ".png")
                        dir.Add(p);
                }
            }
            else
            {
                string[] filelist = Directory.GetFiles(dirs[0]);
                foreach (string p in filelist)
                {
                    if (System.IO.Path.GetExtension(p) == ".jpg" || System.IO.Path.GetExtension(p) == ".png")
                        dir.Add(p);
                }
            }
            zipStream.Close();
        }
        public void openRAR(string filename)
        {
            curpagenum = 0;
            dir = new ArrayList();
            Chilkat.Rar rar = new Chilkat.Rar();
            rar.Open(filename);
            totalpages = 0;
            if (System.IO.Directory.Exists(bpath))
            {
                DeleteDirectory(bpath);
            }
            Random rand = new Random();
            int r = rand.Next();
            path = bpath + r.ToString();
            System.IO.Directory.CreateDirectory(path);
            rar.Unrar(path);

            string[] dirs = Directory.GetDirectories(path);
            if (dirs.GetLength(0) == 0)
            {
                string[] filelist = Directory.GetFiles(path);
                foreach (string p in filelist)
                {
                    if (System.IO.Path.GetExtension(p) == ".jpg" || System.IO.Path.GetExtension(p) == ".png")
                    {
                        dir.Add(p);
                        totalpages++;
                    }
                }
            }
            else
            {
                string[] filelist = Directory.GetFiles(dirs[0]);
                foreach (string p in filelist)
                {
                    if (System.IO.Path.GetExtension(p) == ".jpg" || System.IO.Path.GetExtension(p) == ".png")
                    {
                        dir.Add(p);
                        totalpages++;
                    }                        
                }
            }
            rar.Close();
        }
        private static bool DeleteDirectory(string target_dir)
        {
            bool result = false;

            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                try
                {

                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                catch 
                { }

            }

            foreach (string dir in dirs)
            {
                try
                {
                    DeleteDirectory(dir);
                }
                catch { }
            }

            try
            {
                Directory.Delete(target_dir, false);
            }
            catch { }

            return result;
        }
                 
    }


}
