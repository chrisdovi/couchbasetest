using Couchbase.Lite;
using Couchbase.Lite.Query;
using Couchbase.Lite.Sync;

//setup the database
var databaseConfig = new DatabaseConfiguration
{
    Directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
};

// Get the database (and create it if it doesn't exist)
var database = new Database("mydb", databaseConfig);

// Create replicator to push and pull changes to and from the cloud
// var targetEndpoint = new URLEndpoint(new Uri("wss://ujdiyqhabp-3f-lt.apps.cloud.couchbase.com:4984/location"));
// var collection = database.CreateCollection("test", "inventory");

// Create replicator to push and pull changes from docker
var targetEndpoint = new URLEndpoint(new Uri("ws://localhost:4984/inventorydemo"));
var collection = database.CreateCollection("projects", "audit");

var replConfig = new ReplicatorConfiguration(targetEndpoint);
replConfig.AddCollection(collection);

replConfig.Authenticator = new BasicAuthenticator("sync", "P@33w0rdS7nc");
replConfig.Continuous = true;
replConfig.ReplicatorType = ReplicatorType.PushAndPull;

// Create replicator (make sure to add an instance or static variable
// named _Replicator)
var replicator = new Replicator(replConfig);
replicator.AddChangeListener(OnReplicatorUpdate);

try {
    replicator.Start();
} catch (Exception e) {
    Console.WriteLine(e);
}

// Create a query to fetch documents of type SDK
// i.e. SELECT * FROM database
using var query = QueryBuilder.Select(SelectResult.All())
    .From(DataSource.Collection(collection));

// Run the query
var result = query.Execute();
Console.WriteLine($"Number of rows :: {result.AllResults().Count}");

Console.WriteLine("Press any key to stop...");
Console.ReadKey();

// Later, stop and dispose the replicator *before* closing/disposing the database
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