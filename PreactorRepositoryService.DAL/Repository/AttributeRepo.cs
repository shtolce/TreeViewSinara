using NLog;
using Preactor;
using PreactorRepositoryService.DAL.DTOModels;
using PreactorRepositoryService.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static PreactorRepositoryService.DAL.DTOModels.AttributeDTO;

namespace PreactorRepositoryService.DAL.Repository
{
    public class AttributeRepo:IRepository<AttributeDTO>
    {
        private static Logger logger;
        private IPreactor _preactor;
        private string _entityName = "Attribute";
        public AttributeRepo(IPreactor preactor)
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Создан экземпляр сервиса AttributeRepo");
            this._preactor = preactor;
        }

        private bool CreateAttrN(AttributeStruct el, string entityNoStr)
        {

            try
            {
                int AttributeRecNo = _preactor.CreateRecord(entityNoStr);
                //_preactor.WriteField(_entityName, "Number", AttributeRecNo, el.Id);
                _preactor.WriteField(entityNoStr, "Name", AttributeRecNo, el.Name);
                int idxConstr = 0;
                if (el.SecondaryConstraints!=null)
                {
                    _preactor.SetAutoListSize($"{entityNoStr}", "Secondary Constraints", AttributeRecNo, el.SecondaryConstraints.Count);
                    foreach (string elSc in el.SecondaryConstraints)
                    {
                        idxConstr++;
                        _preactor.WriteListField($"{entityNoStr}", "Secondary Constraints", AttributeRecNo, elSc, idxConstr);
                    }
                }
                logger.Trace($"Создан экземпляр Attribute {el.Name}");
                return true;
            }
            catch(Exception ex)
            {
                logger.Trace($"Ошибка создания экземпляра {ex.Message}{entityNoStr} {el.Name}");
            }
            return false;
        }

        /// <summary>
        /// Создает аттрибуты в преакторе, с помощью заполненых соответствующих коллекций в AttributeDTO
        /// для заполнения Constrains нужно заполнить соответствующий список в AttributeDTO
        /// </summary>
        /// <param name="obj">Объект с коллекциями аттрибутов</param>
        /// <returns></returns>
        public bool Create(AttributeDTO obj)
        {
            string entityNoStr;
            //int attrNo = 1;
            try
            {
                for (int attrNo = 1; attrNo <= 5; attrNo++)
                {
                    AttributeDTO foundAttr = GetByNo(attrNo.ToString());
                    

                    PropertyInfo fieldAttr = typeof(AttributeDTO).GetProperty($"Attribute{attrNo.ToString()}");
                    object fieldValue = fieldAttr.GetValue(obj);
                    entityNoStr = $"{_entityName} {attrNo}";
                    if (fieldValue == null)
                        continue;
                    foreach (AttributeStruct el in (List<AttributeStruct>)fieldValue)
                    {
                        List<AttributeStruct> fieldValueFound = (List<AttributeStruct>)fieldAttr.GetValue(foundAttr);
                        AttributeStruct foundEl = fieldValueFound.FirstOrDefault(x => x.Name == el.Name);
                        if (foundEl.Name!=null)
                        {
                                logger.Trace($"Экземпляр Attribute уже существует {el.Name}");
                        }
                        else
                            CreateAttrN(el, entityNoStr);
                    }

                }
                return true;

            }
            catch(Exception ex)
            {
                logger.Trace($"Ошибка в создании экземпляра{ex.Message} {_entityName}");
            }
            return false;
        }

        public void Delete(int id,int nAttr)
        {
            try
            {
                int recordNumber = _preactor.GetRecordNumber($"{_entityName} {nAttr}", new PrimaryKey(id));
                _preactor.DeleteRecord($"{_entityName} {nAttr}", recordNumber);
                logger.Trace($"Экземпляр Attribute успешно удален, номер записи {id}");
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка удаления экземпляра Attribute {ex.Message} {id}");

            }
        }
        /// <summary>
        /// Получает список всех ресурсов в преакторе в 5 объектаъ DTO
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AttributeDTO> GetAll()
        {

            logger.Trace($"Получение списка всех аттрибутов от 1 до  5  начато");
            List<AttributeDTO> res = new List<AttributeDTO>();
            AttributeDTO foundAttribute = null;
            for (int i = 1; i <= 5; i++)
            {
                foundAttribute = GetByNo(i.ToString());
                res.Add(foundAttribute);
            }
            logger.Trace($"Получение спискавсех аттрибутов от 1 до  5   закончено");
            return res;
        }

