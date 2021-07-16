using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public class Panorama : MonoBehaviour
{
    public static Panorama Instance { set; get; }

    private static int SAMPLE_SECONDS = 1;
    public static int countPoint = 0;
    private static int DEGREE_IN_SECONDS = 60 * 60;
    private static double SPAN = 120.0f;
    private static int WHITE = 255;
    private static int DARKEST = 10;
    private static int BLACK = 0;
    private static double GLOBE = 3959.0f;
    private static double RADIUS = 6371000.0f;
    private static int SAMPLES_PER_ROW = (DEGREE_IN_SECONDS / SAMPLE_SECONDS) + 1;
    private static int DEGREE_IN_SAMPLES = DEGREE_IN_SECONDS / SAMPLE_SECONDS;
    private static int BYTES_PER_ROW = SAMPLES_PER_ROW * 2;
    private static bool OCEANFRONT = false;
    private static double DEGREE_IN_METERS = (double)((RADIUS * 2 * Math.PI) / 360.0);
    private static double SAMPLE_IN_METERS = DEGREE_IN_METERS / (60 * (60 / SAMPLE_SECONDS));
    private static double step = SAMPLE_IN_METERS;
    private static double height;
    private static int image_height = 1080;
    private static int horizon = (int)(image_height / 2);
    private static ConcurrentDictionary<string, byte[]> dict = new ConcurrentDictionary<string, byte[]>(10, 3);
    public static List<Tuple<System.Drawing.Color, int>> colorDepth;
    public static int viewrange;
    public static int width;
    static int lat;
    static int lon;
    static int d_lat;
    static int d_lon;


    public static byte[] getBuffer(float latitude, float longitude, float altitude, float bearingSmartphone)
    {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        colorDepth = new List<Tuple<System.Drawing.Color, int>>();
        height = altitude;
        double north = latitude;
        double east = longitude;
        double bearing = bearingSmartphone;

        viewrange = 25000;

        bearing = (double)ConvertDegreesToRadians(cartesian(bearing));
        double halfspan = (double)ConvertDegreesToRadians(SPAN) / 2;
        double d_bearing = (double)Math.Asin((step) / viewrange);
        double angle = bearing + halfspan;
        int widhtSteps = (int)((Math.Abs(bearing - halfspan) - Math.Abs(angle)) / Math.Abs(d_bearing)) + 1;
        Console.WriteLine("loop: " + widhtSteps);

        List<Tuple<short, int, int>>[] allLook = new List<Tuple<short, int, int>>[widhtSteps];

        Parallel.For(0, widhtSteps, i =>
        {
            double selectAngle = angle - i * Math.Abs(d_bearing);
            List<Tuple<short, int, int>> oneLook = look((double)ConvertRadiansToDegrees(selectAngle), latitude, longitude, viewrange, step, d_bearing);
            allLook[i] = oneLook;
        });


        dict.Clear();
        width = allLook.Length;



        var rect = new Rectangle(0, 0, width, 1080);

        var depthImage = 32 / 8; //bytes per pixel

        var buffer = new byte[width * 1080 * depthImage];

        Parallel.For(0, buffer.Length, (i) =>
        {
            buffer[i] = 255;
        });



        Parallel.For(0, allLook.Length, (i) =>
        {
            List<Tuple<short, int, int>> pointList = new List<Tuple<short, int, int>>();
            int x = i;
            List<Tuple<short, int, int>> t = allLook[i];
            pointList.Add(t.ElementAt(0));
            pointList.AddRange(t);
            pointList.Add(new Tuple<short, int, int>(0, 0, image_height));
            for (int depth = pointList.Count - 3; depth > 0; depth--)
            {

                System.Drawing.Color ridgecolor;
                System.Drawing.Color color;
                List<Tuple<short, int, int>> context = pointList.GetRange(depth - 1, 4);
                if (context.ElementAt(1).Item3 == context.ElementAt(0).Item3)
                    pointList[depth] = pointList[depth + 1];
                else if (context.ElementAt(1).Item2 > context.ElementAt(0).Item2)
                {
                    if (OCEANFRONT && (context.ElementAt(1).Item1 == 0))
                        ridgecolor = color = System.Drawing.Color.Black;

                    else
                    {
                        bool testColor = false;

                        if (testColor)
                        {
                            float divider = pointList.Count / (float)(WHITE - DARKEST);
                            int gray = (int)(depth / divider) + DARKEST;

                            //Console.WriteLine("gray: " + gray);
                            color = System.Drawing.Color.FromArgb(gray, gray, gray, gray);

                            Tuple<System.Drawing.Color, int> temp = new Tuple<System.Drawing.Color, int>(color, depth);


                            //on stocke chaque couleur unique avec sa depth de référence
                            if (!colorDepth.Contains(temp))
                            {
                                colorDepth.Add(temp);
                            }

                        }
                        else
                        {
                            float code = ((float)depth / (float)pointList.Count) * 360.0f;


                            color = newHSVtoRGB(code, 1.0f, 1.0f, 1.0f);//Color.FromArgb(gray, gray, gray, gray);


                            Tuple<System.Drawing.Color, int> temp = new Tuple<System.Drawing.Color, int>(color, depth);

                            //on stocke chaque couleur unique avec sa depth de référence
                            if (!colorDepth.Contains(temp))
                            {

                                colorDepth.Add(temp);
                            }
                        }

                           ridgecolor = System.Drawing.Color.Black;

                    }
                    int y = (int)Math.Max(0, context.ElementAt(1).Item3);

                    if (y < context.ElementAt(2).Item3)
                        if (y < image_height)
                        {
                            var offset = (y * width + x) * depthImage;
                            buffer[offset] = ridgecolor.R;
                            buffer[offset + 1] = ridgecolor.G;
                            buffer[offset + 2] = ridgecolor.B;


                        }

                        else if ((y > context.ElementAt(2).Item3) /*|| testPix(x, y, ridgecolor, image)*/)
                            if (y < image_height)
                            {
                                var offset = (y * width + x) * depthImage;
                                buffer[offset] = color.R;
                                buffer[offset + 1] = color.G;
                                buffer[offset + 2] = color.B;
                            }
                    foreach (int plot in Enumerable.Range(y + 1, context.ElementAt(0).Item3 + 1))
                    {

                        if (plot < image_height)
                        {
                            var offset = (plot * width + x) * depthImage;
                            buffer[offset] = color.R;
                            buffer[offset + 1] = color.G;
                            buffer[offset + 2] = color.B;
                        }
                    }

                }

            }
        });





        stopWatch.Stop();
        Console.WriteLine("RunTime " + stopWatch.ElapsedMilliseconds);

        Array.Reverse(buffer, 0, buffer.Length);


        return buffer;
    }


    private static double cartesian(double bearing)
    {
        double converted = (90 - bearing) % 360;
        if (converted > 180)
            converted -= 360;
        else if (converted < -180)
            converted += 360;
        return converted;
    }

    private static byte[] getHgtFile(double north, double east)
    {
        string filebase;

        int latitude = (int)north;
        int longitude = (int)east;



        if (latitude < 0)
        {
            filebase = "S" + (Math.Abs(latitude) + 1).ToString("00");
            lat = latitude;
            d_lat = SAMPLE_SECONDS;
        }
        else
        {
            filebase = "N" + (latitude).ToString("00");
            lat = latitude + 1;
            d_lat = -SAMPLE_SECONDS;
        }
        if (longitude < 0)
        {
            filebase += "W" + Math.Abs(longitude - 1).ToString("000") + ".hgt";
            lon = longitude - 1;
        }
        else
        {
            filebase += "E" + longitude.ToString("000") + ".hgt";
            lon = longitude;
        }
        d_lon = SAMPLE_SECONDS;

        if (!dict.ContainsKey(filebase))
        {

            var b = importFile(filebase);
            dict.TryAdd(filebase, b);

        }
        byte[] c;
        dict.TryGetValue(filebase, out c);
        return c;
    }


    public static byte[] importFile(string file)
    {
        byte[] results = new byte[32768];

        results = BetterStreamingAssets.ReadAllBytes(file);





        return results;


    }



    public static short getHeight(double north, double east)
    {
        byte[] b = getHgtFile(north, east);
        double Nmin = getDMinute(north);
        double Emin = getDMinute(east);



        int offset = north_offset((int)Nmin, (int)getDSecond(Nmin), d_lat) + east_offset((int)Emin, (int)getDSecond(Emin), d_lon);
        byte[] buffer = new byte[2];

        buffer[0] = b[offset + 1];
        buffer[1] = b[offset];
        short ret = BitConverter.ToInt16(buffer, 0);

        return ret;
    }

    public static double getDMinute(double Ddegree)
    {
        return (Ddegree - (int)(Ddegree)) * 60;
    }

    public static double getDSecond(double Dmin)
    {
        return ((Dmin - (int)Dmin) * 60);
    }

    public static int north_offset(int northM, int northS, double direction)
    {
        int row = (int)(Math.Abs((northM * 60) + northS) / Math.Abs(direction));
        if (CopySign(1, direction) != CopySign(1, northM))
            row = SAMPLES_PER_ROW - row - 1;
        return row * BYTES_PER_ROW;
    }

    public static int east_offset(int eastM, int eastS, double direction)
    {
        int offest = (int)Math.Abs((eastM * 60 + eastS) / Math.Abs(direction));
        if (CopySign(1, direction) != CopySign(1, eastM))
            offest = SAMPLES_PER_ROW - offest - 1;
        return offest * 2;
    }

    public static double ConvertDegreesToRadians(double degrees)
    {
        double radians = (Math.PI / 180) * degrees;
        return (radians);
    }

    public static double ConvertRadiansToDegrees(double radians) { double degrees = (180 / Math.PI) * radians; return (degrees); }
    public static List<Tuple<short, int, int>> look(double angle, double north, double east, int distance, double d_travel, double d_bearing)
    {
        List<Tuple<short, int, int>> elevations = new List<Tuple<short, int, int>>();
        double traversed = 0;
        double radian = (double)ConvertDegreesToRadians(angle);
        int count = 0;
        while (traversed < distance)
        {
            short elevation = getHeight(north, east);
            if (elevation < 0)
            {
                Console.WriteLine("elevation : " + elevation);
            }
            double CorrElevation = elevation - height;
            double theta = Math.Atan(CorrElevation / (step * count));

            int projected = (int)(Math.Round(theta / Math.Abs(d_bearing)));
            int proj2 = horizon - 1 - projected;

            Tuple<short, int, int> tu = new Tuple<short, int, int>(elevation, projected, proj2);
            elevations.Add(tu);
            Tuple<double, double> t = move(north, east, angle, d_travel, false);
            north = t.Item1;
            east = t.Item2;
            traversed += d_travel;
            count++;



        }
        return elevations;
    }
    public static double CopySign(double num1, double num2)
    {

        if (num2 < 0)
            return num1 * -1.0;
        else
            return num1;
    }
    public static Tuple<double, double> move(double north, double east, double angle, double d_travel, bool bearing)
    {
        double radian = (double)ConvertDegreesToRadians(angle);

        double distance_x = (double)Math.Sin(radian) * (d_travel / DEGREE_IN_METERS);
        double distance_y = (double)Math.Cos(radian) * (d_travel / DEGREE_IN_METERS);


        return (Tuple.Create(north + distance_x, east + distance_y));

    }

    public static System.Drawing.Color HSVtoRGB(float hue, float saturation, float value, float alpha)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60 - Math.Floor(hue / 60);

        value = value * 255;
        int v = Convert.ToInt32(value);
        int p = Convert.ToInt32(value * (1 - saturation));
        int q = Convert.ToInt32(value * (1 - f * saturation));
        int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        if (hi == 0)
            return System.Drawing.Color.FromArgb(255, v, t, p);
        else if (hi == 1)
            return System.Drawing.Color.FromArgb(255, q, v, p);
        else if (hi == 2)
            return System.Drawing.Color.FromArgb(255, p, v, t);
        else if (hi == 3)
            return System.Drawing.Color.FromArgb(255, p, q, v);
        else if (hi == 4)
            return System.Drawing.Color.FromArgb(255, t, p, v);
        else
            return System.Drawing.Color.FromArgb(255, v, p, q);
    }

    public static System.Drawing.Color newHSVtoRGB(float hue, float saturation, float value, float alpha)
    {
        double r = 0, g = 0, b = 0;

        if (saturation == 0)
        {
            r = value;
            g = value;
            b = value;
        }
        else
        {
            int i;
            double f, p, q, t;

            if (hue == 360)
                hue = 0;
            else
                hue = hue / 60;

            i = (int)Math.Truncate(hue);
            f = hue - i;

            p = value * (1.0 - saturation);
            q = value * (1.0 - (saturation * f));
            t = value * (1.0 - (saturation * (1.0 - f)));

            switch (i)
            {
                case 0:
                    r = value;
                    g = t;
                    b = p;
                    break;

                case 1:
                    r = q;
                    g = value;
                    b = p;
                    break;

                case 2:
                    r = p;
                    g = value;
                    b = t;
                    break;

                case 3:
                    r = p;
                    g = q;
                    b = value;
                    break;

                case 4:
                    r = t;
                    g = p;
                    b = value;
                    break;

                default:
                    r = value;
                    g = p;
                    b = q;
                    break;
            }

        }



        return System.Drawing.Color.FromArgb(255, (int)(r * 255), (int)(g * 255), (int)(b * 255));
    }

}

