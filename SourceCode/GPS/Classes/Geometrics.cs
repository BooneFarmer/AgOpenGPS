using System;

namespace AgOpenGPS
{
    public static class Geometrics
    {
        /*
        *  Geometric vector defined in two dimensions.
        *  Defined in terms of cartesian x,y change
        *  and polar magnitude and direction.
        */
        public struct
        Vector2
        {
            public readonly double x;
            public readonly double y;
            public readonly double magnitude;
            public readonly double direction;

            private
            Vector2(double p_x, double p_y, double p_magnitude, double p_direction)
            {
                x = p_x;
                y = p_y;
                magnitude = p_magnitude;
                direction = p_direction;
            }

            public static Vector2
            Polar(double magnitude, double direction)
            {
                magnitude = Math.Abs(magnitude);

                const double twoPI = 2 * Math.PI;
                for (; direction < 0D;) { direction += twoPI; }
                for (; direction > twoPI;) { direction -= twoPI; }

                double x = (magnitude * Math.Cos(direction));
                double y = (magnitude * Math.Sin(direction));

                return new Vector2(x, y, magnitude, direction);
            }

            public static Vector2
            Cartesian(double x, double y)
            {
                double magnitude =
                Math.Abs(Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)));

                double direction = Math.Atan2(y, x);
                {
                    const double twoPI = 2 * Math.PI;
                    for (; direction < 0D;) { direction += twoPI; }
                    for (; direction > twoPI;) { direction -= twoPI; }
                }

                return new Vector2(x, y, magnitude, direction);
            }

            public static Vector2 operator +
            (Vector2 left, Vector2 right)
            {
                return Vector2.Cartesian
                    (left.x + right.x, left.y + right.y);
            }

            public static Vector2 operator -
            (Vector2 left, Vector2 right)
            {
                return Vector2.Cartesian
                    (left.x + (-right.x), left.y + (-right.y));
            }

            public static Vector2
            Reverse(Vector2 vector)
            {
                return Vector2.Cartesian
                    (-(vector.x), -(vector.y));
            }

