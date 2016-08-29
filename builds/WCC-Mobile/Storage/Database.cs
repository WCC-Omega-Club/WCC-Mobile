
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace WCCMobile
{




    public interface IRepository<T> where T : class, new()
    {
        /// <summary>
        /// Gets a <see cref="List{T}"/> present implementing classes.
        /// </summary>
        /// <returns></returns>
        Task<List<T>> Get();
        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Task<T> Get(int id);
        /// <summary>
        /// Gets the specified predicate.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <param name="orderBy">The order by.</param>
        /// <returns></returns>
        Task<List<T>> Get<TValue>(Expression<Func<T, bool>> predicate = null, Expression<Func<T, TValue>> orderBy = null);
        /// <summary>
        /// Gets the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        Task<T> Get(Expression<Func<T, bool>> predicate);
        /// <summary>
        /// Gets the <see cref="Table{T}"/>.
        /// </summary>
        /// <returns></returns>
        AsyncTableQuery<T> AsQueryable();
        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        Task<int> Insert(T entity);
        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        Task<int> Update(T entity);
        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        Task<int> Delete(T entity);
    }

    public class Repository<T> : IRepository<T> where T : class, new()
    {
        /// <summary>
        /// The SQLite asynchronous database connection object.
        /// </summary>
        private SQLiteAsyncConnection db;
        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{T}"/> class.
        /// </summary>
        /// <param name="db">The database.</param>
        public Repository(SQLiteAsyncConnection db)
        {
            this.db = db;
        }

        public AsyncTableQuery<T> AsQueryable() =>
            db.Table<T>();

        public async Task<List<T>> Get() =>
            await db.Table<T>().ToListAsync();

        public async Task<List<T>> Get<TValue>(Expression<Func<T, bool>> predicate = null, Expression<Func<T, TValue>> orderBy = null)
        {
            var query = db.Table<T>();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            if (orderBy != null)
            {
                query = query.OrderBy(orderBy);
            }

            return await query.ToListAsync();
        }
        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<T> Get(int id) =>
            await db.FindAsync<T>(id);

        public async Task<T> Get(Expression<Func<T, bool>> predicate) =>
            await db.FindAsync<T>(predicate);

        public async Task<int> Insert(T entity) =>
            await db.InsertAsync(entity);

        public async Task<int> Update(T entity) =>
            await db.UpdateAsync(entity);

        public async Task<int> Delete(T entity) =>
            await db.DeleteAsync(entity);
    }
}
