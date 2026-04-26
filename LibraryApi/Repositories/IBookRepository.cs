using LibraryApi.Dtos;
using LibraryApi.Models;

namespace LibraryApi.Repositories
{
    public interface IBookRepository
    {
        Task<List<Book>> GetAll();
        Task<Book?> GetById(Guid id);
        Task<Book> Add(Book book);
        Task Update(Book book);
        Task Delete(Guid id);
    }
}