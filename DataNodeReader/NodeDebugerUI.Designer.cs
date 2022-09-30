namespace DataNodeReader
{
	partial class NodeDebugerUI
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NodeDebugerUI));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.openFile = new System.Windows.Forms.ToolStripButton();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.saveToFile = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFile,
            this.saveToFile});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(800, 40);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // openFile
            // 
            this.openFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.openFile.Image = ((System.Drawing.Image)(resources.GetObject("openFile.Image")));
            this.openFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openFile.Name = "openFile";
            this.openFile.Size = new System.Drawing.Size(105, 34);
            this.openFile.Text = "Open File";
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.HideSelection = false;
            this.treeView1.LabelEdit = true;
            this.treeView1.Location = new System.Drawing.Point(0, 40);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(800, 410);
            this.treeView1.TabIndex = 1;
            // 
            // saveToFile
            // 
            this.saveToFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.saveToFile.Enabled = false;
            this.saveToFile.Image = ((System.Drawing.Image)(resources.GetObject("saveToFile.Image")));
            this.saveToFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToFile.Name = "saveToFile";
            this.saveToFile.Size = new System.Drawing.Size(124, 34);
            this.saveToFile.Text = "Save To File";
            // 
            // NodeDebugerUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "NodeDebugerUI";
            this.Text = "Node Debuger";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ToolStrip toolStrip1;
		private ToolStripButton openFile;
        private TreeView treeView1;
        private ToolStripButton saveToFile;
    }
}