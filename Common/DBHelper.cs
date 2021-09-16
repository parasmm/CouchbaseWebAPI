using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;

namespace CouchbaseWebAPI.Common
{
    public class DBHelper : IDBHelper
    {
        private IBucket _bucket;
        private ICluster _cluster;
        public DBHelper(INamedBucketProvider bucketProvider, 
                        IClusterProvider clusterProvider)
        {
            _bucket = bucketProvider.GetBucketAsync()
                                    .GetAwaiter()
                                    .GetResult();
            _cluster = clusterProvider.GetClusterAsync()
                                      .GetAwaiter()
                                      .GetResult();
        }

        public async Task<IList<EntityType>> ExecutePreparedQueryAsync<EntityType>(string QueryName,
                                                    string Query,
                                                    KeyValuePair<string, object>[] parameters)
        {
            string errMessage = string.Empty;
            try
            {
                var queryResult = await _cluster.QueryAsync<EntityType>($"EXECUTE {QueryName}", 
                                                options => options.Parameter(parameters).RetryStrategy(new MyRetryStrategy()));
                
                return await queryResult.Rows.ToListAsync<EntityType>();
            }
            catch (Exception ex)
            {
                if(ex.Message == "QueryPreparedStatementFailure")
                {
                    var deletePreparedQuery = await _cluster.QueryAsync<dynamic>($"DELETE FROM system:prepareds where name = {QueryName}");

                    if(deletePreparedQuery.MetaData.Status == Couchbase.Query.QueryStatus.Success)
                    {
                        var prepareQuery = await _cluster.QueryAsync<dynamic>($"PREPARE {QueryName} FROM {Query}");
                        if(prepareQuery.MetaData.Status == Couchbase.Query.QueryStatus.Success)
                        {
                            var queryResult = await _cluster.QueryAsync<EntityType>($"EXECUTE {QueryName}", 
                                                    options => options.Parameter(parameters));
                    
                            return await queryResult.Rows.ToListAsync<EntityType>();
                        }
                        else
                        {
                            foreach(var err in prepareQuery.Errors)
                            {
                                errMessage = $"{errMessage} Error Code: {err.Code}, Error Message: {err.Message}";
                            }
                            throw new Exception(errMessage);
                        }
                    }
                    else
                    {
                        foreach(var err in deletePreparedQuery.Errors)
                        {
                            errMessage = $"{errMessage} Error Code: {err.Code}, Error Message: {err.Message}";
                        }
                        throw new Exception(errMessage);
                    }
                }
                else
                {
                    throw ex;
                }
            }
        }
    }
}