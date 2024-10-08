﻿using System.Globalization;

using Microsoft.EntityFrameworkCore;

using Wkkim.Blog.Web.Data;
using Wkkim.Blog.Web.Models.Domain;

namespace Wkkim.Blog.Web.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly BlogDbContext blogDbContext;

        public TagRepository(BlogDbContext blogDbContext)
        {
            this.blogDbContext = blogDbContext;
        }
        
        public async Task<Tag> AddAsync(Tag tag)
        {
            await blogDbContext.Tags.AddAsync(tag);
            await blogDbContext.SaveChangesAsync();

            return tag;
        }

        public async Task<int> CountAsync()
        {
            return await blogDbContext.Tags.CountAsync();
        }

        public async Task<Tag?> DeleteAsync(Guid id)
        {
            var existingTag = await blogDbContext.Tags.FindAsync(id);

            if (existingTag != null)
            {
                blogDbContext.Tags.Remove(existingTag);
                await blogDbContext.SaveChangesAsync();

                return existingTag;
            }

            return null;
        }

        public async Task<IEnumerable<Tag>> GetAllAsync(string? searchQuery, string? sortBy, bool isDesc, int pageNumber =1, int pageSize = 100)
        {
            var query = blogDbContext.Tags.AsQueryable();

            // Filtering
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(t=>t.Name.Contains(searchQuery) || t.DisplayName.Contains(searchQuery));
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (string.Equals(sortBy, "Name", StringComparison.OrdinalIgnoreCase))
                {
                    query = isDesc ? query.OrderByDescending(t => t.Name): query.OrderBy(t => t.Name);
                }

                if (string.Equals(sortBy, "DisplayName", StringComparison.OrdinalIgnoreCase))
                {
                    query = isDesc ? query.OrderByDescending(t => t.DisplayName) : query.OrderBy(t => t.DisplayName);
                }
            }

            // Pagination
            // Skip 0 Take 5 => Page 1 of 5 Results
            // Skip 5 Take next 5 => Page 2 of 5 Results

            var skipResults = (pageNumber - 1) * pageSize;
            query = query.Skip(skipResults).Take(pageSize);

            return await query.ToListAsync();
        }

        public async Task<Tag?> GetAsync(Guid id)
        {
            return await blogDbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Tag?> UpdateAsync(Tag tag)
        {
            var existingTag = await blogDbContext.Tags.FindAsync(tag.Id);

            if (existingTag!=null)
            {
                existingTag.Name = tag.Name;
                existingTag.DisplayName = tag.DisplayName;

                await blogDbContext.SaveChangesAsync();

                return existingTag;
            }

            return null;
        }
    }
}
