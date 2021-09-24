
namespace RTools
{
    partial class CSVExtractor
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.path1Button = new System.Windows.Forms.Button();
            this.path2Box = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.loadingCircle = new System.Windows.Forms.PictureBox();
            this.loadingLabel = new System.Windows.Forms.Label();
            this.startButton = new System.Windows.Forms.Button();
            this.path1Box = new System.Windows.Forms.TextBox();
            this.path2Button = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.loadingCircle)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.path1Button);
            this.panel1.Controls.Add(this.path2Box);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.path1Box);
            this.panel1.Controls.Add(this.path2Button);
            this.panel1.Location = new System.Drawing.Point(0, -1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(341, 354);
            this.panel1.TabIndex = 18;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(19, 148);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(128, 23);
            this.button1.TabIndex = 18;
            this.button1.Text = "View Ignored Entries";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label1.Location = new System.Drawing.Point(16, 132);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(295, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "As of right now, this tool only detects and extracts debit cards\r\n";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(15, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 20);
            this.label2.TabIndex = 16;
            this.label2.Text = "Export to CSV";
            // 
            // path1Button
            // 
            this.path1Button.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.path1Button.Cursor = System.Windows.Forms.Cursors.Hand;
            this.path1Button.FlatAppearance.BorderSize = 0;
            this.path1Button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.path1Button.ForeColor = System.Drawing.Color.White;
            this.path1Button.Location = new System.Drawing.Point(19, 79);
            this.path1Button.Name = "path1Button";
            this.path1Button.Size = new System.Drawing.Size(142, 29);
            this.path1Button.TabIndex = 1;
            this.path1Button.Text = "Select Text File Path";
            this.path1Button.UseVisualStyleBackColor = false;
            this.path1Button.Click += new System.EventHandler(this.path1Button_Click);
            // 
            // path2Box
            // 
            this.path2Box.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.path2Box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.path2Box.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.path2Box.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.path2Box.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.path2Box.Location = new System.Drawing.Point(19, 198);
            this.path2Box.Name = "path2Box";
            this.path2Box.ReadOnly = true;
            this.path2Box.Size = new System.Drawing.Size(310, 22);
            this.path2Box.TabIndex = 14;
            this.path2Box.TabStop = false;
            this.path2Box.Text = "CSV File Path";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(31)))));
            this.panel2.Controls.Add(this.loadingCircle);
            this.panel2.Controls.Add(this.loadingLabel);
            this.panel2.Controls.Add(this.startButton);
            this.panel2.Location = new System.Drawing.Point(0, 261);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(341, 92);
            this.panel2.TabIndex = 0;
            // 
            // loadingCircle
            // 
            this.loadingCircle.Image = global::RTools.Properties.Resources.ajax_loader_1_;
            this.loadingCircle.Location = new System.Drawing.Point(156, 64);
            this.loadingCircle.Name = "loadingCircle";
            this.loadingCircle.Size = new System.Drawing.Size(21, 24);
            this.loadingCircle.TabIndex = 19;
            this.loadingCircle.TabStop = false;
            this.loadingCircle.Visible = false;
            this.loadingCircle.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // loadingLabel
            // 
            this.loadingLabel.AutoSize = true;
            this.loadingLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.loadingLabel.Location = new System.Drawing.Point(13, 65);
            this.loadingLabel.Name = "loadingLabel";
            this.loadingLabel.Size = new System.Drawing.Size(0, 13);
            this.loadingLabel.TabIndex = 6;
            // 
            // startButton
            // 
            this.startButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(134)))), ((int)(((byte)(27)))), ((int)(((byte)(45)))));
            this.startButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.startButton.FlatAppearance.BorderSize = 0;
            this.startButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.startButton.ForeColor = System.Drawing.Color.White;
            this.startButton.Location = new System.Drawing.Point(12, 13);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(311, 45);
            this.startButton.TabIndex = 5;
            this.startButton.Text = "Export";
            this.startButton.UseVisualStyleBackColor = false;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // path1Box
            // 
            this.path1Box.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.path1Box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.path1Box.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.path1Box.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.path1Box.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.path1Box.Location = new System.Drawing.Point(19, 50);
            this.path1Box.Name = "path1Box";
            this.path1Box.ReadOnly = true;
            this.path1Box.Size = new System.Drawing.Size(310, 23);
            this.path1Box.TabIndex = 12;
            this.path1Box.TabStop = false;
            this.path1Box.Text = "Transaction Report TXT Path\r\n";
            // 
            // path2Button
            // 
            this.path2Button.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.path2Button.Cursor = System.Windows.Forms.Cursors.Hand;
            this.path2Button.FlatAppearance.BorderSize = 0;
            this.path2Button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.path2Button.ForeColor = System.Drawing.Color.White;
            this.path2Button.Location = new System.Drawing.Point(19, 226);
            this.path2Button.Name = "path2Button";
            this.path2Button.Size = new System.Drawing.Size(142, 29);
            this.path2Button.TabIndex = 4;
            this.path2Button.Text = "Select CSV File Path";
            this.path2Button.UseVisualStyleBackColor = false;
            this.path2Button.Click += new System.EventHandler(this.path2Button_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.SystemColors.Control;
            this.label3.Location = new System.Drawing.Point(16, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(176, 13);
            this.label3.TabIndex = 19;
            this.label3.Text = "Select the transaction report text file";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.SystemColors.Control;
            this.label4.Location = new System.Drawing.Point(16, 182);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(219, 13);
            this.label4.TabIndex = 20;
            this.label4.Text = "Select folder where output file is to be placed";
            // 
            // CSVExtractor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(341, 352);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "CSVExtractor";
            this.Text = "CSVExtractor";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.loadingCircle)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button path1Button;
        private System.Windows.Forms.TextBox path2Box;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.TextBox path1Box;
        private System.Windows.Forms.Button path2Button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label loadingLabel;
        private System.Windows.Forms.PictureBox loadingCircle;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
    }
}