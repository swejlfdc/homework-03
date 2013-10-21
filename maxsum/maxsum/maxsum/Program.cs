using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO.Pipes;
using System.IO;
using System.Data;
using System.Drawing;
using CalculateMaxArea;

namespace maxsum
{
    partial class MainForm : Form
    {
        public void AddTab(int[,] TABLE, bool[,] select)
        {
            System.Windows.Forms.TabPage newPage = new TabPage();
            {
                DataGridView dataView = new DataGridView();
                DataTable dt = new DataTable();
                BindingSource bs = new BindingSource();
                for (int i = 0; i < TABLE.GetLength(1); i++)
                {
                    dt.Columns.Add(i.ToString(), typeof(int));
                    dataView.Columns.Add(i.ToString(), i.ToString());
                }
                dt.Clear();
                for (int i = 0; i < TABLE.GetLength(0); i++)
                {
                    dataView.Rows.Add();
                    DataRow dr = dt.NewRow();
                    for (int j = 0; j < TABLE.GetLength(1); j++) {
                        dr[j] = TABLE[i, j];
                        dataView.CurrentCell = dataView[j, i];
                        dataView.CurrentCell.Value = TABLE[i, j];
                        if (select[i, j])
                        {
                            
                            //dataView.BeginEdit(true);
                            dataView.CurrentCell.Style.BackColor = Color.Yellow;
                            //dataView.EndEdit();
                        }
                    }
                    dt.Rows.Add(dr);
                }

                bs.DataSource = dt;                
                //dataView.DataSource = bs;
                dataView.AutoGenerateColumns = true;
                dataView.Visible = true;
                dataView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
                dataView.ColumnHeadersVisible = false;
                dataView.RowHeadersVisible = false;
                dataView.AllowUserToAddRows = false;
                newPage.Controls.Add(dataView);
                //dataView.Refresh();
                //dataView.Update();
                //dataView.ResetBindings();               
            }
            displayTab.Controls.Add(newPage);
            newPage.Name = "file";
            newPage.Text = "file";
            displayTab.SelectedTab = newPage;

        }
    }
    class Admin{
        private static MainForm form_entity = null;
        Thread ServerThread = null;
        Thread FormThread = null;
        NamedPipeServerStream pipeServer = null;
        bool _shouldStop = false;
        ProcessCore core;
        public delegate void InvokeDelegate(int[,] TABLEs, bool[,] select);

        void stopServer(object sender, EventArgs e)
        {
            if (ServerThread != null)
            {
                _shouldStop = true;
                NamedPipeClientStream pipeClient = 
                    new NamedPipeClientStream(".", "maxsum_pipe", PipeDirection.Out);
                pipeClient.Connect();
                pipeClient.Dispose();
            }
        }
        [STAThreadAttribute]
        public void getForm()
        {
            if (form_entity == null)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);                
                Application.Run(form_entity = new MainForm());
            }
        }
        public void Server()
        {
            Thread.Sleep(500);
            form_entity.Disposed += new System.EventHandler(this.stopServer);
            for (;!_shouldStop;)
                {
                pipeServer = new NamedPipeServerStream("maxsum_pipe", PipeDirection.In, 3);
                pipeServer.WaitForConnection();
                if (_shouldStop) break;
                BinaryReader dataReader = new BinaryReader(pipeServer);
                /*
                int row_size, col_size;
                row_size = dataReader.ReadInt32();
                col_size = dataReader.ReadInt32();
                int[,] data = new int[row_size, col_size];
                bool[,] select = new bool[row_size, col_size];
                for (int i = 0; i < row_size; ++i)
                    for (int j = 0; j < col_size; ++j)
                    {
                        data[i, j] = dataReader.ReadInt32();
                    }
                for (int i = 0; i < row_size; ++i)
                    for (int j = 0; j < col_size; ++j)
                    {
                        select[i, j] = dataReader.ReadBoolean();
                    }
                 * */
                string info = dataReader.ReadString();
                string[] imp = info.Split(';');
                Environment.CurrentDirectory = imp[0];
                core = new ProcessCore(imp[1]);
                core.Calcute();
                form_entity.TopLevelControl.BeginInvoke(
                        new InvokeDelegate(form_entity.AddTab), 
                        core.table,
                        core.select
                    );
                dataReader.Close();
            }
        }

        public void Run()
        {
            NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "maxsum_pipe", PipeDirection.Out);
            try{
                pipeClient.Connect(500);
            } 
            catch (TimeoutException e) {
                FormThread = new Thread(this.getForm);
                ServerThread = new Thread(this.Server);
                FormThread.SetApartmentState (ApartmentState.STA);
                FormThread.Start();
                ServerThread.Start();
                try
                {
                    pipeClient.Connect(1000);
                }
                catch (TimeoutException e2)
                {
                    Console.WriteLine("Error");
                    return;
                }
            }
            BinaryWriter pipeWriter = new BinaryWriter(pipeClient);
            /*
            pipeWriter.Write(3);
            pipeWriter.Write((int)3);
            for (int i = 0; i < 9; ++i) pipeWriter.Write(i);
            for (int i = 0; i < 9; ++i) pipeWriter.Write((i & 1) == 0);
            * */
            pipeWriter.Write(Environment.CurrentDirectory + ";" + Environment.CommandLine);
            pipeWriter.Flush();
            pipeWriter.Close();
            //pipeClient.Close();
        }
    }
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            new Admin().Run();
        }
    }
}
