using NLog;
using Preactor;
using PreactorRepositoryService.DAL.DTOModels;
using PreactorRepositoryService.DAL.Interfaces;
using System;
using System.Collections.Generic;

namespace PreactorRepositoryService.DAL.Repository
{
    public class DemandRepo : IRepository<DemandDTO>
    {
        private string _entityName = "Demand";
        private static Logger logger;
        private ResourceGroupRepo resGrpRepo;
        private ResourceRepo resRepo;
        private IPreactor _preactor;

        public DemandRepo(IPreactor preactor)
        {
            DemandRepo.logger = LogManager.GetCurrentClassLogger();
            DemandRepo.logger.Trace("Создан экземпляр сервиса DemandRepo");
            this._preactor = preactor;
        }

        public bool Create(DemandDTO obj)
        {
            if (this.GetByNo(obj.OrderNo) != null)
            {
                DemandRepo.logger.Trace(string.Format("Экземпляр заказа Demand уже существует {0}", (object)obj.OrderNo));
                return false;
            }
            int record = this._preactor.CreateRecord(this._entityName);
            this._preactor.WriteField(this._entityName, "Belongs to Order No.", record, obj.BelongsToOrderNo);
            this._preactor.WriteField(this._entityName, "Number", record, obj.Id);
            this._preactor.WriteField(this._entityName, "Order No.", record, obj.OrderNo);
            this._preactor.WriteField(this._entityName, "Order Type", record, obj.OrderType);
            this._preactor.WriteField(this._entityName, "Order Line", record, obj.OrderLine);
            this._preactor.WriteField(this._entityName, "Davalec", record, obj.Davalec);
            this._preactor.WriteField(this._entityName, "PartNoOld", record, obj.PartNoOld);
            this._preactor.WriteField(this._entityName, "DemandState", record, obj.DemandState);
            this._preactor.WriteField(this._entityName, "OrderNo1c", record, obj.OrderNo1c);
            this._preactor.WriteField(this._entityName, "StringNo1c", record, obj.StringNo1c);
            this._preactor.WriteField(this._entityName, "Owner", record, obj.Owner);
            this._preactor.WriteField(this._entityName, "OwnerNo", record, obj.OwnerNo);
            this._preactor.WriteField(this._entityName, "String Attribute 1", record, obj.StringAttribute1);
            this._preactor.WriteField(this._entityName, "String Attribute 2", record, obj.StringAttribute2);
            this._preactor.WriteField(this._entityName, "String Attribute 3", record, obj.StringAttribute3);
            this._preactor.WriteField(this._entityName, "String Attribute 4", record, obj.StringAttribute4);
            this._preactor.WriteField(this._entityName, "DateMonthRO 2", record, obj.DateMonthRO2);
            this._preactor.WriteField(this._entityName, "Part No.", record, obj.PartNo);
            this._preactor.WriteField(this._entityName, "Description", record, obj.Description);
            this._preactor.WriteField(this._entityName, "Demand Date", record, obj.DemandDate);
            this._preactor.WriteField(this._entityName, "Priority", record, obj.Priority);
            this._preactor.WriteField(this._entityName, "Quantity", record, obj.Quantity);
            DemandRepo.logger.Trace(string.Format("Создан экземпляр заказа Demand {0}", (object)obj.OrderNo));
            return true;
        }

        public void Delete(int id)
        {
            try
            {
                this._preactor.DeleteRecord(this._entityName, this._preactor.GetRecordNumber(this._entityName, new PrimaryKey((double)id)));
                DemandRepo.logger.Trace(string.Format("Экземпляр Demand успешно удален, номер записи {0}", (object)id));
            }
            catch (Exception ex)
            {
                DemandRepo.logger.Trace(string.Format("Ошибка удаления экземпляра Demand {0} {1}", (object)ex.Message, (object)id));
            }
        }

