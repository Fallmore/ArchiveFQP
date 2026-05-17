using ArchiveFqp.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Interfaces
{
    public class CrudGeneric
    {
        /// <summary>
        /// Upsert элемента от типа <typeparamref name="T"/> в базу данных. Если элемент с таким Id уже существует, он будет обновлен, иначе - добавлен.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="dbFactory"></param>
        /// <returns>Возвращает true, если элемент был обновлен, добавлен - false.</returns>
        public virtual async Task<bool> Upsert<T>(T item, IDbContextFactory<ArchiveFqpContext> dbFactory) where T : class
        {
            using ArchiveFqpContext context = dbFactory.CreateDbContext();
            DbSet<T> dbSet = context.Set<T>();
            bool exists = dbSet.Any(x => x == item);
            if (!exists)
                dbSet.Add(item);
            else
                dbSet.Update(item);
            await context.SaveChangesAsync();

            return exists;
        }

        /// <summary>
        /// удаляет элемент типа <typeparamref name="T"/> из базы данных по его идентификатору. Возвращает true, если элемент был найден и удален, иначе - false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="dbFactory"></param>
        /// <returns></returns>
        public virtual async Task<bool> Delete<T>(int id, IDbContextFactory<ArchiveFqpContext> dbFactory) where T : class
        {
            using var context = dbFactory.CreateDbContext();
            DbSet<T> dbSet = context.Set<T>();
            var item = await dbSet.FindAsync(id);
            if (item != null)
            {
                dbSet.Remove(item);
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
