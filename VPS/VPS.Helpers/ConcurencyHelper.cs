namespace VPS.Helpers
{
    public static class ConcurrencyHelper
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1); 

        public static SemaphoreSlim GetSemaphorInstance()
        {
            return semaphore;
        }
    }
}
