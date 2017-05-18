using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using Library.API.Models;
using System.Collections.Generic;
using AutoMapper;
using System;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController: Controller
    {
        private ILibraryRepository _libraryRepository;

        public AuthorsController (ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet]
        public IActionResult GetAuthors()
        {
            var authorsFromRepo = _libraryRepository.GetAuthors();
            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);            

            return Ok(authors);
        }

        [HttpGet("{id}")]
        public IActionResult GetAuthor(Guid id)
        {           
            var authorFromRepo = _libraryRepository.GetAuthor(id);
            if(authorFromRepo == null)
            {
                return NotFound();
            }
            var author = Mapper.Map<AuthorDto>(authorFromRepo);

            return Ok(author);
        }
    }
}

/*
 http://localhost:6058/api/authors
 [
  {
    "id": "f74d6899-9ed2-4137-9876-66b070553f8f",
    "name": "Douglas Adams",
    "age": 65,
    "genre": "Science fiction"
  },
  {
    "id": "76053df4-6687-4353-8937-b45556748abe",
    "name": "George RR Martin",
    "age": 68,
    "genre": "Fantasy"
  },
  {
    "id": "a1da1d8e-1988-4634-b538-a01709477b77",
    "name": "Jens Lapidus",
    "age": 42,
    "genre": "Thriller"
  },
  {
    "id": "412c3012-d891-4f5e-9613-ff7aa63e6bb3",
    "name": "Neil Gaiman",
    "age": 56,
    "genre": "Fantasy"
  },
  {
    "id": "25320c5e-f58a-4b1f-b63a-8ee07a840bdf",
    "name": "Stephen King",
    "age": 69,
    "genre": "Horror"
  },
  {
    "id": "578359b7-1967-41d6-8b87-64ab7605587e",
    "name": "Tom Lanoye",
    "age": 58,
    "genre": "Various"
  }
]

    http://localhost:6058/api/authors/76053df4-6687-4353-8937-b45556748abe
 
 */