            public static Vector2
            Scale(Vector2 vector, double scalar)
            {
                return Vector2.Polar
                    (vector.magnitude * scalar, vector.direction);
            }
        }

        public struct
        Line2
        {
            public readonly double
            slope,
            xIntercept,
            yIntercept;

            public Line2(double p_slope, double p_xIntercept, double p_yIntercept)
            {
                slope = p_slope;
                xIntercept = p_xIntercept;
                yIntercept = p_yIntercept;
            }

            public static Line2
            DefineByX(double xIntercept)
            {
                return new Line2(double.NaN, xIntercept, double.NaN);
            }

            public static Line2
            DefineByY(double yIntercept)
            {
                return new Line2(0D, double.NaN, yIntercept);
            }
        }

        public struct
        LineSegment2
        {
            public readonly vec2
            point1,
            point2;

            public readonly Line2
            equation;

            public
            LineSegment2(vec2 p_point1, vec2 p_point2)
            {
                point1 = p_point1;
                point2 = p_point2;

                double dx = Math.Abs(point2.northing - point1.northing);
                double dy = Math.Abs(point2.easting - point1.easting);

                double
                xIntercept,
                yIntercept;

                if (dx == 0D && dy == 0D)
                {
                    equation = new Line2(double.NaN, double.NaN, double.NaN);
                    return;
                }
                if (dx == 0)
                {
                    xIntercept = (point1.northing + point2.northing) * 0.5;
                    equation = Line2.DefineByX(xIntercept);
                    return;
                }
                if (dy == 0D)
                {
                    yIntercept = (point1.easting + point2.easting) * 0.5;
                    equation = Line2.DefineByY(yIntercept);
                    return;
                }

                double slope =
                    (point2.easting - point1.easting) /
                    (point2.northing - point1.northing);

                yIntercept =
                point1.easting -
                (slope * point1.northing);

                xIntercept = (0D - yIntercept) / slope;

                equation = new Line2(slope, xIntercept, yIntercept);
            }

            public double
            Length()
            {
                return Math.Sqrt(
                    Math.Pow(point2.northing - point1.northing, 2) +
                    Math.Pow(point2.easting - point1.easting, 2));
            }
        }

        public struct
        Ray2
        {
            public readonly vec2 point;
            public readonly double direction;
            public readonly Line2 equation;

            public
            Ray2(vec2 p_point, double p_direction)
            {
                point = p_point;

                direction = p_direction;
                {
                    const double twoPI = 2 * Math.PI;
                    for (; direction < 0D;) { direction += twoPI; }
                    for (; direction > twoPI;) { direction -= twoPI; }
                }

                if (direction == 0D || direction == Math.PI)
                {
                    equation = Line2.DefineByY(point.easting);
                    return;
                }
                if (direction == (Math.PI * 0.5) ||
                    direction == (Math.PI * 1.5))
                {
                    equation = Line2.DefineByX(point.northing);
                    return;
                }

                double
                slope = Math.Tan(direction),
                yIntercept = point.easting - (slope * point.northing),
                xIntercept = (0D - yIntercept) / slope;

                equation = new Line2(slope, xIntercept, yIntercept);
            }
        }

        private static int
        CheckParallel(Line2 line)
        {
            if (line.slope != 0D && !double.IsNaN(line.slope)) return 0;
            if (line.slope == 0D) return 1;
            if (double.IsNaN(line.slope)) return 2;
            return -1;
        }

        private static int
        IntersectPoint
        (Line2[] equation, out vec2 point)
        {
            int[] orientation = new int[2];
            {
                int[] check = new int[2]
                {
                    CheckParallel(equation[0]),
                    CheckParallel(equation[1])
                };

                if (check[0] == -1 || check[1] == -1)
                {
                    point = new vec2(double.NaN, double.NaN);
                    return -1;
                }

                if (check[0] <= check[1])
                {
                    orientation[0] = check[0];
                    orientation[1] = check[1];
                }
                if (check[1] < check[0])
                {
                    orientation[0] = check[1];
                    orientation[1] = check[0];
                    equation = new Line2[] { equation[1], equation[0] };
                }
            }

            if (orientation[0] != orientation[1])
            {
                if (orientation[0] == 0)
                {
                    if (orientation[1] == 1)
                    {
                        double y = equation[1].yIntercept;
                        double x =
                            (y - equation[0].yIntercept) / equation[0].slope;
                        point = new vec2(y, x);
                        return 1;
                    }
                    if (orientation[1] == 2)
                    {
                        double x = equation[1].xIntercept;
                        double y =
                            (equation[0].slope * x) + equation[0].yIntercept;
                        point = new vec2(y, x);
                        return 1;
                    }
                }
                if (orientation[0] == 1)
                {
                    if (orientation[1] == 2)
                    {
                        double y = equation[0].yIntercept;
                        double x = equation[1].xIntercept;
                        point = new vec2(y, x);
                        return 1;
                    }
                }
            }

            if (orientation[0] != 0 && orientation[1] != 0)
            {
                point = new vec2(double.NaN, double.NaN);
                return 0;
            }

            if (equation[0].slope == equation[1].slope)
            {
                point = new vec2(double.NaN, double.NaN);
                return 0;
            }

            {
                double x =
                    (equation[0].yIntercept - equation[1].yIntercept) /
                    (equation[1].slope - equation[0].slope);
                double y = (equation[0].slope * x) + equation[0].yIntercept;
                point = new vec2(y, x);
                return 1;
            }
        }

        private static int
        BetweenPoints(vec2 refPoint1, vec2 refPoint2, vec2 point)
        {
            /*
             * Credit to Stack Overflow user AKN's answer to:
             * https://stackoverflow.com/questions/42868214/
             * that was the basis for the code below.
             */

            double
            dxl = refPoint2.northing - refPoint1.northing,
            dyl = refPoint2.easting - refPoint1.easting;

            if (Math.Abs(dxl) >= Math.Abs(dyl))
            {
                if (dxl > 0)
                {
                    if (refPoint1.northing <= point.northing
                        && point.northing <= refPoint2.northing) return 1;
                    else return 0;
                }
                else
                {
                    if (refPoint2.northing <= point.northing
                        && point.northing <= refPoint1.northing) return 1;
                    else return 0;
                }
            }
            else
            {
                if (dyl > 0)
                {
                    if (refPoint1.easting <= point.easting
                        && point.easting <= refPoint2.easting) return 1;
                    else return 0;
                }
                else
                {
                    if (refPoint2.easting <= point.easting
                        && point.easting <= refPoint1.easting) return 1;
                    else return 0;
                }
            }
        }

        public static int
        Intersect(Ray2 ray, LineSegment2 line, out vec2 point)
        {
            Line2[] equations = new Line2[2] { ray.equation, line.equation };

            int status;
            if ((status = IntersectPoint
                    (equations, out point)) == 1)
            {
                if ((status = BetweenPoints
                        (line.point1, line.point2, point)) == 1)
                {
                    double direction = Math.Atan2
                        (point.easting - ray.point.easting,
                         point.northing - ray.point.northing);
                    {
                        const double twoPI = 2 * Math.PI;
                        for (; direction < 0D;) { direction += twoPI; }
                        for (; direction > twoPI;) { direction -= twoPI; }
                    }

                    if (Math.Abs(direction - ray.direction) < 1D) { }
                    else
                    {
                        point = new vec2(double.NaN, double.NaN);
                        status = 0;
                    }
                }
            }

            return status;
        }
    }
}
