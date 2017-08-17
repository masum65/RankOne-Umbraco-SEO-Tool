﻿using System;
using RankOne.Models;
using RankOne.IOC;
using RankOne.Interfaces;
using Umbraco.Web;

namespace RankOne.Repositories
{
    public class NodeReportRepository : BaseRepository<NodeReport>, INodeReportRepository
    {
        public NodeReportRepository(RankOneContext rankOneContext) : base(rankOneContext)
        {}

        public override NodeReport Insert(NodeReport dbEntity)
        {
            dbEntity.CreatedOn = DateTime.Now;
            return base.Insert(dbEntity);
        }

        public override NodeReport Update(NodeReport dbEntity)
        {
            dbEntity.UpdatedOn = DateTime.Now;
            return base.Update(dbEntity);
        }
    }
}