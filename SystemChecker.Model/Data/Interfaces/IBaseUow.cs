using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemChecker.Model.Data.Interfaces
{
    public interface IBaseUow : IDisposable
    {
        Task Commit();
        DbContext DatabaseContext { get; }
    }
}
