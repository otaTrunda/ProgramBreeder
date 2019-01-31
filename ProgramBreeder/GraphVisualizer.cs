using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProgramBreeder
{
	partial class GraphVisualizer : Form
	{
		Microsoft.Msagl.GraphViewerGdi.GViewer viewer;
		//Graph MSAGL_graph;
		private bool useAsDialog = true;

		public GraphVisualizer(bool useAsDialog = true)
		{
			InitializeComponent();
			this.useAsDialog = useAsDialog;
			viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
			//viewer.CurrentLayoutMethod = Microsoft.Msagl.GraphViewerGdi.LayoutMethod.IcrementalLayout;
			//viewer.CurrentLayoutMethod = Microsoft.Msagl.GraphViewerGdi.LayoutMethod.MDS;
			viewer.CurrentLayoutMethod = Microsoft.Msagl.GraphViewerGdi.LayoutMethod.SugiyamaScheme;
			viewer.Dock = DockStyle.Fill;
			this.Controls.Add(viewer);
            this.Width = 500;
            this.Height = 250;
		}

		public void visualizeDialog(Microsoft.Msagl.Drawing.Graph g)
		{
			viewer.Graph = g;
			this.ShowDialog();
		}

		public void visualize(Microsoft.Msagl.Drawing.Graph g)
		{
			viewer.Graph = g;
			Application.Run(this);
			//this.Show();
		}
	}
}
