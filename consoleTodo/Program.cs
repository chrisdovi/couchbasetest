using Couchbase.Lite;
using Couchbase.Lite.Logging;
using Couchbase.Lite.Query;
using Couchbase.Lite.Sync;

Database.Log.Console.Domains = LogDomain.All; 
Database.Log.Console.Level = LogLevel.Debug; 

// Get the database (and create it if it doesn't exist)
var database = new Database("mydb");

// Create replicator to push and pull changes to and from docker
var targetEndpoint = new URLEndpoint(new Uri("ws://localhost:4984/audit"));
var collection = database.CreateCollection("projects", "audit");

var replConfig = new ReplicatorConfiguration(targetEndpoint);
replConfig.AddCollection(collection);

//authenticate with local user
replConfig.Authenticator = new BasicAuthenticator("demo@example.com", "P@ssw0rd12");

replConfig.Continuous = true;
replConfig.ReplicatorType = ReplicatorType.PushAndPull;

// Create replicator (make sure to add an instance or static variable named _Replicator)
var replicator = new Replicator(replConfig);
replicator.AddChangeListener(OnReplicatorUpdate);

replicator.Start();

// Create a query to fetch documents of type SDK
// i.e. SELECT * FROM database
using var query = QueryBuilder.Select(SelectResult.All())
    .From(DataSource.Collection(collection));

// Run the query
var result = query.Execute();
Console.WriteLine($"Number of rows :: {result.AllResults().Count}");

Console.ReadKey();

// Later, stop and dispose the replicator *before* closing/disposing the database

while (true) 
{
    Console.WriteLine("Waiting");
}

void OnReplicatorUpdate(object sender, ReplicatorStatusChangedEventArgs e)
{
    var status = e.Status;

    switch (status.Activity)
    {
        case ReplicatorActivityLevel.Busy:
            Console.WriteLine("Busy transferring data.");
            break;
        case ReplicatorActivityLevel.Connecting:
            Console.WriteLine("Connecting to Sync Gateway.");
            break;
        case ReplicatorActivityLevel.Idle:
            Console.WriteLine("Replicator in idle state.");
            break;
        case ReplicatorActivityLevel.Offline:
            Console.WriteLine("Replicator in offline state.");
            break;
        case ReplicatorActivityLevel.Stopped:
            Console.WriteLine("Completed syncing documents.");
            break;
    }

    if (status.Progress.Completed == status.Progress.Total)
    {
        Console.WriteLine("All documents synced.");
    }
    else
    {
        Console.WriteLine($"Documents {status.Progress.Total - status.Progress.Completed} still pending sync");
    }
}
