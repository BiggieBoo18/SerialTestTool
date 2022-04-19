using System.IO.Ports;
namespace SerialTestTool
{
    public partial class Form1 : Form
    {
        private SerialCommunication sc; // �V���A���ʐM�p�C���X�^���X

        public Form1()
        {
            InitializeComponent();
            this.sc = SerialCommunication.GetInstance;
            this.sc.setForm(this);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // �L���ȃV���A���|�[�g���
            string[] ports = SerialPort.GetPortNames();
            comboBoxCom.Items.Clear(); // ������COM�|�[�g���X�g���폜
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
                sc.disconnect(); // ���ɊJ���Ă���ꍇ�͐ؒf
                buttonConnect.Text = "�ڑ�";
                this.statusInfo = "�V���A���|�[�g��ؒf���܂���";
            }
            else
            {
                string comName = comboBoxCom.Text; // �I������Ă���COM�|�[�g�����擾
                if (String.IsNullOrEmpty(comName)) // ��̏ꍇ�̓��b�Z�[�W��\��
                {
                    this.statusError = "�L����COM�|�[�g������܂���";
                    return;
                }
                if (this.sc.connect(comName)) // �V���A���|�[�g�ɐڑ�
                {
                    this.statusInfo = "�V���A���|�[�g�ɐڑ����܂���";
                    buttonConnect.Text = "�ؒf";
                }
                else
                {
                    this.statusError = "�V���A���|�[�g�̐ڑ��Ɏ��s���܂���"; // �ڑ��G���[�̏ꍇ�̓��b�Z�[�W��\��
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
                this.statusError = "Byte��������������܂���";
            }

            try
            {
                startNum = Int32.Parse(textBoxStartNum.Text);
                endNum = Int32.Parse(textBoxEndNum.Text);
                if (startNum < 0 || endNum < 1 || startNum >= endNum)
                {
                    this.statusError = "�J�n�ԍ��܂��͏I���ԍ�������������܂���";
                }
            } catch
            {
                this.statusError = "�J�n�ԍ��܂��͏I���ԍ�������������܂���";
            }
            try
            {
                interval = double.Parse(textBoxInterval.Text);
            } catch
            {
                this.statusError = "�C���^�[�o��������������܂���";
            }

            System.Diagnostics.Debug.WriteLine($"nBytes: {nBytes}\nstartNum: {startNum} - endNum: {endNum}\ninterval: {interval}");

            this.statusInfo = "�e�X�g��";
            if (await sc.sendTestData(nBytes, startNum, endNum, interval))
            {
                textBoxResult.ForeColor = Color.Blue;
                textBoxResult.Text = "OK";
                this.statusInfo = "�e�X�g�I��";
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
            //0�`9�ƁA�o�b�N�X�y�[�X�ȊO�̎��́A�C�x���g���L�����Z������
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void textBoxStartNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0�`9�ƁA�o�b�N�X�y�[�X�ȊO�̎��́A�C�x���g���L�����Z������
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void textBoxEndNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0�`9�ƁA�o�b�N�X�y�[�X�ȊO�̎��́A�C�x���g���L�����Z������
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void textBoxInterval_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0�`9�ƁA�o�b�N�X�y�[�X�A�h�b�g�ȊO�̎��́A�C�x���g���L�����Z������
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b' && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }
    }
}