var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.ProjectManager_API>("api");

var frontend = builder.AddNpmApp("vue-frontend", "../project-managament")
    .WithReference(api)
    .WithEnvironment("VITE_API_URL", api.GetEndpoint("http")) // <--- API adresini buraya bağlıyoruz
    .WithHttpEndpoint(targetPort : 5176, name : "http")
    .AsHttp2Service();
    //.WithExternalHttpEndpoints();

builder.Build().Run();
