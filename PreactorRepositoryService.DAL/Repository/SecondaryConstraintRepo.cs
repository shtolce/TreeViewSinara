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
    public class SecondaryConstraintRepo:IRepository<SecondaryConstraintDTO>
    {
        private static Logger logger;
        private IPreactor _preactor;
        private string _entityName = "Secondary Constraints";
        public SecondaryConstraintRepo(IPreactor preactor)
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Создан экземпляр сервиса SecondaryConstraintRepo");
            this._preactor = preactor;
        }
        /// <summary>
        /// Создает ресурс в преакторе, с помощью заполненого SecondaryConstraintDTO
        /// для заполнения Constrains нужно заполнить соответствующий список в SecondaryConstraintDTO
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Create(SecondaryConstraintDTO obj)
        {
            if (GetByNo(obj.Name) != null)
            {
                logger.Trace($"Экземпляр SecondaryConstraint уже существует {obj.Name}");
                return false;
            }

            try
            {
                int SecondaryConstraintRecNo = _preactor.CreateRecord(_entityName);
                //_preactor.WriteField(_entityName, "Number", SecondaryConstraintRecNo, obj.Id);
                _preactor.WriteField(_entityName, "Name", SecondaryConstraintRecNo, obj.Name);
                _preactor.WriteField(_entityName, "Use as a Constraint", SecondaryConstraintRecNo, obj.UseAsAConstraint);
                _preactor.WriteField(_entityName, "Calendar Effect", SecondaryConstraintRecNo, obj.CalendarEffect);
                logger.Trace($"Создан экземпляр SecondaryConstraint {obj.Name}");
                return true;
            }
            catch
            {
                logger.Trace($"Ошибка создания экземпляра SecondaryConstraint  {obj.Name}");
            }
            return false;
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Получает список всех вторичных ограничений в преакторе
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SecondaryConstraintDTO> GetAll()
        {
            logger.Trace($"Получение списка вторичных ограничений начато");
            int recCount = _preactor.RecordCount(_entityName);
            string resName = "-1";
            List<SecondaryConstraintDTO> secConstr = new List<SecondaryConstraintDTO>();
            SecondaryConstraintDTO foundSecondaryConstraint = null;
            for (int i = 1; i <= recCount; i++)
            {
                try
                {
                    resName = _preactor.ReadFieldString(_entityName, "Name", i);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (получение вторичных ограничений по всем записям) " + ex.TargetSite.Name);
                }
                foundSecondaryConstraint = GetByNo(resName);
                secConstr.Add(foundSecondaryConstraint);
            }
            logger.Trace($"Получение списка вторичных ограничений закончено");
            return secConstr;
        }

        /// <summary>
        /// Получает вторичное ограничение по его имени в Preactor поле Name
        /// </summary>
        /// <param name="scName">Имя вторичного ограничения в преакторе поле Name</param>
        /// <returns></returns>
        public SecondaryConstraintDTO GetByNo(string scName)
        {
            logger.Trace($"Получение вторичного ограничения по наименованию {scName}");
            string requestStr = $"~{{$Name}}~==~{scName}~";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            if (recordNo < 0)
                return null;

            int id = 0;
            string secondaryconstraintName = "";
            string calendarEffect = "";
            bool useAsAConstraint = default(bool);
            try
            {
                id = _preactor.ReadFieldInt(_entityName, "Number", firstRecNo);
                secondaryconstraintName = _preactor.ReadFieldString(_entityName, "Name", firstRecNo);
                calendarEffect = _preactor.ReadFieldString(_entityName, "Calendar Effect", firstRecNo);
                useAsAConstraint = _preactor.ReadFieldBool(_entityName, "Use as a Constraint", firstRecNo);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " SecondaryConstraint" + secondaryconstraintName);
            }
            SecondaryConstraintDTO result = new SecondaryConstraintDTO()
            {
                Id = id,
                Name = secondaryconstraintName,
                CalendarEffect = calendarEffect,
                UseAsAConstraint = useAsAConstraint
            };
            logger.Trace($"Получение SecondaryConstraint по имени завершено {secondaryconstraintName}");
            return result;
        }
        public void Update(SecondaryConstraintDTO obj)
        {
            throw new NotImplementedException();
        }
        ~SecondaryConstraintRepo()
        {
            LogManager.Flush();
        }




    }
}
