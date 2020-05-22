﻿using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace AgOpenGPS
{
    public class CTurn
    {
        //copy of the mainform address
        private readonly FormGPS mf;

        private readonly double boxLength;

        /// <summary>
        /// array of turns
        /// </summary>
        public List<CTurnLines> turnArr = new List<CTurnLines>();

        //constructor
        public CTurn(FormGPS _f)
        {
            mf = _f;
            turnSelected = 0;
            //scanWidth = 1.5;
            boxLength = 2000;
        }

        // the list of possible bounds points
        public List<vec4> turnClosestList = new List<vec4>();

        public int turnSelected, closestTurnNum;

        //generated box for finding closest point
        public vec2 boxA = new vec2(9000, 9000), boxB = new vec2(9000, 9002);

        public vec2 boxC = new vec2(9001, 9001), boxD = new vec2(9002, 9003);

        //point at the farthest turn segment from pivotAxle
        public vec3 closestTurnPt = new vec3();


        public void FindClosestTurnPoint(bool isYouTurnRight, vec3 fromPt, double headAB)
        {
            //initial scan is straight ahead of pivot point of vehicle to find the right turnLine/boundary
            vec3 pt = new vec3();
            vec3 rayPt = new vec3();

            bool isFound = false;
            int closestTurnNum = 99;

            double cosHead = Math.Cos(headAB);
            double sinHead = Math.Sin(headAB);

            for (int b = 1; b < mf.maxCrossFieldLength; b += 2)
            {
                pt.easting = fromPt.easting + (sinHead * b);
                pt.northing = fromPt.northing + (cosHead * b);

                if (turnArr[0].IsPointInTurnWorkArea(pt))
                {
                    for (int t = 1; t < mf.bnd.bndArr.Count; t++)
                    {
                        if (mf.bnd.bndArr[t].isDriveThru || mf.bnd.bndArr[t].isDriveAround) continue;
                        if (mf.turn.turnArr[t].IsPointInTurnWorkArea(pt))
                        {
                            isFound = true;
                            closestTurnNum = t;
                            rayPt.easting = pt.easting;
                            rayPt.northing = pt.northing;
                            break;
                        }
                    }
                    if (isFound) break;
                }
                else
                {
                    closestTurnNum = 0;
                    rayPt.easting = pt.easting;
                    rayPt.northing = pt.northing;
                    break;
                }
            }

            //second scan is straight ahead of outside of tool based on turn direction
            double scanWidthL, scanWidthR;
            if (isYouTurnRight) //its actually left
            {
                scanWidthL = -(mf.tool.ToolWidth * 0.25) - (mf.tool.ToolWidth * 0.5);
                scanWidthR = (mf.tool.ToolWidth * 0.25) - (mf.tool.ToolWidth * 0.5);
            }
            else
            {
                scanWidthL = -(mf.tool.ToolWidth * 0.25) + (mf.tool.ToolWidth * 0.5);
                scanWidthR = (mf.tool.ToolWidth * 0.25) + (mf.tool.ToolWidth * 0.5);
            }

            //isYouTurnRight actuall means turning left - Painful, but it switches later
            boxA.easting = fromPt.easting + (Math.Sin(headAB + Glm.PIBy2) * scanWidthL);
            boxA.northing = fromPt.northing + (Math.Cos(headAB + Glm.PIBy2) * scanWidthL);

            boxB.easting = fromPt.easting + (Math.Sin(headAB + Glm.PIBy2) * scanWidthR);
            boxB.northing = fromPt.northing + (Math.Cos(headAB + Glm.PIBy2) * scanWidthR);

            boxC.easting = boxB.easting + (Math.Sin(headAB) * boxLength);
            boxC.northing = boxB.northing + (Math.Cos(headAB) * boxLength);

            boxD.easting = boxA.easting + (Math.Sin(headAB) * boxLength);
            boxD.northing = boxA.northing + (Math.Cos(headAB) * boxLength);

            //determine if point is inside bounding box of the 1 turn chosen above
            turnClosestList.Clear();

            mf.turn.closestTurnNum = closestTurnNum;
            vec4 inBox;

            int ptCount = turnArr[closestTurnNum].turnLine.Count;
            for (int p = 0; p < ptCount; p++)
            {
                if ((((boxB.easting - boxA.easting) * (turnArr[closestTurnNum].turnLine[p].northing - boxA.northing))
                        - ((boxB.northing - boxA.northing) * (turnArr[closestTurnNum].turnLine[p].easting - boxA.easting))) < 0) { continue; }

                if ((((boxD.easting - boxC.easting) * (turnArr[closestTurnNum].turnLine[p].northing - boxC.northing))
                        - ((boxD.northing - boxC.northing) * (turnArr[closestTurnNum].turnLine[p].easting - boxC.easting))) < 0) { continue; }

                if ((((boxC.easting - boxB.easting) * (turnArr[closestTurnNum].turnLine[p].northing - boxB.northing))
                        - ((boxC.northing - boxB.northing) * (turnArr[closestTurnNum].turnLine[p].easting - boxB.easting))) < 0) { continue; }

                if ((((boxA.easting - boxD.easting) * (turnArr[closestTurnNum].turnLine[p].northing - boxD.northing))
                        - ((boxA.northing - boxD.northing) * (turnArr[closestTurnNum].turnLine[p].easting - boxD.easting))) < 0) { continue; }

                //it's in the box, so add to list
                inBox.easting = turnArr[closestTurnNum].turnLine[p].easting;
                inBox.northing = turnArr[closestTurnNum].turnLine[p].northing;
                inBox.heading = turnArr[closestTurnNum].turnLine[p].heading;
                inBox.index = closestTurnNum;

                //which turn/headland is it from
                turnClosestList.Add(inBox);
            }

            if (turnClosestList.Count == 0)
            {
                if (isYouTurnRight) //its actually left
                {
                    scanWidthL = -(mf.tool.ToolWidth * 0.5);
                    scanWidthR = 0;
                }
                else
                {
                    scanWidthL = 0;
                    scanWidthR = (mf.tool.ToolWidth * 0.5);
                }

                //isYouTurnRight actuall means turning left - Painful, but it switches later
                boxA.easting = fromPt.easting + (Math.Sin(headAB + Glm.PIBy2) * scanWidthL);
                boxA.northing = fromPt.northing + (Math.Cos(headAB + Glm.PIBy2) * scanWidthL);

                boxB.easting = fromPt.easting + (Math.Sin(headAB + Glm.PIBy2) * scanWidthR);
                boxB.northing = fromPt.northing + (Math.Cos(headAB + Glm.PIBy2) * scanWidthR);

                boxC.easting = boxB.easting + (Math.Sin(headAB) * boxLength);
                boxC.northing = boxB.northing + (Math.Cos(headAB) * boxLength);

                boxD.easting = boxA.easting + (Math.Sin(headAB) * boxLength);
                boxD.northing = boxA.northing + (Math.Cos(headAB) * boxLength);

                //determine if point is inside bounding box of the 1 turn chosen above
                turnClosestList.Clear();

                mf.turn.closestTurnNum = closestTurnNum;

                ptCount = turnArr[closestTurnNum].turnLine.Count;
                for (int p = 0; p < ptCount; p++)
                {
                    if ((((boxB.easting - boxA.easting) * (turnArr[closestTurnNum].turnLine[p].northing - boxA.northing))
                            - ((boxB.northing - boxA.northing) * (turnArr[closestTurnNum].turnLine[p].easting - boxA.easting))) < 0) { continue; }

                    if ((((boxD.easting - boxC.easting) * (turnArr[closestTurnNum].turnLine[p].northing - boxC.northing))
                            - ((boxD.northing - boxC.northing) * (turnArr[closestTurnNum].turnLine[p].easting - boxC.easting))) < 0) { continue; }

                    if ((((boxC.easting - boxB.easting) * (turnArr[closestTurnNum].turnLine[p].northing - boxB.northing))
                            - ((boxC.northing - boxB.northing) * (turnArr[closestTurnNum].turnLine[p].easting - boxB.easting))) < 0) { continue; }

                    if ((((boxA.easting - boxD.easting) * (turnArr[closestTurnNum].turnLine[p].northing - boxD.northing))
                            - ((boxA.northing - boxD.northing) * (turnArr[closestTurnNum].turnLine[p].easting - boxD.easting))) < 0) { continue; }

                    //it's in the box, so add to list
                    inBox.easting = turnArr[closestTurnNum].turnLine[p].easting;
                    inBox.northing = turnArr[closestTurnNum].turnLine[p].northing;
                    inBox.heading = turnArr[closestTurnNum].turnLine[p].heading;
                    inBox.index = closestTurnNum;

                    //which turn/headland is it from
                    turnClosestList.Add(inBox);
                }
            }
            //which of the points is closest
            //closestTurnPt.easting = -20000; closestTurnPt.northing = -20000;
            ptCount = turnClosestList.Count;
            if (ptCount != 0)
            {
                double totalDist = 0.75 * Math.Sqrt(((fromPt.easting - rayPt.easting) * (fromPt.easting - rayPt.easting))
                + ((fromPt.northing - rayPt.northing) * (fromPt.northing - rayPt.northing)));

                //determine closest point
                double minDistance = 9999999;
                for (int i = 0; i < ptCount; i++)
                {
                    double dist = Math.Sqrt(((fromPt.easting - turnClosestList[i].easting) * (fromPt.easting - turnClosestList[i].easting))
                                    + ((fromPt.northing - turnClosestList[i].northing) * (fromPt.northing - turnClosestList[i].northing)));

                    //double distRay = ((rayPt.easting - turnClosestList[i].easting) * (rayPt.easting - turnClosestList[i].easting))
                    //                + ((rayPt.northing - turnClosestList[i].northing) * (rayPt.northing - turnClosestList[i].northing));

                    if (minDistance >= dist && dist > totalDist)
                    {
                        minDistance = dist;
                        closestTurnPt.easting = turnClosestList[i].easting;
                        closestTurnPt.northing = turnClosestList[i].northing;
                        closestTurnPt.heading = turnClosestList[i].heading;
                    }
                }
                if (closestTurnPt.heading < 0) closestTurnPt.heading += Glm.twoPI;
            }
        }

        public void BuildTurnLines(int Num)
        {
            if (mf.bnd.bndArr.Count == 0) return;
            //to fill the list of line points
            vec3 point = new vec3();

            //determine how wide a headland space
            double totalHeadWidth = mf.yt.triggerDistanceOffset;

            //inside boundaries
            for (int j = (Num < 0) ? 0 : Num; j < mf.bnd.bndArr.Count; j++)
            {
                turnArr[j].turnLine.Clear();

                int ChangeDirection = j == 0 ? 1 : -1;

                int ptCount = mf.bnd.bndArr[j].bndLine.Count;

                for (int i = ptCount - 1; i >= 0; i--)
                {
                    //calculate the point outside the boundary
                    point.easting = mf.bnd.bndArr[j].bndLine[i].easting + (-Math.Sin(Glm.PIBy2 + mf.bnd.bndArr[j].bndLine[i].heading) * totalHeadWidth * ChangeDirection);
                    point.northing = mf.bnd.bndArr[j].bndLine[i].northing + (-Math.Cos(Glm.PIBy2 + mf.bnd.bndArr[j].bndLine[i].heading) * totalHeadWidth * ChangeDirection);
                    point.heading = mf.bnd.bndArr[j].bndLine[i].heading;

                    //only add if outside actual field boundary
                    if ((j == 0 && mf.bnd.bndArr[j].IsPointInsideBoundary(point)) || (j != 0 && !mf.bnd.bndArr[j].IsPointInsideBoundary(point)))
                    {
                        vec3 tPnt = new vec3(point.easting, point.northing, point.heading);
                        turnArr[j].turnLine.Add(tPnt);
                    }
                }
                turnArr[j].FixTurnLine(totalHeadWidth, mf.bnd.bndArr[j].bndLine, mf.tool.ToolWidth * 0.4);
                turnArr[j].PreCalcTurnLines();

                if (Num > -1) break;
            }
        }

        public void DrawTurnLines()
        {
            GL.LineWidth(mf.ABLine.lineWidth);
            GL.Color3(0.3555f, 0.6232f, 0.20f);
            //GL.PointSize(2);

            for (int i = 0; i < mf.bnd.bndArr.Count; i++)
            {
                if (mf.bnd.bndArr[i].isDriveAround || mf.bnd.bndArr[i].isDriveThru) continue;

                ////draw the turn line oject
                int ptCount = mf.turn.turnArr[i].turnLine.Count;
                if (ptCount < 1) return;
                GL.Begin(PrimitiveType.LineLoop);
                for (int h = 0; h < ptCount; h++) GL.Vertex3(mf.turn.turnArr[i].turnLine[h].easting, mf.turn.turnArr[i].turnLine[h].northing, 0);
                GL.End();
            }
        }

        //draws the derived closest point
        public void DrawClosestPoint()
        {
            //GL.PointSize(6.0f);
            //GL.Color3(0.219f, 0.932f, 0.070f);
            //GL.Begin(PrimitiveType.Points);
            //GL.Vertex3(closestTurnPt.easting, closestTurnPt.northing, 0);
            //GL.End();

            //GL.LineWidth(1);
            //GL.Color3(0.92f, 0.62f, 0.42f);
            //GL.Begin(PrimitiveType.LineStrip);
            //GL.Vertex3(boxD.easting, boxD.northing, 0);
            //GL.Vertex3(boxA.easting, boxA.northing, 0);
            //GL.Vertex3(boxB.easting, boxB.northing, 0);
            //GL.Vertex3(boxC.easting, boxC.northing, 0);
            //GL.End();
        }
    }
}