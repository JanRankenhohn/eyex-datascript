using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsvApp
{
    class Program
    {
        private static string path = "";
        private static string pathForFixationList = "";
        private static string fileName = "";
        private static int startTime = 0;
        private static int errorFoundTime = 0;
        private static List<int[]> errorFoundList = new List<int[]>();

        static void Main(string[] args)
        {
            // DEFINE PATH FOR CSV DATA:

            //pathForFixationList = @"C:\Users\janra\.gradle\caches\modules-2\files-2.1\com.jetbrains.intellij.idea\ideaIC\2020.2\4fe93bb81525f2fa7a6f0fd7ba41c3b9cce9e8b6\ideaIC-2020.2\bin\t1\gazedata_code_p7_1.csv";

            // Hardcoded TODO: read defect detection times from csv
            errorFoundList.Add(new int[] { 53979, 61069, 47100, 93387 });
            errorFoundList.Add(new int[] { 282297, 0, 0, 0 });
            errorFoundList.Add(new int[] { 0,0,0,174745 });
            errorFoundList.Add(new int[] { 161909,79751,157800,206464 });
            errorFoundList.Add(new int[] { 259695,71280,95946,344329 });
            errorFoundList.Add(new int[] { 97081,73670,114886,116207 });
            errorFoundList.Add(new int[] { 137970,0,171644,78445 });
            errorFoundList.Add(new int[] { 134951,48104,60127,64171 });
            errorFoundList.Add(new int[] { 0,0,0,245985 });
            errorFoundList.Add(new int[] { 142334,68800,38336,203620 });
            errorFoundList.Add(new int[] { 240591,0,113179,109964 });
            errorFoundList.Add(new int[] { 80196,38059,66774,131408 });
            errorFoundList.Add(new int[] { 0,100505,0,261542 });
            errorFoundList.Add(new int[] { 190047,188641,138308,150525 });

            //computeFixationList();

            // List for relevant code lines of stimuli
            List<List<int>> trialList = new List<List<int>>(); 
            trialList.Add(new List<int>() { 4,5,6,7,9,10,11,12,14,17,18,20,21,22,23 });
            trialList.Add(new List<int>() { 5,6,7,9,10,11,12,13,14,17 });
            trialList.Add(new List<int>() { 5,6,7,8,10,12,13,14,15,16,17,20,21 });
            trialList.Add(new List<int>() { 5,6,7,9,10,12,13,14,15,16,19,20,21,22 });
            int[] limiterList = new int[] {13,8,11,12 };

            // List for defect Code Lines for stimuli
            List<List<int>> defectLinesList = new List<List<int>>();
            defectLinesList.Add(new List<int> { 10 });
            defectLinesList.Add(new List<int> { 7,13 });
            defectLinesList.Add(new List<int> { 20 });
            defectLinesList.Add(new List<int> { 15 });

            // List for Averages
            var scanTimeList = new List<int>();
            var defectDetectionTimeList = new List<int>();

            //Loop for all 4 trials
            for (int r = 1; r < 5; r++)
                {
                    //path = @"C:\Users\janra\.gradle\caches\modules-2\files-2.1\com.jetbrains.intellij.idea\ideaIC\2020.2\4fe93bb81525f2fa7a6f0fd7ba41c3b9cce9e8b6\ideaIC-2020.2\bin\t" + r + "\\";
                    path = @"data\t" + r + "\\";

                    Console.WriteLine("");
                    Console.WriteLine("Trial " + r + ":");
                    Console.WriteLine("");

                    // Output total Line Count
                    Console.WriteLine("LineCount: " + trialList[r - 1].Count);

                    Console.WriteLine("");

                    // Compute 80% of Lines Count as limiter
                    int limiter = Convert.ToInt32(trialList[r - 1].Count * 0.8);
                    Console.WriteLine("Limiter: " + limiter);

                    // Compute data for all 14 participants
                    for (int i = 1; i < 15; i++)
                    {
                        Console.WriteLine("Participant " + i + ":");
                        fileName = path + "gazedata_code_p" + i + "_" + r + ".csv";
                        errorFoundTime = errorFoundList[i - 1][r - 1];

                        int scanTime = computeScanTime(trialList[r - 1], limiter);

                        int defectDetectionTime = computeDefectDetectionTime(i, r, scanTime);

                        if (defectDetectionTime > 0 && scanTime > 0)
                        {
                            scanTimeList.Add(scanTime);
                            defectDetectionTimeList.Add(defectDetectionTime + scanTime);
                        }

                        int afterPercentageTime = (int)((defectDetectionTime + scanTime) * 0.3);

                        computeAfterPercentageCount(afterPercentageTime, trialList[r - 1]);

                        comupteFixationCountAndDuration(errorFoundTime, defectLinesList[r - 1]);

                        Console.WriteLine("");
                    }
                }

            //Output Averages and normalized Variables
            double scanTimeAverage = scanTimeList.Average();
            double defectDetectionTimeAverage = defectDetectionTimeList.Average();
            Console.WriteLine("");
            Console.WriteLine("Scan Time Average: " + scanTimeAverage);
            Console.WriteLine("Defect Detection Time Average: " + defectDetectionTimeAverage);
            Console.WriteLine("Normalized Scan / Defect Detection Time: ");
            for (int j = 0; j < scanTimeList.Count; j++)
            {
                Console.Write(Math.Round((scanTimeList[j] / scanTimeAverage), 2).ToString().Replace(",", ".") + " ");
                //Console.Write(Math.Round((defectDetectionTimeList[j] / defectDetectionTimeAverage), 2) + " ");
            }
            Console.WriteLine("");
            for (int j = 0; j < scanTimeList.Count; j++)
            {
                //Console.Write(Math.Round((scanTimeList[j] / scanTimeAverage), 2) + " ");
                Console.Write(Math.Round((defectDetectionTimeList[j] / defectDetectionTimeAverage), 2).ToString().Replace(",", ".") + " ");
            }

            Console.ReadKey();
        }

        private static void comupteFixationCountAndDuration(int endTime, List<int> defectLines)
        {
            using (var reader = new StreamReader(fileName))
            {
                int fixationCout = 0;
                int relevantFixationCount = 0;
                List<int> fixationDurationList = new List<int>();
                List<int> relevantFixationDurationList = new List<int>();

                while (!reader.EndOfStream)
                {
                    try
                    {
                        var line = reader.ReadLine().Replace("\"", "");
                        var values = line.Split(',');

                        int codeLine = Int32.Parse(values[1]);
                        int timestamp = Int32.Parse(values[2]);
                        int duration = Int32.Parse(values[3]);

                        if (timestamp < endTime)
                        {
                            fixationCout++;
                            fixationDurationList.Add(duration);
                            if (defectLines.Contains(codeLine))
                            {
                                relevantFixationCount++;
                                relevantFixationDurationList.Add(duration);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        // pass
                    }
                }
                try
                {
                    Console.WriteLine("Fixation Count: " + fixationCout);
                    Console.WriteLine("Relevant Fixation Count: " + relevantFixationCount);

                    Console.WriteLine("Fixation Duration: " + fixationDurationList.Average());
                    Console.WriteLine("Relevant Fixation Duration: " + relevantFixationDurationList.Average());
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Fixation Duration: NA");
                    Console.WriteLine("Relevant Fixation Duration: NA");
                    // pass
                }
            }
        }

        private static void computeAfterPercentageCount(int afterPercentageTime, List<int> relevantLines)
        {
            List<int> savedLines = new List<int>();

            using (var reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    try
                    {
                        var line = reader.ReadLine().Replace("\"", "");
                        var values = line.Split(',');

                        int codeLine = Int32.Parse(values[1]);
                        int timestamp = Int32.Parse(values[2]);

                        if (relevantLines.Contains(codeLine))
                        {
                            if (!savedLines.Contains(codeLine))
                            {
                                savedLines.Add(codeLine);
                            }
                        }

                        if ((timestamp - startTime) > afterPercentageTime)
                        {
                            Console.WriteLine("Line Numbers at 30 percent: " + savedLines.Count);
                            break;
                        }
                    }
                    catch(FormatException ex)
                    {
                        // skip line
                    }
                    
                }
            }
        }

        private static int computeDefectDetectionTime(int p, int t, int scanTime)
        {
            int defectDetectionTime = errorFoundTime - scanTime - startTime;
            int defectDetectionTimeWithST = errorFoundTime - startTime;
            Console.WriteLine("DefectDetection Time: " + defectDetectionTime);
            Console.WriteLine("DefectDetection Time with ST: " + defectDetectionTimeWithST);
            return defectDetectionTime;
        }

        private static int computeScanTime(List<int> relevantLines, int limiter)
        {
            List<int> savedLines = new List<int>();

            int scanTime = 0;

            using (var reader = new StreamReader(fileName))
            {
                bool startOfStream = true;
                bool scanTimeFound = false;

                while (!reader.EndOfStream)
                {
                    try{
                        var line = reader.ReadLine().Replace("\"", "");
                        var values = line.Split(',');

                        int codeLine = Int32.Parse(values[1]);

                        if (startOfStream)
                        {
                            startTime = Int32.Parse(values[2]);
                            startOfStream = false;
                        }

                        if (relevantLines.Contains(codeLine))
                        {
                            if (!savedLines.Contains(codeLine))
                            {
                                savedLines.Add(codeLine);
                            }
                        }

                        if (savedLines.Count == limiter && !scanTimeFound)
                        {
                            scanTime = Int32.Parse(values[2]) - startTime;
                            scanTimeFound = true;
                        }
                    }
                    catch(FormatException ex)
                    {
                        // skip line
                    }
                }
            }

            Console.WriteLine("Scan Time: " + scanTime);
            return scanTime;
        }

        private static void computeFixationList()
        {
            List<int[]> valueList = new List<int[]>();
            int duration = 0;
            List<int> resultList = new List<int>();

            using (var reader = new StreamReader(pathForFixationList))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Replace("\"", "");
                    var values = line.Split(',');

                    try
                    {
                        valueList.Add(new int[] { Int32.Parse(values[1]), Int32.Parse(values[2]) });
                    }
                    catch(Exception ex)
                    {
                        //pass
                    }
                    if (reader.EndOfStream)
                    {
                        duration = (int)(Int32.Parse(values[2])) + 1000;
                    }
                }
            }

            Console.WriteLine("Seconds: ");

            int previousLine = 0;
            int i = 0;
            while (i < duration)
            {
                double d = (double)i;
                Console.Write(d / 1000 + " ");
                i = i + 500;
                bool match = false;
                foreach (int[] v in valueList)
                {
                    int line = v[0];
                    int timestamp = v[1];

                    if (timestamp > (i - 500) && timestamp < i)
                    {
                        match = true;
                        previousLine = line;
                    }
                    else
                    {
                        if (match)
                        {
                            resultList.Add(previousLine);
                            break;
                        }
                    }
                }
                if (!match)
                {
                    resultList.Add(previousLine);
                }
            }

            Console.WriteLine("Values: ");

            foreach (var x in resultList)
            {
                Console.Write(x + " ");
            }
        }
    }
}
