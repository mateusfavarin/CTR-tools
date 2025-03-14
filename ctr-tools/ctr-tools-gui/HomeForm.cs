﻿using CTRFramework.Shared;
using CTRTools.Controls;
using System;
using System.IO;
using System.Windows.Forms;

namespace CTRTools
{
    public partial class HomeForm : Form
    {
        //main work area control
        UserControl currentControl;

        // a set of specific controls, assigned to current control one at a time
        // how about a dictionary here
        AboutControl aboutControl = new AboutControl();
        BigFileControl fileControl = new BigFileControl();
        VramControl vramControl = new VramControl();
        XaControl xaControl = new XaControl();
        LevControl levControl = new LevControl();
        HowlControl howlControl = new HowlControl();
        LangControl langControl = new LangControl();
        CtrControl ctrControl = new CtrControl();
        FontControl mpkControl = new FontControl();

        public HomeForm()
        {
            InitializeComponent();

            this.Text = $"CTR-tools-gui - {Meta.Version}";

            OBJ.FixCulture();

            SwitchControl(aboutControl);
        }

        #region [Button click events]
        private void ctrButton_Click(object sender, EventArgs e) => SwitchControl(ctrControl);
        private void fileButton_Click(object sender, EventArgs e) => SwitchControl(fileControl);
        private void xaButton_Click(object sender, EventArgs e) => SwitchControl(xaControl);
        private void langButton_Click(object sender, EventArgs e) => SwitchControl(langControl);
        private void levButton_Click(object sender, EventArgs e) => SwitchControl(levControl);
        private void vramButton_Click(object sender, EventArgs e) => SwitchControl(vramControl);
        private void howlButton_Click(object sender, EventArgs e) => SwitchControl(howlControl);
        private void aboutButton_Click(object sender, EventArgs e) => SwitchControl(aboutControl);
        private void mpkButton_Click(object sender, EventArgs e) => SwitchControl(mpkControl);
        #endregion

        /// <summary>
        /// Handles main work area control switching.
        /// </summary>
        /// <param name="control"></param>
        private void SwitchControl(UserControl control)
        {
            // just in case
            if (control == null) return;

            // early check for clicking the same button
            if (currentControl == control) return;

            this.SuspendLayout();

            if (currentControl != null)
                currentControl.Visible = false;

            // if there is control active
            if (workArea.Controls.Contains(currentControl))
            {
                // hide and disable active control
                currentControl.Enabled = false;
                currentControl.Visible = false;

                workArea.Controls.Remove(currentControl);
            }

            // update active control
            currentControl = control;
            workArea.Controls.Add(currentControl);

            // make sure it fills the entire parent and show 
            currentControl.Dock = DockStyle.Fill;
            currentControl.Enabled = true;
            currentControl.Visible = true;

            currentControl.Visible = true;

            this.ResumeLayout();
        }

        private void HomeForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length > 0)
            {
                MaybeSwitchTab(files[0]);
            }
        }

        public void MaybeSwitchTab(string filename)
        {
            switch (Path.GetExtension(filename).ToUpper())
            {
                case ".CTR":
                case ".OBJ":
                case ".PLY": SwitchControl(ctrControl); break;
                case ".VRM": SwitchControl(vramControl); break;
                case ".MPK": SwitchControl(vramControl); break;
                case ".BIG": SwitchControl(fileControl); break;
                case ".XNF": SwitchControl(xaControl); break;
                case ".WAV":
                case ".VAG":
                case ".HWL":
                case ".CSEQ": SwitchControl(howlControl); break;
                case ".LNG":
                case ".TXT": SwitchControl(langControl); break;
                case ".LEV":
                    if (currentControl != vramControl)
                        SwitchControl(levControl);
                    break;
            }
        }
    }
}