        public IEnumerable<DemandDTO> GetAll()
        {
            DemandRepo.logger.Trace("Получение списка ордеров начато");
            int num = this._preactor.RecordCount(this._entityName);
            string DemandNo = "-1";
            string str = "";
            List<DemandDTO> demandDtoList = new List<DemandDTO>();
            for (int record = 1; record <= num; ++record)
            {
                try
                {
                    DemandNo = this._preactor.ReadFieldString(this._entityName, "Order No.", record);
                }
                catch (Exception ex)
                {
                    DemandRepo.logger.Error(ex.Message + " (получение Order No. по всем записям) " + ex.TargetSite.Name);
                }
                if (DemandNo != str)
                {
                    DemandDTO byNo = this.GetByNo(DemandNo);
                    demandDtoList.Add(byNo);
                }
                str = DemandNo;
            }
            DemandRepo.logger.Trace("Получение списка заказов закончено");
            return (IEnumerable<DemandDTO>)demandDtoList;
        }

        public DemandDTO GetByNo(string DemandNo)
        {
            DemandRepo.logger.Trace(string.Format("Получение ордера по номеру начато {0}", (object)DemandNo));
            int matchingRecord = this._preactor.FindMatchingRecord(this._entityName, 0, string.Format("~{{$Order No.}}~==~{0}~", (object)DemandNo));
            int record = matchingRecord;
            if (matchingRecord < 0)
                return (DemandDTO)null;
            try
            {
                DemandDTO demandDto = new DemandDTO()
                {
                    BelongsToOrderNo = this._preactor.ReadFieldString(this._entityName, "Belongs to Order No.", record),
                    Id = this._preactor.ReadFieldInt(this._entityName, "Number", record),
                    OrderNo = this._preactor.ReadFieldString(this._entityName, "Order No.", record),
                    OrderType = this._preactor.ReadFieldString(this._entityName, "Order Type", record),
                    OrderLine = this._preactor.ReadFieldInt(this._entityName, "Order Line", record),
                    Davalec = this._preactor.ReadFieldInt(this._entityName, "Davalec", record),
                    PartNoOld = this._preactor.ReadFieldString(this._entityName, "PartNoOld", record),
                    DemandState = this._preactor.ReadFieldString(this._entityName, "DemandState", record),
                    OrderNo1c = this._preactor.ReadFieldString(this._entityName, "OrderNo1c", record),
                    StringNo1c = this._preactor.ReadFieldString(this._entityName, "StringNo1c", record),
                    Owner = this._preactor.ReadFieldString(this._entityName, "Owner", record),
                    OwnerNo = this._preactor.ReadFieldString(this._entityName, "OwnerNo", record),
                    StringAttribute1 = this._preactor.ReadFieldString(this._entityName, "String Attribute 1", record),
                    StringAttribute2 = this._preactor.ReadFieldString(this._entityName, "String Attribute 2", record),
                    StringAttribute3 = this._preactor.ReadFieldString(this._entityName, "String Attribute 3", record),
                    StringAttribute4 = this._preactor.ReadFieldString(this._entityName, "String Attribute 4", record),
                    PartNo = this._preactor.ReadFieldString(this._entityName, "Part No.", record),
                    Description = this._preactor.ReadFieldString(this._entityName, "Description", record),
                    DemandDate = this._preactor.ReadFieldDateTime(this._entityName, "Demand Date", record),
                    Priority = (double)this._preactor.ReadFieldInt(this._entityName, "Priority", record),
                    Quantity = this._preactor.ReadFieldDouble(this._entityName, "Quantity", record)
                };
                DemandRepo.logger.Trace(string.Format("Получение ордера по номеру завершено {0}", (object)DemandNo));
                return demandDto;
            }
            catch (Exception ex)
            {
                DemandRepo.logger.Error(ex.Message + " (Demand header)" + ex.TargetSite.Name + " Номер заказа" + DemandNo);
                return (DemandDTO)null;
            }
        }

