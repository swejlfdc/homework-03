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
        int n, m;
        bool isHorizontal;
        public ProcessCore(string filename)
        {
            string[] strs = File.ReadAllLines(@filename);
            if (strs.Length < 2) Console.WriteLine("文件格式不合法");
            string[] subStrs = strs[0].Split(',');
            isHorizontal = false;
            n = int.Parse(subStrs[0]);
            subStrs = strs[1].Split(',');
            m = int.Parse(subStrs[0]);
            table = new int[n, m];
            select = new bool[n, m];
            for (int i = 2; i != 2 + n; ++i)
            {
                subStrs = strs[i].Split(',');
                for (int j = 0; j != m; ++j)
                    table[i - 2, j] = int.Parse(subStrs[j]);
            }
            int tmp;
            if (n > m)
            {
                for (int i = 0; i != n; ++i)
                    for (int j = 0; j != n; ++j)
                    {
                        tmp = table[i, j];
                        table[i, j] = table[j, i];
                        table[j, i] = tmp;
                    }
                tmp = n;
                n = m;
                m = tmp;
                isHorizontal = true;
            }
        }
        private void Regular();
        private void Ring();
        private void Horizontal();
        private void Vertical();
        private void InRegular();
        public bool Calcute(string[] mode)
        {
            switch (mode.Length)
            {
                case 0:
                    Regular();
                    break;
                case 1:
                    switch (mode[0][2])
                    {
                        case 'a': InRegular(); break;
                        case 'h': isHorizontal = !isHorizontal; if (isHorizontal) Horizontal(); else Vertical(); break;
                        case 'v': if (isHorizontal) Horizontal(); else Vertical(); break;
                        default:
                            return false;
                    }
                    break;
                case 2:
                    Ring();
                    break;
                case 3:
                    InRegular();
                    break;
                default:
                    return false;
            }
            return true;
        }

    }
}

