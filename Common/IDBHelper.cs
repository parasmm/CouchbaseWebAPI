using System.Collections.Generic;
using System.Threading.Tasks;

namespace CouchbaseWebAPI.Common
{
    public interface IDBHelper
    {
         Task<IList<EntityType>> ExecutePreparedQueryAsync<EntityType>(string QueryName,
                                                    string Query,
                                                    KeyValuePair<string, object>[] parameters);
    }
}