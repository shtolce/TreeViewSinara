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
    public class OrderRepoSql

    {
        private static Logger logger;
        private ResourceGroupRepo resGrpRepo;
        private ResourceRepo resRepo;
        private IPreactor _preactor;
        private string _entityName = "Orders";
        private string _conString;
        


        public OrderRepoSql(IPreactor preactor)
        {
            _conString = preactor.ParseShellString("{DB CONNECT STRING}");
            logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Создан экземпляр сервиса OrderRepoSql");
            this._preactor = preactor;
            resGrpRepo = new ResourceGroupRepo(_preactor);
            resRepo = new ResourceRepo(_preactor);
        }

        private string GetScheduledOrdersQuery
        {
            get
            {
                return @"
               SELECT 
	               ord.OrdersId as Id
	              ,ord.[BelongsToOrderNo] as BelongsToOrderNo
                  ,ord.[OrderNo] as OrderNo
                  ,ord.[OpNo] as OpNo
                  ,ord.[OperationName] as OpName
                  ,ord.[Quantity] as Quantity
                  ,ord.[Product] as Product
                  ,ord.[ResourceGroup] as ResourceGroup
	              ,res.Name as Resource
	              ,res.ResourcesId as ResourceId
                  ,ord.[EarliestStartDate] as EarliestStartDate
                  ,ord.[DueDate] as DueDate
                  ,ord.[Priority] as Priority
                  ,ord.[SetupStart] as SetupStart
                  ,ord.[StartTime] as StartTime
                  ,ord.[EndTime] as EndTime
                  ,ord.[PartNo] as PartNo
                  ,ord.[NumericalAttribute1] as NumericalAttribute1
                  ,ord.[NumericalAttribute2] as NumericalAttribute2
                  ,ord.[NumericalAttribute3] as NumericalAttribute3
                  ,ord.[TableAttribute1] as TableAttribute1
                  ,ord.[TableAttribute1Rank] as TableAttribute1Rank
                  ,ord.[TableAttribute2] as TableAttribute2
                  ,ord.[TableAttribute2Rank] as TableAttribute2Rank
                  ,ord.[TableAttribute3] as TableAttribute3
                  ,ord.[TableAttribute3Rank] as TableAttribute3Rank
                  ,ord.[StringAttribute1] as StringAttribute1
                  ,ord.[StringAttribute2] as StringAttribute2
                  ,ord.[StringAttribute3] as StringAttribute3
                  ,ord.[ResSpecType] as ResSpecType
                  ,ord.[Hold] as Hold
               FROM [UserData].[Orders] as ord
               LEFT JOIN [UserData].[Resources] as res
               on  res.[ResourcesId] = ord.[Resource]
               WHERE ord.DatasetId in 
               (   
	            SELECT [DatasetId]
	            FROM [UserData].[Orders_Dataset]
	            WHERE name=N'Schedule'
               )
               AND
	            ord.StartTime is not null
               AND
	            ord.EndTime is not null
                    ";
            }
        }
        private string GetScheduledOrdersQueryInfinite
        {
            get
            {
                return @"
               SELECT 
	               ord.OrdersId as Id
	              ,ord.[BelongsToOrderNo] as BelongsToOrderNo
                  ,ord.[OrderNo] as OrderNo
                  ,ord.[OpNo] as OpNo
                  ,ord.[OperationName] as OpName
                  ,ord.[Quantity] as Quantity
                  ,ord.[Product] as Product
                  ,ord.[ResourceGroup] as ResourceGroup
	              ,res.Name as Resource
	              ,res.ResourcesId as ResourceId
                  ,ord.[EarliestStartDate] as EarliestStartDate
                  ,ord.[DueDate] as DueDate
                  ,ord.[Priority] as Priority
                  ,ord.[SetupStart] as SetupStart
                  ,ord.[SetupTime]*60*24 as SetupTime
                  ,ord.[StartTime] as StartTime
                  ,ord.[EndTime] as EndTime
                  ,ord.[PartNo] as PartNo
                  ,ord.[NumericalAttribute1] as NumericalAttribute1
                  ,ord.[NumericalAttribute2] as NumericalAttribute2
                  ,ord.[NumericalAttribute3] as NumericalAttribute3
                  ,ord.[NumericalAttribute15] as NumericalAttribute15
                  ,ord.[TableAttribute1] as TableAttribute1
                  ,ord.[TableAttribute1Rank] as TableAttribute1Rank
                  ,ord.[TableAttribute2] as TableAttribute2
                  ,ord.[TableAttribute2Rank] as TableAttribute2Rank
                  ,ord.[TableAttribute3] as TableAttribute3
                  ,ord.[TableAttribute3Rank] as TableAttribute3Rank
                  ,ord.[StringAttribute1] as StringAttribute1
                  ,ord.[StringAttribute2] as StringAttribute2
                  ,ord.[StringAttribute3] as StringAttribute3
                  ,ord.[ResSpecType] as ResSpecType
                  ,ord.[Hold] as Hold
                  ,ord.[Hold] as Hold
               FROM [UserData].[Orders] as ord
               LEFT JOIN [UserData].[Resources] as res
               on  res.[ResourcesId] = ord.[Resource]
               WHERE ord.DatasetId in 
               (   
	            SELECT [DatasetId]
	            FROM [UserData].[Orders_Dataset]
	            WHERE name=N'Schedule'
               )
               AND
	            ord.StartTime is not null
               AND
	            ord.EndTime is not null
               AND 
	            res.FiniteOrInfinite<0
                    ";
            }
        }
        private string GetOrdersQueryInfinite
        {
            get
            {
                return @"
               SELECT 
	               ord.OrdersId as Id
	              ,ord.[BelongsToOrderNo] as BelongsToOrderNo
                  ,ord.[OrderNo] as OrderNo
                  ,ord.[OpNo] as OpNo
                  ,ord.[OperationName] as OpName
                  ,ord.[Quantity] as Quantity
                  ,ord.[Product] as Product
                  ,ord.[ResourceGroup] as ResourceGroup
	              ,res.Name as Resource
	              ,res.ResourcesId as ResourceId
                  ,ord.[EarliestStartDate] as EarliestStartDate
                  ,ord.[DueDate] as DueDate
                  ,ord.[Priority] as Priority
                  ,ord.[SetupStart] as SetupStart
                  ,ord.[SetupTime]*60*24 as SetupTime
                  ,ord.[StartTime] as StartTime
                  ,ord.[EndTime] as EndTime
                  ,ord.[PartNo] as PartNo
                  ,ord.[NumericalAttribute1] as NumericalAttribute1
                  ,ord.[NumericalAttribute2] as NumericalAttribute2
                  ,ord.[NumericalAttribute3] as NumericalAttribute3
                  ,ord.[NumericalAttribute15] as NumericalAttribute15
                  ,ord.[TableAttribute1] as TableAttribute1
                  ,ord.[TableAttribute1Rank] as TableAttribute1Rank
                  ,ord.[TableAttribute2] as TableAttribute2
                  ,ord.[TableAttribute2Rank] as TableAttribute2Rank
                  ,ord.[TableAttribute3] as TableAttribute3
                  ,ord.[TableAttribute3Rank] as TableAttribute3Rank
                  ,ord.[StringAttribute1] as StringAttribute1
                  ,ord.[StringAttribute2] as StringAttribute2
                  ,ord.[StringAttribute3] as StringAttribute3
                  ,ord.[ResSpecType] as ResSpecType
                  ,ord.[Hold] as Hold
               FROM [UserData].[Orders] as ord
               LEFT JOIN [UserData].[ResourceGroups] as resGrp
               on  resGrp.ResourceGroupsId = ord.ResourceGroup
               LEFT JOIN [UserData].[ResourceGroupsResources] as resGrpRes
               on  resGrp.ResourceGroupsId = resGrpRes.ResourceGroupsId
			   LEFT JOIN [UserData].[Resources] as res
               on  res.[ResourcesId] = resGrpRes.Resources


               WHERE
                ord.DatasetId in 
               (   
	            SELECT [DatasetId]
	            FROM [UserData].[Orders_Dataset]
	            WHERE name=N'Schedule'
               )
               AND 
	            res.FiniteOrInfinite<0
                    ";
            }
        }

        private string GetOrdersQueryFinite
        {
            get
            {
                return @"
               SELECT 
	               ord.OrdersId as Id
	              ,ord.[BelongsToOrderNo] as BelongsToOrderNo
                  ,ord.[OrderNo] as OrderNo
                  ,ord.[OpNo] as OpNo
                  ,ord.[OperationName] as OpName
                  ,ord.[Quantity] as Quantity
                  ,ord.[Product] as Product
                  ,ord.[ResourceGroup] as ResourceGroup
	              ,res.Name as Resource
	              ,res.ResourcesId as ResourceId
                  ,ord.[EarliestStartDate] as EarliestStartDate
                  ,ord.[DueDate] as DueDate
                  ,ord.[Priority] as Priority
                  ,ord.[SetupStart] as SetupStart
                  ,ord.[SetupTime]*60*24 as SetupTime
                  ,ord.[StartTime] as StartTime
                  ,ord.[EndTime] as EndTime
                  ,ord.[PartNo] as PartNo
                  ,ord.[NumericalAttribute1] as NumericalAttribute1
                  ,ord.[NumericalAttribute2] as NumericalAttribute2
                  ,ord.[NumericalAttribute3] as NumericalAttribute3
                  ,ord.[NumericalAttribute15] as NumericalAttribute15
                  ,ord.[TableAttribute1] as TableAttribute1
                  ,ord.[TableAttribute1Rank] as TableAttribute1Rank
                  ,ord.[TableAttribute2] as TableAttribute2
                  ,ord.[TableAttribute2Rank] as TableAttribute2Rank
                  ,ord.[TableAttribute3] as TableAttribute3
                  ,ord.[TableAttribute3Rank] as TableAttribute3Rank
                  ,ord.[StringAttribute1] as StringAttribute1
                  ,ord.[StringAttribute2] as StringAttribute2
                  ,ord.[StringAttribute3] as StringAttribute3
                  ,ord.[ResSpecType] as ResSpecType
                  ,ord.[Hold] as Hold
               FROM [UserData].[Orders] as ord
               LEFT JOIN [UserData].[ResourceGroups] as resGrp
               on  resGrp.ResourceGroupsId = ord.ResourceGroup
               LEFT JOIN [UserData].[ResourceGroupsResources] as resGrpRes
               on  resGrp.ResourceGroupsId = resGrpRes.ResourceGroupsId
			   LEFT JOIN [UserData].[Resources] as res
               on  res.[ResourcesId] = resGrpRes.Resources


               WHERE
                ord.DatasetId in 
               (   
	            SELECT [DatasetId]
	            FROM [UserData].[Orders_Dataset]
	            WHERE name=N'Schedule'
               )
               AND 
	            res.FiniteOrInfinite>0
                    ";
            }
        }

        private string GetScheduledOrdersQueryFinite
        {
            get
            {
                return @"
               SELECT 
	               ord.OrdersId as Id
	              ,ord.[BelongsToOrderNo] as BelongsToOrderNo
                  ,ord.[OrderNo] as OrderNo
                  ,ord.[OpNo] as OpNo
                  ,ord.[OperationName] as OpName
                  ,ord.[Quantity] as Quantity
                  ,ord.[Product] as Product
                  ,ord.[ResourceGroup] as ResourceGroup
	              ,res.Name as Resource
	              ,res.ResourcesId as ResourceId
                  ,ord.[EarliestStartDate] as EarliestStartDate
                  ,ord.[DueDate] as DueDate
                  ,ord.[Priority] as Priority
                  ,ord.[SetupStart] as SetupStart
                  ,ord.[StartTime] as StartTime
                  ,ord.[EndTime] as EndTime
                  ,ord.[PartNo] as PartNo
                  ,ord.[NumericalAttribute1] as NumericalAttribute1
                  ,ord.[NumericalAttribute2] as NumericalAttribute2
                  ,ord.[NumericalAttribute3] as NumericalAttribute3
                  ,ord.[TableAttribute1] as TableAttribute1
                  ,ord.[TableAttribute1Rank] as TableAttribute1Rank
                  ,ord.[TableAttribute2] as TableAttribute2
                  ,ord.[TableAttribute2Rank] as TableAttribute2Rank
                  ,ord.[TableAttribute3] as TableAttribute3
                  ,ord.[TableAttribute3Rank] as TableAttribute3Rank
                  ,ord.[StringAttribute1] as StringAttribute1
                  ,ord.[StringAttribute2] as StringAttribute2
                  ,ord.[StringAttribute3] as StringAttribute3
                  ,ord.[ResSpecType] as ResSpecType
                  ,ord.[Hold] as Hold
      
               FROM [UserData].[Orders] as ord
               LEFT JOIN [UserData].[Resources] as res
               on  res.[ResourcesId] = ord.[Resource]
               WHERE ord.DatasetId in 
               (   
	            SELECT [DatasetId]
	            FROM [UserData].[Orders_Dataset]
	            WHERE name=N'Schedule'
               )
               AND
	            ord.StartTime is not null
               AND
	            ord.EndTime is not null
               AND 
	            res.FiniteOrInfinite>0

                    ";
            }
        }


        private string GetUnScheduledOrdersQueryFinite
        {
            get
            {
                return @"
               SELECT DISTINCT
	               ord.OrdersId as Id
	              ,ord.[BelongsToOrderNo] as BelongsToOrderNo
                  ,ord.[OrderNo] as OrderNo
                  ,ord.[OpNo] as OpNo
                  ,ord.[OperationName] as OpName
                  ,ord.[Quantity] as Quantity
                  ,ord.[Product] as Product
                  ,ord.[ResourceGroup] as ResourceGroup
	              --,res.Name as Resource
	              --,res.ResourcesId as ResourceId
                  ,ord.[EarliestStartDate] as EarliestStartDate
                  ,ord.[DueDate] as DueDate
                  ,ord.[Priority] as Priority
                  ,ord.[SetupStart] as SetupStart
                  ,ord.[StartTime] as StartTime
                  ,ord.[EndTime] as EndTime
                  ,ord.[PartNo] as PartNo
                  ,ord.[NumericalAttribute1] as NumericalAttribute1
                  ,ord.[NumericalAttribute2] as NumericalAttribute2
                  ,ord.[NumericalAttribute3] as NumericalAttribute3
                  ,ord.[TableAttribute1] as TableAttribute1
                  ,ord.[TableAttribute1Rank] as TableAttribute1Rank
                  ,ord.[TableAttribute2] as TableAttribute2
                  ,ord.[TableAttribute2Rank] as TableAttribute2Rank
                  ,ord.[TableAttribute3] as TableAttribute3
                  ,ord.[TableAttribute3Rank] as TableAttribute3Rank
                  ,ord.[StringAttribute1] as StringAttribute1
                  ,ord.[StringAttribute2] as StringAttribute2
                  ,ord.[StringAttribute3] as StringAttribute3
                  ,ord.[ResSpecType] as ResSpecType
                  ,ord.[Hold] as Hold
      
               FROM [UserData].[Orders] as ord
               LEFT JOIN [UserData].[ResourceGroups] as resGrp
               on  resGrp.ResourceGroupsId = ord.ResourceGroup
               LEFT JOIN [UserData].[ResourceGroupsResources] as resGrpRes
               on  resGrp.ResourceGroupsId = resGrpRes.ResourceGroupsId
			   LEFT JOIN [UserData].[Resources] as res
               on  res.[ResourcesId] = resGrpRes.Resources
               WHERE ord.DatasetId in 
               (   
	            SELECT [DatasetId]
	            FROM [UserData].[Orders_Dataset]
	            WHERE name=N'Schedule'
               )
               AND
	            ord.StartTime is null
               AND 
	            res.FiniteOrInfinite=1

                    ";
            }
        }


        private string GetOrdersQuery
        {
            get
            {
                return @"
               SELECT 
	               ord.OrdersId as Id
	              ,ord.[BelongsToOrderNo] as BelongsToOrderNo
                  ,ord.[OrderNo] as OrderNo
                  ,ord.[OpNo] as OpNo
                  ,ord.[OperationName] as OpName
                  ,ord.[Quantity] as Quantity
                  ,ord.[Product] as Product
                  ,ord.[ResourceGroup] as ResourceGroup
	              ,res.Name as Resource
	              ,res.ResourcesId as ResourceId
                  ,ord.[EarliestStartDate] as EarliestStartDate
                  ,ord.[DueDate] as DueDate
                  ,ord.[Priority] as Priority
                  ,ord.[SetupStart] as SetupStart
                  ,ord.[SetupTime]*60*24 as SetupTime
                  ,ord.[StartTime] as StartTime
                  ,ord.[EndTime] as EndTime
                  ,ord.[PartNo] as PartNo
                  ,ord.[NumericalAttribute1] as NumericalAttribute1
                  ,ord.[NumericalAttribute2] as NumericalAttribute2
                  ,ord.[NumericalAttribute3] as NumericalAttribute3
                  ,ord.[NumericalAttribute15] as NumericalAttribute15
                  ,ord.[TableAttribute1] as TableAttribute1
                  ,ord.[TableAttribute1Rank] as TableAttribute1Rank
                  ,ord.[TableAttribute2] as TableAttribute2
                  ,ord.[TableAttribute2Rank] as TableAttribute2Rank
                  ,ord.[TableAttribute3] as TableAttribute3
                  ,ord.[TableAttribute3Rank] as TableAttribute3Rank
                  ,ord.[StringAttribute1] as StringAttribute1
                  ,ord.[StringAttribute2] as StringAttribute2
                  ,ord.[StringAttribute3] as StringAttribute3
                  ,ord.[ResSpecType] as ResSpecType
                  ,ord.[Hold] as Hold
               FROM [UserData].[Orders] as ord
               LEFT JOIN [UserData].[Resources] as res
               on  res.[ResourcesId] = ord.[Resource]


               WHERE
                ord.DatasetId in 
               (   
	            SELECT [DatasetId]
	            FROM [UserData].[Orders_Dataset]
	            WHERE name=N'Schedule'
               )
               AND 
	            ord.Resource is null
                    ";
            }
        }


        /// <summary>
        /// Получаем все спланированые секвенсерем операции
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OrderDTO> getAllScheduled()
        {
            IEnumerable<OrderDTO> list;
            using (var connection = new SqlConnection(_conString))
            {
                var sql = GetScheduledOrdersQuery;
                connection.Open();
                list = connection.Query<OrderDTO>(sql,commandType: CommandType.Text);
            };
            return list;
        }


        /// <summary>
        /// Получаем все спланированые секвенсерем операции
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OrderDTO> getAllUnScheduled()
        {
            IEnumerable<OrderDTO> list;
            using (var connection = new SqlConnection(_conString))
            {
                var sql = GetOrdersQuery;
                connection.Open();
                list = connection.Query<OrderDTO>(sql, commandType: CommandType.Text);
            };
            return list;
        }



        /// <summary>
        /// получаем операции спланированные на бесконечных ресурсах
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OrderDTO> getAllScheduledInfinite()
        {
            IEnumerable<OrderDTO> list;
            using (var connection = new SqlConnection(_conString))
            {
                var sql = GetScheduledOrdersQueryInfinite;
                connection.Open();
                list = connection.Query<OrderDTO>(sql, commandType: CommandType.Text);
            };
            return list;
        }

        /// <summary>
        /// получаем операции на бесконечных ресурсах
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OrderDTO> getAllInfinite()
        {
            IEnumerable<OrderDTO> list;
            using (var connection = new SqlConnection(_conString))
            {
                var sql = GetOrdersQueryInfinite;
                connection.Open();
                list = connection.Query<OrderDTO>(sql, commandType: CommandType.Text);
            };
            return list;
        }


        /// <summary>
        /// получаем операции на конечных ресурсах
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OrderDTO> getAllFinite()
        {
            IEnumerable<OrderDTO> list;
            using (var connection = new SqlConnection(_conString))
            {
                var sql = GetOrdersQueryFinite;
                connection.Open();
                list = connection.Query<OrderDTO>(sql, commandType: CommandType.Text);
            };
            return list;
        }



        /// <summary>
        /// получаем операции спланированные на конечных ресурсах
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OrderDTO> getAllScheduledFinite()
        {
            IEnumerable<OrderDTO> list;
            using (var connection = new SqlConnection(_conString))
            {
                var sql = GetScheduledOrdersQueryFinite;
                connection.Open();
                list = connection.Query<OrderDTO>(sql, commandType: CommandType.Text);
            };
            return list;
        }


        /// <summary>
        /// получаем операции спланированные на конечных ресурсах
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OrderDTO> getAllUnScheduledFinite()
        {
            IEnumerable<OrderDTO> list;
            using (var connection = new SqlConnection(_conString))
            {
                var sql = GetUnScheduledOrdersQueryFinite;
                connection.Open();
                list = connection.Query<OrderDTO>(sql, commandType: CommandType.Text);
            };
            return list;
        }


        ~OrderRepoSql()
        {
            LogManager.Flush();
        }

    }
}
