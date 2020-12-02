using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Text;
using Serilog;

public class ClsDB
{
    /*
     * Always use the following command for the datbase
     *  ALTER DATABASE DeliveryApp  
        SET ALLOW_SNAPSHOT_ISOLATION ON  
  
        ALTER DATABASE DeliveryApp  
        SET READ_COMMITTED_SNAPSHOT ON  
     */
    SqlConnection conn;
    SqlTransaction tran;
    SqlCommand cmd;

    #region DataBase Connection Property

    public string ConnString { get; set; }

    #endregion

    void ReadSetting()
    {
        try
        {
            ConnString = ConfigurationManager.ConnectionStrings["CONN"].ConnectionString;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public ClsDB()
    {
        try
        {
            conn = new SqlConnection();
            ReadSetting();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// Connect To DataBase
    /// </summary>
    public void Connect()
    {
        try
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.ConnectionString = ConnString;
                conn.Open();
                cmd = new SqlCommand();
                cmd = conn.CreateCommand();
                /*Default time out is 30 seconds*/
                cmd.CommandTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["CommandTimeOutInSec"].ToString());
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// DisConnect From DataBase
    /// </summary>
    public void DisConnect()
    {
        try
        {
            if (conn.State == ConnectionState.Open && conn != null)
            {
                cmd.Dispose();
                conn.Close();
                conn.Dispose();
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// Return Data If transaction start then within transaction if tran is not start then without transaction
    /// </summary>
    /// <param name="sQry">Pass Sql Query</param>
    /// <returns>DataSet</returns>
    public DataSet GetDataSet(string sQry) //can be called in transaction
    {
        try
        {
            cmd.CommandText = sQry;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            ds.Dispose();
            da.Dispose();
            return ds;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// Return Data If transaction start then within transaction if tran is not start then without transaction
    /// </summary>
    /// <param name="sQry">Pass Sql Query</param>
    /// <returns>DataTable</returns>
    public DataTable GetDataTable(string sQry) //this method can be called in transaction also
    {
        try
        {
            cmd.CommandText = sQry;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dt.Dispose();
            da.Dispose();
            return dt;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
  
    /// <summary>
    /// Insert,Update,Delete Data Wihin or Without Transaction
    /// </summary>
    /// <param name="sQry">Pass Sql Query For Inserion,Updation,Deletion</param>
    /// <returns>Affected No Of Rows</returns>
    public int ExecuteNonQuery(string sQry)
    {
        try
        {
            int iCount = 0;
            cmd.CommandText = sQry;
            iCount = cmd.ExecuteNonQuery();
            return iCount;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public object ExecuteScalar(string sQry)
    {
        try
        {
            cmd.CommandText = sQry;
            var vObj = cmd.ExecuteScalar();
            return vObj;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    #region Use Stored Procedure

    //*******All the methods can be called within Transaction
    /// <summary>
    /// Execute stored procedure,If procedure does not require any parameter.
    /// </summary>
    /// <param name="ProcName">Pass Stored Procedure Name</param>
    /// <returns>DataTable</returns>
    public DataTable GetDataTable_Proc(string ProcName)
    {
        try
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = ProcName;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dt.Dispose();
            da.Dispose();
            return dt;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// Execute stored procedure with parameters.
    /// </summary>
    /// <param name="ProcName">Pass Stored Procedure Name</param>
    /// <param name="parameters">Array Of Stored Procedure Parameter with Value</param>
    /// <returns>DataTable</returns>
    public DataTable GetDataTable_Proc(string ProcName, SqlParameter[] parameters = null) 
    {
        try
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = ProcName;
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dt.Dispose();
            da.Dispose();
            /*
             * Monitor every quey in debug mode
             */
#if DEBUG
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Exec {cmd.CommandText}");
            foreach (SqlParameter param in cmd.Parameters)
            {
                sb.AppendLine($"{param.ParameterName} = {param.Value},");
            }
            Log.Information(sb.ToString());
#endif
            return dt;
        }
        catch (SqlException sqex)
        {
            /*
             * When any error occured with parameter then it is difficult to produce the error from database by manually entering all the parameters.
             * So whenever any error will occur it will send all the paremeter with value so that procedure can be executed from databse directly.
             */
            StringBuilder sb = new StringBuilder();
#if DEBUG
            sb.AppendLine($"Exec {cmd.CommandText}");
#endif
            foreach (SqlParameter param in cmd.Parameters)
            {
                sb.AppendLine($"{param.ParameterName} = {param.Value},");
            }
            throw new Exception(sqex.Message + ", Error Qry:-" + sb.ToString(), sqex);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// Execute stored procedure with parameters.
    /// </summary>
    /// <param name="ProcName">Pass Stored Procedure Name</param>
    /// <param name="parameters">Dictionary with parameter and value</param>
    /// <returns>DataTable</returns>
    public DataTable GetDataTable_Proc(string ProcName, Dictionary<string, string> DicParameter = null)
    {
        try
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = ProcName;
            if (DicParameter != null)
            {
                foreach (var key in DicParameter.Keys)
                    cmd.Parameters.AddWithValue(key, DicParameter[key]);
            }
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dt.Dispose();
            da.Dispose();
            /*
             * Monitor every quey in debug mode
             */
#if DEBUG
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Exec {cmd.CommandText}");
            foreach (SqlParameter param in cmd.Parameters)
            {
                sb.AppendLine($"{param.ParameterName} = {param.Value},");
            }
            Log.Information(sb.ToString());
#endif
            return dt;
        }
        catch (SqlException sqex)
        {
            /*
             * When any error occured with parameter then it is difficult to produce the error from database by manually entering all the parameters.
             * So whenever any error will occur it will send all the paremeter with value so that procedure can be executed from databse directly.
             */
            StringBuilder sb = new StringBuilder();
#if DEBUG
            sb.AppendLine($"Exec {cmd.CommandText}");
#endif
            foreach (SqlParameter param in cmd.Parameters)
            {
                sb.AppendLine($"{param.ParameterName} = {param.Value},");
            }
            throw new Exception(sqex.Message + ", Error Qry:-" + sb.ToString(), sqex);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    #endregion

    #region Transaction

    /// <summary>
    /// Begin Transaction
    /// </summary>
    public void BeginTran()
    {
        try
        {
            Connect();
            tran = conn.BeginTransaction();
            cmd.Transaction = tran;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// Commit the transaction 
    /// </summary>
    public void CommitTran()
    {
        try
        {
            if (tran != null)
            {
                tran.Commit();
                tran.Dispose();
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// RollBackTran the transaction
    /// </summary>
    public void RollBackTran()
    {
        try
        {
            if (tran != null)
            {
                tran.Rollback();
                tran.Dispose();
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    #endregion
}

