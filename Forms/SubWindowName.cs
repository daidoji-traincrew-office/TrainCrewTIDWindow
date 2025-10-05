

namespace TrainCrewTIDWindow.Forms {
    public partial class SubWindowName : Form {

        private SubWindow window;

        public SubWindowName(SubWindow window) {
            this.window = window;
            InitializeComponent();

        }



        private void buttonCancel_Click(object sender, EventArgs e) {
            Close();
        }

        private void buttonDecision_Click(object sender, EventArgs e) {
            DecideName();
        }

        private void DecideName() {
            var name = textBox1.Text;
            if(name.Length <= 0) {
                TaskDialog.ShowDialog(new TaskDialogPage {
                    Caption = "使用できないウィンドウ名 | TID - ダイヤ運転会",
                    Heading = "使用できないウィンドウ名",
                    Icon = TaskDialogIcon.Warning,
                    Text =
                        $"ウィンドウ名を入力してください。"
                });
            }
            else if (name == "全線TID") {
                TaskDialog.ShowDialog(new TaskDialogPage {
                    Caption = "使用できないウィンドウ名 | TID - ダイヤ運転会",
                    Heading = "使用できないウィンドウ名",
                    Icon = TaskDialogIcon.Warning,
                    Text =
                        $"このウィンドウ名は使用できません。"
                });
            }
            else {
                window.SetWindowName(name);
                Close();
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                DecideName();
            }
        }
    }
}
