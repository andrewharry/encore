using Microsoft.EntityFrameworkCore;

namespace Encore
{
    public static class DbContextExtensions
    {
        public static bool IsInMemory(this DbContext context)
        {
            return context.Database.IsInMemory();
        }
    }
}
