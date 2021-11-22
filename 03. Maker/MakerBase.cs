using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace nmsAbmsWeb {
    public abstract class MakerBase {
        protected ILogger logger;
        protected string msg;

        public abstract bool action();
    }
}
