﻿using System;
using OpenTK.Graphics.OpenGL;


namespace AgOpenGPS
{
    public class CTool
    {
        private readonly FormGPS mf;

        public double ToolWidth;
        public double ToolFarLeftSpeed = 0;
        public double ToolFarRightSpeed = 0;

        public double toolOverlap;
        public double toolTrailingHitchLength, toolTankTrailingHitchLength;
        public double toolOffset;

        public double LookAheadOffSetting, LookAheadOnSetting;
        public double TurnOffDelay, MappingOnDelay, MappingOffDelay;

        public double lookAheadDistanceOnPixelsLeft, lookAheadDistanceOnPixelsRight;
        public double lookAheadDistanceOffPixelsLeft, lookAheadDistanceOffPixelsRight;

        public bool isToolTrailing, isToolTBT;
        public bool isToolBehindPivot;
        public string toolAttachType;

        public double hitchLength;


        //how many individual sections
        public int numOfSections;

        public int numSuperSection;

        //used for super section off on
        public int toolMinUnappliedPixels;

        //read pixel values
        public int rpXPosition;
        public int rpWidth;

        //Constructor called by FormGPS
        public CTool(FormGPS _f)
        {
            mf = _f;

            //from settings grab the vehicle specifics
            ToolWidth = Properties.Vehicle.Default.setVehicle_toolWidth;
            toolOverlap = Properties.Vehicle.Default.setVehicle_toolOverlap;
            toolOffset = Properties.Vehicle.Default.setVehicle_toolOffset;

            toolTrailingHitchLength = Properties.Vehicle.Default.setTool_toolTrailingHitchLength;
            toolTankTrailingHitchLength = Properties.Vehicle.Default.setVehicle_tankTrailingHitchLength;
            hitchLength = Properties.Vehicle.Default.setVehicle_hitchLength;

            isToolBehindPivot = Properties.Vehicle.Default.setTool_isToolBehindPivot;
            isToolTrailing = Properties.Vehicle.Default.setTool_isToolTrailing;
            isToolTBT = Properties.Vehicle.Default.setTool_isToolTBT;

            LookAheadOnSetting = Properties.Vehicle.Default.setVehicle_toolLookAheadOn;
            LookAheadOffSetting = Properties.Vehicle.Default.setVehicle_toolLookAheadOff;
            TurnOffDelay = Properties.Vehicle.Default.setVehicle_toolOffDelay;

            MappingOnDelay = Properties.Vehicle.Default.setVehicle_MappingOnDelay;
            MappingOffDelay = Properties.Vehicle.Default.setVehicle_MappingOffDelay;

            numOfSections = Properties.Vehicle.Default.setVehicle_numSections;
            numSuperSection = numOfSections + 1;

            toolMinUnappliedPixels = Properties.Vehicle.Default.setVehicle_minApplied;

        }

        public void DrawTool()
        {
            //translate and rotate at pivot axle
            GL.Translate(mf.pivotAxlePos.easting, mf.pivotAxlePos.northing, 0);
            GL.PushMatrix();

            //translate down to the hitch pin
            GL.Translate(Math.Sin(mf.fixHeading) * hitchLength,
                            Math.Cos(mf.fixHeading) * hitchLength, 0);

            //settings doesn't change trailing hitch length if set to rigid, so do it here
            double trailingTank, trailingTool;
            if (isToolTrailing)
            {
                trailingTank = toolTankTrailingHitchLength;
                trailingTool = toolTrailingHitchLength;
            }
            else { trailingTank = 0; trailingTool = 0; }

            //there is a trailing tow between hitch
            if (isToolTBT && isToolTrailing)
            {
                //rotate to tank heading
                GL.Rotate(Glm.ToDegrees(-mf.tankPos.heading), 0.0, 0.0, 1.0);

                //draw the tank hitch
                GL.LineWidth(2f);
                GL.Color3(0.7f, 0.7f, 0.97f);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0.0, trailingTank, 0.0);
                GL.Vertex3(0, 0, 0);
                GL.End();

                //pivot markers
                GL.Color3(0.95f, 0.95f, 0f);
                GL.PointSize(6f);
                GL.Begin(PrimitiveType.Points);
                GL.Vertex3(0.0, trailingTank, 0.0);
                GL.End();

                //move down the tank hitch, unwind, rotate to section heading
                GL.Translate(0.0, trailingTank, 0.0);
                GL.Rotate(Glm.ToDegrees(mf.tankPos.heading), 0.0, 0.0, 1.0);
                GL.Rotate(Glm.ToDegrees(-mf.toolPos.heading), 0.0, 0.0, 1.0);
            }

            //no tow between hitch
            else
            {
                GL.Rotate(Glm.ToDegrees(-mf.toolPos.heading), 0.0, 0.0, 1.0);
            }

            //draw the hitch if trailing
            if (isToolTrailing)
            {
                GL.LineWidth(2);
                GL.Color3(0.7f, 0.7f, 0.97f);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0.0, trailingTool, 0.0);
                GL.Vertex3(0, 0, 0);
                GL.End();
            }

