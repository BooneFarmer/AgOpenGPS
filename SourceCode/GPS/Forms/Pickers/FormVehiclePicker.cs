﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace AgOpenGPS
{
    public partial class FormVehiclePicker : Form
    {
        //class variables
        private readonly FormGPS mf;

        public FormVehiclePicker(Form callingForm)
        {
            //get copy of the calling main form
            mf = callingForm as FormGPS;
            InitializeComponent();

            //this.bntOK.Text = gStr.gsForNow;
            //this.btnSave.Text = gStr.gsToFile;

            this.Text = gStr.gsLoadVehicle;
        }

        private void FormFlags_Load(object sender, EventArgs e)
        {
            lblLast.Text = gStr.gsCurrent + mf.vehicleFileName;
            DirectoryInfo dinfo = new DirectoryInfo(mf.vehiclesDirectory);
            FileInfo[] Files = dinfo.GetFiles("*.txt");
            if (Files.Length == 0)
            {
                Close();
                mf.TimedMessageBox(2000, gStr.gsNoVehiclesSaved, gStr.gsSaveAVehicleFirst);

            }

            foreach (FileInfo file in Files)
            {
                cboxVeh.Items.Add(Path.GetFileNameWithoutExtension(file.Name));
            }
        }

        private void CboxVeh_SelectedIndexChanged(object sender, EventArgs e)
        {
            mf.FileOpenVehicle(mf.vehiclesDirectory + cboxVeh.SelectedItem.ToString() + ".txt");
            Close();
        }
    }
}