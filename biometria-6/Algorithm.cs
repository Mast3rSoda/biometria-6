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

public enum DistanceType
{
    Manhattan = 1,
    Eucledan = 2,
    Chebyshev = 3,
    ChiSquare = 4,
}
public static class Algorithm
{
    public static int CalcDistance(List<int> listA, List<int> listB, DistanceType distance)
    {
        switch (distance)
        {
            case DistanceType.Manhattan:
                return ManhattanLength(listA, listB);
            case DistanceType.Eucledan:
                return EucledanLength(listA, listB);
            case DistanceType.Chebyshev:
                return Chebyshev(listA, listB);
            case DistanceType.ChiSquare:
                return ChiSquare(listA,listB);
            default:
                return ManhattanLength(listA, listB);
        }
    }
    public static int ManhattanLength(List<int> listA, List<int> listB)
    {
        var listC = new List<int>();
        for (int i = 0; i < listA.Count; i++)
        {
            listC.Add(Math.Abs(listA[i] - listB[i]));
        }

        return Math.Abs(listC.Sum());
    }

    public static int EucledanLength(List<int> listA, List<int> listB)
    {
        var listC = new List<int>();
        for (int i = 0; i < listA.Count; i++)
        {
            listC.Add((int)Math.Sqrt(listA[i] * listA[i] + listB[i] * listB[i]));
        }

        return Math.Abs(listC.Sum());
    }
    public static int Chebyshev(List<int> listA, List<int> listB)
    {
        var listC = new List<int>();
        for (int i = 0; i < listA.Count; i++)
        {
            listC.Add(Math.Max(listA[i], listB[i]));
        }

        return Math.Abs(listC.Sum());
    }
    public static int ChiSquare(List<int> listA, List<int> listB)
    {
        int sum = 0;
        for (int i = 0; i < listA.Count; i++)
        {
                var div = listA[i] + listB[i];
                if (div == 0) continue;
                sum += (listA[i] - listB[i]) * (listA[i] - listB[i]) / div;
        }

            return sum / 2;
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
    public static Dictionary<string, int> GetLenghts(Dictionary<string, List<Measure>> dictionary, string mainFileName, ClasifyBy clasifyBy, DistanceType distance)
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
                            lengthList.Add(measures.Key, CalcDistance(dictionary[mainFileName].Select(p => p.DwellTime).ToList(), measures.Value.Select(p => p.DwellTime).ToList(), distance));
                    }
                    break;
                }

            case ClasifyBy.FlightTime:
                {
                    foreach (var measures in dictionary)
                    {
                        if (measures.Key != mainFileName)
                            lengthList.Add(measures.Key, CalcDistance(dictionary[mainFileName].Select(p => p.FlightTime).ToList(), measures.Value.Select(p => p.FlightTime).ToList(),distance));
                    }
                    break;
                }
            case ClasifyBy.Both:
                {
                    foreach (var measures in dictionary)
                    {
                        if (measures.Key != mainFileName)
                            lengthList.Add(measures.Key, CalcDistance(dictionary[mainFileName].Select(p => p.FlightTime + p.DwellTime).ToList(), measures.Value.Select(p => p.FlightTime + p.DwellTime).ToList(),distance));
                    }
                    break;
                }
        }

        return lengthList;

    }

    public static Dictionary<string, double> KNN(Dictionary<string, List<Measure>> dictionary, string mainFileName, ClasifyBy clasifyBy, DistanceType distance, int k = 3 )
    {
        Dictionary<string, int> lengthList = new();


        lengthList = GetLenghts(dictionary, mainFileName, clasifyBy, distance).OrderBy(p => p.Value).Take(k).ToDictionary(x => x.Key, x => x.Value);

        var most = lengthList.GroupBy(i => i.Key[^8..^6]).OrderByDescending(grp => grp.Count())
                .ToDictionary(x => x.Key, x => x.Select(k => k.Value).First());

        return most.ToDictionary(x => x.Key, x => 1 - (double)x.Value / lengthList.Select(p => p.Value).ToList().Sum());
    }

    public static Dictionary<string, KeyValuePair<double, double>> Bayes(Dictionary<string, List<Measure>> dictionary, string mainFileName, ClasifyBy clasifyBy, int k)
    {
        Dictionary<string, int> lengthListMean = new();
        Dictionary<string, double> lengthListVariance = new();
        Dictionary<string, KeyValuePair<double, double>> resultList = new();
        List<KeyValuePair<string, KeyValuePair<double, double>>> sortedList = new();
        lengthListMean = GetMean(dictionary, mainFileName, clasifyBy);
        lengthListVariance = GetVariance(dictionary, lengthListMean, mainFileName, clasifyBy);
        double prob = 1.0 / ((double)dictionary.Count - 1);
        foreach (var item in dictionary)
        {
            if (item.Key != mainFileName)
            {
                double result = 1.0;
                double exponent = 0;
                foreach (var val in item.Value)
                {
                    double value = 1.0;
                    switch (clasifyBy)
                    {
                        case ClasifyBy.DwellTime:
                            {
                                value *= Math.Pow((double)val.DwellTime - (double)lengthListMean[val.KeyName], 2.0);
                                value *= -1.0;
                                value /= Math.Pow((double)lengthListVariance[val.KeyName], 2.0) * 2;
                                value = Math.Exp(value);
                                value /= Math.Sqrt(Math.Pow((double)lengthListVariance[val.KeyName], 2.0) * Math.PI * 2);
                                break;
                            }
                        case ClasifyBy.FlightTime:
                            {
                                value *= Math.Pow((double)val.FlightTime - (double)lengthListMean[val.KeyName], 2.0);
                                value *= -1.0;
                                value /= Math.Pow((double)lengthListVariance[val.KeyName], 2.0) * 2;
                                value = Math.Exp(value);
                                value /= Math.Sqrt(Math.Pow((double)lengthListVariance[val.KeyName], 2.0) * Math.PI * 2);
                                break;
                            }
                        case ClasifyBy.Both:
                            {
                                value *= Math.Pow(((double)val.DwellTime + (double)val.FlightTime) - (double)lengthListMean[val.KeyName], 2.0);
                                value *= -1.0;
                                value /= Math.Pow((double)lengthListVariance[val.KeyName], 2.0) * 2;
                                value = Math.Exp(value);
                                value /= Math.Sqrt(Math.Pow((double)lengthListVariance[val.KeyName], 2.0) * Math.PI * 2);
                                break;
                            }
                    }
                    result *= (double)value;


                    while (result < 1.0)
                    {
                        result *= 10.0;
                        ++exponent;
                    }
                }
                result *= (double)prob;
                if (result < 0.1)
                {
                    result *= 10.0;
                    ++exponent;
                }

                resultList.Add(item.Key.Substring(item.Key.IndexOf('#') + 1, 4), new KeyValuePair<double, double>(result, exponent));
            }
        }
        sortedList = resultList.OrderByDescending(val => val.Value.Value).ThenBy(val => val.Value.Key).ToList();
        resultList.Clear();
        for (int i = 0; i < k; i++)
        {
            resultList.Add(sortedList[i].Key, sortedList[i].Value);
        }

        return resultList;

    }

    public static Dictionary<string, int> GetMean(Dictionary<string, List<Measure>> dictionary, string fileName, ClasifyBy clasifyBy)
    {
        Dictionary<string, int> lengthList = new();
        foreach (var item in dictionary)
        {
            if (item.Key != fileName)
                foreach (var val in item.Value)
                {
                    switch (clasifyBy)
                    {
                        case ClasifyBy.DwellTime:
                            {
                                if (!lengthList.ContainsKey(val.KeyName))
                                {
                                    lengthList.Add(val.KeyName, val.DwellTime);
                                    break;
                                }
                                lengthList[val.KeyName] += val.DwellTime;
                                break;
                            }
                        case ClasifyBy.FlightTime:
                            {

                                if (!lengthList.ContainsKey(val.KeyName))
                                {
                                    lengthList.Add(val.KeyName, val.FlightTime);
                                    break;
                                }
                                lengthList[val.KeyName] += val.FlightTime;
                                break;
                            }
                        case ClasifyBy.Both:
                            {
                                if (!lengthList.ContainsKey(val.KeyName))
                                {
                                    lengthList.Add(val.KeyName, val.DwellTime + val.FlightTime);
                                    break;
                                }
                                lengthList[val.KeyName] += val.DwellTime + val.FlightTime;
                                break;
                            }
                    }

                }
        }
        foreach (var item in lengthList)
        {
            lengthList[item.Key] /= dictionary.Count;
        }
        return lengthList;
    }

    public static Dictionary<string, double> GetVariance(Dictionary<string, List<Measure>> dictionary, Dictionary<string, int> meanValues, string fileName, ClasifyBy clasifyBy)
    {
        var varianceValues = new Dictionary<string, double>();
        var count = new Dictionary<string, double>();

        foreach (var item in dictionary)
        {
            if (item.Key != fileName)
                foreach (var val in item.Value)
                {
                    switch (clasifyBy)
                    {
                        case ClasifyBy.DwellTime:
                            {
                                if (!varianceValues.ContainsKey(val.KeyName))
                                {
                                    varianceValues.Add(val.KeyName, Math.Pow(((double)val.DwellTime - (double)meanValues[val.KeyName]), 2.0));
                                    count.Add(val.KeyName, 1.0);
                                    break;
                                }
                                varianceValues[val.KeyName] += Math.Pow(((double)val.DwellTime - (double)meanValues[val.KeyName]), 2.0);
                                count[val.KeyName] += 1.0;
                                break;
                            }
                        case ClasifyBy.FlightTime:
                            {
                                if (!varianceValues.ContainsKey(val.KeyName))
                                {
                                    varianceValues.Add(val.KeyName, Math.Pow(((double)val.FlightTime - (double)meanValues[val.KeyName]), 2.0));
                                    count.Add(val.KeyName, 1.0);
                                    break;
                                }
                                varianceValues[val.KeyName] += Math.Pow(((double)val.FlightTime - (double)meanValues[val.KeyName]), 2.0);
                                count[val.KeyName] += 1.0;
                                break;
                            }
                        case ClasifyBy.Both:
                            {
                                if (!varianceValues.ContainsKey(val.KeyName))
                                {
                                    varianceValues.Add(val.KeyName, Math.Pow(((double)val.DwellTime + (double)val.FlightTime - (double)meanValues[val.KeyName]), 2.0));
                                    count.Add(val.KeyName, 1.0);
                                    break;
                                }
                                varianceValues[val.KeyName] += Math.Pow(((double)val.DwellTime + (double)val.FlightTime - (double)meanValues[val.KeyName]), 2.0);
                                count[val.KeyName] += 1.0;
                                break;
                            }
                    }
                }
        }
        foreach (var val in varianceValues)
        {
            varianceValues[val.Key] /= (double)count[val.Key];
        }
        return varianceValues;

    }
}
