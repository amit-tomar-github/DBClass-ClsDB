using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBClass
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            /*
               * Enable Logging
               * File size is 500 kb = 512000
               * this is using statis Log class from library just use the reference on any form and use method from Log class
             */
            Log.Logger = new LoggerConfiguration().WriteTo.File("Logs/Log_.log", rollingInterval: RollingInterval.Day,
              fileSizeLimitBytes: 512000, retainedFileCountLimit: 50, rollOnFileSizeLimit: true).CreateLogger();
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            ClsDB db = new ClsDB();
            try
            {
                string qry = $"Exec Prc_User {txtId.Text},'{txtName.Text}'";
                db.Connect();
                db.GetDataTable(qry);
                MessageBox.Show("Saved");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); Log.Error(ex,"Error Occured"); }
            finally
            {
                db.DisConnect();
                db = null;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ClsDB db = new ClsDB();
            try
            {
                db.Connect();
                SqlParameter[] parameters = {
                new SqlParameter("@Id",txtId.Text),
                new SqlParameter("@Name",txtName.Text)
                };
                 db.GetDataTable_Proc("Prc_User",parameters);
                MessageBox.Show("Saved");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); Log.Error(ex, "Error Occured"); }
            finally
            {
                db.DisConnect();
                db = null;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            ClsDB db = new ClsDB();
            try
            {
                db.Connect();
                db.GetDataTable_Proc("Prc_User");
                MessageBox.Show("Saved");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); Log.Error(ex, "Error Occured"); }
            finally
            {
                db.DisConnect();
                db = null;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ClsDB db = new ClsDB();
            try
            {
                db.Connect();
                Dictionary<string, string> DicProcParameter = new Dictionary<string, string>();
                DicProcParameter.Add("@Id",txtId.Text);
                DicProcParameter.Add("@Name", txtName.Text);
 
                 db.GetDataTable_Proc("Prc_User", DicProcParameter);
                MessageBox.Show("Saved");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); Log.Error(ex, "Error Occured"); }
            finally
            {
                db.DisConnect();
                db = null;
            }
        }
    }
}