            //look ahead lines
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.Lines);

            //lookahead section on
            GL.Color3(0.20f, 0.7f, 0.2f);//-5.25  and 5.35
            GL.Vertex3(mf.section[0].positionLeft, (mf.tool.lookAheadDistanceOnPixelsLeft) * 0.1 + trailingTool, 0);
            GL.Vertex3(mf.section[mf.tool.numOfSections - 1].positionRight, (mf.tool.lookAheadDistanceOnPixelsRight) * 0.1 + trailingTool, 0);

            //lookahead section off
            GL.Color3(0.70f, 0.2f, 0.2f);
            GL.Vertex3(mf.section[0].positionLeft, (mf.tool.lookAheadDistanceOffPixelsLeft) * 0.1 + trailingTool, 0);
            GL.Vertex3(mf.section[mf.tool.numOfSections - 1].positionRight, (mf.tool.lookAheadDistanceOffPixelsRight) * 0.1 + trailingTool, 0);

            if (mf.vehicle.isHydLiftOn)
            {
                GL.Color3(0.70f, 0.2f, 0.72f);
                GL.Vertex3(mf.section[0].positionLeft, (mf.vehicle.hydLiftLookAheadDistanceLeft * 0.1) + trailingTool, 0);
                GL.Vertex3(mf.section[mf.tool.numOfSections - 1].positionRight, (mf.vehicle.hydLiftLookAheadDistanceRight * 0.1) + trailingTool, 0);
            }

            GL.End();

            //draw the sections
            GL.LineWidth(2);

            double hite = mf.camera.camSetDistance / -100;
            if (hite > 1.3) hite = 1.0;
            if (hite < 0.5) hite = 0.5;

            //draw super section line
            //if (mf.section[numOfSections].isSectionOn)
            //{
            //    if (mf.section[0].manBtnState == FormGPS.manBtn.Auto) GL.Color3(0.50f, 0.97f, 0.950f);
            //    else GL.Color3(0.99, 0.99, 0);

            //    for (int j = 0; j < numOfSections; j++)
            //    {
            //        double mid = (((mf.section[j].positionRight + 100) - (mf.section[j].positionLeft + 100))) / 2 + mf.section[j].positionLeft;

            //        GL.Begin(PrimitiveType.TriangleFan);
            //        {
            //            GL.Vertex3(mf.section[j].positionLeft, trailingTool, 0);
            //            GL.Vertex3(mf.section[j].positionLeft, trailingTool - hite, 0);

            //            GL.Vertex3(mid, trailingTool - hite*2, 0);

            //            GL.Vertex3(mf.section[j].positionRight, trailingTool - hite, 0);
            //            GL.Vertex3(mf.section[j].positionRight, trailingTool, 0);
            //        }
            //        GL.End();

            //        GL.Begin(PrimitiveType.LineLoop);
            //        {
            //            GL.Color3(0.0, 0.0, 0.0);
            //            GL.Vertex3(mf.section[j].positionLeft, trailingTool, 0);
            //            GL.Vertex3(mf.section[j].positionLeft, trailingTool - hite, 0);

            //            GL.Vertex3(mid, trailingTool - hite*2, 0);

            //            GL.Vertex3(mf.section[j].positionRight, trailingTool - hite, 0);
            //            GL.Vertex3(mf.section[j].positionRight, trailingTool, 0);
            //        }
            //        GL.End();
            //    }
            //}
            //else
            {
                for (int j = 0; j < numOfSections; j++)
                {
                    //if section is on, green, if off, red color
                    if (mf.section[j].IsSectionOn || mf.section[numOfSections].IsSectionOn)
                    {
                        if (mf.section[j].BtnSectionState == FormGPS.btnStates.Auto )
                        {
                            GL.Color3(0.0f, 0.9f, 0.0f);
                            //if (mf.section[j].isMappingOn) GL.Color3(0.0f, 0.7f, 0.0f);
                            //else GL.Color3(0.70f, 0.0f, 0.90f);
                        }
                        else GL.Color3(0.97, 0.97, 0);
                    }
                    else
                    {
                        //if (!mf.section[j].isMappingOn) GL.Color3(0.70f, 0.2f, 0.2f);
                        //else GL.Color3(0.00f, 0.250f, 0.90f);
                        GL.Color3(0.7f, 0.2f, 0.2f);
                    }

                    double mid = (((mf.section[j].positionRight+100) - (mf.section[j].positionLeft+100))) / 2 + mf.section[j].positionLeft;

                    GL.Begin(PrimitiveType.TriangleFan);
                    {
                        GL.Vertex3(mf.section[j].positionLeft, trailingTool, 0);
                        GL.Vertex3(mf.section[j].positionLeft, trailingTool - hite, 0);

                        GL.Vertex3(mid, trailingTool - hite * 1.5, 0);

                        GL.Vertex3(mf.section[j].positionRight, trailingTool - hite, 0);
                        GL.Vertex3(mf.section[j].positionRight, trailingTool, 0);
                    }
                    GL.End();

                    GL.Begin(PrimitiveType.LineLoop);
                    {
                        GL.Color3(0.0, 0.0, 0.0);
                        GL.Vertex3(mf.section[j].positionLeft, trailingTool, 0);
                        GL.Vertex3(mf.section[j].positionLeft, trailingTool - hite, 0);

                        GL.Vertex3(mid, trailingTool - hite * 1.5, 0);

                        GL.Vertex3(mf.section[j].positionRight, trailingTool - hite, 0);
                        GL.Vertex3(mf.section[j].positionRight, trailingTool, 0);
                    }
                    GL.End();
                }
            }

            //GL.End();

            //draw section markers if close enough
            if (mf.camera.camSetDistance > -250)
            {
                GL.Color3(0.0f, 0.0f, 0.0f);
                //section markers
                GL.PointSize(3.0f);
                GL.Begin(PrimitiveType.Points);
                for (int j = 0; j < numOfSections - 1; j++)
                    GL.Vertex3(mf.section[j].positionRight, trailingTool, 0);
                GL.End();
            }

            //GL.Color3(0.30f, 1.0f, 1.0f);
            ////section markers
            //GL.PointSize(4.0f);
            //GL.Begin(PrimitiveType.Points);
            ////for (int j = 0; j < numOfSections - 1; j++)
            //GL.End();

            GL.PopMatrix();
        }
    }
}
