namespace connectionTest
{
    partial class ConnectionTest
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.connectButton = new System.Windows.Forms.Button();
            this.comPortLabel = new System.Windows.Forms.Label();
            this.comboBoxComPorts = new System.Windows.Forms.ComboBox();
            this.labelConnectionState = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxSamplingRate = new System.Windows.Forms.TextBox();
            this.textBoxAccelRange = new System.Windows.Forms.TextBox();
            this.textBoxGyroRange = new System.Windows.Forms.TextBox();
            this.textBoxGSRRange = new System.Windows.Forms.TextBox();
            this.textBoxEnabledSensors = new System.Windows.Forms.TextBox();
            this.toolStripStatusLabel1 = new System.Windows.Forms.Label();
            this.disconnectButton = new System.Windows.Forms.Button();
            this.currentGSRLabel = new System.Windows.Forms.Label();
            this.streamButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(244, 66);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(207, 50);
            this.connectButton.TabIndex = 0;
            this.connectButton.Text = "Connect to Device";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // comPortLabel
            // 
            this.comPortLabel.AutoSize = true;
            this.comPortLabel.Location = new System.Drawing.Point(12, 81);
            this.comPortLabel.Name = "comPortLabel";
            this.comPortLabel.Size = new System.Drawing.Size(75, 20);
            this.comPortLabel.TabIndex = 1;
            this.comPortLabel.Text = "comports";
            // 
            // comboBoxComPorts
            // 
            this.comboBoxComPorts.FormattingEnabled = true;
            this.comboBoxComPorts.Location = new System.Drawing.Point(103, 78);
            this.comboBoxComPorts.Name = "comboBoxComPorts";
            this.comboBoxComPorts.Size = new System.Drawing.Size(121, 28);
            this.comboBoxComPorts.TabIndex = 3;
            // 
            // labelConnectionState
            // 
            this.labelConnectionState.AutoSize = true;
            this.labelConnectionState.Location = new System.Drawing.Point(12, 132);
            this.labelConnectionState.Name = "labelConnectionState";
            this.labelConnectionState.Size = new System.Drawing.Size(98, 20);
            this.labelConnectionState.TabIndex = 4;
            this.labelConnectionState.Text = "currentState";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 20);
            this.label1.TabIndex = 5;
            this.label1.Text = "Sampling Rate";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(273, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 20);
            this.label2.TabIndex = 6;
            this.label2.Text = "Accel Range";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(536, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 20);
            this.label3.TabIndex = 7;
            this.label3.Text = "Gyro Range";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(784, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 20);
            this.label4.TabIndex = 8;
            this.label4.Text = "GSR Range";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(1028, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(131, 20);
            this.label5.TabIndex = 9;
            this.label5.Text = "Enabled Sensors";
            // 
            // textBoxSamplingRate
            // 
            this.textBoxSamplingRate.Location = new System.Drawing.Point(141, 16);
            this.textBoxSamplingRate.Name = "textBoxSamplingRate";
            this.textBoxSamplingRate.Size = new System.Drawing.Size(100, 26);
            this.textBoxSamplingRate.TabIndex = 10;
            // 
            // textBoxAccelRange
            // 
            this.textBoxAccelRange.Location = new System.Drawing.Point(379, 16);
            this.textBoxAccelRange.Name = "textBoxAccelRange";
            this.textBoxAccelRange.Size = new System.Drawing.Size(100, 26);
            this.textBoxAccelRange.TabIndex = 11;
            // 
            // textBoxGyroRange
            // 
            this.textBoxGyroRange.Location = new System.Drawing.Point(637, 16);
            this.textBoxGyroRange.Name = "textBoxGyroRange";
            this.textBoxGyroRange.Size = new System.Drawing.Size(100, 26);
            this.textBoxGyroRange.TabIndex = 12;
            // 
            // textBoxGSRRange
            // 
            this.textBoxGSRRange.Location = new System.Drawing.Point(887, 16);
            this.textBoxGSRRange.Name = "textBoxGSRRange";
            this.textBoxGSRRange.Size = new System.Drawing.Size(100, 26);
            this.textBoxGSRRange.TabIndex = 13;
            // 
            // textBoxEnabledSensors
            // 
            this.textBoxEnabledSensors.Location = new System.Drawing.Point(1165, 16);
            this.textBoxEnabledSensors.Name = "textBoxEnabledSensors";
            this.textBoxEnabledSensors.Size = new System.Drawing.Size(100, 26);
            this.textBoxEnabledSensors.TabIndex = 14;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.AutoSize = true;
            this.toolStripStatusLabel1.Location = new System.Drawing.Point(12, 156);
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(163, 20);
            this.toolStripStatusLabel1.TabIndex = 15;
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // disconnectButton
            // 
            this.disconnectButton.Location = new System.Drawing.Point(927, 66);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(207, 50);
            this.disconnectButton.TabIndex = 16;
            this.disconnectButton.Text = "Disconnect Device";
            this.disconnectButton.UseVisualStyleBackColor = true;
            this.disconnectButton.Click += new System.EventHandler(this.disconnectButton_Click);
            // 
            // currentGSRLabel
            // 
            this.currentGSRLabel.AutoSize = true;
            this.currentGSRLabel.Location = new System.Drawing.Point(1161, 96);
            this.currentGSRLabel.Name = "currentGSRLabel";
            this.currentGSRLabel.Size = new System.Drawing.Size(134, 20);
            this.currentGSRLabel.TabIndex = 17;
            this.currentGSRLabel.Text = "currentGSRLabel";
            // 
            // streamButton
            // 
            this.streamButton.Location = new System.Drawing.Point(475, 66);
            this.streamButton.Name = "streamButton";
            this.streamButton.Size = new System.Drawing.Size(207, 50);
            this.streamButton.TabIndex = 18;
            this.streamButton.Text = "Start Streaming";
            this.streamButton.UseVisualStyleBackColor = true;
            this.streamButton.Click += new System.EventHandler(this.streamButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(701, 66);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(207, 50);
            this.stopButton.TabIndex = 19;
            this.stopButton.Text = "Stop Streaming";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // ConnectionTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1459, 185);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.streamButton);
            this.Controls.Add(this.currentGSRLabel);
            this.Controls.Add(this.disconnectButton);
            this.Controls.Add(this.toolStripStatusLabel1);
            this.Controls.Add(this.textBoxEnabledSensors);
            this.Controls.Add(this.textBoxGSRRange);
            this.Controls.Add(this.textBoxGyroRange);
            this.Controls.Add(this.textBoxAccelRange);
            this.Controls.Add(this.textBoxSamplingRate);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelConnectionState);
            this.Controls.Add(this.comboBoxComPorts);
            this.Controls.Add(this.comPortLabel);
            this.Controls.Add(this.connectButton);
            this.Name = "ConnectionTest";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Label comPortLabel;
        private System.Windows.Forms.ComboBox comboBoxComPorts;
        private System.Windows.Forms.Label labelConnectionState;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxSamplingRate;
        private System.Windows.Forms.TextBox textBoxAccelRange;
        private System.Windows.Forms.TextBox textBoxGyroRange;
        private System.Windows.Forms.TextBox textBoxGSRRange;
        private System.Windows.Forms.TextBox textBoxEnabledSensors;
        private System.Windows.Forms.Label toolStripStatusLabel1;
        private System.Windows.Forms.Button disconnectButton;
        private System.Windows.Forms.Label currentGSRLabel;
        private System.Windows.Forms.Button streamButton;
        private System.Windows.Forms.Button stopButton;
    }
}

