using System.IO.Ports;
namespace SerialTestTool
{
    public partial class Form1 : Form
    {
        private SerialCommunication sc; // シリアル通信用インスタンス

        public Form1()
        {
            InitializeComponent();
            this.sc = SerialCommunication.GetInstance;
            this.sc.setForm(this);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 有効なシリアルポートを列挙
            string[] ports = SerialPort.GetPortNames();
            comboBoxCom.Items.Clear(); // 既存のCOMポートリストを削除
            foreach (string port in ports)
            {
                comboBoxCom.Items.Add(port);
            }
            if (comboBoxCom.Items.Count > 0)
                comboBoxCom.SelectedIndex = 0;
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (sc.serialPort.IsOpen)
            {
                sc.disconnect(); // 既に開いている場合は切断
                buttonConnect.Text = "接続";
                this.statusInfo = "シリアルポートを切断しました";
            }
            else
            {
                string comName = comboBoxCom.Text; // 選択されているCOMポート名を取得
                if (String.IsNullOrEmpty(comName)) // 空の場合はメッセージを表示
                {
                    this.statusError = "有効なCOMポートがありません";
                    return;
                }
                if (this.sc.connect(comName)) // シリアルポートに接続
                {
                    this.statusInfo = "シリアルポートに接続しました";
                    buttonConnect.Text = "切断";
                }
                else
                {
                    this.statusError = "シリアルポートの接続に失敗しました"; // 接続エラーの場合はメッセージを表示
                }
            }
        }

        async private void buttonStart_Click(object sender, EventArgs e)
        {
            int nBytes = 0;
            int startNum = 0;
            int endNum = 0;
            double interval = 0.0;
            try
            {
                nBytes = Int32.Parse(textBoxBytes.Text);
            } catch
            {
                this.statusError = "Byte数が正しくありません";
            }

            try
            {
                startNum = Int32.Parse(textBoxStartNum.Text);
                endNum = Int32.Parse(textBoxEndNum.Text);
                if (startNum < 0 || endNum < 1 || startNum >= endNum)
                {
                    this.statusError = "開始番号または終了番号が正しくありません";
                }
            } catch
            {
                this.statusError = "開始番号または終了番号が正しくありません";
            }
            try
            {
                interval = double.Parse(textBoxInterval.Text);
            } catch
            {
                this.statusError = "インターバルが正しくありません";
            }

            System.Diagnostics.Debug.WriteLine($"nBytes: {nBytes}\nstartNum: {startNum} - endNum: {endNum}\ninterval: {interval}");

            this.statusInfo = "テスト中";
            if (await sc.sendTestData(nBytes, startNum, endNum, interval)) // テスト開始
            {
                textBoxResult.ForeColor = Color.Blue;
                textBoxResult.Text = "OK";
                this.statusInfo = "テスト終了";
            } else
            {
                textBoxResult.ForeColor = Color.Red;
                textBoxResult.Text = "NG";
            }
        }

        public string statusError
        {
            set
            {
                statusBar.ForeColor = Color.Red;
                statusBar.Text = value;
            }
        }
        public string statusInfo
        {
            set
            {
                statusBar.ForeColor = Color.Blue;
                statusBar.Text = value;
            }
        }

        public string setTextElapsed
        {
            set
            {
                textBoxElapsed.Text = value;
            }
        }

        public string setTextDataMsec
        {
            set
            {
                textBoxDataMsec.Text = value;
            }
        }

        private void textBoxBytes_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void textBoxStartNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void textBoxEndNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void textBoxInterval_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース、ドット以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b' && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }
    }
}