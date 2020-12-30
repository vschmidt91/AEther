namespace AEther.WindowsForms
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.tlpPanel = new System.Windows.Forms.TableLayoutPanel();
            this.pgConfiguration = new System.Windows.Forms.PropertyGrid();
            this.lbState = new System.Windows.Forms.ListBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.tlpPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpPanel
            // 
            this.tlpPanel.ColumnCount = 1;
            this.tlpPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPanel.Controls.Add(this.pgConfiguration, 0, 1);
            this.tlpPanel.Controls.Add(this.lbState, 0, 0);
            this.tlpPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.tlpPanel.Location = new System.Drawing.Point(0, 0);
            this.tlpPanel.Name = "tlpPanel";
            this.tlpPanel.RowCount = 2;
            this.tlpPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPanel.Size = new System.Drawing.Size(240, 561);
            this.tlpPanel.TabIndex = 0;
            this.tlpPanel.Visible = false;
            // 
            // pgConfiguration
            // 
            this.pgConfiguration.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgConfiguration.Location = new System.Drawing.Point(3, 103);
            this.pgConfiguration.Name = "pgConfiguration";
            this.pgConfiguration.Size = new System.Drawing.Size(234, 455);
            this.pgConfiguration.TabIndex = 1;
            this.pgConfiguration.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.pgConfiguration_PropertyValueChanged);
            // 
            // lbState
            // 
            this.lbState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbState.FormattingEnabled = true;
            this.lbState.ItemHeight = 15;
            this.lbState.Location = new System.Drawing.Point(3, 3);
            this.lbState.Name = "lbState";
            this.lbState.Size = new System.Drawing.Size(234, 94);
            this.lbState.TabIndex = 2;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tlpPanel);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Text = "AEther";
            this.tlpPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpPanel;
        private System.Windows.Forms.PropertyGrid pgConfiguration;
        private System.Windows.Forms.ListBox lbState;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
    }
}