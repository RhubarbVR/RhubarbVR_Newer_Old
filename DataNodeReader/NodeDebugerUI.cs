using System.Windows.Forms;

using RhuEngine.DataStructure;
using RhuEngine;

namespace DataNodeReader
{
	public partial class NodeDebugerUI : Form
	{
		public NodeDebugerUI() {
			InitializeComponent();
			openFile.Click += OpenFile_Click;
			saveToFile.Click += SaveToFile_Click;
			_openFileDialog = new OpenFileDialog() {
				FileName = "Rhubarb DataNode file",
				Title = "Open DataNode file",
			};
			_openFileDialog.FileOk += OpenFileDialog_FileOk;
			_saveFileDialog = new SaveFileDialog() {
				FileName = "Rhubarb DataNode file",
				Title = "Save DataNode file",
				CheckFileExists = true,
				CheckPathExists = true,
			};
			_saveFileDialog.FileOk += SaveFileDialog_FileOk;
			_node = null;
			treeView1.LabelEdit = true;
			treeView1.AfterLabelEdit += TreeView1_AfterLabelEdit;
			treeView1.BeforeLabelEdit += TreeView1_BeforeLabelEdit;
			NodeGroupUpdate();
		}

		private void SaveFileDialog_FileOk(object? sender, System.ComponentModel.CancelEventArgs e) {
			Task.Run(SaveFile);
		}

		private async Task SaveFile() {
			var fileStream = _saveFileDialog.OpenFile();
			await fileStream.WriteAsync(new DataSaver(_node).SaveStore());
			fileStream.Close();
		}

		private void SaveToFile_Click(object? sender, EventArgs e) {
			_saveFileDialog.ShowDialog();
		}

		private void TreeView1_BeforeLabelEdit(object? sender, NodeLabelEditEventArgs e) {
			if(e.Node.Tag is null) {
				e.CancelEdit = true;
			}
		}

		private void TreeView1_AfterLabelEdit(object? sender, NodeLabelEditEventArgs e) {
			if (e.CancelEdit) {
				return;
			}
			if(e.Node.Tag is IDateNodeValue editingValue) {
				try {
					var data = Convert.ChangeType(e.Label, editingValue.Type);
					editingValue.ObjectValue = data;
				}
				catch {

				}
				e.Node.Text = editingValue.ObjectValue.ToString();
			}
		}

		private void OpenFileDialog_FileOk(object? sender, System.ComponentModel.CancelEventArgs e) {
			Task.Run(LoadData);

		}

		private async Task LoadData() {
			try {
				using var memoryStream = new MemoryStream();
				var fileStream = _openFileDialog.OpenFile();
				await fileStream.CopyToAsync(memoryStream);
				fileStream.Close();
				_node = new DataReader(memoryStream.ToArray()).Data;
				Invoke(NodeGroupUpdate);
			}
			catch (Exception er) {
				MessageBox.Show(er.ToString(), "Error Opening file",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				_node = null;
				Invoke(NodeGroupUpdate);
			}
		}
		private readonly SaveFileDialog _saveFileDialog;

		private readonly OpenFileDialog _openFileDialog;

		private IDataNode? _node;

		private void OpenFile_Click(object? sender, EventArgs e) {
			_openFileDialog.ShowDialog();
		}

		private void NodeGroupUpdate() {
			treeView1.Nodes.Clear();
			if(_node is null) {
				saveToFile.Enabled = false;
				treeView1.Nodes.Add("No Node Loaded");
				return;
			}
			saveToFile.Enabled = true;
			var sack = new List<(string AddedName, IDataNode dataNode, TreeNode parrent)> {
				("Root", _node, treeView1.Nodes.Add("Holder"))
			};
			for (var i = 0; i < sack.Count; i++) {
				LoadTreeNode(sack[i],sack);
			}

		}

		private static void LoadTreeNode((string AddedName, IDataNode dataNode, TreeNode parrent) current, List<(string AddedName, IDataNode dataNode, TreeNode parrent)> stack) {
			var node = new TreeNode(current.AddedName + " " + current.dataNode.GetType().GetFormattedName());
			if (current.dataNode is DataNodeGroup dataNodeGroup) {
				foreach (var item in dataNodeGroup._nodeGroup) {
					stack.Add((item.Key, item.Value, node));
				}
			}
			else if (current.dataNode is DataNodeList dataNodeList){
				for (var i = 0; i < dataNodeList._nodeGroup.Count; i++) {
					stack.Add((i.ToString(), dataNodeList._nodeGroup[i], node));
				}
			}
			else if (current.dataNode is IDateNodeValue dataValue) {
				var ValueNode = node.Nodes.Add("Value", dataValue.ObjectValue?.ToString()??"Null");
				ValueNode.Tag = dataValue;
			}
			current.parrent.Nodes.Add(node);
		}
	}
}