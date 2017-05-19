using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using Library.API.Models;
using System.Collections.Generic;
using AutoMapper;
using System;
using Library.API.Entities;

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

        [HttpGet("{id}", Name = "GetAuthor")]
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

        [HttpPost]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if(author == null)
            {
                return BadRequest();
            }

            var authorEntity = Mapper.Map<Author>(author);
            _libraryRepository.AddAuthor(authorEntity);
            /*
            if (!_libraryRepository.Save())
            {
                //return StatusCode(500, "A problem happed with handling your request.");

                //Another option use global area
                throw new Exception("Creating an author fialed on save");
            }
            */
            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);

            return CreatedAtRoute("GetAuthor", new { id = authorToReturn.Id }, authorToReturn);
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

    post
    http://localhost:6058/api/authors
    {
	"firstName" : "James", 
	"lastName" : "Ellroy", 
	"dateOfBirth" : "1948-03-04T00:00:00", 
	"firstname" : "Thriller" 
    }

    returned:
{
  "id": "69c79c07-36bd-4616-b634-b901cce43865",
  "name": "Thriller Ellroy",
  "age": 69,
  "genre": null
}
http://localhost:6058/api/authors/25320c5e-f58a-4b1f-b63a-8ee07a840bdf

 
 */
