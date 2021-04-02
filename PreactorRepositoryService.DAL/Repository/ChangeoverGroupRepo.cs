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
    public class ChangeoverGroupRepo:IRepository<ChangeoverGroupDTO>
    {
        private static Logger logger;
        private IPreactor _preactor;
        private string _entityName = "Changeover Groups";
        public ChangeoverGroupRepo(IPreactor preactor)
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Создан экземпляр сервиса ChangeoverGroupRepo");
            this._preactor = preactor;
        }
        /// <summary>
        /// Создает группу переналадки в преакторе, с помощью заполненого ChangeoverGroupDTO
        /// для заполнения Constrains нужно заполнить соответствующий список в ChangeoverGroupDTO
        /// </summary>
        /// <param name="obj"> Время матриц указывать в минутах,new TimeSpan(0,12,0)</param>
        /// <returns></returns>
        public bool Create(ChangeoverGroupDTO obj)
        {
            if (GetByNo(obj.Name) != null)
            {
                logger.Trace($"Экземпляр ChangeoverGroup уже существует {obj.Name}");
                return false;
            }
            
            try
            {
                int ChangeoverGroupRecNo = _preactor.CreateRecord(_entityName);
                //_preactor.WriteField(_entityName, "Number", ChangeoverGroupRecNo, obj.Id);
                    _preactor.WriteField(_entityName, "Name", ChangeoverGroupRecNo, obj.Name);
                if (obj.Attribute1ChangeoverTime != default(TimeSpan))
                    _preactor.WriteField(_entityName, "Attribute 1 Changeover Time", ChangeoverGroupRecNo, obj.Attribute1ChangeoverTime.Minutes / (60.0 * 24.0));
                if (obj.Attribute2ChangeoverTime != default(TimeSpan))
                    _preactor.WriteField(_entityName, "Attribute 2 Changeover Time", ChangeoverGroupRecNo, obj.Attribute2ChangeoverTime.Minutes / (60.0 * 24.0));
                if (obj.Attribute3ChangeoverTime != default(TimeSpan))
                    _preactor.WriteField(_entityName, "Attribute 3 Changeover Time", ChangeoverGroupRecNo, obj.Attribute3ChangeoverTime.Minutes / (60.0 * 24.0));
                if (obj.Attribute4ChangeoverTime != default(TimeSpan))
                    _preactor.WriteField(_entityName, "Attribute 4 Changeover Time", ChangeoverGroupRecNo, obj.Attribute4ChangeoverTime.Minutes / (60.0 * 24.0));
                if (obj.Attribute5ChangeoverTime != default(TimeSpan))
                    _preactor.WriteField(_entityName, "Attribute 5 Changeover Time", ChangeoverGroupRecNo, obj.Attribute5ChangeoverTime.Minutes / (60.0 * 24.0));

                if (obj.Attribute1ChangeoverMatrix != null)
                    foreach (KeyValuePair<MatrixDimensions,TimeSpan> el in obj.Attribute1ChangeoverMatrix)
                    {
                        if (el.Value != default(TimeSpan))
                            _preactor.WriteMatrixField(_entityName, "Attribute 1 Changeover Matrix", ChangeoverGroupRecNo, el.Value.Minutes/(60.0*24.0)
                            , el.Key.X, el.Key.Y);
                    }
                if (obj.Attribute2ChangeoverMatrix != null)
                    foreach (KeyValuePair<MatrixDimensions, TimeSpan> el in obj.Attribute2ChangeoverMatrix)
                    {
                        if (el.Value != default(TimeSpan))
                            _preactor.WriteMatrixField(_entityName, "Attribute 2 Changeover Matrix", ChangeoverGroupRecNo, el.Value.Minutes / (60.0 * 24.0)
                            , el.Key.X, el.Key.Y);
                    }
                if (obj.Attribute3ChangeoverMatrix != null)
                    foreach (KeyValuePair<MatrixDimensions, TimeSpan> el in obj.Attribute3ChangeoverMatrix)
                    {
                        if (el.Value != default(TimeSpan))
                            _preactor.WriteMatrixField(_entityName, "Attribute 3 Changeover Matrix", ChangeoverGroupRecNo, el.Value.Minutes / (60.0 * 24.0)
                            , el.Key.X, el.Key.Y);
                    }
                if (obj.Attribute4ChangeoverMatrix != null)
                    foreach (KeyValuePair<MatrixDimensions, TimeSpan> el in obj.Attribute4ChangeoverMatrix)
                    {
                        if (el.Value != default(TimeSpan))
                            _preactor.WriteMatrixField(_entityName, "Attribute 4 Changeover Matrix", ChangeoverGroupRecNo, el.Value.Minutes / (60.0 * 24.0)
                            , el.Key.X, el.Key.Y);
                    }
                if (obj.Attribute5ChangeoverMatrix != null)
                    foreach (KeyValuePair<MatrixDimensions, TimeSpan> el in obj.Attribute5ChangeoverMatrix)
                    {
                        if (el.Value != default(TimeSpan))
                            _preactor.WriteMatrixField(_entityName, "Attribute 5 Changeover Matrix", ChangeoverGroupRecNo, el.Value.Minutes / (60.0 * 24.0)
                            , el.Key.X, el.Key.Y);
                    }

                logger.Trace($"Создан экземпляр ChangeoverGroup {obj.Name}");
                return true;
            }
            catch(Exception ex)
            {
                logger.Trace($"Ошибка создания экземпляра ChangeoverGroup{ex.Message}  {obj.Name}");
            }
            return false;
        }

        public void Delete(int id)
        {
            try
            {
                int recordNumber = _preactor.GetRecordNumber(_entityName, new PrimaryKey(id));
                _preactor.DeleteRecord(_entityName, recordNumber);
                logger.Trace($"Экземпляр ChangeoverGroup успешно удален, номер записи {id}");
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка удаления экземпляра ChangeoverGroup {ex.Message} {id}");

            }
        }
        /// <summary>
        /// Получает список всех групп переналадок в преакторе
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ChangeoverGroupDTO> GetAll()
        {
            logger.Trace($"Получение списка групп переналадок начато");
            int recCount = _preactor.RecordCount(_entityName);
            string chOvName = "-1";
            List<ChangeoverGroupDTO> res = new List<ChangeoverGroupDTO>();
            ChangeoverGroupDTO foundChangeoverGroup = null;
            for (int i = 1; i <= recCount; i++)
            {
                try
                {
                    chOvName = _preactor.ReadFieldString(_entityName, "Name", i);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (получение групп переналадок по всем записям) " + ex.TargetSite.Name);
                }
                foundChangeoverGroup = GetByNo(chOvName);
                res.Add(foundChangeoverGroup);
            }
            logger.Trace($"Получение списка групп переналадок закончено");
            return res;
        }

        /// <summary>
        /// Получает  группу переналадок  по его имени в Preactor поле Name,
        /// attributeNChangeoverMatrixStr  только для получения данных, для создания использовать только attribute1ChangeoverMatrix
        /// </summary>
        /// <param name="chOvName">Имя  группы переналадок  в преакторе поле Name</param>
        /// <returns></returns>
        public ChangeoverGroupDTO GetByNo(string chOvName)
        {
            logger.Trace($"Получение  группы переналадок  по номеру начато {chOvName}");
            string requestStr = $"~{{$Name}}~==~{chOvName}~";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            Dictionary<MatrixDimensions, TimeSpan> attribute1ChangeoverMatrix = new Dictionary<MatrixDimensions, TimeSpan>();
            Dictionary<MatrixDimensions, TimeSpan> attribute2ChangeoverMatrix = new Dictionary<MatrixDimensions, TimeSpan>();
            Dictionary<MatrixDimensions, TimeSpan> attribute3ChangeoverMatrix = new Dictionary<MatrixDimensions, TimeSpan>();
            Dictionary<MatrixDimensions, TimeSpan> attribute4ChangeoverMatrix = new Dictionary<MatrixDimensions, TimeSpan>();
            Dictionary<MatrixDimensions, TimeSpan> attribute5ChangeoverMatrix = new Dictionary<MatrixDimensions, TimeSpan>();

            Dictionary<MatrixStringDimensions, TimeSpan> attribute1ChangeoverMatrixStr = new Dictionary<MatrixStringDimensions, TimeSpan>();
            Dictionary<MatrixStringDimensions, TimeSpan> attribute2ChangeoverMatrixStr = new Dictionary<MatrixStringDimensions, TimeSpan>();
            Dictionary<MatrixStringDimensions, TimeSpan> attribute3ChangeoverMatrixStr = new Dictionary<MatrixStringDimensions, TimeSpan>();
            Dictionary<MatrixStringDimensions, TimeSpan> attribute4ChangeoverMatrixStr = new Dictionary<MatrixStringDimensions, TimeSpan>();
            Dictionary<MatrixStringDimensions, TimeSpan> attribute5ChangeoverMatrixStr = new Dictionary<MatrixStringDimensions, TimeSpan>();
            if (recordNo < 0)
                return null;

            MatrixDimensions size1 = _preactor.MatrixFieldSize(_entityName, "Attribute 1 Changeover Matrix", recordNo);
            MatrixDimensions size2 = _preactor.MatrixFieldSize(_entityName, "Attribute 2 Changeover Matrix", recordNo);
            MatrixDimensions size3 = _preactor.MatrixFieldSize(_entityName, "Attribute 3 Changeover Matrix", recordNo);
            MatrixDimensions size4 = _preactor.MatrixFieldSize(_entityName, "Attribute 4 Changeover Matrix", recordNo);
            MatrixDimensions size5 = _preactor.MatrixFieldSize(_entityName, "Attribute 5 Changeover Matrix", recordNo);
            AttributeRepo attrRepo = new AttributeRepo(_preactor);
            List<AttributeDTO> attrService = attrRepo.GetAll().ToList();
            string xStr;
            string yStr;
            for (int indexX = 1; indexX <= size1.X; indexX++)
            {
                for (int indexY = 1; indexY <= size1.Y; indexY++)
                {
                    try
                    {
                        double value = _preactor.ReadFieldDouble(_entityName, "Attribute 1 Changeover Matrix", recordNo, indexX, indexY);
                        attribute1ChangeoverMatrix.Add(new MatrixDimensions(indexX, indexY), TimeSpan.FromDays(value));
                        xStr = attrService[0].Attribute1.FirstOrDefault(x => x.OrderNumber == indexX).Name;
                        yStr = attrService[0].Attribute1.FirstOrDefault(x => x.OrderNumber == indexY).Name;
                        attribute1ChangeoverMatrixStr.Add(new MatrixStringDimensions(xStr, yStr), TimeSpan.FromDays(value));
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message + " (ChangeoverGroup matrix1) " + ex.TargetSite.Name + " ChangeoverGroup name" + chOvName);
                    }
                }
            }
            for (int indexX = 1; indexX <= size2.X; indexX++)
            {
                for (int indexY = 1; indexY <= size2.Y; indexY++)
                {
                    try
                    {
                        double value = _preactor.ReadFieldDouble(_entityName, "Attribute 2 Changeover Matrix", recordNo, indexX, indexY);
                        attribute2ChangeoverMatrix.Add(new MatrixDimensions(indexX, indexY), TimeSpan.FromDays(value));
                        xStr = attrService[1].Attribute2.FirstOrDefault(x => x.OrderNumber == indexX).Name;
                        yStr = attrService[1].Attribute2.FirstOrDefault(x => x.OrderNumber == indexY).Name;
                        attribute1ChangeoverMatrixStr.Add(new MatrixStringDimensions(xStr, yStr), TimeSpan.FromDays(value));
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message + " (ChangeoverGroup matrix2) " + ex.TargetSite.Name + " ChangeoverGroup name" + chOvName);
                    }
                }
            }
            for (int indexX = 1; indexX <= size3.X; indexX++)
            {
                for (int indexY = 1; indexY <= size3.Y; indexY++)
                {
                    try
                    {
                        double value = _preactor.ReadFieldDouble(_entityName, "Attribute 3 Changeover Matrix", recordNo, indexX, indexY);
                        attribute3ChangeoverMatrix.Add(new MatrixDimensions(indexX, indexY), TimeSpan.FromDays(value));
                        xStr = attrService[2].Attribute3.FirstOrDefault(x => x.OrderNumber == indexX).Name;
                        yStr = attrService[2].Attribute3.FirstOrDefault(x => x.OrderNumber == indexY).Name;
                        attribute1ChangeoverMatrixStr.Add(new MatrixStringDimensions(xStr, yStr), TimeSpan.FromDays(value));

                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message + " (ChangeoverGroup matrix3) " + ex.TargetSite.Name + " ChangeoverGroup name" + chOvName);
                    }

                }
            }
            for (int indexX = 1; indexX <= size4.X; indexX++)
            {
                for (int indexY = 1; indexY <= size4.Y; indexY++)
                {
                    try
                    {
                        double value = _preactor.ReadFieldDouble(_entityName, "Attribute 4 Changeover Matrix", recordNo, indexX, indexY);
                        attribute4ChangeoverMatrix.Add(new MatrixDimensions(indexX, indexY), TimeSpan.FromDays(value));
                        xStr = attrService[3].Attribute4.FirstOrDefault(x => x.OrderNumber == indexX).Name;
                        yStr = attrService[3].Attribute4.FirstOrDefault(x => x.OrderNumber == indexY).Name;
                        attribute1ChangeoverMatrixStr.Add(new MatrixStringDimensions(xStr, yStr), TimeSpan.FromDays(value));
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message + " (ChangeoverGroup matrix4) " + ex.TargetSite.Name + " ChangeoverGroup name" + chOvName);
                    }
                }
            }
            for (int indexX = 1; indexX <= size5.X; indexX++)
            {
                for (int indexY = 1; indexY <= size5.Y; indexY++)
                {
                    try
                    {
                        double value = _preactor.ReadFieldDouble(_entityName, "Attribute 5 Changeover Matrix", recordNo, indexX, indexY);
                        attribute5ChangeoverMatrix.Add(new MatrixDimensions(indexX, indexY), TimeSpan.FromDays(value));
                        xStr = attrService[4].Attribute5.FirstOrDefault(x => x.OrderNumber == indexX).Name;
                        yStr = attrService[4].Attribute5.FirstOrDefault(x => x.OrderNumber == indexY).Name;
                        attribute1ChangeoverMatrixStr.Add(new MatrixStringDimensions(xStr, yStr), TimeSpan.FromDays(value));
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message + " (ChangeoverGroup matrix5) " + ex.TargetSite.Name + " ChangeoverGroup name" + chOvName);
                    }
                }
            }


            int id = 0;
            string _chOvName = "";
            TimeSpan attr1CT = default(TimeSpan);
            TimeSpan attr2CT = default(TimeSpan);
            TimeSpan attr3CT = default(TimeSpan);
            TimeSpan attr4CT = default(TimeSpan);
            TimeSpan attr5CT = default(TimeSpan);
            try
            {
                id = _preactor.ReadFieldInt(_entityName, "Number", firstRecNo);
                _chOvName = _preactor.ReadFieldString(_entityName, "Name", firstRecNo);
                attr1CT = TimeSpan.FromDays(_preactor.ReadFieldDouble(_entityName, "Attribute 1 Changeover Time", firstRecNo));
                attr2CT = TimeSpan.FromDays(_preactor.ReadFieldDouble(_entityName, "Attribute 2 Changeover Time", firstRecNo));
                attr3CT = TimeSpan.FromDays(_preactor.ReadFieldDouble(_entityName, "Attribute 3 Changeover Time", firstRecNo));
                attr4CT = TimeSpan.FromDays(_preactor.ReadFieldDouble(_entityName, "Attribute 4 Changeover Time", firstRecNo));
                attr5CT = TimeSpan.FromDays(_preactor.ReadFieldDouble(_entityName, "Attribute 5 Changeover Time", firstRecNo));
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " ChangeoverGroup" + chOvName);
            }
            ChangeoverGroupDTO result = new ChangeoverGroupDTO()
            {
                Id = id,
                Name = _chOvName,
                Attribute1ChangeoverTime = attr1CT,
                Attribute2ChangeoverTime = attr2CT,
                Attribute3ChangeoverTime = attr3CT,
                Attribute4ChangeoverTime = attr4CT,
                Attribute5ChangeoverTime = attr5CT,
                Attribute1ChangeoverMatrix = attribute1ChangeoverMatrix,
                Attribute2ChangeoverMatrix = attribute2ChangeoverMatrix,
                Attribute3ChangeoverMatrix = attribute3ChangeoverMatrix,
                Attribute4ChangeoverMatrix = attribute4ChangeoverMatrix,
                Attribute5ChangeoverMatrix = attribute5ChangeoverMatrix,
                Attribute1ChangeoverMatrixStr = attribute1ChangeoverMatrixStr,
                Attribute2ChangeoverMatrixStr = attribute2ChangeoverMatrixStr,
                Attribute3ChangeoverMatrixStr = attribute3ChangeoverMatrixStr,
                Attribute4ChangeoverMatrixStr = attribute4ChangeoverMatrixStr,
                Attribute5ChangeoverMatrixStr = attribute5ChangeoverMatrixStr
            };
            logger.Trace($"Получение ChangeoverGroup по имени завершено {chOvName}");
            return result;
        }
        public void Update(ChangeoverGroupDTO obj)
        {
            ChangeoverGroupDTO foundChOv = GetByNo(obj.Name);
            if (foundChOv == null)
            {
                logger.Trace($"Экземпляр Resource не найден, нечего обновлять {obj.Name}");
                return;
            }
            int resourceRecNo = foundChOv.Id;
            try
            {
                Delete(foundChOv.Id);
                if (Create(obj) == true)
                    logger.Trace($"Обновлен экземпляр Resource {obj.Name}");
                return;
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка создания экземпляра Resource {ex.Message} {obj.Name}");
            }
            return;
        }
        ~ChangeoverGroupRepo()
        {
            LogManager.Flush();
        }




    }
}
