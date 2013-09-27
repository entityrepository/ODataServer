using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scrum.WebApi.Areas.Trace.Models
{
    /// <summary>
    /// Class that holds a collection of <see cref="RequestTrace"/> items
    /// for a single unique <see cref="Uri"/>.
    /// </summary>
    public class TraceGroup
    {
        private List<RequestTrace> _requestTraces = new List<RequestTrace>();

        public string Uri { get; set; }

        public string HttpMethod { get; set; }

        public IList<RequestTrace> RequestTraces { get { return _requestTraces; } }

        public string Path { get; set; }
    }
}
