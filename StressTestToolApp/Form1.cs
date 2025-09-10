using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StressTestToolApp
{
    public partial class Form1 : Form
    {
        private StressTestController _controller = null!;
        private CancellationTokenSource? _cts;

        public Form1()
        {
            InitializeComponent();

            if (!this.DesignMode)
            {
                txtServerIP.Text = "127.0.0.1";
                txtServerPort.Text = "12345";
                txtClientCount.Text = "700";
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                lblStatus.Text = "��� ��";
            }
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (_cts != null && !_cts.IsCancellationRequested) return; // �̹� �������̸� ����

            btnStart.Enabled = false;
            btnStop.Enabled = true;
            lstLog.Items.Clear();

            string ip = txtServerIP.Text;
            int port = int.Parse(txtServerPort.Text);
            int udpPort = int.Parse(txtUdpPort.Text);
            int count = int.Parse(txtClientCount.Text);

            _cts = new CancellationTokenSource();

            _controller = new StressTestController
            {
                ClientCount = count,
                ServerIp = ip,
                ServerPort = port,
                UdpPort = udpPort,
                //LogAction = (msg) => this.Invoke((Action)(() => lstLog.Items.Add(msg)))
                LogAction = (msg) => this.Invoke((Action)(() => {
                    if (lstLog.Items.Count > 1000)
                        lstLog.Items.RemoveAt(0);  // ���� ������ �α� ����
                    lstLog.Items.Add(msg);
                }))
            };

            var task = _controller.StartAsync(_cts.Token);

            // ���� ����͸� �½�ũ
            _ = Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    int active = _controller.Clients.Count(c => c.Connected);
                    //this.Invoke((Action)(() => lblStatus.Text = $"����: {active} / {count}"));
                    await Task.Delay(1000);
                }
            });

            await task;

            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            lblStatus.Text = "������";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void lblStatus_Click(object sender, EventArgs e)
        {

        }

        private void ConnectCount_Click(object sender, EventArgs e)
        {

        }
    }
}
