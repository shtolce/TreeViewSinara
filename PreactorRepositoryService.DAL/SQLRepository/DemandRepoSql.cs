using Dapper;
using NLog;
using Preactor;
using PreactorRepositoryService.DAL.DTOModels;
using PreactorRepositoryService.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//конфигурационный файл , должен быть  в папке преактора exe

namespace PreactorRepositoryService.DAL.Repository
{
    public class DemandRepoSql
    {
        private string _entityName = "Demand";
        private static Logger logger;
        private ResourceGroupRepo resGrpRepo;
        private ResourceRepo resRepo;
        private IPreactor _preactor;
        private string _conString;

        public List<DemandDTO> DemandCollection { get; set; }

        public DemandRepoSql(IPreactor preactor)
        {
            this._conString = preactor.ParseShellString("{DB CONNECT STRING}");
            DemandRepoSql.logger = LogManager.GetCurrentClassLogger();
            DemandRepoSql.logger.Trace("Создан экземпляр сервиса DemandRepoSql");
            this._preactor = preactor;
        }

        private string GetMaxDemandDate
        {
            get
            {
                return "\r\n                       SELECT\r\n                       max([DemandDate])\r\n                       FROM[UserData].[Demand]  ord\r\n                       WHERE ord.DatasetId in \r\n                       (   \r\n\t                    SELECT[DatasetId]\r\n                        FROM[UserData].[Orders_Dataset]\r\n                        WHERE name = N'Schedule'\r\n                       )\r\n                       ";
            }
        }

        private string GetMinDemandDate
        {
            get
            {
                return "\r\n                       SELECT\r\n                       min([DemandDate])\r\n                       FROM[UserData].[Demand]  ord\r\n                       WHERE ord.DatasetId in \r\n                       (   \r\n\t                    SELECT[DatasetId]\r\n                        FROM[UserData].[Orders_Dataset]\r\n                        WHERE name = N'Schedule'\r\n                       )\r\n                       ";
            }
        }

        private string GetAllDemandsQuery
        {
            get
            {
                return "\r\n                       SELECT \r\n\t                       [DemandId] as Id\r\n                          ,[OrderNo]\r\n                          ,[PartNo]\r\n                          ,[Quantity]\r\n                          ,[MultipleQuantity]\r\n                          ,[DatasetId]\r\n                          ,[__seq__Demand]\r\n                          ,[Priority]\r\n                          ,[Description]\r\n                          ,[BelongsToOrderNo]\r\n                          ,[OrderLine]\r\n                          ,[DemandDate]\r\n                          ,[OrderType]\r\n                          ,[TableAttribute1]\r\n                          ,[TableAttribute2]\r\n                          ,[TableAttribute3]\r\n                          ,[StringAttribute1]\r\n                          ,[StringAttribute2]\r\n                          ,[StringAttribute3]\r\n                          ,[StringAttribute4]\r\n                          ,[Owner]\r\n                          ,[OwnerNo]\r\n                          ,[DateMonthRO2]\r\n                          ,[DemandState]\r\n                          ,[OrderNo1c]\r\n                          ,[StringNo1c]\r\n                          ,[Davalec]\r\n                          ,[PartNoOld]\r\n                      FROM [UserData].[Demand] ord\r\n                       WHERE ord.DatasetId in \r\n                       (   \r\n\t                    SELECT [DatasetId]\r\n\t                    FROM [UserData].[Orders_Dataset]\r\n\t                    WHERE name=N'Schedule'\r\n                       )\r\n                   \r\n                       ";
            }
        }

        public List<DemandDTO> getAll()
        {
            List<DemandDTO> list;
            using (SqlConnection cnn = new SqlConnection(this._conString))
            {
                string getAllDemandsQuery = this.GetAllDemandsQuery;
                cnn.Open();
                list = cnn.Query<DemandDTO>(getAllDemandsQuery, (object)null, (IDbTransaction)null, true, new int?(), new CommandType?(CommandType.Text)).ToList<DemandDTO>();
            }
            return list;
        }

        public DateTime? getMaxDate(DateTime defaultDate)
        {
            DateTime? nullable;
            using (SqlConnection cnn = new SqlConnection(this._conString))
            {
                string getMaxDemandDate = this.GetMaxDemandDate;
                cnn.Open();
                nullable = cnn.QueryFirst<DateTime?>(getMaxDemandDate, (object)null, (IDbTransaction)null, new int?(), new CommandType?(CommandType.Text));
            }
            return !nullable.HasValue ? new DateTime?(defaultDate) : nullable;
        }

        public DateTime? getMinDate(DateTime defaultDate)
        {
            DateTime? nullable;
            using (SqlConnection cnn = new SqlConnection(this._conString))
            {
                string getMinDemandDate = this.GetMinDemandDate;
                cnn.Open();
                nullable = cnn.QueryFirst<DateTime?>(getMinDemandDate, (object)null, (IDbTransaction)null, new int?(), new CommandType?(CommandType.Text));
            }
            return !nullable.HasValue ? new DateTime?(defaultDate) : nullable;
        }

        ~DemandRepoSql()
        {
            LogManager.Flush();
        }
    }
}
