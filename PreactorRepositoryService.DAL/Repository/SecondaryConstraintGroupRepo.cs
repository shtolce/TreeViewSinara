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
    public class SecondaryConstraintGroupRepo:IRepository<SecondaryConstraintGroupDTO>
    {
        private static Logger logger;
        private IPreactor _preactor;
        private string _entityName = "Secondary Constraint Groups";
        private SecondaryConstraintRepo scRepo;
        public SecondaryConstraintGroupRepo(IPreactor preactor)
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Создан экземпляр сервиса SecondaryConstraintGroupRepo");
            this._preactor = preactor;
            scRepo = new SecondaryConstraintRepo(_preactor);
    }
    /// <summary>
    /// Создает группу вторичных ограничений с вложениями  в преакторе, с помощью заполненого SecondaryConstraintGroupDTO
    /// </summary>
    /// <param name="obj">Заполните параметр  типа SecondaryConstraintGroupDTO</param>
    /// <returns></returns>
    public bool Create(SecondaryConstraintGroupDTO obj)
        {
            if (GetByNo(obj.Name) != null)
            {
                logger.Trace($"Экземпляр SecondaryConstraintGroup уже существует {obj.Name}");
                return false;
            }

            try
            {
                int SecondaryConstraintGroupRecNo = _preactor.CreateRecord(_entityName);
                //_preactor.WriteField(_entityName, "Number", SecondaryConstraintGroupRecNo, obj.Id);
                _preactor.WriteField(_entityName, "Name", SecondaryConstraintGroupRecNo, obj.Name);
                int idxSc =0;
                _preactor.SetAutoListSize(_entityName, "Secondary Constraints", SecondaryConstraintGroupRecNo, obj.SecondaryConstraints.Count);
                foreach (SecondaryConstraintDTO el in obj.SecondaryConstraints)
                {
                    idxSc++;
                    _preactor.WriteListField(_entityName, "Secondary Constraints", SecondaryConstraintGroupRecNo, el.Name, idxSc);
                }

                logger.Trace($"Создан экземпляр SecondaryConstraintGroup {obj.Name}");
                return true;
            }
            catch
            {
                logger.Trace($"Ошибка создания экземпляра SecondaryConstraintGroup  {obj.Name}");
            }
            return false;
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Получает список групп вторичных ограничений в Преакторе
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SecondaryConstraintGroupDTO> GetAll()
        {
            logger.Trace($"Получение списка групп вторичных ограничений начато");
            int recCount = _preactor.RecordCount(_entityName);
            string scGrpName = "-1";
            List<SecondaryConstraintGroupDTO> res = new List<SecondaryConstraintGroupDTO>();
            SecondaryConstraintGroupDTO foundSecondaryConstraintGroup = null;

            for (int i = 1; i <= recCount; i++)
            {
                try
                {
                    scGrpName = _preactor.ReadFieldString(_entityName, "Name", i);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (получение групп вторичных ограничений по всем записям) " + ex.TargetSite.Name);
                }
                foundSecondaryConstraintGroup = GetByNo(scGrpName);
                res.Add(foundSecondaryConstraintGroup);
            }
            logger.Trace($"Получение списка групп вторичных ограничений закончено");
            return res;
        }

        /// <summary>
        /// Получает группу вторичных ограничений по ее имени в Preactor поле Name
        /// </summary>
        /// <param name="scGrpName">Имя группы вторичных ограничений в преакторе поле Name</param>
        /// <returns></returns>
        public SecondaryConstraintGroupDTO GetByNo(string scGrpName)
        {
            logger.Trace($"Получение группы вторичных ограничений по наименованию начато {scGrpName}");
            string requestStr = $"~{{$Name}}~==~{scGrpName}~";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            SecondaryConstraintDTO scFoundElement;
            List<SecondaryConstraintDTO> scList = new List<SecondaryConstraintDTO>();
            if (recordNo < 0)
                return null;
            int constraintCount = _preactor.MatrixFieldSize(_entityName, "Secondary Constraints", recordNo).X;
            for (int i=1; i<=constraintCount; i++)
            {
                try
                {
                    string scGrpNameStr = _preactor.ReadFieldString(_entityName, "Secondary Constraints", recordNo, i);
                    scFoundElement = scRepo.GetByNo(scGrpNameStr);
                    if (scFoundElement != null)
                        scList.Add(scFoundElement);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (SecondaryConstraintGroup Resources) " + ex.TargetSite.Name + " SecondaryConstraintGroup name" + scGrpName);
                }
            }

            int _id = 0;
            string _scGrpName = "";
            try
            {
                _id = _preactor.ReadFieldInt(_entityName, "Number", firstRecNo);
                _scGrpName = _preactor.ReadFieldString(_entityName, "Name", firstRecNo);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " SecondaryConstraintGroup" + scGrpName);
            }
            SecondaryConstraintGroupDTO result = new SecondaryConstraintGroupDTO()
            {
                Id = _id,
                Name = _scGrpName,
                SecondaryConstraints = scList
            };
            logger.Trace($"Получение SecondaryConstraintGroup по имени завершено {scGrpName}");
            return result;
        }
        public void Update(SecondaryConstraintGroupDTO obj)
        {
            throw new NotImplementedException();
        }
        ~SecondaryConstraintGroupRepo()
        {
            LogManager.Flush();
        }

    }
}
