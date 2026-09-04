using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Models;
using EventParkingSystem.API.Repositories;

namespace EventParkingSystem.API.Services;

public interface ICategoryService
{
    Task<List<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryRequest request);
    Task DeleteAsync(int id);
}

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;
    public CategoryService(ICategoryRepository categories) => _categories = categories;

    public async Task<List<CategoryResponse>> GetAllAsync() => (await _categories.GetAllAsync()).Select(Map).ToList();

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var name = request.Name.Trim();
        if (await _categories.NameExistsAsync(name.ToLowerInvariant()))
            throw ApiException.Conflict("A category with this name already exists.");
        var category = new EventCategory { Name = name };
        await _categories.AddAsync(category);
        await _categories.SaveChangesAsync();
        return Map(category);
    }

    public async Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _categories.GetByIdAsync(id) ?? throw ApiException.NotFound("Category not found.");
        var name = request.Name.Trim();
        if (await _categories.NameExistsAsync(name.ToLowerInvariant(), id))
            throw ApiException.Conflict("A category with this name already exists.");
        category.Name = name;
        await _categories.SaveChangesAsync();
        return Map(category);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _categories.GetByIdAsync(id) ?? throw ApiException.NotFound("Category not found.");
        if (await _categories.HasEventsAsync(id))
            throw ApiException.Conflict("Category cannot be deleted while events use it.");
        _categories.Remove(category);
        await _categories.SaveChangesAsync();
    }

    private static CategoryResponse Map(EventCategory c) => new(c.CategoryId, c.Name, c.CreatedAt);
}
