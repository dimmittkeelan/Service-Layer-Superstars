using LibraryApi.DTOs.Books;
using LibraryApi.DTOs.Borrow;
using LibraryApi.DTOs.Members;
using LibraryApi.Entities;

namespace LibraryApi.DTOs.Mappings;

public static class DtoMappings
{
    public static BookResponseDto ToBookResponse(this Book book)
    {
        return new BookResponseDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies,
            RowVersion = Convert.ToBase64String(book.RowVersion)
        };
    }

    public static MemberResponseDto ToMemberResponse(this Member member)
    {
        return new MemberResponseDto
        {
            Id = member.Id,
            FullName = member.FullName,
            Email = member.Email,
            MembershipDate = member.MembershipDate,
            RowVersion = Convert.ToBase64String(member.RowVersion)
        };
    }

    public static BorrowRecordResponseDto ToBorrowRecordResponse(this BorrowRecord record)
    {
        return new BorrowRecordResponseDto
        {
            Id = record.Id,
            BookId = record.BookId,
            MemberId = record.MemberId,
            BorrowDate = record.BorrowDate,
            ReturnDate = record.ReturnDate,
            Status = record.Status.ToString()
        };
    }
}
