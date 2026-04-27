namespace LibraryApi.Services;

public interface IBookCacheService
{
    void InvalidateBookCache(int bookId);
}
