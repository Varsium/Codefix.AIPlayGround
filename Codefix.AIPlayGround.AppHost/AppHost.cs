var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Codefix_AIPlayGround>("codefix-aiplayground");

builder.Build().Run();
