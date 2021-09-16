using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Couchbase.Extensions.DependencyInjection;
using Couchbase;
using CouchbaseWebAPI.Common;
using Couchbase.Query;

namespace CouchbaseWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AirportController : ControllerBase
    {
        private readonly ILogger<AirportController> _logger;
        private readonly IBucket _bucket;
        private readonly ICluster cluster;
        private readonly IDBHelper dbHelper;

        public AirportController(ILogger<AirportController> logger,
                                INamedBucketProvider bucketProvider, 
                                IClusterProvider clusterProvider,
                                IDBHelper _dbHelper)
        {
            _logger = logger;
            _bucket = bucketProvider.GetBucketAsync().GetAwaiter().GetResult();
            cluster = clusterProvider.GetClusterAsync().GetAwaiter().GetResult();
            dbHelper = _dbHelper;
        }

        [HttpGet]
        [Route("{Id}")]
        public async Task<Airport> Get(string Id)
        {
            // Get default collection object
            var collection = await _bucket.DefaultCollectionAsync();
            // Get single document using KV search
            var getResult = await collection.GetAsync(Id);
            return getResult.ContentAs<Airport>();
        }

        [HttpGet]
        [Route("byCity/{city}")]
        public async Task<IList<Airport>> GetByCity(string city)
        {
            try
            {
                KeyValuePair<string, object>[] parameters = new KeyValuePair<string, object>[1];
                parameters[0] = new KeyValuePair<string, object>("$City", city);
                var queryResult = await cluster.QueryAsync<Airport>(@"select airportname, city, country, faa  
                                                                                        from `travel-sample` t 
                                                                                        where type = 'airport' 
                                                                                        and t.city = $City", 
                                                options => options.Parameter(parameters));
                
                return await queryResult.ToListAsync<Airport>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        [Route("byCityPrepared/{city}")]
        public async Task<IList<Airport>> GetByCityPrepared(string city)
        {
            try
            {
                KeyValuePair<string, object>[] parameters = new KeyValuePair<string, object>[1];
                parameters[0] = new KeyValuePair<string, object>("$City", city);
                var queryResult = await dbHelper.ExecutePreparedQueryAsync<Airport>("AirportInfoByCity", @"select airportname, city, country, faa  
                                                                                        from `travel-sample` t 
                                                                                        where type = 'airport' 
                                                                                        and t.city = $City",
                                                                                        parameters);
                return queryResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    
        [HttpPut]
        public async Task put([FromBody]Airport airport)
        {
            if (airport.id != 0)
            {    
                throw new Exception("Error in input data, Id should not be set!");
            }

            var collection = await _bucket.DefaultCollectionAsync();
            
            // defaulting the id value to insert. New Id generation has different approaches which is not discussed here. 
            airport.id = 1;
            
            // using the collection object insert the new airport object  
            await collection.InsertAsync<Airport>($"airport_{airport.id}", airport);

        }

        [HttpPost]
        public async Task post([FromBody]Airport airport)
        {
            if (airport.id == 0)
            {    
                throw new Exception("Error in input data, Id is required!");
            }

		    // get default collection of the bucket 
            var collection = await _bucket.DefaultCollectionAsync();
            // call ReplaceAsync function to save the modified version of the document 
            await collection.ReplaceAsync<Airport>($"airport_{airport.id}", airport);
        }

        [HttpDelete]
        [Route("{Id}")]
        public async Task delete(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {    
                throw new Exception("Error in input data, Id is required!");
            }

            var collection = await _bucket.DefaultCollectionAsync();
            // Id contains key in required k/v search. e.g. airport_1
            await collection.RemoveAsync(Id);
        }

    } 
}