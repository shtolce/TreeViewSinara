using NLog;
using Preactor;
using PreactorRepositoryService.DAL.DTOModels;
using PreactorRepositoryService.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.Repository
{
    public class ShortagesRepo : IRepository<ShortagesDTO>
    {
        private string _entityName = "Shortages";
        private static Logger logger;
        private IPreactor _preactor;

        public ShortagesRepo(IPreactor preactor)
        {
            ShortagesRepo.logger = LogManager.GetCurrentClassLogger();
            ShortagesRepo.logger.Trace("Создан экземпляр сервиса ShortagesRepo");
            this._preactor = preactor;
        }

        public bool Create(ShortagesDTO obj)
        {
            if (this.GetByNo(obj.PartNo) != null)
            {
                ShortagesRepo.logger.Trace(string.Format("Экземпляр Shortages уже существует {0} IntDemOrd{1} ExtDemOrd{2}", (object)obj.PartNo, (object)obj.InternalDemandOrder, (object)obj.ExternalDemandOrder));
                return false;
            }
            foreach (ShortagesDTOItem shortagesDtoItem in obj.items)
            {
                try
                {
                    int record = this._preactor.CreateRecord(this._entityName);
                    this._preactor.WriteField(this._entityName, "Part No.", record, shortagesDtoItem.PartNo);
                    this._preactor.WriteField(this._entityName, "Shortage Quantity", record, shortagesDtoItem.ShortageQuantity);
                    this._preactor.WriteField(this._entityName, "Internal Demand Order", record, shortagesDtoItem.InternalDemandOrder);
                    this._preactor.WriteField(this._entityName, "External Demand Order", record, shortagesDtoItem.ExternalDemandOrder);
                    ShortagesRepo.logger.Trace(string.Format("Создан экземпляр Shortages {0} IntDemOrd{1} \r\n                                        ExtDemOrd{2}", (object)shortagesDtoItem.PartNo, (object)shortagesDtoItem.InternalDemandOrder, (object)shortagesDtoItem.ExternalDemandOrder));
                }
                catch (Exception ex)
                {
                    ShortagesRepo.logger.Trace(string.Format("Ошибка создания экземпляра Shortages {0} IntDemOrd{1}\r\n                                    ExtDemOrd{2}", (object)ex.Message, (object)shortagesDtoItem.InternalDemandOrder, (object)shortagesDtoItem.ExternalDemandOrder));
                    return false;
                }
            }
            return true;
        }

        public void Delete(int id)
        {
            try
            {
                this._preactor.DeleteRecord(this._entityName, this._preactor.GetRecordNumber(this._entityName, new PrimaryKey((double)id)));
                ShortagesRepo.logger.Trace(string.Format("Экземпляр Shortages успешно удален, номер записи {0}", (object)id));
            }
            catch (Exception ex)
            {
                ShortagesRepo.logger.Trace(string.Format("Ошибка удаления экземпляра Shortages {0} {1}", (object)ex.Message, (object)id));
            }
        }


        public IEnumerable<ShortagesDTO> GetAll()
        {
            ShortagesRepo.logger.Trace("Получение списка Shortages начато");
            int num = this._preactor.RecordCount("Products");
            string partNo = "-1";
            List<ShortagesDTO> shortagesDtoList = new List<ShortagesDTO>();
            for (int record = 1; record <= num; ++record)
            {
                if (!(partNo == this._preactor.ReadFieldString("Products", "Part No.", record)))
                {
                    try
                    {
                        partNo = this._preactor.ReadFieldString("Products", "Part No.", record);
                    }
                    catch (Exception ex)
                    {
                        ShortagesRepo.logger.Error(ex.Message + " (получение Shortages по всем записям) " + ex.TargetSite.Name);
                        continue;
                    }
                    ShortagesDTO byNo = this.GetByNo(partNo);
                    if (byNo != null)
                        shortagesDtoList.Add(byNo);
                }
            }
            ShortagesRepo.logger.Trace("Получение списка Shortages закончено");
            return (IEnumerable<ShortagesDTO>)shortagesDtoList;
        }

        public ShortagesDTO GetByNo(string partNo)
        {
            ShortagesRepo.logger.Trace(string.Format("Получение Shortages по номеру начато {0}", (object)partNo));
            string expression = string.Format("~{{$Part No.}}~==~{0}~", (object)partNo);
            int matchingRecord = this._preactor.FindMatchingRecord(this._entityName, 0, expression);
            int record = matchingRecord;
            if (matchingRecord < 0)
                return (ShortagesDTO)null;
            int num1 = 0;
            double num2 = 0.0;
            List<ShortagesDTOItem> shortagesDtoItemList = new List<ShortagesDTOItem>();
            if (matchingRecord < 0)
                return (ShortagesDTO)null;
            for (; matchingRecord >= 0; matchingRecord = this._preactor.FindMatchingRecord(this._entityName, matchingRecord, expression))
            {
                if (matchingRecord >= 0)
                {
                    try
                    {
                        int num3 = this._preactor.ReadFieldInt(this._entityName, "Number", matchingRecord);
                        string str = this._preactor.ReadFieldString(this._entityName, "Part No.", matchingRecord);
                        int num4 = this._preactor.ReadFieldInt("Shortages", "Internal Demand Order", matchingRecord);
                        int num5 = this._preactor.ReadFieldInt("Shortages", "External Demand Order", matchingRecord);
                        double num6 = this._preactor.ReadFieldDouble("Shortages", "Shortage Quantity", matchingRecord);
                        ShortagesDTOItem shortagesDtoItem = new ShortagesDTOItem()
                        {
                            Id = num3,
                            PartNo = str,
                            InternalDemandOrder = num4,
                            ExternalDemandOrder = num5,
                            ShortageQuantity = num6
                        };
                        shortagesDtoItemList.Add(shortagesDtoItem);
                    }
                    catch (Exception ex)
                    {
                        ShortagesRepo.logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " Shortages" + partNo);
                    }
                }
                else
                    break;
            }
            try
            {
                num1 = this._preactor.ReadFieldInt(this._entityName, "Number", record);
                string str = this._preactor.ReadFieldString(this._entityName, "Part No.", record);
                int num3 = this._preactor.ReadFieldInt("Shortages", "Internal Demand Order", record);
                int num4 = this._preactor.ReadFieldInt("Shortages", "External Demand Order", record);
                num2 = this._preactor.ReadFieldDouble("Shortages", "Shortage Quantity", record);
                ShortagesDTO shortagesDto = new ShortagesDTO()
                {
                    PartNo = str,
                    InternalDemandOrder = num3,
                    ExternalDemandOrder = num4,
                    items = shortagesDtoItemList
                };
                ShortagesRepo.logger.Trace(string.Format("Получение Shortages по PartNo завершено {0}", (object)partNo));
                return shortagesDto;
            }
            catch (Exception ex)
            {
                ShortagesRepo.logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " Shortages" + partNo);
                return (ShortagesDTO)null;
            }
        }
        /// <summary>
        /// проверка на наличие внешних дефицитов
        /// </summary>
        /// <returns></returns>
        public bool IsExternalDemandShortageExist()
        {
            return this._preactor.FindMatchingRecord(this._entityName, 0, "~{#External Demand Order}~>~0~") >= 0;
        }


        public void Update(ShortagesDTO obj)
        {
            ShortagesDTO byNo = this.GetByNo(obj.PartNo);
            if (byNo == null)
            {
                ShortagesRepo.logger.Trace(string.Format("Экземпляр Shortages не найден, нечего обновлять {0}", (object)obj.PartNo));
            }
            else
            {
                try
                {
                    foreach (ShortagesDTOItem shortagesDtoItem in byNo.items)
                        this.Delete(shortagesDtoItem.Id);
                    if (!this.Create(obj))
                        return;
                    ShortagesRepo.logger.Trace(string.Format("Обновлен экземпляр Shortages {0}", (object)obj.PartNo));
                }
                catch (Exception ex)
                {
                    ShortagesRepo.logger.Trace(string.Format("Ошибка создания экземпляра Shortages {0} {1}", (object)ex.Message, (object)obj.PartNo));
                }
            }
        }

        ~ShortagesRepo()
        {
            LogManager.Flush();
        }
    }
}
