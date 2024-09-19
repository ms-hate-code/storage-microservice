namespace BuildingBlocks.Utils
{
    public class NoSynchronizationContextScope
    {
        public static Disposable Enter()
        {
            var context = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);

            return new Disposable(context);
        }

        public class Disposable(SynchronizationContext synchronizationContext) : IDisposable
        {
            private readonly SynchronizationContext synchronizationContext = synchronizationContext;

            public void Dispose()
            {
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            }
        }
    }
}
