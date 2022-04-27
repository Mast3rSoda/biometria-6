using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biometria_6;
public enum ClasifyBy
{
    DwellTime = 1,
    FlightTime = 2,
    Both = 3,
}
public static class Algorithm
{
    public static int ManhattanLength(List<int> listA, List<int> listB)
    {
        var listC = new List<int>();
        for (int i = 0; i < listA.Count; i++)
        {
            listC.Add(Math.Abs(listA[i] - listB[i]));
        }

        return Math.Abs(listC.Sum());
    }

    public static List<Measure> AvarageOutMeasures(List<Measure> measures)
    {
        int count = measures.Count;
        var measureList = new List<Measure>();
        for (int i = 0; i < count; i++)
        {
            string keyName = measures[i].KeyName;

            int firstAvg = (int)measures.Where(p => p.KeyName == keyName).Select(p => p.DwellTime).Average();
            int secondAvg = (int)measures.Where(p => p.KeyName == keyName).Select(p => p.FlightTime).Average();

            measureList.Add(new Measure(keyName, firstAvg, secondAvg));

            measureList.RemoveAll(p => p.KeyName == keyName);
        }

        return measureList;
    }
    public static Dictionary<string, int> GetLenghts(Dictionary<string, List<Measure>> dictionary, string mainFileName, ClasifyBy clasifyBy)
    {
        Dictionary<string, int> lengthList = new();
        var mainAvg = AvarageOutMeasures(dictionary[mainFileName]);
        switch (clasifyBy)
        {
            case ClasifyBy.DwellTime:
                {
                    foreach (var measures in dictionary)
                    {
                        if (measures.Key != mainFileName)
                            lengthList.Add(measures.Key, ManhattanLength(dictionary[mainFileName].Select(p => p.DwellTime).ToList(), measures.Value.Select(p => p.DwellTime).ToList()));
                    }
                    break;
                }

            case ClasifyBy.FlightTime:
                {
                    foreach (var measures in dictionary)
                    {
                        if (measures.Key != mainFileName)
                            lengthList.Add(measures.Key, ManhattanLength(dictionary[mainFileName].Select(p => p.FlightTime).ToList(), measures.Value.Select(p => p.FlightTime).ToList()));
                    }
                    break;
                }
            case ClasifyBy.Both:
                {
                    foreach (var measures in dictionary)
                    {
                        if (measures.Key != mainFileName)
                            lengthList.Add(measures.Key, ManhattanLength(dictionary[mainFileName].Select(p => p.FlightTime + p.DwellTime).ToList(), measures.Value.Select(p => p.FlightTime + p.DwellTime).ToList()));
                    }
                    break;
                }
        }

        return lengthList;

    }

    public static Dictionary<string, double> KNN(Dictionary<string, List<Measure>> dictionary, string mainFileName, ClasifyBy clasifyBy, int k=3)
    {
        Dictionary<string, int> lengthList = new();


        lengthList = GetLenghts(dictionary, mainFileName, clasifyBy).OrderBy(p => p.Value).Take(k).ToDictionary(x => x.Key, x => x.Value);

        var most = lengthList.GroupBy(i => i.Key[^8..^6]).OrderByDescending(grp => grp.Count())
                .ToDictionary(x => x.Key, x => x.Select(k=>k.Value).First());

        return most.ToDictionary(x => x.Key, x => 1-(double)x.Value/lengthList.Select(p=>p.Value).ToList().Sum());
    }

    public static Dictionary<string, int> Bayes(Dictionary<string, List<Measure>> dictionary, string mainFileName, ClasifyBy clasifyBy, int k = 3)
    {
        var propability = 3.0/dictionary.Keys.Count;
        Dictionary<string, int> lengthList = new();


        lengthList = GetLenghts(dictionary, mainFileName, clasifyBy).ToDictionary(x => x.Key, x => x.Value);

        Dictionary<string, double> lengthListMean = new();
        string previous = "";
        int counter = 0;
        double meanVal = 0;
        foreach(var item in lengthList)
        {
            if(item.Key == previous)
            {
                meanVal += item.Value;
                counter++;
            }
            else
            {
                lengthListMean.Add(previous, meanVal / counter);
                meanVal = item.Value;
                counter = 1;
                previous = item.Key; //mean values for every user
            }
        }
        //nie rozumiem jak to zrobić
    }





}
