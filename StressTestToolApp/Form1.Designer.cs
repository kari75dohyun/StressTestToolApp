namespace StressTestToolApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            txtServerPort = new TextBox();
            Port = new Label();
            ConnectCount = new Label();
            txtClientCount = new TextBox();
            lstLog = new ListBox();
            btnStart = new Button();
            btnStop = new Button();
            lblStatus = new Label();
            txtServerIP = new TextBox();
            ServerIp = new Label();
            Ip = new Label();
            txtUdpPort = new TextBox();
            lblUdpPort = new Label();
            SuspendLayout();
            // 
            // txtServerPort
            // 
            resources.ApplyResources(txtServerPort, "txtServerPort");
            txtServerPort.Name = "txtServerPort";
            // 
            // Port
            // 
            resources.ApplyResources(Port, "Port");
            Port.Name = "Port";
            // 
            // ConnectCount
            // 
            resources.ApplyResources(ConnectCount, "ConnectCount");
            ConnectCount.Name = "ConnectCount";
            ConnectCount.Click += ConnectCount_Click;
            // 
            // txtClientCount
            // 
            resources.ApplyResources(txtClientCount, "txtClientCount");
            txtClientCount.Name = "txtClientCount";
            // 
            // lstLog
            // 
            resources.ApplyResources(lstLog, "lstLog");
            lstLog.FormattingEnabled = true;
            lstLog.Name = "lstLog";
            // 
            // btnStart
            // 
            resources.ApplyResources(btnStart, "btnStart");
            btnStart.Name = "btnStart";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // btnStop
            // 
            resources.ApplyResources(btnStop, "btnStop");
            btnStop.Name = "btnStop";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // lblStatus
            // 
            resources.ApplyResources(lblStatus, "lblStatus");
            lblStatus.Name = "lblStatus";
            lblStatus.Click += lblStatus_Click;
            // 
            // txtServerIP
            // 
            resources.ApplyResources(txtServerIP, "txtServerIP");
            txtServerIP.BorderStyle = BorderStyle.None;
            txtServerIP.Name = "txtServerIP";
            // 
            // ServerIp
            // 
            resources.ApplyResources(ServerIp, "ServerIp");
            ServerIp.Name = "ServerIp";
            // 
            // Ip
            // 
            resources.ApplyResources(Ip, "Ip");
            Ip.Name = "Ip";
            // 
            // txtUdpPort
            // 
            resources.ApplyResources(txtUdpPort, "txtUdpPort");
            txtUdpPort.Name = "txtUdpPort";
            // 
            // lblUdpPort
            // 
            resources.ApplyResources(lblUdpPort, "lblUdpPort");
            lblUdpPort.Name = "lblUdpPort";
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblUdpPort);
            Controls.Add(txtUdpPort);
            Controls.Add(Ip);
            Controls.Add(ServerIp);
            Controls.Add(txtServerIP);
            Controls.Add(lblStatus);
            Controls.Add(btnStop);
            Controls.Add(btnStart);
            Controls.Add(lstLog);
            Controls.Add(txtClientCount);
            Controls.Add(ConnectCount);
            Controls.Add(Port);
            Controls.Add(txtServerPort);
            Name = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox txtServerIP;
        private TextBox txtServerPort;
        private Label Port;
        private Label ConnectCount;
        private TextBox txtClientCount;
        private ListBox lstLog;
        private Button btnStart;
        private Button btnStop;
        private Label lblStatus;
        private Label ServerIp;
        private Label Ip;
        private TextBox txtUdpPort;
        private Label lblUdpPort;
    }
}
