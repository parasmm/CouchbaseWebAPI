# CouchbaseWebAPI
 Using Couchbase with .Net Core Web API

**Pre-requisites:**

Install and configure Couchbase Community Edition 7.0.0
Install sample bucket – “travel-sample”
DotNet Core 3.1 SDK
Visual Studio Code 

**To get the source code -**
1. copy the clone URL from Github
2. Open Git Bash
3. Change the current working directory to the location where you want the cloned directory.
4. Type git clone, and then paste the URL you copied earlier.
```
$ git clone https://github.com/parasmm/CouchbaseWebAPI.git
```

**Configure couchbase connection in appsettings.json:**

Update appsettings.json to include appropriate connection info for Couchbase – 
```
"Couchbase" : { 
    "ConnectionString": "couchbase://127.0.0.1", 
    "UseSsl": false, 
    "UserName": "<Couchbase userid>", 
    "Password": "<Couchbase password>" 
} 
```

**To Build and run:**
1. Open command prompt
2. Browse to CouchbaseWebAPI folder 
3. Run Command `dotnet build`
4. Run Command `dotnet run`

At this point, application is ready to take requests.

**Get:**
Run below curl command to run API endpoint for Get
```
curl -X GET "https://localhost:5001/Airport/airport_3494" 
-H  "accept: text/plain"
```

**Create:**
Run below curl command to run API endpoint for create
```
curl -X PUT "https://localhost:5001/Airport" 
-H  "accept: */*" 
-H  "Content-Type: application/json" 
-d "{\"airportname\":\"MyAirport\",\"city\":\"MyCity\",\"country\":\"MyCountry\",\"faa\":\"mya\",\"geo\":{\"alt\":10,\"lat\":10,\"lon\":10},\"icao\":\"mya\",\"id\":0,\"type\":\"airport\",\"tz\":\"America/New_York\"}"
```

**Update:**
Run below curl command to run API endpoint for update
```
curl -X POST "https://localhost:5001/Airport" 
-H  "accept: */*" 
-H  "Content-Type: application/json" 
-d "{\"airportname\":\"MyAirport1\",\"city\":\"MyCity\",\"country\":\"MyCountry\",\"faa\":\"mya\",\"geo\":{\"alt\":10,\"lat\":10,\"lon\":10},\"icao\":\"mya\",\"id\":1,\"type\":\"airport\",\"tz\":\"America/New_York\"}"
```

**Delete:**
Run below curl command to run API endpoint for delete
```
curl -X DELETE "https://localhost:5001/Airport/airport_1" 
-H  "accept: */*"
```