using System;
using Build;
using Cake.AzurePipelines.Module;
using Cake.Frosting;

return new CakeHost()
    .UseContext<BuildContext>()
    .UseModule<AzurePipelinesModule>()
    //TODO: Use local tool manifest
    .InstallTool(new Uri("dotnet:?package=Grynwald.ChangeLog&version=0.4.135"))  
    .Run(args);
