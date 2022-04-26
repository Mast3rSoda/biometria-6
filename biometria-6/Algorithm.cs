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


}
