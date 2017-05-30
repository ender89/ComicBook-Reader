
using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CBZ_Reader
{
    /// <summary>
    /// Interaction logic for ThumbnailControl.xaml
    /// </summary>
    public partial class ThumbnailControl : UserControl
    {
        public ThumbnailControl()
        {
            InitializeComponent();
        }

        public void setText(string Text)
        {
            ThumbnailLabel.Content = Text;
        }

        public void setImage(string imageURI)
        {
            BitmapImage pagerendered = new BitmapImage();
            pagerendered.CacheOption = BitmapCacheOption.OnDemand;
            pagerendered.BeginInit();
            pagerendered.UriSource = new Uri(imageURI);
            pagerendered.EndInit();            
            ThumbnailImage.Source = pagerendered;
        }
    }
}
