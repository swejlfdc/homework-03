using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO.Pipes;
using System.Data;

namespace maxsum
{
    partial class MainForm : Form
    {
        public void AddTab()
        {
            System.Windows.Forms.TabPage newPage = new TabPage();
            {
                DataGridView dataView = new DataGridView();
                int[,] TABLE = new int[,] { { 1, 2, 3 }, { 4, 5, 6 } };
                DataTable dt = new DataTable();
                for (int i = 0; i < TABLE.GetLength(1); i++)
                    dt.Columns.Add(i.ToString(), typeof(int));
                for (int i = 0; i < TABLE.GetLength(0); i++)
                {
                    DataRow dr = dt.NewRow();
                    for (int j = 0; j < TABLE.GetLength(1); j++)
                        dr[j] = TABLE[i, j];
                    dt.Rows.Add(dr);
                }
                dataView.DataSource = dt;
                dataView.Visible = true;
                dataView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
                dataView.ColumnHeadersVisible = false;
                dataView.RowHeadersVisible = false;
                newPage.Controls.Add(dataView);
            }
            displayTab.Controls.Add(newPage);
            newPage.Name = "file";
            newPage.Text = "file";
            displayTab.SelectedTab = newPage;

        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        
        }
    }
    class Admin{
        private static MainForm form_entity = null;
        Thread ServerThread = null;
        Thread FormThread = null;
        NamedPipeServerStream pipeServer = null;
        bool _shouldStop = false;
        public delegate void InvokeDelegate();

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
            pipeServer = new NamedPipeServerStream("maxsum_pipe", PipeDirection.In, 2);
            for (pipeServer.WaitForConnection(); 
                !_shouldStop;
                pipeServer.WaitForConnection())
            {
                form_entity.TopLevelControl.BeginInvoke(new InvokeDelegate(form_entity.AddTab));
                pipeServer.Disconnect();
            }
        }

        [STAThreadAttribute]
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
                pipeClient.Dispose();
            }
        }
    }
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThreadAttribute]
        static void Main() {
            new Admin().Run();
        }
    }
}
