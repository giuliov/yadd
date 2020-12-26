# yadd
Yet another DB deployer

An exercise on validating DB deployment, inspired by Git design.

```Powershell
$env:YADD_PROVIDERNAME = "postegresql"
$env:YADD_CONNECTIONSTRING = "Host=localhost;Username=giuliov;Database=mydb"
 
$env:YADD_PROVIDERNAME = "mssql"
$env:YADD_CONNECTIONSTRING = "Data Source=(localdb)\ProjectsV13;Initial Catalog=yadd-test;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
 
yadd init
yadd history
yadd add .\sample-scripts\Create.sql
yadd add .\sample-scripts\Alter.sql
yadd show-stage
yadd commit "done"
yadd history

yadd upgrade-from 218a8757 
```