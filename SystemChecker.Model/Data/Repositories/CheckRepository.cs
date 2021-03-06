﻿using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace SystemChecker.Model.Data.Repositories
{
    public class CheckRepository : Repository<Check>, ICheckRepository
    {
        public CheckRepository(ICheckerContext context)
            : base(context) { }

        public async Task<Check> GetDetails(int id, bool includeResults = false)
        {
            return await GetDetailsQuery(includeResults)
                .Where(x => x.ID == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Check>> GetDetails(bool activeOnly = false)
        {
            var query = GetDetailsQuery();
            if (activeOnly)
            {
                query = query.Where(x => x.Active);

            }
            return await query.ToListAsync();
        }

        private IQueryable<Check> GetDetailsQuery(bool includeResults = false)
        {
            var query = GetAll()
                .Include(x => x.Schedules)
                .Include(x => x.Data)
                .Include(x => x.Type)
                .Include(x => x.Group)
                .Include(x => x.SubChecks).ThenInclude(y => y.Type)
                .Include(x => x.Notifications);

            if (includeResults)
            {
                return query.Include(x => x.Results);
            }

            return query;
        }
    }
}
