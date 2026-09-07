using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PosSelfOrdering.Client;
using PosSelfOrdering.Client.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register POS Domain Services & State
builder.Services.AddPosServices(builder.Configuration);

await builder.Build().RunAsync();

