using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTEAPI.Models;

namespace MTEAPI.Services
{
    public interface IService<T> where T : class
    {
        T Get();
        T Get(AppSettings appsettings);
    }

    public class CachedDataService: IService<CachedDataClass.cachedData>
    {
        DataAccessCachedData  ds;
 
        public CachedDataService(DataAccessCachedData d)
        {
            ds = d;
        }
        public CachedDataClass.cachedData Get()
        {
            return ds.Get();
        }

        public CachedDataClass.cachedData Get(AppSettings appsettings)
        {
            return ds.Get(appsettings);
        }
    }

}
