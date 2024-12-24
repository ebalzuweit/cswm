using Microsoft.Extensions.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace cswm.Core.Test;

public class Startup
{
	public void ConfigureServices(IServiceCollection services)
	{
		services
			.AddCswmServices()
			.AddLogging(x => x.AddXunitOutput());
	}
}
