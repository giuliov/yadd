# yadd
Yet another DB deployer

An exercise on validating DB deployment, inspired by Git design.

```Powershell
$env:YADD_PROVIDERNAME = "postegresql"
$env:YADD_CONNECTIONSTRING = "Host=localhost;Username=giuli;Database=mydb"

yadd info
 
yadd init
yadd history
yadd add .\sample-scripts\Create.sql
yadd add .\sample-scripts\Alter.sql
yadd show-stage
yadd commit "phase 1"
yadd history
yadd add .\sample-scripts\Alter-Next.sql
yadd show-stage
yadd commit "phase 2"
yadd history

# DROP TABLE pippo;

yadd upgrade
```

```Powershell
$env:YADD_PROVIDERNAME = "mssql"
$env:YADD_CONNECTIONSTRING = "Data Source=(localdb)\ProjectsV13;Initial Catalog=yadd-test;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"

yadd info
 
yadd init
yadd history
yadd add .\sample-scripts\Create.sql
yadd add .\sample-scripts\Alter.sql
yadd show-stage
yadd commit "phase 1"
yadd history
yadd add '.\sample-scripts\Alter-Next(MSSQL).sql'
yadd show-stage
yadd commit "phase 2"
yadd history

# DROP TABLE pippo;

yadd upgrade
```
