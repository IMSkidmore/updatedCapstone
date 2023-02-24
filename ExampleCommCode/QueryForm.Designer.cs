namespace ExampleCommCode
{
    partial class QueryForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QueryForm));
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tscmbxCommPorts = new System.Windows.Forms.ToolStripComboBox();
            this.tsbtnRefresh = new System.Windows.Forms.ToolStripButton();
            this.txtbxHyperTerm = new System.Windows.Forms.TextBox();
            this.txtbxHyperTermOutput = new System.Windows.Forms.TextBox();
            this.btnSendHyperTerm = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.txtbxUptime = new System.Windows.Forms.TextBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.portname = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.voltage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.current = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.power = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblUptime = new System.Windows.Forms.Label();
            this.toolStrip1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 3);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(93, 3);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tscmbxCommPorts,
            this.tsbtnRefresh});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(474, 25);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tscmbxCommPorts
            // 
            this.tscmbxCommPorts.Name = "tscmbxCommPorts";
            this.tscmbxCommPorts.Size = new System.Drawing.Size(250, 25);
            this.tscmbxCommPorts.SelectedIndexChanged += new System.EventHandler(this.tscmbxCommPorts_SelectedIndexChanged);
            // 
            // tsbtnRefresh
            // 
            this.tsbtnRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbtnRefresh.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnRefresh.Image")));
            this.tsbtnRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnRefresh.Name = "tsbtnRefresh";
            this.tsbtnRefresh.Size = new System.Drawing.Size(80, 22);
            this.tsbtnRefresh.Text = "Refresh Ports";
            this.tsbtnRefresh.Click += new System.EventHandler(this.tsbtnRefresh_Click);
            // 
            // txtbxHyperTerm
            // 
            this.txtbxHyperTerm.Location = new System.Drawing.Point(12, 209);
            this.txtbxHyperTerm.Name = "txtbxHyperTerm";
            this.txtbxHyperTerm.Size = new System.Drawing.Size(276, 20);
            this.txtbxHyperTerm.TabIndex = 5;
            this.txtbxHyperTerm.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtbxHyperTerm_KeyPress);
            // 
            // txtbxHyperTermOutput
            // 
            this.txtbxHyperTermOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtbxHyperTermOutput.Location = new System.Drawing.Point(0, 0);
            this.txtbxHyperTermOutput.Multiline = true;
            this.txtbxHyperTermOutput.Name = "txtbxHyperTermOutput";
            this.txtbxHyperTermOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtbxHyperTermOutput.Size = new System.Drawing.Size(474, 200);
            this.txtbxHyperTermOutput.TabIndex = 6;
            // 
            // btnSendHyperTerm
            // 
            this.btnSendHyperTerm.Location = new System.Drawing.Point(307, 206);
            this.btnSendHyperTerm.Name = "btnSendHyperTerm";
            this.btnSendHyperTerm.Size = new System.Drawing.Size(125, 23);
            this.btnSendHyperTerm.TabIndex = 7;
            this.btnSendHyperTerm.Text = "Send Message";
            this.btnSendHyperTerm.UseVisualStyleBackColor = true;
            this.btnSendHyperTerm.Click += new System.EventHandler(this.btnSendHyperTerm_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lblUptime);
            this.splitContainer1.Panel1.Controls.Add(this.txtbxUptime);
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
            this.splitContainer1.Panel1.Controls.Add(this.btnStart);
            this.splitContainer1.Panel1.Controls.Add(this.txtbxHyperTerm);
            this.splitContainer1.Panel1.Controls.Add(this.btnSendHyperTerm);
            this.splitContainer1.Panel1.Controls.Add(this.btnStop);
            this.splitContainer1.Panel1MinSize = 240;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtbxHyperTermOutput);
            this.splitContainer1.Panel2MinSize = 100;
            this.splitContainer1.Size = new System.Drawing.Size(474, 444);
            this.splitContainer1.SplitterDistance = 240;
            this.splitContainer1.TabIndex = 9;
            // 
            // txtbxUptime
            // 
            this.txtbxUptime.Location = new System.Drawing.Point(352, 6);
            this.txtbxUptime.Name = "txtbxUptime";
            this.txtbxUptime.Size = new System.Drawing.Size(110, 20);
            this.txtbxUptime.TabIndex = 9;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.portname,
            this.voltage,
            this.current,
            this.power});
            this.dataGridView1.Location = new System.Drawing.Point(12, 32);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(452, 155);
            this.dataGridView1.TabIndex = 8;
            // 
            // portname
            // 
            this.portname.HeaderText = "Port Name";
            this.portname.Name = "portname";
            // 
            // voltage
            // 
            this.voltage.HeaderText = "Voltage (mV)";
            this.voltage.Name = "voltage";
            // 
            // current
            // 
            this.current.HeaderText = "Current (mA)";
            this.current.Name = "current";
            // 
            // power
            // 
            this.power.HeaderText = "Power (mW)";
            this.power.Name = "power";
            // 
            // lblUptime
            // 
            this.lblUptime.AutoSize = true;
            this.lblUptime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUptime.Location = new System.Drawing.Point(292, 8);
            this.lblUptime.Name = "lblUptime";
            this.lblUptime.Size = new System.Drawing.Size(54, 16);
            this.lblUptime.TabIndex = 10;
            this.lblUptime.Text = "Uptime:";
            // 
            // QueryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 469);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.MinimumSize = new System.Drawing.Size(490, 400);
            this.Name = "QueryForm";
            this.Text = "Protonex Example Comm Code";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QueryForm_FormClosing);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripComboBox tscmbxCommPorts;
        private System.Windows.Forms.ToolStripButton tsbtnRefresh;
        private System.Windows.Forms.TextBox txtbxHyperTerm;
        private System.Windows.Forms.TextBox txtbxHyperTermOutput;
        private System.Windows.Forms.Button btnSendHyperTerm;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TextBox txtbxUptime;
        private System.Windows.Forms.DataGridViewTextBoxColumn portname;
        private System.Windows.Forms.DataGridViewTextBoxColumn voltage;
        private System.Windows.Forms.DataGridViewTextBoxColumn current;
        private System.Windows.Forms.DataGridViewTextBoxColumn power;
        private System.Windows.Forms.Label lblUptime;
    }
}

