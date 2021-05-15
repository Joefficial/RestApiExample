using System;
using RestApiExample.Dtos;
using RestApiExample.Entities;

namespace RestApiExample.Extensions
{
    public static class ItemExtensions
    {
        public static ItemDto AsDto(this Item item)
        {
            return new ItemDto
            (
                item.Id,
                item.Name,
                item.Description,
                item.Price,
                item.CreatedDate
            );
        }
    }
}
