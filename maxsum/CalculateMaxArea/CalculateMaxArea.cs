using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CalculateMaxArea
{
    public class ProcessCore
    {
        int[,] table;
        bool[,] select;
        int n,m;
        public ProcessCore(string filename)
        {
            string[] strs = File.ReadAllLines(@filename);
            if (strs.Length < 2) Console.WriteLine("文件格式不合法");
            string[] subStrs = strs[0].Split(',');
            n = int.Parse(subStrs[0]);
            subStrs = strs[1].Split(',');
            m = int.Parse(subStrs[0]);
            table = new int[n,m];
            select = new bool[n,m];
            for (int i = 2; i != 2 + n; ++i)
            {
                subStrs = strs[i].Split(',');
                for (int j = 0; j != m; ++j)
                    table[i - 2, j] = int.Parse(subStrs[j]);
            }
        }
        public bool Calcute(string[] mode)
        {


    }
}
