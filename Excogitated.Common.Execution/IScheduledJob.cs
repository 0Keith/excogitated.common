using System.Threading.Tasks;

namespace Excogitated.Common.Execution
{
    public interface IScheduledJob
    {
        ValueTask<JobResult> Execute(JobContext context);
    }

    public class JobContext
    {
    }

    public class JobResult
    {
        public bool Success { get; set; }
    }
}
