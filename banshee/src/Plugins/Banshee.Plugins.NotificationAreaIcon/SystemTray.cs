/***************************************************************************
 *  SystemTray.cs
 *
 *  Copyright (C) 2005-2006 Novell, Inc.
 *  Written by Scott Peterson <scottp@gnome.org>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Windows.Forms;

namespace Banshee.Plugins.NotificationAreaIcon
{
    internal class SystemTray : IDisposable
    {
        private class SystemTrayWindow : Form
        {
            public NotifyIcon notify_icon;
            private System.ComponentModel.IContainer components = null;

            protected override void Dispose(bool disposing)
            {
                if(disposing && (components != null)) {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

            private void InitializeComponent(string name, ContextMenu context_menu)
            {
                components = new System.ComponentModel.Container();
                notify_icon = new System.Windows.Forms.NotifyIcon(this.components);
                SuspendLayout();

                notify_icon.Visible = true;
                notify_icon.Text = name;
                notify_icon.ContextMenu = context_menu;
                notify_icon.Icon = new System.Drawing.Icon(GetType(), "banshee_icon.ico");

                ShowInTaskbar = false;
                TopMost = false;
                Visible = false;
                ResumeLayout(false);

            }

            public SystemTrayWindow(string name, ContextMenu context_menu)
            {
                InitializeComponent(name, context_menu);
            }
        }

        private SystemTrayWindow window;

        public event EventHandler Click;
        public event EventHandler DoubleClick;

        public void Dispose()
        {
            window.Dispose();
        }

        public SystemTray(string name, ContextMenu context_menu)
        {
            window = new SystemTrayWindow(name, context_menu);
            window.notify_icon.Click += notify_icon_Click;
            window.notify_icon.DoubleClick += notify_icon_DoubleClick;
        }

        void notify_icon_Click(object sender, EventArgs e)
        {
            if(Click != null) {
                Click(this, e);
            }
        }

        void notify_icon_DoubleClick(object sender, EventArgs e)
        {
            if(DoubleClick != null) {
                DoubleClick(this, e);
            }
        }

        public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
        {
            window.notify_icon.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);
        }

        public string Text
        {
            get { return window.notify_icon.Text; }
            set { window.notify_icon.Text = value; }
        }
    }
}
