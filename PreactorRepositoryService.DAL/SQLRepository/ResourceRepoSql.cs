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
    public class ResourceRepoSql

    {
        private static Logger logger;
        private ResourceGroupRepo resGrpRepo;
        private IPreactor _preactor;
        private string _entityName = "Resources";
        private string _conString;
        private void DapperInit()
        {
            var secConstrMap = new CustomPropertyTypeMap(
                typeof(SecondaryConstraintDTO),
                (type, columnName) =>
                {
                    if (columnName == "SecConstraintsId")
                    {
                        return type.GetProperty("Id");
                    }

                    if (columnName == "SecConstraintsName")
                    {
                        return type.GetProperty("Name");
                    }
                    if (columnName == "SecConstraintsUseAsAConstraint")
                    {
                        return type.GetProperty("UseAsAConstraint");
                    }
                    if (columnName == "SecConstraintsCalendarEffect")
                    {
                        return type.GetProperty("CalendarEffect");
                    }
                    if (columnName == "SecConstraintsMaxValue")
                    {
                        return type.GetProperty("MaxValue");
                    }
                    if (columnName == "SecConstraintsMinValue")
                    {
                        return type.GetProperty("MinValue");
                    }
                    if (columnName == "SecConstraintsReferenceDate")
                    {
                        return type.GetProperty("ReferenceDate");
                    }
                    if (columnName == "SecConstraintsFromDate")
                    {
                        return type.GetProperty("FromDate");
                    }
                    if (columnName == "SecConstraintsToDate")
                    {
                        return type.GetProperty("ToDate");
                    }
                    if (columnName == "SecConstraintsDim")
                    {
                        return type.GetProperty("Dim");
                    }


                    throw new InvalidOperationException($"No matching mapping for {columnName}");
                }
            );
            Dapper.SqlMapper.SetTypeMap(typeof(SecondaryConstraintDTO), secConstrMap);            

        }





        public ResourceRepoSql(IPreactor preactor)
        {
            _conString = preactor.ParseShellString("{DB CONNECT STRING}");
            logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Создан экземпляр сервиса ResourceRepoSql");
            this._preactor = preactor;
            resGrpRepo = new ResourceGroupRepo(_preactor);
            DapperInit();
        }

        private string GetResourcesQuery
        {
            get
            {
                return @"
                        
                        SELECT 
	                           res.[ResourcesId] as Id
                              ,res.[Name] as Name
                              ,res.[FiniteOrInfinite] as FiniteOrInfinite
                              ,res.[InfiniteModeBehavior] as InfiniteModeBehavior 
                              ,res.[ChangeoverGroup] as ChangeoverGroup
                              ,res.[Attribute1] as Attribute1
                              ,res.[Attribute2] as Attribute2
                              ,res.[Attribute3] as Attribute3
	                          ,sc.SecondaryConstraintsId as SecConstraintsId
	                          ,scp.ReferenceDate as SecConstraintsReferenceDate
	                          ,scp.FromDate as SecConstraintsFromDate
	                          ,scp.ToDate as SecConstraintsToDate
	                          ,sc.Name as SecConstraintsName
                              ,sc.CalendarEffect as SecConstraintsCalendarEffect
                              ,scp.MaxValue as SecConstraintsDim
	                          ,scp.MaxValue as SecConstraintsMaxValue
	                          ,scp.MinValue as SecConstraintsMinValue
                          FROM [UserData].[Resources] res
                          LEFT JOIN [UserData].[ResourcesSecondaryConstraints] rsc
                              on rsc.ResourcesId = res.ResourcesId
                          LEFT JOIN  [UserData].[SecondaryCalendarPeriods] scp
                              on scp.Resource = rsc.SecondaryConstraints
                          LEFT JOIN  [UserData].[SecondaryConstraints] sc
                              on sc.SecondaryConstraintsId = rsc.SecondaryConstraints
                        ";
            }
        }

        private string GetResourcesWithGroupsQuery
        {
            get
            {
                return @"
                        
                        SELECT 
	                           res.[ResourcesId] as Id
                              ,res.[Name] as Name
							  ,rg.Name as ResourceGroupName
                              ,res.[FiniteOrInfinite] as FiniteOrInfinite
                              ,res.[InfiniteModeBehavior] as InfiniteModeBehavior 
                              ,res.[ChangeoverGroup] as ChangeoverGroup
                              ,res.[Attribute1] as Attribute1
                              ,res.[Attribute2] as Attribute2
                              ,res.[Attribute3] as Attribute3
	                          ,sc.SecondaryConstraintsId as SecConstraintsId
	                          ,scp.ReferenceDate as SecConstraintsReferenceDate
	                          ,scp.FromDate as SecConstraintsFromDate
	                          ,scp.ToDate as SecConstraintsToDate
	                          ,sc.Name as SecConstraintsName
                              ,sc.CalendarEffect as SecConstraintsCalendarEffect
                              ,scp.MaxValue as SecConstraintsDim
	                          ,scp.MaxValue as SecConstraintsMaxValue
	                          ,scp.MinValue as SecConstraintsMinValue
                          FROM [UserData].[Resources] res
						  LEFT JOIN [UserData].[ResourceGroupsResources] rgr
							  on rgr.Resources = res.ResourcesId
						  LEFT JOIN [UserData].[ResourceGroups] rg
							  on rg.ResourceGroupsId = rgr.ResourceGroupsId
                          LEFT JOIN [UserData].[ResourcesSecondaryConstraints] rsc
                              on rsc.ResourcesId = res.ResourcesId
                          LEFT JOIN  [UserData].[SecondaryCalendarPeriods] scp
                              on scp.Resource = rsc.SecondaryConstraints
                          LEFT JOIN  [UserData].[SecondaryConstraints] sc
                              on sc.SecondaryConstraintsId = rsc.SecondaryConstraints
                        ";
            }
        }

        /// <summary>
        /// Получаем все ресурсы в базе
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ResourceDTO> getAll()
        {
            IEnumerable<ResourceDTO> list;
            Dictionary<int, ResourceDTO> baseObject = new Dictionary<int, ResourceDTO>();
            using (var connection = new SqlConnection(_conString))
            {
                var sql = GetResourcesQuery;

                connection.Open();
                list = connection.Query<ResourceDTO,SecondaryConstraintDTO,ResourceDTO>(sql,(res,secConstr)=>
                {
                    ResourceDTO resEnt;

                    if (!baseObject.TryGetValue(res.Id, out resEnt))
                    {
                        baseObject.Add(res.Id, resEnt = res);
                    }

                    if (resEnt.SecConstraints == null)
                        resEnt.SecConstraints = new List<SecondaryConstraintDTO>();
                    if (secConstr!=null)
                        if (!resEnt.SecConstraints.Any(x => x.Id == secConstr.Id))
                            resEnt.SecConstraints.Add(secConstr);
                   
                    return resEnt;
                },splitOn: "SecConstraintsId");
            };
            return baseObject.Values.ToList();
        }


        /// <summary>
        /// Получаем все ресурсы с их группами
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ResourceDTO> getAllWithGroup()
        {
            IEnumerable<ResourceDTO> list;
            Dictionary<int, ResourceDTO> baseObject = new Dictionary<int, ResourceDTO>();
            using (var connection = new SqlConnection(_conString))
            {
                var sql = GetResourcesWithGroupsQuery;

                connection.Open();
                list = connection.Query<ResourceDTO, SecondaryConstraintDTO, ResourceDTO>(sql, (res, secConstr) =>
                {
                    ResourceDTO resEnt;

                    if (!baseObject.TryGetValue(res.Id, out resEnt))
                    {
                        baseObject.Add(res.Id, resEnt = res);
                    }

                    if (resEnt.SecConstraints == null)
                        resEnt.SecConstraints = new List<SecondaryConstraintDTO>();
                    if (secConstr != null)
                        if (!resEnt.SecConstraints.Any(x => x.Id == secConstr.Id))
                            resEnt.SecConstraints.Add(secConstr);

                    return resEnt;
                }, splitOn: "SecConstraintsId");
            };
            return baseObject.Values.ToList();
        }


        ~ResourceRepoSql()
        {
            LogManager.Flush();
        }

    }
}
