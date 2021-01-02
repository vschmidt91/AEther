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
            this.pgOptions = new System.Windows.Forms.PropertyGrid();
            this.lbState = new System.Windows.Forms.ListBox();
            this.lbInput = new System.Windows.Forms.ListBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.gbInput = new System.Windows.Forms.GroupBox();
            this.gbState = new System.Windows.Forms.GroupBox();
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.tlpPanel.SuspendLayout();
            this.gbInput.SuspendLayout();
            this.gbState.SuspendLayout();
            this.gbOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpPanel
            // 
            this.tlpPanel.ColumnCount = 1;
            this.tlpPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPanel.Controls.Add(this.gbInput, 0, 0);
            this.tlpPanel.Controls.Add(this.gbState, 0, 1);
            this.tlpPanel.Controls.Add(this.gbOptions, 0, 2);
            this.tlpPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.tlpPanel.Location = new System.Drawing.Point(0, 0);
            this.tlpPanel.Name = "tlpPanel";
            this.tlpPanel.RowCount = 3;
            this.tlpPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPanel.Size = new System.Drawing.Size(240, 561);
            this.tlpPanel.TabIndex = 0;
            this.tlpPanel.Visible = false;
            // 
            // pgOptions
            // 
            this.pgOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgOptions.Location = new System.Drawing.Point(3, 19);
            this.pgOptions.Name = "pgOptions";
            this.pgOptions.Size = new System.Drawing.Size(228, 333);
            this.pgOptions.TabIndex = 1;
            this.pgOptions.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.pgOptions_PropertyValueChanged);
            // 
            // lbState
            // 
            this.lbState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbState.FormattingEnabled = true;
            this.lbState.ItemHeight = 15;
            this.lbState.Location = new System.Drawing.Point(3, 19);
            this.lbState.Name = "lbState";
            this.lbState.Size = new System.Drawing.Size(228, 72);
            this.lbState.TabIndex = 2;
            // 
            // lbInput
            // 
            this.lbInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbInput.FormattingEnabled = true;
            this.lbInput.ItemHeight = 15;
            this.lbInput.Location = new System.Drawing.Point(3, 19);
            this.lbInput.Name = "lbInput";
            this.lbInput.Size = new System.Drawing.Size(228, 72);
            this.lbInput.TabIndex = 3;
            this.lbInput.SelectedValueChanged += new System.EventHandler(this.lbInput_SelectedValueChanged);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // gbInput
            // 
            this.gbInput.Controls.Add(this.lbInput);
            this.gbInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbInput.Location = new System.Drawing.Point(3, 3);
            this.gbInput.Name = "gbInput";
            this.gbInput.Size = new System.Drawing.Size(234, 94);
            this.gbInput.TabIndex = 0;
            this.gbInput.TabStop = false;
            this.gbInput.Text = "Input";
            // 
            // gbState
            // 
            this.gbState.Controls.Add(this.lbState);
            this.gbState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbState.Location = new System.Drawing.Point(3, 103);
            this.gbState.Name = "gbState";
            this.gbState.Size = new System.Drawing.Size(234, 94);
            this.gbState.TabIndex = 1;
            this.gbState.TabStop = false;
            this.gbState.Text = "Output";
            // 
            // gbOptions
            // 
            this.gbOptions.Controls.Add(this.pgOptions);
            this.gbOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbOptions.Location = new System.Drawing.Point(3, 203);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Size = new System.Drawing.Size(234, 355);
            this.gbOptions.TabIndex = 2;
            this.gbOptions.TabStop = false;
            this.gbOptions.Text = "Options";
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
            this.gbInput.ResumeLayout(false);
            this.gbState.ResumeLayout(false);
            this.gbOptions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpPanel;
        private System.Windows.Forms.PropertyGrid pgOptions;
        private System.Windows.Forms.ListBox lbState;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ListBox lbInput;
        private System.Windows.Forms.GroupBox gbInput;
        private System.Windows.Forms.GroupBox gbState;
        private System.Windows.Forms.GroupBox gbOptions;
    }
}