using System.Windows.Forms;
using TrainCrewTIDWindow.Manager;

namespace TrainCrewTIDWindow.Forms {
    public partial class VersionWindow : Form {


        public VersionWindow(Icon? icon) {
            InitializeComponent();
            Icon = icon;
            var bitmap = icon != null ? new Icon(icon, 256, 256).ToBitmap() : new Bitmap(10, 10);
            this.icon.Image = bitmap;
            this.icon.Size = new Size(bitmap.Width, bitmap.Height);
            labelVersion.Text = $"TrainCrewTIDWindow\nVer. {ServerAddress.Version.Replace("TrainCrewTIDWindow_", "")}";

        }
    }
}
