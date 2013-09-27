using System.Web.Http;
using System.Web.Mvc;

namespace Scrum.WebApi.Areas.Trace
{
    /// <summary>
    /// This class exists to register the In-Memory tracing feature.
    /// It is called during application start-up.
    /// </summary>
    /// <remarks>
	/// This code was obtained from <a href="https://aspnet.codeplex.com/SourceControl/changeset/view/7be4db2f1e67#Samples/Net4/CS/WebApi/MemoryTracingSample/ReadMe.txt">MemoryTracingSample</a>.
    /// </remarks>
    public class TraceAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Trace";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Trace_default",
                "Trace/{action}/{id}",
                new { controller = "Trace", action = "Index", id = UrlParameter.Optional }
            );

            TraceConfig.Register(GlobalConfiguration.Configuration);
        }
    }
}
