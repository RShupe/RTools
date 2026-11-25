// Visual Studio designer-owned layout. Runtime styling is attached separately.
#nullable disable
namespace RTools.UI.Pages
{
    partial class SystemInformationPage
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.result = new System.Windows.Forms.RichTextBox();
            this.localSystemInformationHeading = new System.Windows.Forms.Label();
            this.displaysWindowsVersionHardwareLocaLabel = new System.Windows.Forms.Label();
            this.refreshbutton = new System.Windows.Forms.Button();
            this.copyAllButton = new System.Windows.Forms.Button();
            this.saveReportButton = new System.Windows.Forms.Button();
            this.rowlayout = new System.Windows.Forms.FlowLayoutPanel();
            this.localSystemInformationCard = new System.Windows.Forms.TableLayoutPanel();
            this.pageLayout = new System.Windows.Forms.TableLayoutPanel();
            this.rowlayout.SuspendLayout();
            this.localSystemInformationCard.SuspendLayout();
            this.pageLayout.SuspendLayout();
            this.SuspendLayout();
            // result
            this.result.Name = "result";
            this.result.Location = new System.Drawing.Point(16, 133);
            this.result.Size = new System.Drawing.Size(668, 510);
            this.result.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.result.TabIndex = 3;
            this.result.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular);
            this.result.Dock = System.Windows.Forms.DockStyle.Fill;
            this.result.Text = "Select Refresh to collect local system information.";
            this.result.ReadOnly = true;
            this.result.WordWrap = false;
            this.result.DetectUrls = false;
            this.result.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
            this.result.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.result.BackColor = System.Drawing.Color.FromArgb(52, 52, 52);
            this.result.ForeColor = System.Drawing.Color.FromArgb(246, 246, 246);
            // localSystemInformationHeading
            this.localSystemInformationHeading.Name = "localSystemInformationHeading";
            this.localSystemInformationHeading.Location = new System.Drawing.Point(16, 16);
            this.localSystemInformationHeading.Size = new System.Drawing.Size(668, 24);
            this.localSystemInformationHeading.Margin = new System.Windows.Forms.Padding(0, 0, 0, 7);
            this.localSystemInformationHeading.TabIndex = 0;
            this.localSystemInformationHeading.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.localSystemInformationHeading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.localSystemInformationHeading.Text = "Local system information";
            this.localSystemInformationHeading.AutoSize = false;
            this.localSystemInformationHeading.UseMnemonic = false;
            this.localSystemInformationHeading.ForeColor = System.Drawing.Color.FromArgb(246, 246, 246);
            this.localSystemInformationHeading.BackColor = System.Drawing.Color.Transparent;
            // displaysWindowsVersionHardwareLocaLabel
            this.displaysWindowsVersionHardwareLocaLabel.Name = "displaysWindowsVersionHardwareLocaLabel";
            this.displaysWindowsVersionHardwareLocaLabel.Location = new System.Drawing.Point(16, 47);
            this.displaysWindowsVersionHardwareLocaLabel.Size = new System.Drawing.Size(668, 34);
            this.displaysWindowsVersionHardwareLocaLabel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 7);
            this.displaysWindowsVersionHardwareLocaLabel.TabIndex = 1;
            this.displaysWindowsVersionHardwareLocaLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            this.displaysWindowsVersionHardwareLocaLabel.Tag = "muted";
            this.displaysWindowsVersionHardwareLocaLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.displaysWindowsVersionHardwareLocaLabel.Text = "Displays Windows version, hardware, local storage, uptime and active network adapters without requiring elevation.";
            this.displaysWindowsVersionHardwareLocaLabel.AutoSize = false;
            this.displaysWindowsVersionHardwareLocaLabel.UseMnemonic = false;
            this.displaysWindowsVersionHardwareLocaLabel.ForeColor = System.Drawing.Color.FromArgb(183, 183, 183);
            this.displaysWindowsVersionHardwareLocaLabel.BackColor = System.Drawing.Color.Transparent;
            // refreshbutton
            this.refreshbutton.Name = "refreshbutton";
            this.refreshbutton.Location = new System.Drawing.Point(0, 0);
            this.refreshbutton.Size = new System.Drawing.Size(92, 34);
            this.refreshbutton.Margin = new System.Windows.Forms.Padding(0, 0, 8, 7);
            this.refreshbutton.TabIndex = 0;
            this.refreshbutton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.refreshbutton.Text = "Refresh";
            this.refreshbutton.AutoSize = false;
            this.refreshbutton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.refreshbutton.UseVisualStyleBackColor = false;
            this.refreshbutton.UseMnemonic = false;
            this.refreshbutton.Tag = "primary";
            this.refreshbutton.BackColor = System.Drawing.Color.FromArgb(134, 27, 45);
            this.refreshbutton.ForeColor = System.Drawing.Color.FromArgb(246, 246, 246);
            this.refreshbutton.Cursor = System.Windows.Forms.Cursors.Hand;
            // copyAllButton
            this.copyAllButton.Name = "copyAllButton";
            this.copyAllButton.Location = new System.Drawing.Point(100, 0);
            this.copyAllButton.Size = new System.Drawing.Size(92, 34);
            this.copyAllButton.Margin = new System.Windows.Forms.Padding(0, 0, 8, 7);
            this.copyAllButton.TabIndex = 1;
            this.copyAllButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.copyAllButton.Text = "Copy all";
            this.copyAllButton.AutoSize = false;
            this.copyAllButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.copyAllButton.UseVisualStyleBackColor = false;
            this.copyAllButton.UseMnemonic = false;
            this.copyAllButton.Tag = "action";
            this.copyAllButton.BackColor = System.Drawing.Color.FromArgb(52, 52, 52);
            this.copyAllButton.ForeColor = System.Drawing.Color.FromArgb(246, 246, 246);
            this.copyAllButton.Cursor = System.Windows.Forms.Cursors.Hand;
            // saveReportButton
            this.saveReportButton.Name = "saveReportButton";
            this.saveReportButton.Location = new System.Drawing.Point(200, 0);
            this.saveReportButton.Size = new System.Drawing.Size(116, 34);
            this.saveReportButton.Margin = new System.Windows.Forms.Padding(0, 0, 8, 7);
            this.saveReportButton.TabIndex = 2;
            this.saveReportButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.saveReportButton.Text = "Save report…";
            this.saveReportButton.AutoSize = false;
            this.saveReportButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveReportButton.UseVisualStyleBackColor = false;
            this.saveReportButton.UseMnemonic = false;
            this.saveReportButton.Tag = "action";
            this.saveReportButton.BackColor = System.Drawing.Color.FromArgb(52, 52, 52);
            this.saveReportButton.ForeColor = System.Drawing.Color.FromArgb(246, 246, 246);
            this.saveReportButton.Cursor = System.Windows.Forms.Cursors.Hand;
            // rowlayout
            this.rowlayout.Name = "rowlayout";
            this.rowlayout.Location = new System.Drawing.Point(16, 88);
            this.rowlayout.Size = new System.Drawing.Size(668, 41);
            this.rowlayout.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.rowlayout.TabIndex = 2;
            this.rowlayout.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.rowlayout.Tag = "inherit";
            this.rowlayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rowlayout.Padding = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.rowlayout.AutoSize = false;
            this.rowlayout.TabStop = false;
            this.rowlayout.BackColor = System.Drawing.Color.FromArgb(43, 43, 43);
            this.rowlayout.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.rowlayout.WrapContents = true;
            this.rowlayout.Controls.Add(this.refreshbutton);
            this.rowlayout.Controls.Add(this.copyAllButton);
            this.rowlayout.Controls.Add(this.saveReportButton);
            // localSystemInformationCard
            this.localSystemInformationCard.Name = "localSystemInformationCard";
            this.localSystemInformationCard.Location = new System.Drawing.Point(0, 0);
            this.localSystemInformationCard.Size = new System.Drawing.Size(700, 667);
            this.localSystemInformationCard.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.localSystemInformationCard.TabIndex = 0;
            this.localSystemInformationCard.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.localSystemInformationCard.Tag = "surface";
            this.localSystemInformationCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.localSystemInformationCard.Padding = new System.Windows.Forms.Padding(16, 16, 16, 16);
            this.localSystemInformationCard.AutoSize = false;
            this.localSystemInformationCard.TabStop = false;
            this.localSystemInformationCard.BackColor = System.Drawing.Color.FromArgb(43, 43, 43);
            this.localSystemInformationCard.ColumnCount = 1;
            this.localSystemInformationCard.RowCount = 4;
            this.localSystemInformationCard.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddRows;
            this.localSystemInformationCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.000000F));
            this.localSystemInformationCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.localSystemInformationCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.localSystemInformationCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.localSystemInformationCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 518F));
            this.localSystemInformationCard.Controls.Add(this.localSystemInformationHeading, 0, 0);
            this.localSystemInformationCard.Controls.Add(this.displaysWindowsVersionHardwareLocaLabel, 0, 1);
            this.localSystemInformationCard.Controls.Add(this.rowlayout, 0, 2);
            this.localSystemInformationCard.Controls.Add(this.result, 0, 3);
            // pageLayout
            this.pageLayout.Name = "pageLayout";
            this.pageLayout.Location = new System.Drawing.Point(0, 0);
            this.pageLayout.Size = new System.Drawing.Size(700, 679);
            this.pageLayout.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.pageLayout.TabIndex = 8;
            this.pageLayout.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.pageLayout.Tag = "inherit";
            this.pageLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.pageLayout.Padding = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.pageLayout.AutoSize = false;
            this.pageLayout.TabStop = false;
            this.pageLayout.BackColor = System.Drawing.Color.FromArgb(31, 31, 31);
            this.pageLayout.ColumnCount = 1;
            this.pageLayout.RowCount = 1;
            this.pageLayout.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddRows;
            this.pageLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.000000F));
            this.pageLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 679F));
            this.pageLayout.Controls.Add(this.localSystemInformationCard, 0, 0);
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(31, 31, 31);
            this.ForeColor = System.Drawing.Color.FromArgb(246, 246, 246);
            this.Controls.Add(this.pageLayout);
            this.Name = "SystemInformationPage";
            this.Size = new System.Drawing.Size(700, 679);
            this.pageLayout.ResumeLayout(false);
            this.localSystemInformationCard.ResumeLayout(false);
            this.rowlayout.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        private System.Windows.Forms.RichTextBox result;
        private System.Windows.Forms.Label localSystemInformationHeading;
        private System.Windows.Forms.Label displaysWindowsVersionHardwareLocaLabel;
        private System.Windows.Forms.Button refreshbutton;
        private System.Windows.Forms.Button copyAllButton;
        private System.Windows.Forms.Button saveReportButton;
        private System.Windows.Forms.FlowLayoutPanel rowlayout;
        private System.Windows.Forms.TableLayoutPanel localSystemInformationCard;
        private System.Windows.Forms.TableLayoutPanel pageLayout;

    }
}
