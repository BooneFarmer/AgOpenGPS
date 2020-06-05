﻿using System;
using System.Windows.Forms;

namespace AgOpenGPS
{
    public partial class FormEditAB : Form
    {
        private readonly FormGPS mf;

        private double snapAdj = 0;

        public FormEditAB(Form callingForm)
        {
            //get copy of the calling main form
            mf = callingForm as FormGPS;

            InitializeComponent();

            this.Text = gStr.gsEditABLine;
            label2.Text = gStr.gsABHeading;
            nudMinTurnRadius.Controls[0].Enabled = false;
        }

        private void FormEditAB_Load(object sender, EventArgs e)
        {
            snapAdj = Properties.Settings.Default.setAS_snapDistance * 0.01;
            nudMinTurnRadius.Value = Properties.Settings.Default.setAS_snapDistance;

            tboxHeading.Text = Math.Round(Glm.ToDegrees(mf.ABLine.abHeading), 3).ToString("N3");
            btnCancel.Focus();
            mf.ABLine.isEditing = true;
            mf.layoutPanelRight.Enabled = true;
            label3.Text = "\u00BD";
        }

        private void NudMinTurnRadius_Enter(object sender, EventArgs e)
        {
            mf.KeypadToNUD((NumericUpDown)sender);
            btnCancel.Focus();
        }

        private void NudMinTurnRadius_ValueChanged(object sender, EventArgs e)
        {
            snapAdj = (double)nudMinTurnRadius.Value * 0.01;
        }

        private void BtnAdjRight_Click(object sender, EventArgs e)
        {
            mf.ABLine.MoveABLine(snapAdj);
        }

        private void BtnAdjLeft_Click(object sender, EventArgs e)
        {
            mf.ABLine.MoveABLine(-snapAdj);
        }

        private void BntOk_Click(object sender, EventArgs e)
        {
            mf.ABLine.isEditing = false;

            //index to last one. 
            int idx = mf.ABLine.numABLineSelected - 1;

            if (idx >= 0)
            {

                mf.ABLine.lineArr[idx].heading = mf.ABLine.abHeading;
                //calculate the new points for the reference line and points
                mf.ABLine.lineArr[idx].origin.easting = mf.ABLine.refPoint1.easting;
                mf.ABLine.lineArr[idx].origin.northing = mf.ABLine.refPoint1.northing;

                //sin x cos z for endpoints, opposite for additional lines
                mf.ABLine.lineArr[idx].ref1.easting = mf.ABLine.lineArr[idx].origin.easting - (Math.Sin(mf.ABLine.lineArr[idx].heading) *   1600.0);
                mf.ABLine.lineArr[idx].ref1.northing = mf.ABLine.lineArr[idx].origin.northing - (Math.Cos(mf.ABLine.lineArr[idx].heading) * 1600.0);
                mf.ABLine.lineArr[idx].ref2.easting = mf.ABLine.lineArr[idx].origin.easting + (Math.Sin(mf.ABLine.lineArr[idx].heading) *   1600.0);
                mf.ABLine.lineArr[idx].ref2.northing = mf.ABLine.lineArr[idx].origin.northing + (Math.Cos(mf.ABLine.lineArr[idx].heading) * 1600.0);
            }

            mf.FileSaveABLines();
            mf.ABLine.moveDistance = 0;

            mf.layoutPanelRight.Enabled = true;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            mf.ABLine.isEditing = false;
            int last = mf.ABLine.numABLineSelected;
            mf.FileLoadABLines();

            mf.ABLine.numABLineSelected = last;
            mf.ABLine.refPoint1 = mf.ABLine.lineArr[mf.ABLine.numABLineSelected - 1].origin;
            mf.ABLine.abHeading = mf.ABLine.lineArr[mf.ABLine.numABLineSelected - 1].heading;
            mf.ABLine.SetABLineByHeading();
            mf.ABLine.isABLineSet = true;
            mf.ABLine.isABLineLoaded = true;
            mf.ABLine.moveDistance = 0;

            mf.layoutPanelRight.Enabled = true;
            Close();
        }

        private void BtnSwapAB_Click(object sender, EventArgs e)
        {
            mf.ABLine.abHeading = (mf.ABLine.abHeading + Math.PI) % Glm.twoPI;

            mf.ABLine.refABLineP1.easting = mf.ABLine.refPoint1.easting - (Math.Sin(mf.ABLine.abHeading) *   1600.0);
            mf.ABLine.refABLineP1.northing = mf.ABLine.refPoint1.northing - (Math.Cos(mf.ABLine.abHeading) * 1600.0);
            mf.ABLine.refABLineP2.easting = mf.ABLine.refPoint1.easting + (Math.Sin(mf.ABLine.abHeading) *   1600.0);
            mf.ABLine.refABLineP2.northing = mf.ABLine.refPoint1.northing + (Math.Cos(mf.ABLine.abHeading) * 1600.0);

            mf.ABLine.refPoint2.easting = mf.ABLine.refABLineP2.easting;
            mf.ABLine.refPoint2.northing = mf.ABLine.refABLineP2.northing;

            if (mf.tram.displayMode > 0) mf.ABLine.BuildTram();

            tboxHeading.Text = Math.Round(Glm.ToDegrees(mf.ABLine.abHeading), 3).ToString("N3");
        }

        private void BtnBPoint_Click(object sender, EventArgs e)
        {
            mf.ABLine.SetABLineByBPoint();
            tboxHeading.Text = Math.Round(Glm.ToDegrees(mf.ABLine.abHeading), 3).ToString("N3");

            //update the default
            //if (mf.ABLine.tramPassEvery == 0) mf.mc.machineData[mf.mc.rdTramLine] = 0;

            tboxHeading.Text = Math.Round(Glm.ToDegrees(mf.ABLine.abHeading), 3).ToString("N3");
        }

        private void TboxHeading_Enter(object sender, EventArgs e)
        {
            tboxHeading.Text = "";

            using (var form = new FormNumeric(0, 360, Math.Round(Glm.ToDegrees(mf.ABLine.abHeading), 5)))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    tboxHeading.Text = ((double)form.ReturnValue).ToString("N3");
                    mf.ABLine.abHeading = Glm.ToRadians((double)form.ReturnValue);
                    mf.ABLine.SetABLineByHeading();
                }
            }

            btnCancel.Focus();

        }

        private void BtnContourPriority_Click(object sender, EventArgs e)
        {
            if (mf.ABLine.isABLineSet)
            {
                mf.ABLine.SnapABLine();
            }
        }

        private void BtnRightHalfWidth_Click(object sender, EventArgs e)
        {
            double dist = mf.Tools[0].ToolWidth - mf.Tools[0].ToolOverlap;

            mf.ABLine.MoveABLine(dist * 0.5);

        }

        private void BtnLeftHalfWidth_Click(object sender, EventArgs e)
        {
            double dist = mf.Tools[0].ToolWidth - mf.Tools[0].ToolOverlap;

            mf.ABLine.MoveABLine(-dist*0.5);

        }
    }
}
