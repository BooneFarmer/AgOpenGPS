﻿using System;
using System.Windows.Forms;

namespace AgOpenGPS
{
    public partial class FormEditCurve : Form
    {
        private readonly FormGPS mf;

        private double snapAdj = 0;

        public FormEditCurve(Form callingForm)
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

            //btnLeft.Text = "-"+Properties.Settings.Default.setDisplay_snapDistanceSmall.ToString() + "cm";
            label2.Text = "\u00BD";


            snapAdj = Properties.Settings.Default.setAS_snapDistance * 0.01;
            nudMinTurnRadius.Value = Properties.Settings.Default.setAS_snapDistance;

            btnCancel.Focus();
            mf.curve.isEditing = true;
            mf.layoutPanelRight.Enabled = false;
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
            mf.curve.MoveABCurve(snapAdj);
        }

        private void BtnAdjLeft_Click(object sender, EventArgs e)
        {
            mf.curve.MoveABCurve(-snapAdj);
        }

        private void BntOk_Click(object sender, EventArgs e)
        {
            mf.curve.isEditing = false;

            if (mf.curve.refList.Count > 0)
            {
                //array number is 1 less since it starts at zero
                int idx = mf.curve.numCurveLineSelected - 1;

                if (idx >= 0)
                {
                    mf.curve.curveArr[idx].aveHeading = mf.curve.aveLineHeading;
                    mf.curve.curveArr[idx].curvePts.Clear();
                    //write out the Curve Points
                    foreach (var item in mf.curve.refList)
                    {
                        mf.curve.curveArr[idx].curvePts.Add(item);
                    }
                }

                //save entire list
                mf.FileSaveCurveLines();
                mf.curve.moveDistance = 0;

                mf.layoutPanelRight.Enabled = true;
                Close();
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            mf.curve.isEditing = false;
            int last = mf.curve.numCurveLineSelected;
            mf.FileLoadCurveLines();
            if (mf.curve.curveArr.Count > 0)
            {
                mf.curve.numCurveLineSelected = last;
                int idx = mf.curve.numCurveLineSelected - 1;
                mf.curve.aveLineHeading = mf.curve.curveArr[idx].aveHeading;

                mf.curve.refList?.Clear();
                for (int i = 0; i < mf.curve.curveArr[idx].curvePts.Count; i++)
                {
                    mf.curve.refList.Add(mf.curve.curveArr[idx].curvePts[i]);
                }
                mf.curve.isCurveSet = true;
            }
            mf.layoutPanelRight.Enabled = true;

            Close();
        }

        private void BtnSwapAB_Click(object sender, EventArgs e)
        {
            int cnt = mf.curve.refList.Count;
            if (cnt > 0)
            {
                mf.curve.refList.Reverse();

                Vec3[] arr = new Vec3[cnt];
                cnt--;
                mf.curve.refList.CopyTo(arr);
                mf.curve.refList.Clear();

                mf.curve.aveLineHeading = (mf.curve.aveLineHeading + Math.PI) % Glm.twoPI;

                for (int i = 1; i < cnt; i++)
                {
                    Vec3 pt3 = arr[i];
                    pt3.heading = (pt3.heading + Math.PI) % Glm.twoPI;
                    if (pt3.heading < 0) pt3.heading += Glm.twoPI;
                    mf.curve.refList.Add(pt3);
                }
            }
        }

        private void BtnContourPriority_Click(object sender, EventArgs e)
        {
            if (mf.curve.isBtnCurveOn)
            {
                mf.curve.SnapABCurve();
            }
        }

        private void BtnRightHalfWidth_Click(object sender, EventArgs e)
        {
            double dist = mf.Guidance.GuidanceWidth - mf.Guidance.GuidanceOverlap;

            mf.curve.MoveABCurve(dist*0.5);

        }

        private void BtnLeftHalfWidth_Click(object sender, EventArgs e)
        {
            double dist = mf.Guidance.GuidanceWidth - mf.Guidance.GuidanceOverlap;

            mf.curve.MoveABCurve(-dist*0.5);

        }
    }
}
