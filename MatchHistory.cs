﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RedBallTracker
{
    public partial class MatchHistory : Form
    {
        private string connectionString;
        public MatchHistory()
        {

            InitializeComponent();
            connectionString = ConfigurationManager.AppSettings["connectionString"];
            DataSet dataSet = DataAdapterUsage.Connect(connectionString);

            DataGridView DGV = new DataGridView();
            DGV.DataSource = dataSet;
            this.Controls.Add(DGV);
            DGV.Show();
        }
    }
}