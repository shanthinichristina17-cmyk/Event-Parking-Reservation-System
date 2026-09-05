using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categories;
    public CategoriesController(ICategoryService categories) => _categories = categories;

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> GetAll() => Ok(await _categories.GetAllAsync());

    [HttpPost, Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(CreateCategoryRequest request)
        => StatusCode(StatusCodes.Status201Created, await _categories.CreateAsync(request));

    [HttpPut("{id:int}"), Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(int id, UpdateCategoryRequest request) => Ok(await _categories.UpdateAsync(id, request));

    [HttpDelete("{id:int}"), Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _categories.DeleteAsync(id);
        return NoContent();
    }
}
