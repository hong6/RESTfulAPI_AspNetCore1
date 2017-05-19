using AutoMapper;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController: Controller
    {       
        private ILibraryRepository _libraryRepository;

        public BooksController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet()]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var booksForAuthorFromRepo = _libraryRepository.GetBooksForAuthor(authorId);
            var booksForAuthor = Mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepo);

            return Ok(booksForAuthor);
        }

        [HttpGet("{id}")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid Id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, Id);
            if(bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            var bookForAuthor = Mapper.Map<BookDto>(bookForAuthorFromRepo);

            return Ok(bookForAuthor);
        }
    }
}

/*

http://localhost:6058/api/authors/76053df4-6687-4353-8937-b45556748abe/books
[
  {
    "id": "09af5a52-9421-44e8-a2bb-a6b9ccbc8239",
    "title": "A Dance with Dragons",
    "description": "A Dance with Dragons is the fifth of seven planned novels in the epic fantasy series A Song of Ice and Fire by American author George R. R. Martin.",
    "authorId": "76053df4-6687-4353-8937-b45556748abe"
  },
  {
    "id": "447eb762-95e9-4c31-95e1-b20053fbe215",
    "title": "A Game of Thrones",
    "description": "A Game of Thrones is the first novel in A Song of Ice and Fire, a series of fantasy novels by American author George R. R. Martin. It was first published on August 1, 1996.",
    "authorId": "76053df4-6687-4353-8937-b45556748abe"
  },
  {
    "id": "bc4c35c3-3857-4250-9449-155fcf5109ec",
    "title": "The Winds of Winter",
    "description": "Forthcoming 6th novel in A Song of Ice and Fire.",
    "authorId": "76053df4-6687-4353-8937-b45556748abe"
  }
]

    http://localhost:6058/api/authors/76053df4-6687-4353-8937-b45556748abe/books/bc4c35c3-3857-4250-9449-155fcf5109ec
 
    */