        /// <summary>
        /// Получает все значения аттрибута по его номеру в Preactor, например Аттрибут2 , номер "2"
        /// </summary>
        /// <param name="attrNo">Номер аттрибута в преакторе ("1","2","3","4","5")</param>
        /// <returns></returns>
        public AttributeDTO GetByNo(string attrNo)
        {
            string entityNoStr;
            entityNoStr = $"{_entityName} {attrNo}";

            logger.Trace($"Получение аттрибута по номеру начато {entityNoStr}");
            string requestStr = $"~{{$Number}}~>~0~";
            int recordNo = _preactor.FindMatchingRecord(entityNoStr, 0, requestStr);
            int firstRecNo = recordNo;
            List<string> secondaryConstraintList = new List<string>();
            if (recordNo < 0)
                return null;
            List<AttributeStruct> attrList = new List<AttributeStruct>();
            int orderIdx = 0;
            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                try
                {

                    int constraintCount = _preactor.MatrixFieldSize(entityNoStr, "Secondary Constraints", recordNo).X;
                    for (int i = 1; i <= constraintCount; i++)
                    {
                        try
                        {
                            string secConstr = _preactor.ReadFieldString(entityNoStr, "Secondary Constraints", recordNo, i);
                            secondaryConstraintList.Add(secConstr);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.Message + " (Attribute secondary constraint) " + ex.TargetSite.Name + " Attribute name" + entityNoStr);
                        }
                    }

                    int id = 0;
                    string attrName = "";
                    double color = 0;
                    orderIdx++;
                    try
                    {
                        id = _preactor.ReadFieldInt(entityNoStr, "Number", recordNo);
                        attrName = _preactor.ReadFieldString(entityNoStr, "Name", recordNo);
                        color = _preactor.ReadFieldDouble(entityNoStr, "Color", recordNo);
                        AttributeStruct attrNewElem = new AttributeStruct
                        {
                            Id = id,
                            Name = attrName,
                            Color = color,
                            SecondaryConstraints = secondaryConstraintList,
                            OrderNumber = orderIdx
                        };
                        attrList.Add(attrNewElem);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " Attribute" + entityNoStr);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " Attribute" + entityNoStr);
                }
                recordNo = _preactor.FindMatchingRecord(entityNoStr, recordNo, requestStr);
            }

            AttributeDTO result = new AttributeDTO()
            {
                Attribute1 = attrNo == "1" ? attrList : null,
                Attribute2 = attrNo == "2" ? attrList : null,
                Attribute3 = attrNo == "3" ? attrList : null,
                Attribute4 = attrNo == "4" ? attrList : null,
                Attribute5 = attrNo == "5" ? attrList : null

            };
            logger.Trace($"Получение Attribute по имени завершено {entityNoStr}");
            return result;
        }
        public void Update(AttributeDTO obj)
        {
            for (int i = 1; i <= 5; i++)
            {

                PropertyInfo fieldAttr = typeof(AttributeDTO).GetProperty($"Attribute{i.ToString()}");
                object fieldValue = fieldAttr.GetValue(obj);
                if (fieldValue == null)
                    continue;
                fieldAttr = typeof(AttributeDTO).GetProperty($"Attribute{i.ToString()}");
                AttributeDTO foundAttr = GetByNo(i.ToString());
                List<AttributeStruct> fieldValueFound = (List<AttributeStruct>)fieldAttr.GetValue(foundAttr);
                string entityNoStr = $"{_entityName} {i}";
                foreach (AttributeStruct attr1 in (List<AttributeStruct>)fieldValue)
                {
                    AttributeStruct foundEl = fieldValueFound.FirstOrDefault(x => x.Name == attr1.Name);
                    int attrRecNo = attr1.Id;
                    try
                    {

                        if (foundEl.Name != null)
                        {
                            Delete(foundEl.Id,i);
                            logger.Trace($"Обновлен экземпляр Attribute {attr1.Name}");
                        }
                        else
                            logger.Trace($"Создан экземпляр Attribute {attr1.Name}");

                        CreateAttrN(attr1, entityNoStr);
                    }
                    catch (Exception ex)
                    {
                        logger.Trace($"Ошибка создания экземпляра Attribute {ex.Message} {attr1.Name}");
                    }
                }
            }

            return;
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        ~AttributeRepo()
        {
            LogManager.Flush();
        }




    }
}
