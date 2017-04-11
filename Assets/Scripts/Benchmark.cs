using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

class Benchmark
{
    private static List<GroupedTestResult> accumulatedResults;

    public static List<TestCase> LoadTestCases(string MapName)
    {
        LoadResults(MapName);

        string BaseTestDirectory = GetBaseTestDirectory();

        FileInfo f = new FileInfo(Path.Combine(BaseTestDirectory, string.Format("{0}.scen", MapName)));

        return ReadTestCases(f);
    }

    public static void LoadResults(string MapName)
    {
        string BaseResultDirectory = GetBaseResultsDirectory();
        accumulatedResults = new List<GroupedTestResult>();

        FileInfo f = new FileInfo(Path.Combine(BaseResultDirectory, string.Format("{0}.csv", MapName)));

        if (f.Exists)
        {
            string line;
            string[] blocks;
            //LoadData
            using (FileStream fs = f.OpenRead())
            using (StreamReader sr = new StreamReader(fs))
            {
                line = sr.ReadLine();   //First line is header
                line = sr.ReadLine();
                while (!String.IsNullOrEmpty(line))
                {
                    blocks = line.Split(',');
                    accumulatedResults.Add(new GroupedTestResult()
                    {
                        GroupingNumber = int.Parse(blocks[0]),
                        ClusterSize = int.Parse(blocks[1]),
                        Layers = int.Parse(blocks[2]),
                        Count = int.Parse(blocks[3]),

                        AvgClusterGenerateTime = float.Parse(blocks[4]),
                        AvgAStarRunningTime = float.Parse(blocks[5]),
                        AvgAStarPathLength = float.Parse(blocks[6]),
                        AvgHPARunningTime = float.Parse(blocks[7]),
                        AvgHPAPathLength = float.Parse(blocks[8])
                    });

                    line = sr.ReadLine();
                }
            }
        }
    }

