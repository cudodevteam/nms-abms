using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace nmsAbmsWeb {
    public abstract class ParserBase {
        protected ILogger logger;
        protected string msg;

        public abstract bool action();
    }
}
