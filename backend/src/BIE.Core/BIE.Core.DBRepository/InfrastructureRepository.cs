using BIE.Core.BaseRepository;
using BIE.Core.DataObjects;
using BIE.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace BIE.Core.DBRepository
{
    public class InfrastructureRepository : IInfrastructureRepository
    {
        private readonly IDatabase db;

        public InfrastructureRepository()
        {
            db = Database.Instance;
        }

        public void Add(Infrastructure entity)
        {
            DbCommand cmd = db.CreateSPCommand(Procds.Insert);
            db.AddParameter(cmd, Params.ShortName, entity.ShortName);
            db.AddParameter(cmd, Params.Name, entity.Name);

            entity.InfrastructureID = Convert.ToInt64(db.ExecuteScalar(cmd));
        }

        public void Delete(Infrastructure entity)
        {
            DbCommand cmd = db.CreateSPCommand(Procds.Delete);
            db.AddParameter(cmd, Params.InfrastructureID, entity.InfrastructureID);
            db.Execute(cmd);
        }


        public IEnumerable<Infrastructure> GetAll()
        {
            List<Infrastructure> Infrastructures = new();

            DbCommand cmd = db.CreateSPCommand(Procds.GetAll);
            var result = Convert.ToString(db.ExecuteScalar(cmd));
            Infrastructures = DbHelper.Convert<List<Infrastructure>>(result);

            return Infrastructures;
        }

        public Infrastructure GetById(long id)
        {
            DbCommand cmd = db.CreateSPCommand(Procds.Find);
            db.AddParameter(cmd, Params.InfrastructureID, id);
            string result = Convert.ToString(db.ExecuteScalar(cmd));
            Infrastructure found = DbHelper.Convert<Infrastructure>(result);

            return found;
        }

        public void Update(Infrastructure entity)
        {
            DbCommand cmd = db.CreateSPCommand(Procds.Update);
            db.AddParameter(cmd, Params.InfrastructureID, entity.InfrastructureID);
            db.AddParameter(cmd, Params.ShortName, entity.ShortName);
            db.AddParameter(cmd, Params.Name, entity.Name);
            db.Execute(cmd);
        }


        private struct Procds
        {
            public const string Insert = "spInsertInfrastructure";
            public const string Update = "spUpdateInfrastructure";
            public const string Delete = "spDeleteInfrastructure";
            public const string Find = "spFindInfrastructure";
            public const string GetAll = "spGetAllInfrastructure";
        }

        private struct Params
        {
            public const string InfrastructureID = "@InfrastructureID";
            public const string ShortName = "@ShortName";
            public const string Name = "@Name";
        }
    }
}
