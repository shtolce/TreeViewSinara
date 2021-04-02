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
    public class ShortagesRepoSql
    {
        private string _entityName = "Shortages";
        private static Logger logger;
        private ResourceGroupRepo resGrpRepo;
        private ResourceRepo resRepo;
        private IPreactor _preactor;
        private string _conString;

        public List<ShortagesDTO> ShortagesCollection { get; set; }

        public ShortagesRepoSql(IPreactor preactor)
        {
            this._conString = preactor.ParseShellString("{DB CONNECT STRING}");
            ShortagesRepoSql.logger = LogManager.GetCurrentClassLogger();
            ShortagesRepoSql.logger.Trace("Создан экземпляр сервиса ShortagesRepoSql");
            this._preactor = preactor;
        }


        private string GetProductShortagessQuery
        {
            get
            {
                return @"
                        SELECT [ShortagesId]
                          ,[ExternalDemandOrder]
                          ,[InternalDemandOrder]
                          ,s.[PartNo]
                          ,[ShortageQuantity]
                          ,[DatasetId]
                          ,[__seq__Shortages]
                        FROM [UserData].[Shortages] s
                        inner join [UserData].[Products] p on p.PartNo = s.PartNo
	                    WHERE s.DatasetId in 
	                    (   
	                    SELECT [DatasetId]
	                    FROM [UserData].[Orders_Dataset]
	                    WHERE name=N'Schedule'
	                    )
                        ";
            }
        }


        private string GetAllShortagessQuery
        {
            get
            {
                return @"
                        SELECT [ShortagesId]
                          ,[ExternalDemandOrder]
                          ,[InternalDemandOrder]
                          ,s.[PartNo]
                          ,[ShortageQuantity]
                          ,[DatasetId]
                          ,[__seq__Shortages]
                        FROM [UserData].[Shortages] s
	                    WHERE s.DatasetId in 
	                    (   
	                    SELECT [DatasetId]
	                    FROM [UserData].[Orders_Dataset]
	                    WHERE name=N'Schedule'
	                    )
                        ";
            }
        }

        public List<ShortagesDTO> getAllProductShortage()
        {
            List<ShortagesDTO> list;
            using (SqlConnection cnn = new SqlConnection(this._conString))
            {
                cnn.Open();
                list = cnn.Query<ShortagesDTO>(GetProductShortagessQuery, (object)null, (IDbTransaction)null, true, new int?(), new CommandType?(CommandType.Text)).ToList<ShortagesDTO>();
            }
            return list;
        }



        public List<ShortagesDTO> getAll()
        {
            List<ShortagesDTO> list;
            using (SqlConnection cnn = new SqlConnection(this._conString))
            {
                string getAllShortagessQuery = this.GetAllShortagessQuery;
                cnn.Open();
                list = cnn.Query<ShortagesDTO>(getAllShortagessQuery, (object)null, (IDbTransaction)null, true, new int?(), new CommandType?(CommandType.Text)).ToList<ShortagesDTO>();
            }
            return list;
        }

        ~ShortagesRepoSql()
        {
            LogManager.Flush();
        }
    }
}
