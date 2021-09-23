using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Query;

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
            IList<EntityType> queryResult = null;
            var options = new QueryOptions();
            try
            {
                if(parameters !=null && parameters.Any())
                {
                    options.Parameter(parameters);
                }
                options.RetryStrategy(new MyRetryStrategy());
                queryResult = await TryExecutePreparedQueryAsync<EntityType>(QueryName, options);
            }
            catch (Exception ex)
            {
                if(ex.Message == "QueryPreparedStatementFailure")
                {
                    // if prepared does not exists then delete any existing prepared on any other query nodes 
                    var blnDeletePrepare = await DeletePreparedQuery(QueryName);
                    if(blnDeletePrepare)
                    {
                        // Prepare the query 
                        var blnPrepareQuery = await PreparePreparedQuery(QueryName, Query); 
                        if(blnPrepareQuery)
                        {
                            // Execute the prepared query 
                            queryResult = await TryExecutePreparedQueryAsync<EntityType>(QueryName, options);
                        }
                    }
                }
                else
                {
                    throw ex;
                }
            }
            return queryResult;
        }

        private async Task<bool> DeletePreparedQuery(string QueryName)
        {
            var deletePreparedQuery = await _cluster.QueryAsync<dynamic>($"DELETE FROM system:prepareds where name = {QueryName}");

            if(deletePreparedQuery.MetaData.Status == Couchbase.Query.QueryStatus.Success)
            {
                return true;
            }
            else
            {
                HandleQueryException(deletePreparedQuery.Errors);
                return false;
            }
        }
    
        private async Task<IList<EntityType>> TryExecutePreparedQueryAsync<EntityType>(string QueryName,
                                                    QueryOptions options)
        {
            var queryResult = await _cluster.QueryAsync<EntityType>($"EXECUTE {QueryName}", 
                                                options);
            
            if(queryResult.MetaData.Status != QueryStatus.Success)
            {
                HandleQueryException(queryResult.Errors);
            }
            return await queryResult.Rows.ToListAsync();
        }

        private async Task<bool> PreparePreparedQuery(string QueryName, string Query)
        {
            var prepareQuery = await _cluster.QueryAsync<dynamic>($"PREPARE {QueryName} FROM {Query}");
            
            if(prepareQuery.MetaData.Status == Couchbase.Query.QueryStatus.Success)
            {
                return true;
            }
            else
            {
                HandleQueryException(prepareQuery.Errors);
                return false;
            }
        }

        private void HandleQueryException(List<Error> queryErrors)
        {
            string errMessage = string.Empty;
            foreach(var err in queryErrors)
            {
                errMessage = $"{errMessage} Error Code: {err.Code}, Error Message: {err.Message}";
            }
            throw new Exception(errMessage);
        }
    }
}