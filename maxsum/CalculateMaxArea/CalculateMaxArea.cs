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
        public int[,] table;
        public bool[,] select;
        string[] mode;
        int[,] labx, laby, hrlabx, hrlaby;
        int pointerx, pointery, targetx, targety;
        int n, m;
        bool isHorizontal;
        public int cnt;
        public ProcessCore(string cmd)
        {
            string[] cmdsplit = cmd.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //cnt = cmdsplit.Length;
            //return;
            string filename = cmdsplit[1];
            mode = new string[cmdsplit.Length - 2];
            for (int i = 0; i != mode.Length; ++i) mode[i] = cmdsplit[i + 2];
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
            this.Calcute();
        }
        private void Regular()
        {
            Solve(n, n, m);
            GetArea();
        }
        private void Ring()
        {
            Solve(n, n * 2, m);
            GetArea();
        }
        private void Horizontal()
        {
            Solve(n, n, m);
            GetArea();
        }
        private void Vertical()
        {
            Solve(n, n * 2, m);
            GetArea();
        }
        private void InRegular()
        {
            Regular();
            GetArea();
        }
        private void GetArea()
        {
            if (pointerx > 0)
            {
                for (int i = targetx; i <= pointerx; ++i)
                    for (int j = targety; j <= pointery; ++j)
                        select[i % n, j] = true;
            }
            if (pointerx < 0)
            {
                for (int i = targetx; i <= (-pointerx); ++i)
                {
                    for (int j = 0; j != targety; ++j) select[i % n, j] = true;
                    for (int j = -pointery + 1; j != m; ++j) select[i % n, j] = true;
                }
            }
        }
        private void Solve(int n, int lmt, int m)
        {
            long[,] f = new long[n, m];
            labx = new int[n, m];
            laby = new int[n, m];
            hrlabx = new int[n, m];
            hrlaby = new int[n, m];
            const long NINF = -9999999999;
            const long INF = 9999999999;
            long ans = NINF, tmp = 0, seq = 0, hrAns = INF, hrTot = 0, hrMax = NINF, hrSeq = 0;//hr used for horizontal mode
            for (int i = 0; i != table.GetLength(0); ++i)
                for (int j = 0; j != table.GetLength(1); ++j)
                    f[i, j] = table[i, j];
            if (lmt != table.GetLength(0))
                for (int i = 0; i != table.GetLength(0); ++i)
                    for (int j = 0; j != table.GetLength(1); ++j)
                        f[i + table.GetLength(0), j] = table[i, j];
            for (int i = 1; i != f.GetLength(0); ++i)
                for (int j = 0; j != m; ++j) f[i, j] += f[i - 1, j];

            pointerx = 0;
            pointery = 0;
            int hrpointerx = 0, hrpointery = 0;
            int hrtargetx = 0, hrtargety = 0;

            for (int i = -1; i != n; ++i)
                for (int j = i + 1; j != lmt && j - i <= n; ++j)
                {
                    seq = 0;
                    for (int k = 0; k != m; ++k)
                    {
                        if (i == -1) tmp = f[j, k]; else tmp = f[j, k] - f[i, k];
                        if (seq > 0) labx[j, k] = labx[j, k - 1]; else labx[j, k] = i + 1;
                        if (seq > 0) laby[j, k] = laby[j, k - 1]; else laby[j, k] = k;
                        if (seq > 0) seq = seq + tmp; else seq = tmp;
                        if (ans < seq)
                        {
                            ans = seq;
                            pointerx = j;
                            pointery = k;
                            targetx = labx[j, k];
                            targety = laby[j, k];
                        }
                        if (isHorizontal)
                        {
                            hrTot += tmp;
                            hrMax = Math.Max(hrMax, tmp);
                            if (hrSeq < 0) hrlabx[j, k] = hrlabx[j, k - 1]; else hrlabx[j, k] = i + 1;
                            if (hrSeq < 0) hrlaby[j, k] = hrlaby[j, k - 1]; else hrlaby[j, k] = k;
                            if (hrSeq < 0) hrSeq = hrSeq + tmp; else hrSeq = tmp;
                            if (hrAns > hrSeq)
                            {
                                hrAns = hrSeq;
                                hrpointerx = j;
                                hrpointery = k;
                                hrtargetx = hrlabx[j, k];
                                hrtargety = hrlaby[j, k];
                            }
                        }
                    }
                    if (isHorizontal)
                    {
                        if (ans < 0 && hrTot == hrAns && hrMax < 0) ;//on purpose
                        else
                        {
                            if (ans < hrTot - hrAns)
                            {
                                ans = hrTot - hrAns;
                                pointerx = -hrpointerx;
                                pointery = -hrpointery;
                                targetx = hrtargetx;
                                targety = hrtargety;
                            }
                        }
                        hrAns = INF;
                        hrMax = NINF;
                        hrTot = 0;
                        hrSeq = 0;
                    }
                }
        }
        public bool Calcute()
        {
            switch (mode.Length)
            {
                case 0:
                    Regular();
                    break;
                case 1:
                    switch (mode[0][1])
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
        static void Main()
        {
            ProcessCore psc = new ProcessCore("maxsum.exe C:\\Users\\tonyshaw\\Documents\\GitHub\\homework-03\\maxsum\\tests\\test7.txt -h");
        }
    }
}

