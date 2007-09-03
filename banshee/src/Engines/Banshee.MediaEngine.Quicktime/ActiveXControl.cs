using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Banshee.MediaEngine.Quicktime
{
	public partial class ActiveXControl: Form
	{
		public ActiveXControl()
		{
			InitializeComponent();
		}

        public AxQTOControlLib.AxQTControl axQTControl
        {
            get { return axQTControl1; }
        }
	}
}