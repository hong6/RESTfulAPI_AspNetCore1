using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using Library.API.Models;
using System.Collections.Generic;
using AutoMapper;
using System;
using Library.API.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Library.API.Helpers;
using Microsoft.Extensions.Logging;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private ILibraryRepository _libraryRepository;
        private ILogger<BooksController> _logger;

        public BooksController(ILibraryRepository libraryRepository,
            ILogger<BooksController> logger)
        {
            _logger = logger;
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

        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid Id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, Id);
            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            var bookForAuthor = Mapper.Map<BookDto>(bookForAuthorFromRepo);

            return Ok(bookForAuthor);
        }

        [HttpPost]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if(book.Title == book.Description)
            {
                ModelState.AddModelError(nameof(BookForCreationDto), "The provided description should be different from the title.");
            }

            if (!ModelState.IsValid)
            {
                //return 422
                return new UnprocessableEntityObjectResult(ModelState);
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = Mapper.Map<Book>(book);
            _libraryRepository.AddBookForAuthor(authorId, bookEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Creating a book for author {authorId} failed on save");
            }
            var bookToReturn = Mapper.Map<BookDto>(bookEntity);

            return CreatedAtRoute("GetBookForAuthor",
                new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteBook(bookForAuthorFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting book {id} for author {authorId} failed on save.");
            }

            _logger.LogInformation(100, $"Book {id} for author {authorId} was deleted.");

            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id,
            [FromBody] BookForUpdateDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto), "The provider description should be different from the title.");
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                //return NotFound();
                var bookToAdd = Mapper.Map<Book>(book);
                bookToAdd.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);
                if (!_libraryRepository.Save())
                {
                    throw new Exception($"Upserting book {id} for author {authorId} failed on save.");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id=bookToReturn.Id }, bookToReturn);
            }

            Mapper.Map(book, bookForAuthorFromRepo);
            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Updating book {id} for author {authorId} failed on PUT.");            
            }

            return NoContent();
        }

        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid id,
            [FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                var bookDto = new BookForUpdateDto();
                patchDoc.ApplyTo(bookDto, ModelState);

                if (bookDto.Description == bookDto.Title)
                {
                    ModelState.AddModelError(nameof(BookForUpdateDto),
                        "The provided description should be different from the title.");
                }

                TryValidateModel(bookDto);

                if (!ModelState.IsValid)
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                var bookToAdd = Mapper.Map<Book>(bookDto);
                bookToAdd.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);
                if (!_libraryRepository.Save())
                {
                    throw new Exception($"Upserting book {id} for author {authorId} failed on save PATCH.");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);
            }

            var bookToPatch = Mapper.Map<BookForUpdateDto>(bookForAuthorFromRepo);

            patchDoc.ApplyTo(bookToPatch, ModelState);
            //patchDoc.ApplyTo(bookToPatch); //test Microsoft.Extensions.Logging.Debug

            if (bookToPatch.Description == bookToPatch.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto),
                    "The provided description should be different from the title.");
            }

            TryValidateModel(bookToPatch);
            
            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            Mapper.Map(bookToPatch, bookForAuthorFromRepo);
            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);
            if (!_libraryRepository.Save())
            {
                throw new Exception($"Patching book {id} for author {authorId} failed on save.");
            }

            return NoContent();
        }
    }
}


/*
 EX PATCH Upserting
http://localhost:6058/api/authors/76053df4-6687-4353-8937-b45556748abe/books/448eb762-95e9-4c31-95e1-b20053fbe215
 [
{
  "op": "replace",
  "path": "/title",
  "value": "A Game of Thrones - PATCH Upserting test"
},
{
  "op": "replace",
  "path": "/description",
  "value": "Description - A Game of Thrones - PATCH Upserting test"
}
]
 EX PATCH
http://localhost:6058/api/authors/76053df4-6687-4353-8937-b45556748abe/books/447eb762-95e9-4c31-95e1-b20053fbe215
[{
  "op": "replace",
  "path": "/title",
  "value": "A Game of Thrones - PATCH test"
}]

 ex, PUT
 http://localhost:6058/api/authors/76053df4-6687-4353-8937-b45556748abe/books/447eb762-95e9-4c31-95e1-b20053fbe215
 {
  "title": "A Game of Thrones - updated",
  "description": "updated - "
}


 EX, DELETE
http://localhost:6058/api/authors/25320c5e-f58a-4b1f-b63a-8ee07a840bdf/books/70a1f9b9-0a37-4c1a-99b1-c7709fc64167

http://localhost:6058/api/authors/25320c5e-f58a-4b1f-b63a-8ee07a840bdf/books/
[
  {
    "id": "70a1f9b9-0a37-4c1a-99b1-c7709fc64167",
    "title": "It",
    "description": "It is a 1986 horror novel by American author Stephen King. The story follows the exploits of seven children as they are terrorized by the eponymous being, which exploits the fears and phobias of its victims in order to disguise itself while hunting its prey. 'It' primarily appears in the form of a clown in order to attract its preferred prey of young children.",
    "authorId": "25320c5e-f58a-4b1f-b63a-8ee07a840bdf"
  },
  {
    "id": "a3749477-f823-4124-aa4a-fc9ad5e79cd6",
    "title": "Misery",
    "description": "Misery is a 1987 psychological horror novel by Stephen King. This novel was nominated for the World Fantasy Award for Best Novel in 1988, and was later made into a Hollywood film and an off-Broadway play of the same name.",
    "authorId": "25320c5e-f58a-4b1f-b63a-8ee07a840bdf"
  },
  {
    "id": "c7ba6add-09c4-45f8-8dd0-eaca221e5d93",
    "title": "The Shining",
    "description": "The Shining is a horror novel by American author Stephen King. Published in 1977, it is King's third published novel and first hardback bestseller: the success of the book firmly established King as a preeminent author in the horror genre. ",
    "authorId": "25320c5e-f58a-4b1f-b63a-8ee07a840bdf"
  },
  {
    "id": "60188a2b-2784-4fc4-8df8-8919ff838b0b",
    "title": "The Stand",
    "description": "The Stand is a post-apocalyptic horror/fantasy novel by American author Stephen King. It expands upon the scenario of his earlier short story 'Night Surf' and outlines the total breakdown of society after the accidental release of a strain of influenza that had been modified for biological warfare causes an apocalyptic pandemic which kills off the majority of the world's human population.",
    "authorId": "25320c5e-f58a-4b1f-b63a-8ee07a840bdf"
  }
]


 * ex
http://localhost:6058/api/authors/f74d6899-9ed2-4137-9876-66b070553f8f/books
{
  "title" : "The Retstaurant at the End of the Universe", 
  "description" : "The sequel to The Hitchhiker's Guide to the Galary"
 }
{
  "id": "e12385a9-47cd-4a97-6b69-08d4a51156f6",
  "title": "The Retstaurant at the End of the Universe",
  "description": "The sequel to The Hitchhiker's Guide to the Galary",
  "authorId": "f74d6899-9ed2-4137-9876-66b070553f8f"
}
http://localhost:6058/api/authors/f74d6899-9ed2-4137-9876-66b070553f8f/books/608ec2aa-49a4-47e1-4f60-08d4a511ca5c

ex
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
