using Preactor;
using PreactorRepositoryService.DAL.DTOModels;
using PreactorRepositoryService.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.Services
{
    public class OrderManagementService
    {
        IPreactor _preactor;
        ShortagesRepo shtRepo;
        IgnoreShortagesRepo ignShtRepo;
        OrderRepo ordRepo;
        ProductRepo prdRepo;
        ResourceRepoSql resSqlRepo;
        IEnumerable<ResourceDTO> resList;
        private bool IsDayIntervalIntersected(DateTime st1, DateTime et1, DateTime st2, DateTime et2)
        {
            if ((st2<=st1 && et2>st1)||(st2>=st1 && st2<et1) || (st2 == st1 && et2 == et1))
            {
                return true;

            }
            return false;
        }





        public OrderManagementService(IPreactor preactor)
        {
            _preactor = preactor;
            shtRepo = new ShortagesRepo(_preactor);
            ignShtRepo = new IgnoreShortagesRepo(_preactor);
            ordRepo = new OrderRepo(_preactor);
            prdRepo = new ProductRepo(_preactor);
            resSqlRepo = new ResourceRepoSql(_preactor);
            resList = resSqlRepo.getAll();
        }
        //--------------------------------------------------------------------------------------
        //Ниже идут функции в который OrderDTO использует коллекцию Operations для аггрегирования операций в одном классе заказа
        //--------------------------------------------------------------------------------------
        private double GetTotalShortageQty(ShortagesDTO shtEl, bool ignoreFlag = false)
        {
            double summ = 0.0;
            foreach (ShortagesDTOItem shtItem in shtEl.items)
            {
                IgnoreShortagesDTO ignoreShortageFound = ignShtRepo.GetByNo(shtItem.PartNo);
                int ignFl =0;
                if (ignoreShortageFound!=null)
                {
                    ignFl = ignoreShortageFound.items.Where(x => x.InternalDemandOrder == shtEl.InternalDemandOrder
                                       && x.ExternalDemandOrder == shtEl.ExternalDemandOrder).FirstOrDefault().IgnoreShortages;
                }
                summ += (1 - ignFl) * shtItem.ShortageQuantity;
            }
            return summ;
        }

        public List<OrderDTO> GetOrdersFromShortages(bool ignoreFlag=false)
        {
            IEnumerable<ShortagesDTO> shortagesList = shtRepo.GetAll();
            List<OrderDTO> listResult = new List<OrderDTO>();
            foreach (ShortagesDTO shtEl in shortagesList)
            {
                string partNo = shtEl.PartNo;
                shtEl.ShortageQuantity = GetTotalShortageQty(shtEl, ignoreFlag);
                OrderDTO orderNew = new OrderDTO
                {
                    PartNo = partNo,
                    OpNo = 1,
                    OrderNo = "GNR" + Guid.NewGuid(),
                    Quantity = shtEl.ShortageQuantity
                };

                listResult.Add(orderNew);
            }
                return listResult;
        }
        public bool CreateOrdersFromShortages(bool ignoreFlag = false)
        {
            IEnumerable<ShortagesDTO> shortagesList = shtRepo.GetAll();
            List<OrderDTO> listResult = new List<OrderDTO>();
            foreach (ShortagesDTO shtEl in shortagesList)
            {
                string partNo = shtEl.PartNo;
                string product = prdRepo.GetByNo(partNo)?.Product;
                int opNo = prdRepo.GetByNo(partNo).OpNo;
                string opName = prdRepo.GetByNo(partNo)?.OpName;
                shtEl.ShortageQuantity = GetTotalShortageQty(shtEl, ignoreFlag);
                OrderDTO orderNew = new OrderDTO
                {
                    PartNo = partNo,
                    Product = product,
                    OpNo = opNo,
                    OpName = opName,
                    OrderNo = "GNR" + Guid.NewGuid(),
                    Quantity = shtEl.ShortageQuantity,
                };
                ordRepo.Create(orderNew);
                _preactor.Commit("Orders");


            }
            return true;
        }

        //--------------------------------------------------------------------------------------
        //Ниже идут функции в который OrderDTO не использует коллекцию Operations, один обьект одна операция
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Упорядочивает список операций дате наладки
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public IEnumerable<IGrouping<int, OrderDTO>> SortOperationListBySetupTime(IEnumerable<OrderDTO> list)
        {
            var resultList = from el in list
                             orderby el.ResourceId, el.TableAttribute1Rank, el.SetupStart
                             group el by el.ResourceId into r
                             select r;
            return resultList;
        }

        /// <summary>
        /// Упорядочивает список операций дате наладки
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public IEnumerable<IGrouping<int, OrderDTO>> SortOperationListByRankSetupTime(IEnumerable<OrderDTO> list)
        {
            var resultList = from el in list
                             orderby el.ResourceId, el.TableAttribute1Rank, el.SetupStart
                             group el by el.ResourceId into r
                             select r;
            return resultList;
        }

        /// <summary>
        /// Упорядочивает список операций дате наладки
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public IEnumerable<IGrouping<dynamic, OrderDTO>> SortOperationListBySetupTime_Rank1(IEnumerable<OrderDTO> list)
        {
            var resultList = from el in list
                                 //orderby el.ResourceId ascending, el.TableAttribute1Rank ascending, el.StartTime ascending
                             orderby el.ResourceId ascending, el.SetupStart ascending
                             group el by new { ResourceId = el.ResourceId, TableAttribute1Rank = el.TableAttribute1Rank, TableAttribute2Rank = el.TableAttribute2Rank, st = el.SetupTime } into r
                             select r;
            return resultList;
        }

        public IEnumerable<IGrouping<dynamic, OrderDTO>> SortOperationListBySetupTime_Rank1_2(IEnumerable<OrderDTO> list)
        {
            var resultList = from el in list
                             orderby el.ResourceId ascending, el.TableAttribute1Rank ascending, el.StartTime ascending, el.SetupTime ascending
                             group el by new { ResourceId = el.ResourceId, TableAttribute1Rank = el.TableAttribute1Rank, SetupTime = el.SetupTime } into r
                             select r;
            return resultList;
        }


        /// <summary>
        /// Нахождение последней операции в списке, заполняющей макс. ограничения ресурса
        /// </summary>
        /// <param name="listOp"></param>
        /// <returns></returns>
        public OrderDTO getLastOperationOnResource(IEnumerable<OrderDTO> listOp)
        {
            OrderDTO lastOp = listOp.Last();
            ResourceDTO resCurrent = resList.FirstOrDefault(x => x.Id == lastOp.ResourceId);
            double? resMaxResVal = resCurrent.SecConstraints.FirstOrDefault(x => x.Name.Contains(resCurrent.Name) == true)?.MaxValue;
            double resMinResVal = resCurrent.Attribute2;
            IEnumerable<OrderDTO> resListOp;

            //Если вторичные ограничения по этому рессурсу не прописаны(ограничение по названию рес.), значит он неограничен ничем.
            //проверяем по минимальному накоплению
            if (lastOp.ResourceId == 73)
            {


            }



            if (resMaxResVal == null)
            {
                resListOp = listOp.Sum(x3 => x3.NumericalAttribute15) >= resMinResVal ? listOp : null;
            }
            else //вторичные ограничения есть, проверяем по максимуму и минимуму ресурса
            {
                resListOp = listOp.TakeWhile(x =>
                {
                    double sumTemp = listOp.Where(x1 => x1.StartTime <= x.StartTime).Sum(x3 => x3.NumericalAttribute15);
                    return (sumTemp <= resMaxResVal);
                });
                resListOp = resListOp.Sum(x3 => x3.NumericalAttribute15) >= resMinResVal ? resListOp : null;
            }

            if (resListOp?.Count() > 0)
                return resListOp?.Last();
            else return null;
        }


        /// <summary>
        /// Нахождение последней операции в списке, заполняющей макс. ограничения ресурса,прямым перебором последовательности
        /// т.е не упорядоченой по времени операции, (упорядочение по рангу, операции разных рангов могут быть разбросаны по времени)
        /// </summary>
        /// <param name="listOp"></param>
        /// <returns></returns>
        public OrderDTO getLastOperationOnResourceSerial(IEnumerable<OrderDTO> listOp)
        {
            OrderDTO lastOp = listOp.Last();
            ResourceDTO resCurrent = resList.FirstOrDefault(x => x.Id == lastOp.ResourceId);
            double? resMaxResVal = resCurrent.SecConstraints.FirstOrDefault(x => x.Name.Contains(resCurrent.Name) == true)?.MaxValue;
            double resMinResVal = resCurrent.Attribute2;
            IEnumerable<OrderDTO> resListOp;
            //Если вторичные ограничения по этому рессурсу не прописаны(ограничение по названию рес.), значит он неограничен ничем.
            //проверяем по минимальному накоплению
            if (resMaxResVal == null)
            {
                resListOp = listOp.Sum(x3 => x3.NumericalAttribute15) > resMinResVal ? listOp : null;
            }
            else //вторичные ограничения есть, проверяем по максимуму и минимуму ресурса
            {
                double sum = 0;
                int countMax = 1;
                resListOp = listOp;
                foreach (var el in listOp)
                {

                    if (sum + el.NumericalAttribute15 > resMaxResVal)
                    {
                        resListOp = listOp.Take(countMax-1);
                        break;
                    }
                    sum += el.NumericalAttribute15;
                    countMax++;
                }

                resListOp = resListOp.Sum(x3 => x3.NumericalAttribute15) >= resMinResVal ? resListOp : null;
            }

            if (resListOp?.Count() > 0)
                return resListOp?.Last();
            else return null;
        }


        /// <summary>
        /// Устанавливает время начала операций по последней (группировка динамик - по нескольким ключам)
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public IEnumerable<IGrouping<dynamic, OrderDTO>> SetOperationsStartTimeByLast(IEnumerable<IGrouping<dynamic, OrderDTO>> list)
        {
            //обход по подгруппам (resource-rank)
            DateTime tTime = _preactor.PlanningBoard.TerminatorTime;
            DateTime prevMaxEndDate = default(DateTime);
            ResourceRepoSql resSqlRepo = new ResourceRepoSql(_preactor);
            int resId = -1;
            IEnumerable<OrderDTO> opListTemp = new List<OrderDTO>();
            IEnumerable<OrderDTO> opListTempBuffer = new List<OrderDTO>();
            PlannerService _plannerService = new PlannerService(_preactor);

            foreach (var gr in list)
            {


                //Если меняется ранк, то идет вычисление предыдущей макс даты 
                //Если меняется ресурс, макс дата сбрасывается.
                DateTime st1 = default(DateTime);
                DateTime et1 = default(DateTime);

                OrderDTO lastOp = getLastOperationOnResource(gr.ToList());
                if (lastOp!=null)
                {
                    st1 = lastOp.SetupStart;
                    et1 = lastOp.EndTime;
                }

                if (gr.Select(x => x.ResourceId).First() != resId)
                {
                    _plannerService.UnallocateOpOnResources(gr.Select(x => x.ResourceId).First(), true);
                    if (lastOp != null)
                    {
                        prevMaxEndDate = st1;
                    }
                    else
                    {
                        prevMaxEndDate = tTime;

                    }
                    opListTempBuffer = new List<OrderDTO>();//обнуляем буфер уже запланированых операций на ресурсе
                }
                else
                {
                    if (opListTempBuffer.Count()>0)
                    {
                        List<OrderDTO> testIntervalList = opListTempBuffer.Where(x => {
                            return IsDayIntervalIntersected(st1, et1, x.SetupStart, x.EndTime);
                        }).ToList();
                        if (testIntervalList.Count()==0)
                        {
                            prevMaxEndDate = st1;
                        }
                        else
                        {
                            prevMaxEndDate= opListTempBuffer.Max(x=>x.EndTime);
                        }

                    }


                }


                opListTemp = gr.ToList();



                resId = gr.Select(x => x.ResourceId).First();
                int resNumber = _preactor.GetRecordNumber("Resources", new PrimaryKey(resId));
                while (opListTemp.Count() > 0)
                {



                    lastOp = getLastOperationOnResource(opListTemp);
                    if (lastOp == null)
                    {
                        //оставшееся подмножество операций больше не удовлетворяет условиям
                        break;
                    }

                    DateTime setupStart = lastOp.SetupStart;
                    DateTime startTime = lastOp.StartTime;
                    DateTime endTime = lastOp.EndTime;

                    int opNumber = _preactor.GetRecordNumber("Orders", new PrimaryKey(lastOp.Id));

                    prevMaxEndDate = tTime;

                    //Проверяем на пересечение текущего штабеля операций  с уже запланироваными  
                    if (opListTempBuffer.Count() > 0)
                    {
                        List<OrderDTO> testIntervalList = opListTempBuffer.Where(x => {
                            return IsDayIntervalIntersected(lastOp.SetupStart, lastOp.EndTime, x.SetupStart, x.EndTime);
                        }).ToList();
                        if (testIntervalList.Count() == 0)
                        {
                            prevMaxEndDate = lastOp.SetupStart;
                        }
                        else
                        {
                            //если текущий штабель пересекает запланированые, выясняем время тестированием от терминатора
                            prevMaxEndDate = tTime;
                            OperationTimes? times1 = _preactor.PlanningBoard.TestOperationOnResource(opNumber, resNumber, prevMaxEndDate);
                            if (times1 != null)
                                testIntervalList = opListTempBuffer.Where(x => {
                                    return IsDayIntervalIntersected(times1.Value.ChangeStart, times1.Value.ProcessEnd, x.SetupStart, x.EndTime);
                                }).ToList();


                            if (testIntervalList.Count() == 0)
                            {
                                prevMaxEndDate = times1.Value.ChangeStart;
                            }
                            else
                                prevMaxEndDate = opListTempBuffer.Max(x => x.EndTime);//как последний вариант ставим в конец всех операций
                            
                        }

                    }
                    //-------------------------------------

                    OperationTimes? times = _preactor.PlanningBoard.TestOperationOnResource(opNumber, resNumber, prevMaxEndDate);
                    //При подсчете разницы, учитываем на тестовое время,что есть периоды недоступности ресурсов
                    if (times.HasValue)
                    {

                        DateTime testTimeStart = times.Value.ProcessStart;
                        DateTime testTimeSetup = times.Value.ChangeStart;
                        DateTime testTimeEnd = times.Value.ProcessEnd;
                        startTime = testTimeStart;
                        setupStart = testTimeSetup;
                        endTime = testTimeEnd;
                    }
                    else
                    {

                        times = _preactor.PlanningBoard.QueryOperationOnResource(opNumber, resNumber, prevMaxEndDate);
                        if (times != null)
                        {
                            DateTime testTimeStart = times.Value.ProcessStart;
                            DateTime testTimeSetup = times.Value.ChangeStart;
                            DateTime testTimeEnd = times.Value.ProcessEnd;
                            startTime = testTimeStart;
                            setupStart = testTimeSetup;
                            endTime = testTimeEnd;
                        }
                    }


                    var subListOp = opListTemp.Where(x => x.StartTime <= lastOp.StartTime).ToList();
                    opListTemp = opListTemp.Except(subListOp).ToList();


                    foreach (var el in subListOp)
                    {
                        //if (el.SetupStart < tTime)
                          //  continue;

                        el.SetupStart = setupStart;
                        el.StartTime = startTime;
                        TimeSpan tsStart = endTime - el.StartTime;
                        TimeSpan tsSetup = endTime - el.SetupStart;
                        el.EndTime = el.StartTime.Add(tsStart);
                        el.Scheduled = true;
                    }
                    prevMaxEndDate = subListOp.Max(x => x.EndTime);
                    opListTempBuffer = opListTempBuffer.Concat(subListOp);//постепенно заполняем буфер запланированых операций


                }//while


            }//foreach


            return list;
        }
        public IEnumerable<IGrouping<dynamic, OrderDTO>> SetOperationsStartTimeByLast2(IEnumerable<IGrouping<dynamic, OrderDTO>> list)
        {
            //обход по подгруппам (resource-rank)
            DateTime tTime = _preactor.PlanningBoard.TerminatorTime;
            DateTime prevMaxEndDate = default(DateTime);
            ResourceRepoSql resSqlRepo = new ResourceRepoSql(_preactor);
            int resId = -1;
            IEnumerable<OrderDTO> opListTemp = new List<OrderDTO>();
            IEnumerable<OrderDTO> opListTempBuffer = new List<OrderDTO>();
            PlannerService _plannerService = new PlannerService(_preactor);

            foreach (var gr in list)
            {


                //Если меняется ранк, то идет вычисление предыдущей макс даты 
                //Если меняется ресурс, макс дата сбрасывается.
                DateTime st1 = default(DateTime);
                DateTime et1 = default(DateTime);

                OrderDTO lastOp = getLastOperationOnResource(gr.ToList());
                if (lastOp != null)
                {
                    st1 = lastOp.SetupStart;
                    et1 = lastOp.EndTime;
                }

                if (gr.Select(x => x.ResourceId).First() != resId)
                {
                    _plannerService.UnallocateOpOnResources(gr.Select(x => x.ResourceId).First(), true);
                    if (lastOp != null)
                    {
                        prevMaxEndDate = st1;
                    }
                    else
                    {
                        prevMaxEndDate = tTime;

                    }
                    opListTempBuffer = new List<OrderDTO>();//обнуляем буфер уже запланированых операций на ресурсе
                }
                else
                {
                    if (opListTempBuffer.Count() > 0)
                    {
                        List<OrderDTO> testIntervalList = opListTempBuffer.Where(x => {
                            return IsDayIntervalIntersected(st1, et1, x.SetupStart, x.EndTime);
                        }).ToList();
                        if (testIntervalList.Count() == 0)
                        {
                            prevMaxEndDate = st1;
                        }
                        else
                        {
                            prevMaxEndDate = opListTempBuffer.Max(x => x.EndTime);
                        }

                    }


                }


                opListTemp = gr.ToList();



                resId = gr.Select(x => x.ResourceId).First();
                int resNumber = _preactor.GetRecordNumber("Resources", new PrimaryKey(resId));

                while (opListTemp.Count() > 0)
                {



                    lastOp = getLastOperationOnResource(opListTemp);
                    if (lastOp == null)
                    {
                        //оставшееся подмножество операций больше не удовлетворяет условиям
                        break;
                    }

                    DateTime setupStart = lastOp.SetupStart;
                    DateTime startTime = lastOp.StartTime;
                    DateTime endTime = lastOp.EndTime;

                    int opNumber = _preactor.GetRecordNumber("Orders", new PrimaryKey(lastOp.Id));

                    prevMaxEndDate = tTime;

                    //Проверяем на пересечение текущего штабеля операций  с уже запланироваными  
                    if (opListTempBuffer.Count() > 0)
                    {
                        List<OrderDTO> testIntervalList = opListTempBuffer.Where(x => {
                            return IsDayIntervalIntersected(lastOp.SetupStart, lastOp.EndTime, x.SetupStart, x.EndTime);
                        }).ToList();
                        if (testIntervalList.Count() == 0)
                        {
                            prevMaxEndDate = lastOp.SetupStart;
                        }
                        else
                        {
                            //если текущий штабель пересекает запланированые, выясняем время тестированием от терминатора
                            prevMaxEndDate = tTime;
                            OperationTimes? times1 = _preactor.PlanningBoard.TestOperationOnResource(opNumber, resNumber, prevMaxEndDate);
                            if (times1 != null)
                                testIntervalList = opListTempBuffer.Where(x => {
                                    return IsDayIntervalIntersected(times1.Value.ChangeStart, times1.Value.ProcessEnd, x.SetupStart, x.EndTime);
                                }).ToList();


                            if (testIntervalList.Count() == 0)
                            {
                                prevMaxEndDate = times1.Value.ChangeStart;
                            }
                            else
                                prevMaxEndDate = opListTempBuffer.Max(x => x.EndTime);//как последний вариант ставим в конец всех операций

                        }

                    }
                    //-------------------------------------

                    OperationTimes? times = _preactor.PlanningBoard.TestOperationOnResource(opNumber, resNumber, prevMaxEndDate);
                    //При подсчете разницы, учитываем на тестовое время,что есть периоды недоступности ресурсов
                    if (times.HasValue)
                    {

                        DateTime testTimeStart = times.Value.ProcessStart;
                        DateTime testTimeSetup = times.Value.ChangeStart;
                        DateTime testTimeEnd = times.Value.ProcessEnd;
                        startTime = testTimeStart;
                        setupStart = testTimeSetup;
                        endTime = testTimeEnd;
                    }
                    else
                    {

                        times = _preactor.PlanningBoard.QueryOperationOnResource(opNumber, resNumber, prevMaxEndDate);

                        DateTime testTimeStart = times.Value.ProcessStart;
                        DateTime testTimeSetup = times.Value.ChangeStart;
                        DateTime testTimeEnd = times.Value.ProcessEnd;
                        startTime = testTimeStart;
                        setupStart = testTimeSetup;
                        endTime = testTimeEnd;
                    }


                    var subListOp = opListTemp.Where(x => x.StartTime <= lastOp.StartTime).ToList();
                    opListTemp = opListTemp.Except(subListOp).ToList();


                    foreach (var el in subListOp)
                    {
                        if (el.SetupStart < tTime)
                            continue;

                        el.SetupStart = setupStart;
                        el.StartTime = startTime;
                        TimeSpan tsStart = endTime - el.StartTime;
                        TimeSpan tsSetup = endTime - el.SetupStart;
                        el.EndTime = el.StartTime.Add(tsStart);
                        el.Scheduled = true;
                    }
                    prevMaxEndDate = subListOp.Max(x => x.EndTime);
                    opListTempBuffer = opListTempBuffer.Concat(subListOp);//постепенно заполняем буфер запланированых операций


                }//while


            }//foreach


            return list;
        }

        /// <summary>
        /// Устанавливает время начала операций по последней(группировка только по ресурсу)
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public IEnumerable<IGrouping<int, OrderDTO>> SetOperationsStartTimeByLast2(IEnumerable<IGrouping<int, OrderDTO>> list)
        {
            //обход по подгруппам (resource-rank)
            DateTime tTime = _preactor.PlanningBoard.TerminatorTime;

            foreach (var gr in list)
            {
                IEnumerable<OrderDTO> opListTemp = gr.ToList();
                int resId = gr.Select(x => x.ResourceId).First();
                OrderDTO lastOpRes = getLastOperationOnResource(opListTemp.OrderBy(p => p.SetupStart));
                DateTime prevMaxEndDate = lastOpRes != null ? lastOpRes.SetupStart : default(DateTime);

                while (opListTemp.Count() > 0)
                {
                    OrderDTO lastOp = getLastOperationOnResource(opListTemp);
                    if (lastOp == null)
                    {
                        //оставшееся подмножество операций больше не удовлетворяет условиям
                        break;
                    }

                    DateTime setupStart = lastOp.SetupStart;
                    DateTime startTime = lastOp.StartTime;
                    DateTime endTime = lastOp.EndTime;
                    int resNumber = _preactor.GetRecordNumber("Resources", new PrimaryKey(resId));
                    int opNumber = _preactor.GetRecordNumber("Orders", new PrimaryKey(lastOp.Id));
                    OperationTimes? times = _preactor.PlanningBoard.TestOperationOnResource(opNumber, resNumber, prevMaxEndDate);
                    //При подсчете разницы, учитываем на тестовое время,что есть периоды недоступности ресурсов

                    if (times.HasValue)
                    {
                        DateTime testTimeStart = times.Value.ProcessStart;
                        DateTime testTimeSetup = times.Value.ChangeStart;
                        DateTime testTimeEnd = times.Value.ProcessEnd;
                        if (testTimeStart>startTime)
                        {
                            startTime = testTimeStart;
                            setupStart = testTimeSetup;
                            endTime = testTimeEnd;
                        }
                    }

                    var subListOp = opListTemp.Where(x => x.SetupStart <= lastOp.SetupStart);
                    foreach (var el in subListOp)
                    {
                        if (el.SetupStart < tTime)
                            continue;

                        el.SetupStart = setupStart;
                        el.StartTime = startTime;
                        TimeSpan tsStart = endTime - el.StartTime;
                        TimeSpan tsSetup = endTime - el.SetupStart;
                        el.EndTime = el.StartTime.Add(tsStart);
                        el.Scheduled = true;
                    }
                    prevMaxEndDate = subListOp.Max(x => x.EndTime);
                    opListTemp = opListTemp.Except(subListOp);

                }

            }


            return list;
        }

        /// <summary>
        /// Устанавливает время начала операций по последней(группировка только по ресурсу)
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public IEnumerable<IGrouping<int, OrderDTO>> SetOperationsStartTimeByLast(IEnumerable<IGrouping<int, OrderDTO>> list)
        {
            //обход по подгруппам (resource-rank)
            DateTime tTime = _preactor.PlanningBoard.TerminatorTime;

            foreach (var gr in list)
            {
                IEnumerable<OrderDTO> opListTemp = gr.ToList();
                int resId = gr.Select(x => x.ResourceId).First();
                IEnumerable<OrderDTO> opList2 = opListTemp.OrderBy(p => p.SetupStart);

                OrderDTO lastOpRes = getLastOperationOnResource(opList2);
                DateTime prevMaxEndDate = lastOpRes!=null?lastOpRes.SetupStart:default(DateTime);

                while (opListTemp.Count() > 0)
                {
                    OrderDTO lastOp = getLastOperationOnResourceSerial(opListTemp);
                    if (lastOp == null)
                    {
                        //оставшееся подмножество операций больше не удовлетворяет условиям
                        break;
                    }

                    DateTime setupStart = lastOp.SetupStart;
                    DateTime startTime = lastOp.StartTime;
                    DateTime endTime = lastOp.EndTime;
                    int resNumber = _preactor.GetRecordNumber("Resources", new PrimaryKey(resId));
                    int opNumber = _preactor.GetRecordNumber("Orders", new PrimaryKey(lastOp.Id));
                    OperationTimes? times = _preactor.PlanningBoard.TestOperationOnResource(opNumber, resNumber, prevMaxEndDate);
                    //При подсчете разницы, учитываем на тестовое время,что есть периоды недоступности ресурсов

                    if (times.HasValue)
                    {
                        DateTime testTimeStart = times.Value.ProcessStart;
                        DateTime testTimeSetup = times.Value.ChangeStart;
                        DateTime testTimeEnd = times.Value.ProcessEnd;
                        //if (testTimeStart > startTime)
                        {
                            startTime = testTimeStart;
                            setupStart = testTimeSetup;
                            endTime = testTimeEnd;
                        }
                    }

                    var subListOp = opListTemp.TakeWhile(x => x.Id != lastOp.Id).Union(opListTemp.Where(x=>x.Id==lastOp.Id));
                    foreach (var el in subListOp)
                    {
                        if (el.SetupStart < tTime)
                            continue;

                        el.SetupStart = setupStart;
                        el.StartTime = startTime;
                        TimeSpan tsStart = endTime - el.StartTime;
                        TimeSpan tsSetup = endTime - el.SetupStart;
                        el.EndTime = el.StartTime.Add(tsStart);
                        el.Scheduled = true;
                    }
                    prevMaxEndDate = subListOp.Max(x => x.EndTime);
                    opListTemp = opListTemp.Except(subListOp);

                }

            }


            return list;
        }





    }
}
