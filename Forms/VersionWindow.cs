using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TrainCrewTIDWindow.Forms {
    public partial class VersionWindow : Form {


        public PictureBox PictureIcon => icon;

        public Label LabelVersion => labelVersion;
        public VersionWindow() {
            InitializeComponent();
        }
    }
}
