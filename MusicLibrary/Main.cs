﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MusicLibrary_BLL.Services;
using NAudio.Wave;

namespace MusicLibrary
{
    public partial class Main : Form
    {
        public string RootDirectory = @"D:\OneDrive - nhg.vn\CNTT_HongBang\HK5\.NET\Project\Source\Music";
        TreeViewService _treeViewSerivce = TreeViewService.GetInstance();
        MusicPlayer mp = MusicPlayer.GetInstance();

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            _treeViewSerivce.BindDirectoryToTreeView(trvDirectories, RootDirectory);
        }

        #region Events
        // Buttons click
        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        private void trvDirectories_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            mp.PlayMusic(e.Node.Tag.ToString());
        }
    }
}