    public static void WriteResults(TestResults testResults)
    {
        var groupedNumberRes = testResults.results.GroupBy((TestResult tr) => tr.GroupingNumber).ToList();
        GroupedTestResult groupTests;
        Averages avgs;
        int count;

        foreach (IGrouping<int, TestResult> group in groupedNumberRes)
        {
            avgs = new Averages();
            count = 0;
            //Accumulate the average of this group
            foreach (TestResult t in group)
            {
                avgs.AvgAStarPathLength += t.AStarResult.PathLength;
                avgs.AvgAStarRunningTime += t.AStarResult.RunningTime;
                avgs.AvgHPAPathLength += t.HPAStarResult.PathLength;
                avgs.AvgHPARunningTime += t.HPAStarResult.RunningTime;
                count++;
            }
            //Average
            avgs.AvgAStarPathLength /= count;
            avgs.AvgAStarRunningTime /= count;
            avgs.AvgHPAPathLength /= count;
            avgs.AvgHPARunningTime /= count;

            //Add to current groupdresult if exists
            groupTests = accumulatedResults.Find((GroupedTestResult g) =>
                g.GroupingNumber == group.Key &&
                g.ClusterSize == testResults.ClusterSize &&
                g.Layers == testResults.Layers);

            if (groupTests != null)
            {
                //Accumulate results in this entry
                int newCount = groupTests.Count + count;
                float oldRatio = (float)groupTests.Count / newCount;
                float newRatio = (float)count / newCount;

                groupTests.AvgClusterGenerateTime = (oldRatio * groupTests.AvgClusterGenerateTime) + (newRatio * testResults.GenerateClusterTime);
                groupTests.AvgAStarPathLength = (oldRatio * groupTests.AvgAStarPathLength) + (newRatio * avgs.AvgAStarPathLength);
                groupTests.AvgAStarRunningTime = (oldRatio * groupTests.AvgAStarRunningTime) + (newRatio * avgs.AvgAStarRunningTime);
                groupTests.AvgHPAPathLength = (oldRatio * groupTests.AvgHPAPathLength) + (newRatio * avgs.AvgHPAPathLength);
                groupTests.AvgHPARunningTime = (oldRatio * groupTests.AvgHPARunningTime) + (newRatio * avgs.AvgHPARunningTime);
            } else {
                //Add new entry
                groupTests = new GroupedTestResult()
                {
                    GroupingNumber = group.Key,
                    ClusterSize = testResults.ClusterSize,
                    Layers = testResults.Layers,
                    AvgClusterGenerateTime = testResults.GenerateClusterTime,
                    Count = count,

                    AvgAStarPathLength = avgs.AvgAStarPathLength,
                    AvgAStarRunningTime = avgs.AvgAStarRunningTime,
                    AvgHPAPathLength = avgs.AvgHPAPathLength,
                    AvgHPARunningTime = avgs.AvgHPARunningTime
                };

                accumulatedResults.Add(groupTests);
            }
        }

        //Rewrite in file
        string path = Path.Combine(GetBaseResultsDirectory(), string.Format("{0}.csv", testResults.MapName));
        using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
        using (StreamWriter sw = new StreamWriter(fs))
        {
            //Write header
            sw.WriteLine("Group No., Cluster Size, Layers, Count, Avg Cluster Gen. Time, " +
                "Avg A* Run. Time, Avg A* Path Length, Avg HPA* Run. Time, Avg HPA* Path Length");

            foreach (GroupedTestResult g in accumulatedResults)
            {
                sw.WriteLine(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                    g.GroupingNumber,
                    g.ClusterSize,
                    g.Layers,
                    g.Count,
                    g.AvgClusterGenerateTime,
                    g.AvgAStarRunningTime,
                    g.AvgAStarPathLength,
                    g.AvgHPARunningTime,
                    g.AvgHPAPathLength)
                );
            }
        }
    }


    private static List<TestCase> ReadTestCases(FileInfo f)
    {
        List<TestCase> tests = new List<TestCase>();
        TestCase test;
        string line;

        using (FileStream fs = f.OpenRead())
        using (StreamReader sr = new StreamReader(fs))
        {
            float vers = ReadFloatValue(sr, "version");

            line = sr.ReadLine();
            string[] blocks;
            while (!string.IsNullOrEmpty(line))
            {
                blocks = line.Split(null);

                test = new TestCase()
                {
                    GroupingNumber = int.Parse(blocks[0]),
                    Start = new GridTile(
                        int.Parse(blocks[4]),
                        int.Parse(blocks[5])),
                    destination = new GridTile(
                        int.Parse(blocks[6]),
                        int.Parse(blocks[7])),
                    OptimalLength = float.Parse(blocks[8])
                };
                tests.Add(test);

                line = sr.ReadLine();
            }
        }

        return tests;
    }

    private static float ReadFloatValue(StreamReader sr, string key)
    {
        string[] block = sr.ReadLine().Split(null);
        if (block[0] != key) throw new Exception(
                 string.Format("Invalid format. Expected: {0}, Actual: {1}", key, block[0]));

        return float.Parse(block[1]);
    }

    private static string GetBaseTestDirectory()
    {
        return Path.Combine(Application.dataPath, "../Maps/scen");
    }

    private static string GetBaseResultsDirectory()
    {
        return Path.Combine(Application.dataPath, "../Maps/results");
    }
}

class TestCase
{
    public GridTile Start;
    public GridTile destination;
    public float OptimalLength;
    public int GroupingNumber;
}


class TestResults
{
    public TestResults()
    {
        results = new List<TestResult>();
    }

    public string MapName;
    public int ClusterSize;
    public int Layers;
    public float GenerateClusterTime;

    public List<TestResult> results;
}

class GroupedTestResult
{
    //Those are the grouping key
    public int GroupingNumber;
    public int ClusterSize;
    public int Layers;

    //Those are the grouped results
    public int Count;

    public float AvgClusterGenerateTime;

    public float AvgAStarRunningTime;
    public float AvgAStarPathLength;

    public float AvgHPARunningTime;
    public float AvgHPAPathLength;
}

class Averages
{
    public float AvgAStarRunningTime;
    public float AvgAStarPathLength;

    public float AvgHPARunningTime;
    public float AvgHPAPathLength;
}

class TestResult
{
    public int GroupingNumber;
    public PathfindResult AStarResult;
    public PathfindResult HPAStarResult;
}

class PathfindResult
{
    public LinkedList<Edge> Path;
    public float RunningTime;
    public float PathLength;

    //Calculate Path length from the path inside the object
    public void CalculatePathLength()
    {
        if (Path == null)
            throw new InvalidOperationException("Path must be set to call this function");

        PathLength = 0;
        LinkedListNode<Edge> current = Path.First;
        while (current != null)
        {
            PathLength += current.Value.weight;
            current = current.Next;
        }
    }
}