        public void Update(DemandDTO obj)
        {
            DemandDTO byNo = this.GetByNo(obj.PartNo);
            if (byNo == null)
            {
                DemandRepo.logger.Trace(string.Format("Экземпляр DemandDTO не найден, нечего обновлять {0}", (object)obj.PartNo));
            }
            else
            {
                int id = byNo.Id;
                try
                {
                    this.Delete(byNo.Id);
                    if (!this.Create(obj))
                        return;
                    DemandRepo.logger.Trace(string.Format("Обновлен экземпляр DemandDTO {0}", (object)obj.PartNo));
                }
                catch (Exception ex)
                {
                    DemandRepo.logger.Trace(string.Format("Ошибка создания экземпляра DemandDTO {0} {1}", (object)ex.Message, (object)obj.PartNo));
                }
            }
        }

        public DemandDTO GetById(int _id)
        {
            if (_id < 0)
                return (DemandDTO)null;
            DemandRepo.logger.Trace(string.Format("Получение ордера по номеру начато {0}", (object)_id));
            int matchingRecord = this._preactor.FindMatchingRecord(this._entityName, 0, string.Format("~{{#Number}}~==~{0}~", (object)_id));
            int record = matchingRecord;
            if (matchingRecord < 0)
                return (DemandDTO)null;
            try
            {
                DemandDTO demandDto = new DemandDTO()
                {
                    BelongsToOrderNo = this._preactor.ReadFieldString(this._entityName, "Belongs to Order No.", record),
                    Id = this._preactor.ReadFieldInt(this._entityName, "Number", record),
                    OrderNo = this._preactor.ReadFieldString(this._entityName, "Order No.", record),
                    OrderType = this._preactor.ReadFieldString(this._entityName, "Order Type", record),
                    OrderLine = this._preactor.ReadFieldInt(this._entityName, "Order Line", record),
                    Davalec = this._preactor.ReadFieldInt(this._entityName, "Davalec", record),
                    PartNoOld = this._preactor.ReadFieldString(this._entityName, "PartNoOld", record),
                    DemandState = this._preactor.ReadFieldString(this._entityName, "DemandState", record),
                    OrderNo1c = this._preactor.ReadFieldString(this._entityName, "OrderNo1c", record),
                    StringNo1c = this._preactor.ReadFieldString(this._entityName, "StringNo1c", record),
                    Owner = this._preactor.ReadFieldString(this._entityName, "Owner", record),
                    OwnerNo = this._preactor.ReadFieldString(this._entityName, "OwnerNo", record),
                    StringAttribute1 = this._preactor.ReadFieldString(this._entityName, "String Attribute 1", record),
                    StringAttribute2 = this._preactor.ReadFieldString(this._entityName, "String Attribute 2", record),
                    StringAttribute3 = this._preactor.ReadFieldString(this._entityName, "String Attribute 3", record),
                    StringAttribute4 = this._preactor.ReadFieldString(this._entityName, "String Attribute 4", record),
                    PartNo = this._preactor.ReadFieldString(this._entityName, "Part No.", record),
                    Description = this._preactor.ReadFieldString(this._entityName, "Description", record),
                    DemandDate = this._preactor.ReadFieldDateTime(this._entityName, "Demand Date", record),
                    Priority = (double)this._preactor.ReadFieldInt(this._entityName, "Priority", record),
                    Quantity = this._preactor.ReadFieldDouble(this._entityName, "Quantity", record)
                };
                DemandRepo.logger.Trace(string.Format("Получение ордера по id завершено {0}", (object)_id));
                return demandDto;
            }
            catch (Exception ex)
            {
                DemandRepo.logger.Error(ex.Message + " (Demand header)" + ex.TargetSite.Name + " Номер заказа" + (object)_id);
                return (DemandDTO)null;
            }
        }

        ~DemandRepo()
        {
            LogManager.Flush();
        }
    }
}
