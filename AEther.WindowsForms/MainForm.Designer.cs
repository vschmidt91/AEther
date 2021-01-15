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
            this.tlpPanel = new System.Windows.Forms.TableLayoutPanel();
            this.InputGroup = new System.Windows.Forms.GroupBox();
            this.Input = new System.Windows.Forms.ListBox();
            this.StateGroup = new System.Windows.Forms.GroupBox();
            this.State = new System.Windows.Forms.ListBox();
            this.OptionsGroup = new System.Windows.Forms.GroupBox();
            this.Options = new System.Windows.Forms.PropertyGrid();
            this.tlpPanel.SuspendLayout();
            this.InputGroup.SuspendLayout();
            this.StateGroup.SuspendLayout();
            this.OptionsGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpPanel
            // 
            this.tlpPanel.ColumnCount = 1;
            this.tlpPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPanel.Controls.Add(this.InputGroup, 0, 0);
            this.tlpPanel.Controls.Add(this.StateGroup, 0, 1);
            this.tlpPanel.Controls.Add(this.OptionsGroup, 0, 2);
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
            // InputGroup
            // 
            this.InputGroup.Controls.Add(this.Input);
            this.InputGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InputGroup.Location = new System.Drawing.Point(3, 3);
            this.InputGroup.Name = "InputGroup";
            this.InputGroup.Size = new System.Drawing.Size(234, 94);
            this.InputGroup.TabIndex = 0;
            this.InputGroup.TabStop = false;
            this.InputGroup.Text = "Input";
            // 
            // Input
            // 
            this.Input.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Input.FormattingEnabled = true;
            this.Input.ItemHeight = 15;
            this.Input.Location = new System.Drawing.Point(3, 19);
            this.Input.Name = "Input";
            this.Input.Size = new System.Drawing.Size(228, 72);
            this.Input.TabIndex = 3;
            this.Input.SelectedValueChanged += new System.EventHandler(this.Input_SelectedValueChanged);
            // 
            // StateGroup
            // 
            this.StateGroup.Controls.Add(this.State);
            this.StateGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StateGroup.Location = new System.Drawing.Point(3, 103);
            this.StateGroup.Name = "StateGroup";
            this.StateGroup.Size = new System.Drawing.Size(234, 94);
            this.StateGroup.TabIndex = 1;
            this.StateGroup.TabStop = false;
            this.StateGroup.Text = "Output";
            // 
            // State
            // 
            this.State.Dock = System.Windows.Forms.DockStyle.Fill;
            this.State.FormattingEnabled = true;
            this.State.ItemHeight = 15;
            this.State.Location = new System.Drawing.Point(3, 19);
            this.State.Name = "State";
            this.State.Size = new System.Drawing.Size(228, 72);
            this.State.TabIndex = 2;
            // 
            // OptionsGroup
            // 
            this.OptionsGroup.Controls.Add(this.Options);
            this.OptionsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OptionsGroup.Location = new System.Drawing.Point(3, 203);
            this.OptionsGroup.Name = "OptionsGroup";
            this.OptionsGroup.Size = new System.Drawing.Size(234, 355);
            this.OptionsGroup.TabIndex = 2;
            this.OptionsGroup.TabStop = false;
            this.OptionsGroup.Text = "Options";
            // 
            // Options
            // 
            this.Options.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Options.Location = new System.Drawing.Point(3, 19);
            this.Options.Name = "Options";
            this.Options.Size = new System.Drawing.Size(228, 333);
            this.Options.TabIndex = 1;
            this.Options.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.Options_PropertyValueChanged);
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
            this.InputGroup.ResumeLayout(false);
            this.StateGroup.ResumeLayout(false);
            this.OptionsGroup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpPanel;
        private System.Windows.Forms.PropertyGrid Options;
        private System.Windows.Forms.ListBox State;
        private System.Windows.Forms.ListBox Input;
        private System.Windows.Forms.GroupBox InputGroup;
        private System.Windows.Forms.GroupBox StateGroup;
        private System.Windows.Forms.GroupBox OptionsGroup;
    }
}