using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biometria_6;

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
        for(int i = 0; i<count; i++)
        {
            string keyName = measures[i].KeyName;

            int firstAvg = (int)measures.Where(p=>p.KeyName==keyName).Select(p=>p.DwellTime).Average();
            int secondAvg = (int)measures.Where(p=>p.KeyName==keyName).Select(p=>p.FlightTime).Average();

            measureList.Add(new Measure(keyName, firstAvg, secondAvg));

            measureList.RemoveAll(p => p.KeyName == keyName);
        }

        return measureList;
    }

    public static List<>
}
