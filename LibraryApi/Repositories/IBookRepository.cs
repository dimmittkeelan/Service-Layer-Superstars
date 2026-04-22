using LibraryApi.Dtos;
using LibraryApi.Models;

namespace LibraryApi.Repositories
{
    public interface IBookRepository
    {
        IEnumerable<Book> GetAll();
        Book? GetById(Guid id);
        
        Book Add(Book book);
        void Update(Book book);
        void Delete(Guid id);
    }
}