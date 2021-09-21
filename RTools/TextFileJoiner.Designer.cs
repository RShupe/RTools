
namespace RTools
{
    partial class TextFileJoiner
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
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.deleteFilesBox = new System.Windows.Forms.CheckBox();
            this.numSpaces = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.readyLabel = new System.Windows.Forms.Label();
            this.numberLabel = new System.Windows.Forms.Label();
            this.btnConvert = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.numSpaces)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(13, 9);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(131, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "Text File Joiner";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(108, 238);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 17);
            this.label2.TabIndex = 19;
            this.label2.Text = "Delete original files:";
            // 
            // deleteFilesBox
            // 
            this.deleteFilesBox.AutoSize = true;
            this.deleteFilesBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(56)))), ((int)(((byte)(63)))));
            this.deleteFilesBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.deleteFilesBox.FlatAppearance.BorderSize = 0;
            this.deleteFilesBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.deleteFilesBox.Location = new System.Drawing.Point(246, 241);
            this.deleteFilesBox.Name = "deleteFilesBox";
            this.deleteFilesBox.Size = new System.Drawing.Size(15, 14);
            this.deleteFilesBox.TabIndex = 4;
            this.deleteFilesBox.UseVisualStyleBackColor = false;
            this.deleteFilesBox.CheckedChanged += new System.EventHandler(this.deleteFilesBox_CheckedChanged);
            // 
            // numSpaces
            // 
            this.numSpaces.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.numSpaces.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numSpaces.Cursor = System.Windows.Forms.Cursors.Hand;
            this.numSpaces.ForeColor = System.Drawing.Color.White;
            this.numSpaces.Location = new System.Drawing.Point(238, 200);
            this.numSpaces.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numSpaces.Name = "numSpaces";
            this.numSpaces.Size = new System.Drawing.Size(45, 19);
            this.numSpaces.TabIndex = 3;
            this.numSpaces.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSpaces.ValueChanged += new System.EventHandler(this.numSpaces_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(108, 199);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 17);
            this.label1.TabIndex = 16;
            this.label1.Text = "# of lines to insert:";
            // 
            // readyLabel
            // 
            this.readyLabel.AutoSize = true;
            this.readyLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.readyLabel.ForeColor = System.Drawing.Color.White;
            this.readyLabel.Location = new System.Drawing.Point(144, 272);
            this.readyLabel.Name = "readyLabel";
            this.readyLabel.Size = new System.Drawing.Size(117, 17);
            this.readyLabel.TabIndex = 15;
            this.readyLabel.Text = "Waiting for files...";
            // 
            // numberLabel
            // 
            this.numberLabel.AutoSize = true;
            this.numberLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.numberLabel.ForeColor = System.Drawing.Color.White;
            this.numberLabel.Location = new System.Drawing.Point(108, 100);
            this.numberLabel.Name = "numberLabel";
            this.numberLabel.Size = new System.Drawing.Size(201, 17);
            this.numberLabel.TabIndex = 14;
            this.numberLabel.Text = "Number of files to be joined:  0";
            // 
            // btnConvert
            // 
            this.btnConvert.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.btnConvert.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConvert.FlatAppearance.BorderSize = 0;
            this.btnConvert.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConvert.ForeColor = System.Drawing.Color.White;
            this.btnConvert.Location = new System.Drawing.Point(224, 136);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(131, 46);
            this.btnConvert.TabIndex = 2;
            this.btnConvert.Text = "Join";
            this.btnConvert.UseVisualStyleBackColor = false;
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.btnOpen.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOpen.FlatAppearance.BorderSize = 0;
            this.btnOpen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpen.ForeColor = System.Drawing.Color.White;
            this.btnOpen.Location = new System.Drawing.Point(66, 136);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(131, 46);
            this.btnOpen.TabIndex = 1;
            this.btnOpen.Text = "Open files";
            this.btnOpen.UseVisualStyleBackColor = false;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.numSpaces);
            this.panel1.Controls.Add(this.numberLabel);
            this.panel1.Controls.Add(this.btnOpen);
            this.panel1.ForeColor = System.Drawing.Color.White;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(450, 419);
            this.panel1.TabIndex = 20;
            // 
            // TextFileJoiner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 417);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.deleteFilesBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.readyLabel);
            this.Controls.Add(this.btnConvert);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "TextFileJoiner";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TextFileJoiner";
            ((System.ComponentModel.ISupportInitialize)(this.numSpaces)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox deleteFilesBox;
        private System.Windows.Forms.NumericUpDown numSpaces;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label readyLabel;
        private System.Windows.Forms.Label numberLabel;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Panel panel1;
    }
}