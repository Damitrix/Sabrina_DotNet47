using System.Threading;

using DSharpPlus.Interactivity;

namespace Sabrina.Entities
{
    public interface IDependencies
    {
        InteractivityExtension Interactivity { get; set; }
        StartTimes StartTimes { get; set; }
        CancellationTokenSource Cts { get; set; }
    }

    public class Dependencies : IDependencies
    {
        public Dependencies(InteractivityExtension interactivity)
        {
            Interactivity = interactivity;
        }

        public InteractivityExtension Interactivity { get; set; }

        public StartTimes StartTimes { get; set; }

        public CancellationTokenSource Cts { get; set; }
    }
}