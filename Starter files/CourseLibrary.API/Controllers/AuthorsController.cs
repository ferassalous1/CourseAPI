﻿
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers;

[ApiController]
[Route("api/authors")]
public class AuthorsController : ControllerBase
{
    private readonly ICourseLibraryRepository _courseLibraryRepository;
    private readonly IMapper _mapper;

    public AuthorsController(
        ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper)
    {
        _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet]
    [HttpHead]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors()
    {
        // get authors from repo
        var authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync();

        // return them
        return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
    }

    [HttpGet("{authorId}", Name = "GetAuthor")]
    public async Task<ActionResult<AuthorDto>> GetAuthor(Guid authorId)
    {
        // get author from repo
        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo == null)
        {
            return NotFound();
        }

        // return author
        return Ok(_mapper.Map<AuthorDto>(authorFromRepo));
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorForCreationDto author)
    {
        var authorEntity = _mapper.Map<Author>(author);

        _courseLibraryRepository.AddAuthor(authorEntity);
        await _courseLibraryRepository.SaveAsync();

        var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

        return CreatedAtRoute("GetAuthor",
            new { authorId = authorToReturn.Id },
            authorToReturn);
    }

    //1,2,3
    //key1=val1, key2=val2
    [HttpPost("authorscollection")]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> CreateAuthors(IEnumerable<AuthorForCreationDto> authors)
    {
        var authorsEntity = _mapper.Map<IEnumerable<Author>>(authors);

        foreach (var author in authorsEntity)
        {
            _courseLibraryRepository.AddAuthor(author);
        }

        await _courseLibraryRepository.SaveAsync();

        var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorsEntity);

        var authorsId = string.Join(",",
            authorsToReturn.Select(a => a.Id));

        return CreatedAtRoute("GetAuthorsCollection",
            new { authorsId },
            authorsToReturn
            );
    }

    [HttpGet("authorsCollection/({authorsId})", Name = "GetAuthorsCollection")]
    public async Task<ActionResult<IEnumerable<AuthorDto>>>
        GetAuthorsCollection(
        [ModelBinder(BinderType = typeof(ArrayModelBinder))]
        [FromRoute] IEnumerable<Guid> authorsId)
    {
        var authorsEntities = await _courseLibraryRepository
            .GetAuthorsAsync(authorsId);

        if(authorsId.Count() != authorsEntities.Count())
        {
            return NotFound();
        }

        var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorsEntities);

        return Ok(authorsToReturn);

    }

    [HttpOptions]
    public IActionResult GetAuthorsOptions()
    {
        Response.Headers.Add("Allow", "GET,HEAD,POST,OPTIONS");
        return Ok();
    }
}
