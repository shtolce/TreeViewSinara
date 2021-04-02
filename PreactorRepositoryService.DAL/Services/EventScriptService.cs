using Preactor;
using System;
using System.Data;
using System.Data.SqlClient;

namespace PreactorRepositoryService.DAL.Services
{
    public class EventScriptService
    {
        private IPreactor _preactor;
        private string _conString;

        public EventScriptService(IPreactor preactor)
        {
            this._preactor = preactor;
        }

        public void LoadDemandParam(
          DateTime DateTime1,
          DateTime DateTime2,
          DateTime ZDateTime1,
          DateTime ZDateTime2)
        {
            this._conString = this._preactor.ParseShellString("{DB CONNECT STRING}");
            SqlConnection connection = new SqlConnection(this._conString);
            connection.Open();
            SqlCommand sqlCommand = new SqlCommand("if object_id('tempdb..##LoadDemandParam') is NULL \r\n            begin \r\n                create table ##LoadDemandParam( Date1 datetime, Date2 datetime,ZDate1 datetime, ZDate2 datetime )\r\n            end else begin\r\n                Delete from ##LoadDemandParam  \r\n            end\r\n\t\t        insert ##LoadDemandParam (Date1, Date2, ZDate1, ZDate2 ) values (@Date1,@Date2,@ZDate1,@ZDate2)", connection);
            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.CommandTimeout = 0;
            sqlCommand.Parameters.AddWithValue("@ZDate1", (object)DateTime1);
            sqlCommand.Parameters.AddWithValue("@ZDate2", (object)DateTime2);
            sqlCommand.Parameters.AddWithValue("@Date1", (object)ZDateTime1);
            sqlCommand.Parameters.AddWithValue("@Date2", (object)ZDateTime2);
            sqlCommand.ExecuteNonQuery();
        }
    }
}
