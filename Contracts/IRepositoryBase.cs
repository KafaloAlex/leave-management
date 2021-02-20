using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace leave_management.Contracts
{
    public interface IRepositoryBase<T> where T : class //Permet de passer des classes en paramètres
    {
        Task<ICollection<T>> FindAll();

        Task<T> FindById(int id);

        Task<bool> isExists(int id);

        Task<bool> Create(T entity);

        Task<bool> Update(T entity);

        Task<bool> Delete(T entity);

        Task<bool> Save();
    }

    public interface IGenericRepository<T> where T : class
    {
        Task<IList<T>> FindAll(
            Expression<Func<T, bool>> expression = null, // q => q.Id == 20
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, // q => q.OrderBy
            List<string> includes = null
            );

        Task<T> Find(Expression<Func<T, bool>> expression, List<string> includes = null);

        Task<bool> isExists(Expression<Func<T, bool>> expression = null);

        Task Create(T entity);

        void Update(T entity);

        void Delete(T entity);
    }
}
