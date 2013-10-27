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
        public int[,] table;//数据在这里
        public bool[,] select;//选择的块需要放在这里
        string[] mode;
        int[,] labx, laby, hrlabx, hrlaby;
        int pointerx, pointery, targetx, targety;
        int n, m;//n*m矩阵
        bool isHorizontal;//是否是需要竖直方向相连接
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
        void colorBlock(int[,] mat, int row, int col, int color)
        {
            int row_size, col_size;
            row_size = mat.GetLength(0);
            col_size = mat.GetLength(1);
            Queue<KeyValuePair<int, int>> Q = new Queue<KeyValuePair<int,int>>() ;
            Q.Enqueue(new KeyValuePair<int,int>(row, col));
            while (Q.Count > 0)
            {
                KeyValuePair<int, int> cur = Q.Dequeue();
                int r = cur.Key, c = cur.Value;
                if (r < 0 || r >= row_size || c < 0 || c >= col_size) continue;
                if (mat[r,c] == -1)
                {
                    mat[r,c] = color;
                    Q.Enqueue(new KeyValuePair<int, int>(r - 1, c));
                    Q.Enqueue(new KeyValuePair<int, int>(r, c - 1));
                    Q.Enqueue(new KeyValuePair<int, int>(r + 1, c));
                    Q.Enqueue(new KeyValuePair<int, int>(r, c + 1));
                }
            }
        }

        private void InRegular()
        {
            int row_size = table.GetLength(0), col_size = table.GetLength(1);
            int[,] map = new int[row_size, col_size];
            int[,] mat = table;
            //for(int i = 0; i < row_size; ++i) memset(map[i], 0, sizeof(int) * col_size);

            for (int i = 0; i < row_size; ++i)
                for (int j = 0; j < col_size; ++j)
                    map[i,j] = (mat[i,j] >= 0) ? -1 : 0;

            //color positive block
            int block_num = 0;
            for (int i = 0; i < row_size; ++i)
                for (int j = 0; j < col_size; ++j)
                    if (map[i,j] == -1) colorBlock(map, i, j, ++block_num);
            //count block value 
            int[] block = new int[block_num + 1];
            Array.Clear(block, 0, block.Length);
            for (int i = 0; i < row_size; ++i)
                for (int j = 0; j < col_size; ++j)
                    block[map[i,j]] += mat[i,j];
            int ret = -0x3fffffff, pos = 0;
            for (int i = 1; i <= block_num; ++i)
                if (block[i] > ret)
                {
                    ret = block[i];
                    pos = i;
                }

            for (int i = 0; i < row_size; ++i)
                for (int j = 0; j < col_size; ++j)
                    if (map[i, j] == pos)
                        select[i, j] = true;
                    else
                        select[i, j] = false;
            //return ret;
        }
        private void GetArea()
        {
            if (pointerx >= 0)
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
            long[,] f = new long[lmt, m];
            labx = new int[lmt, m];
            laby = new int[lmt, m];
            hrlabx = new int[lmt, m];
            hrlaby = new int[lmt, m];
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
                    isHorizontal = !isHorizontal;
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

