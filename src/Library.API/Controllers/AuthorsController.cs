using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using Library.API.Models;
using System.Collections.Generic;
using AutoMapper;
using System;
using Library.API.Entities;
using Microsoft.AspNetCore.Http;
using Library.API.Helpers;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController: Controller
    {
        private ILibraryRepository _libraryRepository;
      
        private IUrlHelper _urlHelper;

        public AuthorsController (ILibraryRepository libraryRepository, IUrlHelper urlHelper)
        {
            _libraryRepository = libraryRepository;
            _urlHelper = urlHelper;
        }

        [HttpGet(Name ="GetAuthors")]
        //public IActionResult GetAuthors([FromQuery(Name ="page")] int pageNumber=1, [FromQuery] int pageSize=10)
        public IActionResult GetAuthors(AuthorsResourceParameters authorsResourceParameters)
        {            
            var authorsFromRepo = _libraryRepository.GetAuthors(authorsResourceParameters);

            var previousPageLink = authorsFromRepo.HasPrevious ?
                CreateAuthorsResourceUri(authorsResourceParameters,
                ResourceUriType.PreviousPage) : null;

            var nextPageLink = authorsFromRepo.HasNext ?
                CreateAuthorsResourceUri(authorsResourceParameters,
                ResourceUriType.NextPage) : null;

            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
                previousPageLink = previousPageLink,
                nextPageLink = nextPageLink
            };

            Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);            

            return Ok(authors);
        }
        
        private string CreateAuthorsResourceUri(
            AuthorsResourceParameters authorsResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            orderBy = authorsResourceParameters.OrderBy,
                            searchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber=authorsResourceParameters.PageNumber - 1,
                            pageSize=authorsResourceParameters.PageSize
                        });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            orderBy = authorsResourceParameters.OrderBy,
                            searchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber + 1,
                            pageSize = authorsResourceParameters.PageSize
                        });
                default:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            orderBy = authorsResourceParameters.OrderBy,
                            searchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber,
                            pageSize = authorsResourceParameters.PageSize
                        });
            }
        }

        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid id)         //[FromRoute]
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
           
            if (!_libraryRepository.Save())
            {
                //return StatusCode(500, "A problem happed with handling your request.");

                //Another option use global area
                throw new Exception("Creating an author fialed on save");
            }
           
            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);

            return CreatedAtRoute("GetAuthor", new { id = authorToReturn.Id }, authorToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            if (_libraryRepository.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);
            if(authorFromRepo == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteAuthor(authorFromRepo);
            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting author {id} failed on save.");
            }

            return NoContent();
        }
    }
}

/*
 * search EX
 * http://localhost:6058/api/authors?searchQuery=a&genre=Fantasy&pageNumber=2&pageSize=1
 * 
 * filter EX
 * http://localhost:6058/api/authors?genre=Fantasy&pageNumber=2&pageSize=1
X-Pagination →{"totalCount":2,"pageSize":1,"currentPage":2,"totalPages":2,"previousPageLink":"http://localhost:6058/api/authors?genre=Fantasy&pageNumber=1&pageSize=1","nextPageLink":null}
 
 
 * pagination EX
 * http://localhost:6058/api/authors?pageNumber=1&pageSize=5
 * see response header
 * X-Pagination →{"totalCount":6,"pageSize":5,"currentPage":1,"totalPages":2,"previousPageLink":null,"nextPageLink":"http://localhost:6058/api/authors?pageNumber=2&pageSize=5"}
 * 
 * 
 * ex DELETE 
 * http://localhost:6058/api/authors/25320c5e-f58a-4b1f-b63a-8ee07a840bdf

 ex POST
 http://localhost:6058/api/authors
 {
	"FirstName" : "James", 
	"LastName" : "Ellroy", 
	"DateOfBirth" : "1948-03-04T00:00:00", 
	"Genre" : "Thriller",
    "Books": [
        {
            "Title": American Tabloid",
            "Description": "First book in the Uderworld USA trilogy" 
        },
        {
            "Title":"The Cold Six Thousand",
            "Description":"Second book in the Uderworld USA trilogy"
        }
        ]
    }
 
ex
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
  "id": "740a0810-88fe-4146-818c-b11db67b6c8a",
  "name": "James Ellroy",
  "age": 69,
  "genre": "Thriller"
}


http://localhost:6058/api/authors/25320c5e-f58a-4b1f-b63a-8ee07a840bdf

 
 */
