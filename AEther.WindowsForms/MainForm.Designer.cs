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
            this.tlpPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpPanel
            // 
            this.tlpPanel.ColumnCount = 1;
            this.tlpPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPanel.Controls.Add(this.pgOptions, 0, 2);
            this.tlpPanel.Controls.Add(this.lbState, 0, 1);
            this.tlpPanel.Controls.Add(this.lbInput, 0, 0);
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
            // pgConfiguration
            // 
            this.pgOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgOptions.Location = new System.Drawing.Point(3, 203);
            this.pgOptions.Name = "pgConfiguration";
            this.pgOptions.Size = new System.Drawing.Size(234, 355);
            this.pgOptions.TabIndex = 1;
            this.pgOptions.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.pgOptions_PropertyValueChanged);
            // 
            // lbState
            // 
            this.lbState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbState.FormattingEnabled = true;
            this.lbState.ItemHeight = 15;
            this.lbState.Location = new System.Drawing.Point(3, 103);
            this.lbState.Name = "lbState";
            this.lbState.Size = new System.Drawing.Size(234, 94);
            this.lbState.TabIndex = 2;
            // 
            // lbInput
            // 
            this.lbInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbInput.FormattingEnabled = true;
            this.lbInput.ItemHeight = 15;
            this.lbInput.Location = new System.Drawing.Point(3, 3);
            this.lbInput.Name = "lbInput";
            this.lbInput.Size = new System.Drawing.Size(234, 94);
            this.lbInput.TabIndex = 3;
            this.lbInput.SelectedValueChanged += new System.EventHandler(this.lbInput_SelectedValueChanged);
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
        private System.Windows.Forms.PropertyGrid pgOptions;
        private System.Windows.Forms.ListBox lbState;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ListBox lbInput;
    }
}